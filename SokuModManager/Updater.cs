
using Newtonsoft.Json;
using SokuModManager.Models;
using SokuModManager.Models.Mod;
using SokuModManager.Models.Source;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SokuModManager
{
    public enum UpdaterStatus
    {
        Pending,
        Downloading,
        Extracting,
        CopyingFiles
    }

    public class UpdaterStatusChangedEventArgs : EventArgs
    {
        public UpdaterStatus Status { get; set; }
        public string Target { get; set; }
        public int? Progress { get; set; }
    }

    public class Updater
    {
        public event EventHandler<UpdaterStatusChangedEventArgs> UpdaterStatusChanged;
        private void OnUpdaterStatusChanged(UpdaterStatusChangedEventArgs e)
        {
            UpdaterStatusChanged?.Invoke(this, e);
        }

        public List<UpdateFileInfoModel> AvailableUpdateList { get; private set; } = new List<UpdateFileInfoModel>();
        public List<UpdateFileInfoModel> AvailableInstallList { get; private set; } = new List<UpdateFileInfoModel>();

        private readonly string sokuModUpdateTempDirPath = Path.Combine(Path.GetTempPath(), "SokuModUpdate");

        private ModManager ModManager { get; set; }

        public Updater(string sokuDirFullPath = null)
        {
            ClearUpdateTempDir();
            ModManager = new ModManager(sokuDirFullPath);
        }
        public Updater(ModManager modManager)
        {
            ClearUpdateTempDir();
            ModManager = modManager;
        }

        private void DeleteReadOnlyFilesInDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
            {
                return;
            }

            foreach (var file in Directory.GetFiles(directoryPath))
            {
                var fileInfo = new FileInfo(file);
                fileInfo.IsReadOnly = false;
                File.Delete(file);
            }

            foreach (var subDirectory in Directory.GetDirectories(directoryPath))
            {
                DeleteReadOnlyFilesInDirectory(subDirectory);
                Directory.Delete(subDirectory, true);
            }
        }

        private void ClearUpdateTempDir()
        {
            try
            {
                if (Directory.Exists(sokuModUpdateTempDirPath))
                {
                    DeleteReadOnlyFilesInDirectory(sokuModUpdateTempDirPath);
                    Directory.Delete(sokuModUpdateTempDirPath, true);
                }
            }
            catch { }
            Directory.CreateDirectory(sokuModUpdateTempDirPath);
        }

        public async Task<List<UpdateResultModel>> ExecuteUpdates(List<UpdateFileInfoModel> selectedUpdates)
        {
            var results = new List<UpdateResultModel>();
            foreach (var updateFileInfo in selectedUpdates)
            {
                try
                {
                    await DownloadAndExtractFile(updateFileInfo);
                    CopyAndReplaceFile(updateFileInfo);
                    results.Add(new UpdateResultModel
                    {
                        Name = updateFileInfo.Name,
                        Success = true
                    });
                }
                catch (Exception ex)
                {
                    Logger.LogError("Update failed", ex);

                    results.Add(new UpdateResultModel
                    {
                        Name = updateFileInfo.Name,
                        Success = false,
                        Message = ex.Message
                    });
                }
            }

            return results;
        }

        public List<UpdateFileInfoModel> GetUpdateFileInfosFromZip(string path)
        {
            string fileName = Path.GetFileName(path);

            using (ZipArchive zip = ZipFile.OpenRead(path))
            {

                var localArchiveUri = new Uri(path).AbsoluteUri;
                ZipArchiveEntry updateFileInfosJsonFile = zip.Entries.FirstOrDefault(x => x.FullName.ToLower().EndsWith("version.json") || x.FullName.ToLower().EndsWith("mod.json"));
                if (updateFileInfosJsonFile != null)
                {
                    string archiveUpdateFileDirname = Path.GetDirectoryName(updateFileInfosJsonFile.FullName);

                    using (Stream stream = updateFileInfosJsonFile.Open())
                    {

                        StreamReader reader = new StreamReader(stream);
                        var updateFileInfosJsonString = reader.ReadToEnd();
                        List<UpdateFileInfoModel> updateFileInfos = new List<UpdateFileInfoModel>();

                        try
                        {
                            UpdateFileInfoModel singleModel = JsonConvert.DeserializeObject<UpdateFileInfoModel>(updateFileInfosJsonString);
                            updateFileInfos.Add(singleModel);
                        }
                        catch
                        {
                            updateFileInfos = JsonConvert.DeserializeObject<List<UpdateFileInfoModel>>(updateFileInfosJsonString);
                        }

                        foreach (var updateFileInfo in updateFileInfos ?? new List<UpdateFileInfoModel>())
                        {
                            updateFileInfo.LocalArchiveUri = localArchiveUri;
                            updateFileInfo.UpdateWorkingDir = Path.Combine(archiveUpdateFileDirname, updateFileInfo.UpdateWorkingDir);
                        }

                        CompleteLocalModuleInfo(updateFileInfos);
                        return updateFileInfos;
                    }
                }
                else
                {
                    throw new Exception("Not mod package");
                }
            }
        }

        public List<UpdateFileInfoModel> GetUpdateFileInfosFromSource(SourceModel source)
        {
            var updateFileInfos = source.Modules
                .Where(x => !string.IsNullOrWhiteSpace(x.RecommendedVersionNumber) && x.RecommendedVersion != null)
                .Select(module =>
                {
                    UpdateFileInfoModel newUpdateFileInfoModel = new UpdateFileInfoModel()
                    {
                        Name = module.Name,
                        Description = module.Description,
                        DescriptionI18n = module.DescriptionI18n,
                        Version = module.RecommendedVersionNumber,
                        Notes = module.RecommendedVersion?.Notes ?? "",
                        NotesI18n = module.RecommendedVersion?.NotesI18n,
                        FileName = module.RecommendedVersion?.Main ?? "",
                        ConfigFiles = module.RecommendedVersion?.ConfigFiles,
                        DownloadLinks = module.RecommendedVersion?.DownloadLinks,
                        Compressed = true,
                        UpdateWorkingDir = $"./{module.Name}",
                        Icon = module.Icon,
                        Banner = module.Banner
                    };

                    newUpdateFileInfoModel.DownloadLinks = newUpdateFileInfoModel.DownloadLinks?
                        .OrderByDescending(link => link.Type.ToLower() == source.PreferredDownloadLinkType.ToLower())
                        .ToList();

                    return newUpdateFileInfoModel;
                })
                .ToList();

            CompleteLocalModuleInfo(updateFileInfos);
            return updateFileInfos;
        }

        public void RefreshAvailable(List<UpdateFileInfoModel> updateFileInfos, List<string> checkOnlyTheseMods = null, bool forceAvailableInstall = false)
        {
            try
            {
                AvailableUpdateList.Clear();
                AvailableInstallList.Clear();
                foreach (UpdateFileInfoModel updateFileInfo in updateFileInfos)
                {
                    if (checkOnlyTheseMods != null && !checkOnlyTheseMods.Any(x => x.ToLower() == updateFileInfo.Name.ToLower()))
                    {
                        continue;
                    }

                    if (updateFileInfo.Version != updateFileInfo.LocalFileVersion && updateFileInfo.Installed)
                    {
                        AvailableUpdateList.Add(updateFileInfo);
                    }

                    if (!updateFileInfo.Installed || forceAvailableInstall)
                    {
                        AvailableInstallList.Add(updateFileInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Refresh available failed", ex);
                throw;
            }
        }

        public async Task DownloadAndExtractFile(UpdateFileInfoModel updateFileInfo, CancellationToken? cancellationToken = null)
        {
            try
            {
                List<string> downloadLinks;
                string updateFileDir;

                if (updateFileInfo.FromLocalArchive)
                {
                    string localArchiveName = Path.GetFileNameWithoutExtension(updateFileInfo.LocalArchiveUri);
                    updateFileDir = Path.Combine(sokuModUpdateTempDirPath, localArchiveName);
                    if (Directory.Exists(updateFileDir))
                    {
                        return;
                    }
                    downloadLinks = new List<string> { updateFileInfo.LocalArchiveUri };
                }
                else
                {
                    if (updateFileInfo.DownloadLinks == null || updateFileInfo.DownloadLinks.Count == 0) throw new Exception($"{updateFileInfo.Name} no download link");
                    downloadLinks = updateFileInfo.DownloadLinks.Select(x => x.Url).ToList();
                    updateFileDir = Path.Combine(sokuModUpdateTempDirPath, updateFileInfo.Name);
                    if (Directory.Exists(updateFileDir))
                    {
                        Directory.Delete(updateFileDir, true);
                    }
                }

                Directory.CreateDirectory(updateFileDir);

                foreach (string downloadLink in downloadLinks)
                {
                    Uri uri = new Uri(downloadLink);

                    string remoteFileName = Path.GetFileName(uri.LocalPath);
                    string downloadToTempFilePath = Path.Combine(updateFileDir, remoteFileName);

                    try
                    {
                        if (downloadLink.StartsWith("file://"))
                        {
                            string localFilePath = new Uri(downloadLink).LocalPath;
                            if (File.Exists(localFilePath))
                            {
                                File.Copy(localFilePath, downloadToTempFilePath);
                            }
                            else
                            {
                                throw new Exception($"File not found: {localFilePath}");
                            }
                        }
                        else if (downloadLink.StartsWith("http://") || downloadLink.StartsWith("https://"))
                        {
                            using (HttpClient client = new HttpClient())
                            {

                                client.Timeout = TimeSpan.FromMinutes(10);

                                using (HttpResponseMessage response = await client.GetAsync(downloadLink, HttpCompletionOption.ResponseHeadersRead))
                                {
                                    // 確認回應狀態碼為成功
                                    response.EnsureSuccessStatusCode();

                                    // 確認 Content Headers 中有 Content-Length，以確定進度
                                    if (response.Content.Headers.TryGetValues("Content-Length", out var contentLengthValues))
                                    {
                                        long totalBytes = long.Parse(contentLengthValues?.First() ?? "0");
                                        long downloadedBytes = 0;

                                        // 開啟目標檔案，並使用 Response Content 中的資料寫入
                                        using (FileStream fileStream = File.OpenWrite(downloadToTempFilePath))
                                        {

                                            using (Stream contentStream = await response.Content.ReadAsStreamAsync())
                                            {

                                                byte[] buffer = new byte[8192];
                                                int bytesRead;

                                                // 逐步讀取和寫入檔案
                                                while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                                                {
                                                    cancellationToken?.ThrowIfCancellationRequested();
                                                    await fileStream.WriteAsync(buffer, 0, bytesRead);
                                                    downloadedBytes += bytesRead;

                                                    int progressPercentage = (int)((double)downloadedBytes / totalBytes * 100);
                                                    OnUpdaterStatusChanged(new UpdaterStatusChangedEventArgs
                                                    {
                                                        Status = UpdaterStatus.Downloading,
                                                        Target = updateFileInfo.Name,
                                                        Progress = progressPercentage
                                                    });
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError(string.Format("Failed to download {0}", downloadLink), ex);
                        continue;
                    }



                    if (updateFileInfo.Compressed)
                    {
                        OnUpdaterStatusChanged(new UpdaterStatusChangedEventArgs
                        {
                            Status = UpdaterStatus.Extracting,
                            Target = updateFileInfo.Name
                        });
                        ZipFile.ExtractToDirectory(downloadToTempFilePath, updateFileDir);
                        File.Delete(downloadToTempFilePath);
                    }
                    break;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError(string.Format("Failed to download {0}", updateFileInfo.FileName), ex);
                throw;
            }
        }

        public void CopyAndReplaceFile(UpdateFileInfoModel updateFileInfo)
        {
            OnUpdaterStatusChanged(new UpdaterStatusChangedEventArgs
            {
                Status = UpdaterStatus.CopyingFiles,
                Target = updateFileInfo.Name
            });
            string updateFileDir;
            if (updateFileInfo.FromLocalArchive)
            {
                string localArchiveName = Path.GetFileNameWithoutExtension(updateFileInfo.LocalArchiveUri);
                updateFileDir = Path.Combine(sokuModUpdateTempDirPath, localArchiveName);
            }
            else
            {
                updateFileDir = Path.Combine(sokuModUpdateTempDirPath, updateFileInfo.Name);
            }

            string updateWorkingDir = updateFileDir;
            if (!string.IsNullOrWhiteSpace(updateFileInfo.UpdateWorkingDir))
            {
                updateWorkingDir = Path.GetFullPath(Path.Combine(updateFileDir, updateFileInfo.UpdateWorkingDir));
            }

            // backup config files
            if (updateFileInfo.ConfigFiles != null)
            {
                foreach (string fileName in updateFileInfo.ConfigFiles)
                {
                    string localFileName = Path.Combine(updateFileInfo.LocalFileDir, fileName);
                    string tempFileName = Path.Combine(updateWorkingDir, fileName);
                    string tempFileDir = Path.GetDirectoryName(fileName);

                    if (File.Exists(localFileName))
                    {
                        Directory.CreateDirectory(Path.Combine(updateWorkingDir, tempFileDir));
                        File.Copy(localFileName, tempFileName, true);
                    }
                }
            }

            // replace main file
            if (!string.IsNullOrWhiteSpace(updateFileInfo.FileName))
            {
                string newVersionFileName = Path.Combine(updateWorkingDir, updateFileInfo.FileName);
                if (File.Exists(newVersionFileName))
                {
                    if (!string.IsNullOrWhiteSpace(updateFileInfo.LocalFileName) && updateFileInfo.FileName.ToLower() != Path.GetFileName(updateFileInfo.LocalFileName)?.ToLower())
                    {
                        File.Copy(newVersionFileName, Path.Combine(updateWorkingDir, Path.GetFileName(updateFileInfo.LocalFileName)), true);
                        File.Delete(newVersionFileName);
                    }
                    CopyDirectory(updateWorkingDir, updateFileInfo.LocalFileDir);
                }
                else
                {
                    Logger.LogInformation("Can not copy file:" + updateFileInfo.FileName);
                }
            }

            Directory.Delete(updateWorkingDir, true);
        }

        private void CompleteLocalModuleInfo(List<UpdateFileInfoModel> updateFileInfos)
        {
            foreach (UpdateFileInfoModel updateFileInfo in updateFileInfos)
            {
                ModManager.Refresh();
                ModManager.LoadSWRSToysSetting();

                switch (updateFileInfo.Name)
                {
                    case "SokuModLoader":
                        updateFileInfo.LocalFileName = Path.Combine(ModManager.SokuDirFullPath, "d3d9.dll");
                        updateFileInfo.LocalFileDir = ModManager.SokuDirFullPath;
                        if (!ModManager.SWRSToysD3d9Exist)
                        {
                            updateFileInfo.Installed = false;
                        }
                        else
                        {
                            var modInfoJsonFilename = Path.Combine(ModManager.SokuDirFullPath, "mod.json");
                            if (File.Exists(modInfoJsonFilename))
                            {
                                var json = File.ReadAllText(modInfoJsonFilename, Encoding.UTF8);
                                var result = JsonConvert.DeserializeObject<ModInfoModel>(json) ?? new ModInfoModel();
                                updateFileInfo.LocalFileVersion = result.Version;
                            }
                            else
                            {
                                updateFileInfo.LocalFileVersion = ModManager.GetVersionFromDllFileOrVersionTxt(updateFileInfo.LocalFileName).ToString();
                            }

                            updateFileInfo.Installed = true;
                        }
                        break;
                    default:
                        var modInfo = ModManager.GetModInfoByModName(updateFileInfo.Name) ?? ModManager.GetModInfoByModFileName(updateFileInfo.FileName);
                        if (modInfo == null || !File.Exists(modInfo.FullPath))
                        {
                            updateFileInfo.Installed = false;
                            updateFileInfo.LocalFileDir = Path.Combine(ModManager.DefaultModsDir, updateFileInfo.Name);
                        }
                        else
                        {
                            updateFileInfo.LocalFileName = modInfo.FullPath;
                            updateFileInfo.LocalFileDir = Path.GetDirectoryName(modInfo.FullPath);
                            updateFileInfo.LocalFileVersion = modInfo.Version;
                            updateFileInfo.Icon = modInfo.Icon;
                            updateFileInfo.Installed = true;
                        }
                        break;
                }
            }
        }

        private static void CopyDirectory(string sourceDir, string destinationDir, bool recursive = true)
        {
            var dir = new DirectoryInfo(sourceDir);
            if (!dir.Exists)
            {
                Logger.LogError($"Source directory not found", new DirectoryNotFoundException(dir.FullName));
                return;
            }

            DirectoryInfo[] dirs = dir.GetDirectories();
            Directory.CreateDirectory(destinationDir);
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                var targetFile = new FileInfo(targetFilePath);
                if (targetFile.Exists && targetFile.IsReadOnly)
                {
                    targetFile.IsReadOnly = false;
                }
                File.Copy(file.FullName, targetFilePath, true);
            }

            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, true);
                }
            }
        }
    }
}
