using Newtonsoft.Json;
using SokuLauncher.Controls;
using SokuLauncher.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace SokuLauncher.Utils
{
    public class UpdatesManager
    {
        private const string DEFAULT_VERSION_INFO_URL = "https://soku.latte.today/version.json";
        private const string MOD_VERSION_FILENAME_SUFFIX = ".version.txt";

        public event Action<int> DownloadProgressChanged;
        public event Action<string> StatusChanged;

        public List<UpdateFileInfoModel> AvailableUpdateList { get; private set; } = new List<UpdateFileInfoModel>();
        public string VersionInfoJson { get; private set; }
        public bool IsVersionInfoJsonDownloading { get; private set; } = false;
        public Task VersionInfoJsonDownloadTask { get; private set; }
        public string ReplaceBatPath { get; private set; }

        private readonly string UpdateTempDirPath = Path.Combine(Static.TempDirPath, "Update");

        private ConfigUtil ConfigUtil;
        private ModsManager ModsManager;

        public UpdatesManager(ConfigUtil configUtil, ModsManager modsManager)
        {
            Directory.CreateDirectory(UpdateTempDirPath);
            ConfigUtil = configUtil;
            ModsManager = modsManager;
        }

        private bool IsStopCheckForUpdates = false;
        public void StopCheckForUpdates()
        {
            IsStopCheckForUpdates = true;
        }

        public void CheckForUpdates(bool isAutoUpdates = true)
        {
            try
            {
                GetAvailableUpdateList();
                if (AvailableUpdateList.Count > 0)
                {
                    if (IsStopCheckForUpdates)
                    {
                        IsStopCheckForUpdates = false;
                        return;
                    }

                    UpdateSelectionWindow updateSelectionWindow = new UpdateSelectionWindow
                    {
                        Desc = "The following mods have updates available. Please check the mods you want to update.",
                        AvailableUpdateList = AvailableUpdateList,
                        IsAutoCheckForUpdatesCheckBoxShow = isAutoUpdates,
                        AutoCheckForUpdates = ConfigUtil.Config.AutoCheckForUpdates
                    };

                    bool isRunUpdate = updateSelectionWindow.ShowDialog() == true;

                    if (isAutoUpdates)
                    {
                        if (ConfigUtil.Config.AutoCheckForUpdates != updateSelectionWindow.AutoCheckForUpdates)
                        {
                            ConfigUtil.Config.AutoCheckForUpdates = updateSelectionWindow.AutoCheckForUpdates;
                            ConfigUtil.SaveConfig();
                        }
                    }

                    if (isRunUpdate)
                    {

                        var selectedUpdates = updateSelectionWindow.AvailableUpdateList.Where(x => x.Selected).ToList();

                        UpdatingWindow updatingWindow = new UpdatingWindow
                        {
                            UpdatesManager = this,
                            AvailableUpdateList = selectedUpdates
                        };

                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                foreach (var updateFileInfo in selectedUpdates)
                                {
                                    await DownloadAndExtractFile(updateFileInfo);
                                    CopyAndReplaceFile(updateFileInfo);
                                }
                                updatingWindow.Dispatcher.Invoke(() => updatingWindow.Close());
                                if (ReplaceBatPath != null)
                                {
                                    Process.Start(ReplaceBatPath);
                                    updatingWindow.Dispatcher.Invoke(() => Application.Current.Shutdown());
                                }
                                if (!isAutoUpdates)
                                {
                                    MessageBox.Show("All updates completed", "Updates", MessageBoxButton.OK, MessageBoxImage.Information);
                                }
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show(ex.Message, "Updates", MessageBoxButton.OK, MessageBoxImage.Error);
                                updatingWindow.Dispatcher.Invoke(() => updatingWindow.Close());
                            }
                        });
                        updatingWindow.ShowDialog();
                    }
                }
                else
                {
                    if (!isAutoUpdates)
                    {
                        MessageBox.Show("All available mods have been updated to the latest version", "Updates", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task CheckForInstallable(List<string> modsToCheckList)
        {
            try
            {
                GetAvailableUpdateList(false, true, modsToCheckList);
                if (AvailableUpdateList.Count > 0)
                {
                    UpdateSelectionWindow updateSelectionWindow = new UpdateSelectionWindow
                    {
                        Desc = "The following mods are missing. Please install to make sure the game works correctly.",
                        AvailableUpdateList = AvailableUpdateList,
                        IsAutoCheckForUpdatesCheckBoxShow = false
                    };

                    if (updateSelectionWindow.ShowDialog() == true)
                    {
                        var selectedUpdates = updateSelectionWindow.AvailableUpdateList.Where(x => x.Selected).ToList();

                        foreach (var updateFileInfo in selectedUpdates)
                        {
                            await DownloadAndExtractFile(updateFileInfo);
                            CopyAndReplaceFile(updateFileInfo);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task GetVersionInfoJson()
        {
            if (IsVersionInfoJsonDownloading)
            {
                await VersionInfoJsonDownloadTask;
            }
            else
            {
                IsVersionInfoJsonDownloading = true;
                try
                {
                    VersionInfoJsonDownloadTask = Task.Run(async () =>
                    {
                        using (WebClient client = new WebClient())
                        {
                            client.Encoding = Encoding.UTF8;
                            client.DownloadProgressChanged += (sender, e) =>
                            {
                                DownloadProgressChanged?.Invoke(e.ProgressPercentage);
                                StatusChanged?.Invoke("Check version info... " + $"{e.ProgressPercentage}%");
                            };
                            VersionInfoJson = await client.DownloadStringTaskAsync(string.IsNullOrWhiteSpace(ConfigUtil.Config.VersionInfoUrl) ? DEFAULT_VERSION_INFO_URL : ConfigUtil.Config.VersionInfoUrl);
                        }
                    });
                    await VersionInfoJsonDownloadTask;
                }
                finally
                {
                    IsVersionInfoJsonDownloading = false;
                }
            }
        }

        public void GetAvailableUpdateList(bool checkForUpdates = true, bool checkForInstallable = false, List<string> modsToCheckList = null)
        {
            List<UpdateFileInfoModel> latestVersionInfoList;
            try
            {
                latestVersionInfoList = JsonConvert.DeserializeObject<List<UpdateFileInfoModel>>(VersionInfoJson);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Parsing VersionInfo failed: {ex}", "Update", MessageBoxButton.OK, MessageBoxImage.Error);
                VersionInfoJson = null;
                return;
            }

            try
            {
                AvailableUpdateList.Clear();
                foreach (UpdateFileInfoModel updateFileInfo in latestVersionInfoList)
                {
                    if (modsToCheckList != null && modsToCheckList.FirstOrDefault(x => x.ToLower() == updateFileInfo.Name.ToLower()) == null)
                    {
                        continue;
                    }

                    Version latestVersion = new Version(updateFileInfo.Version);
                    Version currentVersion = null;

                    switch (updateFileInfo.Name)
                    {
                        case "SokuLauncher":
                            currentVersion = GetFileCurrentVersion(Static.SelfFileName);
                            updateFileInfo.LocalFileName = Static.SelfFileName;
                            break;
                        case "SokuModLoader":
                            updateFileInfo.LocalFileName = Path.Combine(ConfigUtil.SokuDirFullPath, "d3d9.dll");
                            if (ModsManager.SWRSToysD3d9Exist)
                            {
                                currentVersion = GetFileCurrentVersion(updateFileInfo.LocalFileName);
                            }
                            else
                            {
                                updateFileInfo.Installed = false;
                            }
                            break;
                        default:
                            var modInfo = ModsManager.GetModInfoByModName(updateFileInfo.Name);
                            if (modInfo == null || !File.Exists(modInfo.FullPath))
                            {
                                string fileNameForbiddenCharactersPattern = @"[\\/:*?""<>|]";
                                string fileName = Regex.Replace(updateFileInfo.Name, fileNameForbiddenCharactersPattern, "_");

                                updateFileInfo.LocalFileName = Path.Combine(ModsManager.DefaultModsDir, updateFileInfo.Name, $"{fileName}{Path.GetExtension(updateFileInfo.FileName)}");
                                updateFileInfo.Installed = false;
                            }
                            else
                            {
                                updateFileInfo.LocalFileName = modInfo.FullPath;
                                currentVersion = GetFileCurrentVersion(updateFileInfo.LocalFileName);
                            }
                            break;
                    }

                    if (currentVersion == null)
                    {
                        string modVersionFileName = Path.Combine(updateFileInfo.LocalFileDir, $"{updateFileInfo.Name}{MOD_VERSION_FILENAME_SUFFIX}");
                        string modCurrentVersion = "0.0.0.0";
                        if (File.Exists(modVersionFileName))
                        {
                            modCurrentVersion = File.ReadAllText(modVersionFileName);
                        }

                        currentVersion = new Version(modCurrentVersion);
                    }

                    if (currentVersion.Revision == -1)
                    {
                        currentVersion = new Version(currentVersion.Major, currentVersion.Minor, currentVersion.Build, 0);
                    }

                    updateFileInfo.LocalFileVersion = currentVersion.ToString();

                    if (checkForUpdates && latestVersion > currentVersion && updateFileInfo.Installed ||
                        checkForInstallable && !updateFileInfo.Installed)
                    {

                        if (updateFileInfo.I18nDesc != null)
                        {
                            string localDesc =
                                updateFileInfo.I18nDesc.FirstOrDefault(x => x.Language == ConfigUtil.Config.Language)?.Content
                                ?? updateFileInfo.I18nDesc.FirstOrDefault(x => x.Language != null && x.Language.Split('-')[0] == ConfigUtil.Config.Language.Split('-')[0])?.Content;
                            if (!string.IsNullOrWhiteSpace(localDesc))
                            {
                                updateFileInfo.Desc = localDesc;
                            }
                        }

                        AvailableUpdateList.Add(updateFileInfo);
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
                    client.DownloadProgressChanged += (sender, e) =>
                    {
                        DownloadProgressChanged?.Invoke(e.ProgressPercentage);
                        StatusChanged?.Invoke($"Downloading {updateFileInfo.Name}... " + $"{e.ProgressPercentage}%");
                    };
                    await client.DownloadFileTaskAsync(updateFileInfo.DownloadUrl, downloadToTempFilePath);
                }

                if (updateFileInfo.Compressed)
                {
                    StatusChanged?.Invoke($"Extracting {updateFileInfo.Name}... ");
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
            StatusChanged?.Invoke($"Updating {updateFileInfo.Name}... ");
            string updateFileDir = Path.Combine(UpdateTempDirPath, updateFileInfo.Name);
            string updateWorkingDir = updateFileDir;
            if (!string.IsNullOrWhiteSpace(updateFileInfo.UpdateWorkingDir))
            {
                updateWorkingDir = Path.GetFullPath(Path.Combine(updateFileDir, updateFileInfo.UpdateWorkingDir));
            }

            // backup config files
            if (updateFileInfo.ConfigFiles != null)
            {
                foreach (var fileName in updateFileInfo.ConfigFiles)
                {
                    string localFileName = Path.Combine(updateFileInfo.LocalFileDir, fileName);
                    string tempFileName = Path.Combine(updateWorkingDir, fileName);
                    string tempFileFileDir = Path.GetDirectoryName(fileName);

                    if (File.Exists(localFileName))
                    {
                        Directory.CreateDirectory(Path.Combine(updateWorkingDir, tempFileFileDir));
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
                    if (updateFileInfo.LocalFileName == Static.SelfFileName)
                    {
                        // replace self
                        string replaceBatPath = Path.Combine(updateFileDir, "replace.bat");

                        string batContent = $"echo Waiting update SokuLauncher...";
                        batContent += $"@ping 127.0.0.1 -n 5 -w 1000 > nul{Environment.NewLine}";
                        batContent += $"copy \"{newVersionFileName}\" \"{Static.SelfFileName}\" /Y{Environment.NewLine}";
                        batContent += $"del \"{newVersionFileName}\"{Environment.NewLine}";
                        batContent += $"start \"\" \"{Static.SelfFileName}\"{Environment.NewLine}";
                        batContent += $"del \"{replaceBatPath}\"";

                        File.WriteAllText(replaceBatPath, batContent, Encoding.Default);
                        ReplaceBatPath = replaceBatPath;
                        return;
                    }
                    else
                    {
                        if (updateFileInfo.FileName.ToLower() != Path.GetFileName(updateFileInfo.LocalFileName).ToLower())
                        {
                            File.Move(newVersionFileName, Path.Combine(updateWorkingDir, Path.GetFileName(updateFileInfo.LocalFileName)));
                        }
                        CopyDirectory(updateWorkingDir, updateFileInfo.LocalFileDir);
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

        public void ClearVersionInfoJson()
        {
            VersionInfoJson = null;
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

        private static void CopyDirectory(string sourceDir, string destinationDir, bool recursive = true)
        {
            var dir = new DirectoryInfo(sourceDir);
            if (!dir.Exists) throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

            DirectoryInfo[] dirs = dir.GetDirectories();
            Directory.CreateDirectory(destinationDir);
            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
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
