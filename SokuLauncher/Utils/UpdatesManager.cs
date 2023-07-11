using Newtonsoft.Json;
using SokuLauncher.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace SokuLauncher.Utils
{
    public class UpdatesManager
    {
        private const string DEFAULT_VERSION_INFO_URL = "https://soku.latte.today/version.json";
        private const string MOD_VERSION_FILENAME_SUFFIX = ".version.txt";

        public event Action<int> DownloadProgressChanged;
        public List<UpdateFileInfoModel> AvailableUpdateList { get; private set; } = new List<UpdateFileInfoModel>();

        private readonly string UpdateTempDirPath = Path.Combine(Static.TempDirPath, "Update");

        private ConfigModel Config;
        private ModsManager ModsManager;
        private string SokuDirFullPath;

        public UpdatesManager(ConfigModel config, ModsManager modsManager, string sokuDirFullPath)
        {
            Directory.CreateDirectory(UpdateTempDirPath);
            Config = config;
            ModsManager = modsManager;
            SokuDirFullPath = sokuDirFullPath;
        }

        private Version GetFileCurrentVersion(string fileName)
        {
            var fileVersionInfo = FileVersionInfo.GetVersionInfo(fileName);
            if (fileVersionInfo?.FileVersion != null)
            {
                return new Version(fileVersionInfo.FileVersion);
            }
            return null;
        }

        public void CheckUpdate()
        {
            try
            {
                WebClient client = new WebClient();
                string response = client.DownloadString(Config.VersionInfoUrl ?? DEFAULT_VERSION_INFO_URL);

                List<UpdateFileInfoModel> latestVersionInfoList = JsonConvert.DeserializeObject<List<UpdateFileInfoModel>>(response);

                AvailableUpdateList.Clear();
                foreach (UpdateFileInfoModel lastestVersionInfo in latestVersionInfoList)
                {
                    Version latestVersion = new Version(lastestVersionInfo.Version);

                    Version currentVersion = null;

                    switch (lastestVersionInfo.Name)
                    {
                        case "SokuLauncher":
                            currentVersion = GetFileCurrentVersion(Static.SelfFileName);
                            lastestVersionInfo.LocalFileName = Static.SelfFileName;
                            break;
                        case "SWRSToys":
                            lastestVersionInfo.LocalFileName = Path.Combine(SokuDirFullPath, "d3d9.dll");
                            if (ModsManager.SWRSToysD3d9Exist)
                            {
                                currentVersion = GetFileCurrentVersion(lastestVersionInfo.LocalFileName);
                            }
                            else
                            {
                                lastestVersionInfo.Installed = false;
                            }
                            break;
                        default:
                            var modInfo = ModsManager.GetModInfoByModName(lastestVersionInfo.Name);
                            if (modInfo == null)
                            {
                                Directory.CreateDirectory(ModsManager.DefaultModsDir);
                                Directory.CreateDirectory(Path.Combine(ModsManager.DefaultModsDir, lastestVersionInfo.Name));
                                lastestVersionInfo.LocalFileName = Path.Combine(ModsManager.DefaultModsDir, lastestVersionInfo.Name, lastestVersionInfo.FileName);
                                lastestVersionInfo.Installed = false;
                            }
                            else
                            {
                                lastestVersionInfo.LocalFileName = modInfo.FullPath;
                                currentVersion = GetFileCurrentVersion(lastestVersionInfo.LocalFileName);
                            }
                            break;
                    }

                    if (currentVersion == null)
                    {
                        string modVersionFileName = Path.Combine(lastestVersionInfo.LocalFileDir, $"{lastestVersionInfo.Name}{MOD_VERSION_FILENAME_SUFFIX}");
                        string modCurrentVersion = "0.0.0.0";
                        if (File.Exists(modVersionFileName))
                        {
                            modCurrentVersion = File.ReadAllText(modVersionFileName);
                        }

                        currentVersion = new Version(modCurrentVersion);
                    }

                    lastestVersionInfo.LocalFileVersion = currentVersion.ToString();

                    if (currentVersion != null && latestVersion > currentVersion)
                    {
                        AvailableUpdateList.Add(lastestVersionInfo);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to check for updates: " + ex.Message);
            }
        }

        public async Task DownloadAndExtractFile(UpdateFileInfoModel updateFileInfo)
        {
            try
            {
                string updateFileDir = Path.Combine(UpdateTempDirPath, updateFileInfo.Name);
                if (Directory.Exists(updateFileDir))
                {
                    Directory.Delete(updateFileDir, true);
                }
                Directory.CreateDirectory(updateFileDir);

                string remoteFileName = Path.GetFileName(updateFileInfo.DownloadUrl);
                string downloadToTempFilePath = Path.Combine(updateFileDir, remoteFileName);

                using (WebClient client = new WebClient())
                {
                    client.DownloadProgressChanged += (sender, e) => DownloadProgressChanged?.Invoke(e.ProgressPercentage);
                    await client.DownloadFileTaskAsync(updateFileInfo.DownloadUrl, downloadToTempFilePath);
                }

                if (updateFileInfo.Compressed)
                {
                    ZipFile.ExtractToDirectory(downloadToTempFilePath, updateFileDir);
                    File.Delete(downloadToTempFilePath);
                }

            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to download {updateFileInfo.FileName}: " + ex.Message);
            }
        }

        public void CopyAndReplaceFile(UpdateFileInfoModel updateFileInfo)
        {
            string updateFileDir = Path.Combine(UpdateTempDirPath, updateFileInfo.Name);

            // replace extra files
            if (updateFileInfo.ExtraFiles != null)
            {
                CopyFilesFromUpdateTempDir(updateFileDir, updateFileInfo.ExtraFiles, updateFileInfo.LocalFileDir, true);
            }

            // copy config files
            if (updateFileInfo.ConfigFiles != null)
            {
                CopyFilesFromUpdateTempDir(updateFileDir, updateFileInfo.ConfigFiles, updateFileInfo.LocalFileDir);
            }

            // replace main file
            if (!string.IsNullOrWhiteSpace(updateFileInfo.FileName))
            {
                string newVersionFileName = Path.Combine(updateFileDir, updateFileInfo.FileName);
                if (File.Exists(newVersionFileName))
                {
                    if (updateFileInfo.LocalFileName == Static.SelfFileName)
                    {
                        // replace self
                        string replaceBatPath = Path.Combine(updateFileDir, "replace.bat");

                        string batContent = $"copy \"{newVersionFileName}\" \"{Static.SelfFileName}\" /Y{Environment.NewLine}";
                        batContent += $"del \"{newVersionFileName}\"{Environment.NewLine}";
                        batContent += $"start \"\" \"{Static.SelfFileName}\"{Environment.NewLine}";
                        batContent += $"del \"{replaceBatPath}\"";

                        File.WriteAllText(replaceBatPath, batContent);

                        Process.Start(replaceBatPath);
                        Environment.Exit(0);
                    }
                    else
                    {
                        File.Copy(newVersionFileName, updateFileInfo.LocalFileName, true);
                        File.Delete(newVersionFileName);
                    }
                }
                else
                {
                    throw new Exception("Can not copy file: " + updateFileInfo.FileName);
                }
            }

            string modVersionFileName = Path.Combine(updateFileInfo.LocalFileDir, $"{updateFileInfo.Name}{MOD_VERSION_FILENAME_SUFFIX}");
            File.WriteAllText(modVersionFileName, updateFileInfo.Version);
            Directory.Delete(updateFileDir, true);
        }

        private void CopyFilesFromUpdateTempDir(string fileDir, List<string> fileNames, string localFileDir, bool overwrite = false)
        {
            foreach (var fileName in fileNames)
            {
                string localFileName = Path.Combine(localFileDir, fileName);
                string newVersionFileName = Path.Combine(fileDir, fileName);

                if (File.Exists(newVersionFileName))
                {
                    if (overwrite || !File.Exists(localFileName))
                    {
                        File.Copy(newVersionFileName, localFileName, overwrite);
                    }
                    File.Delete(newVersionFileName);
                }
                else
                {
                    throw new Exception("Can not copy file: " + fileName);
                }
            }
        }
    }
}
