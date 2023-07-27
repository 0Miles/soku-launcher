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
using System.Threading;
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
        public string UpdateSokuLauncherFilePath { get; private set; }

        private readonly string UpdateTempDirPath = Path.Combine(Static.TempDirPath, "Update");

        public ConfigUtil ConfigUtil { get; set; }
        public ModsManager ModsManager { get; set; }

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

        public async Task CheckForUpdates(bool isAutoUpdates = true, bool checkForInstallable = false, List<string> modsToCheckList = null, bool isShowUpdating = true, bool isShowComplatedMessage = true)
        {
            try
            {
                GetAvailableUpdateList(true, checkForInstallable, modsToCheckList);
                if (AvailableUpdateList.Count > 0)
                {
                    if (IsStopCheckForUpdates)
                    {
                        IsStopCheckForUpdates = false;
                        return;
                    }

                    UpdateSelectionWindow updateSelectionWindow = new UpdateSelectionWindow
                    {
                        Desc = Static.LanguageService.GetString("UpdatesManager-CheckForUpdates-UpdateSelectionWindow-Desc"),
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
                        await ExecuteUpdates(selectedUpdates, isShowUpdating, isShowComplatedMessage);
                    }
                }
                else
                {
                    if (!isAutoUpdates)
                    {
                        MessageBox.Show(
                            Static.LanguageService.GetString("UpdatesManager-CheckForUpdates-AllLatest"),
                            Static.LanguageService.GetString("UpdatesManager-MessageBox-Title"),
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Static.LanguageService.GetString("UpdatesManager-MessageBox-Title"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task CheckForInstallable(List<string> modsToCheckList, bool isShowUpdating = false, bool isShowComplatedMessage = false)
        {
            try
            {
                GetAvailableUpdateList(false, true, modsToCheckList);
                if (AvailableUpdateList.Count > 0)
                {
                    UpdateSelectionWindow updateSelectionWindow = new UpdateSelectionWindow
                    {
                        Desc = Static.LanguageService.GetString("UpdatesManager-CheckForInstallable-UpdateSelectionWindow-Desc"),
                        AvailableUpdateList = AvailableUpdateList,
                        IsAutoCheckForUpdatesCheckBoxShow = false
                    };

                    if (updateSelectionWindow.ShowDialog() == true)
                    {
                        var selectedUpdates = updateSelectionWindow.AvailableUpdateList.Where(x => x.Selected).ToList();
                        await ExecuteUpdates(selectedUpdates, isShowUpdating, isShowComplatedMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Static.LanguageService.GetString("UpdatesManager-MessageBox-Title"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task ExecuteUpdates(List<UpdateFileInfoModel> selectedUpdates, bool isShowUpdating, bool isShowComplatedMessage)
        {
            UpdatingWindow updatingWindow = new UpdatingWindow
            {
                UpdatesManager = this,
                AvailableUpdateList = selectedUpdates
            };

            var updateTask = Task.Run(async () =>
            {
                try
                {
                    foreach (var updateFileInfo in selectedUpdates)
                    {
                        await DownloadAndExtractFile(updateFileInfo);
                        CopyAndReplaceFile(updateFileInfo);
                    }

                    if (isShowUpdating)
                    {
                        updatingWindow.Dispatcher.Invoke(() => updatingWindow.Close());
                    }

                    if (UpdateSokuLauncherFilePath != null)
                    {
                        var processInfo = new ProcessStartInfo
                        {
                            UseShellExecute = true,
                            FileName = UpdateSokuLauncherFilePath,
                            Verb = "runas"
                        };

                        try
                        {
                            Directory.SetCurrentDirectory(Path.GetDirectoryName(UpdateSokuLauncherFilePath));
                            Process.Start(processInfo);
                            Environment.Exit(0);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                            Directory.SetCurrentDirectory(Static.SelfFileDir);
                        }
                    }
                    if (isShowComplatedMessage)
                    {
                        MessageBox.Show(
                            Static.LanguageService.GetString("UpdatesManager-CheckForUpdates-Completed"),
                            Static.LanguageService.GetString("UpdatesManager-MessageBox-Title"),
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, Static.LanguageService.GetString("UpdatesManager-MessageBox-Title"), MessageBoxButton.OK, MessageBoxImage.Error);
                    if (isShowUpdating)
                    {
                        updatingWindow.Dispatcher.Invoke(() => updatingWindow.Close());
                    }
                }
            });

            if (isShowUpdating)
            {
                updatingWindow.ShowDialog();
            }

            await updateTask;
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
                                StatusChanged?.Invoke(Static.LanguageService.GetString("UpdatesManager-GetVersionInfoJson-Message") + $" {e.ProgressPercentage}%");
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
                MessageBox.Show(
                    Static.LanguageService.GetString("UpdatesManager-GetAvailableUpdateList-ParsingVersionInfoFailed") + $": {ex}",
                    Static.LanguageService.GetString("UpdatesManager-MessageBox-Title"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
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

                    switch (updateFileInfo.Name)
                    {
                        case "SokuLauncher":
                            updateFileInfo.LocalFileName = Static.SelfFileName;
                            updateFileInfo.Icon = "%resources%/icon.ico";
                            break;
                        case "SokuModLoader":
                            updateFileInfo.LocalFileName = Path.Combine(ConfigUtil.SokuDirFullPath, "d3d9.dll");
                            if (!ModsManager.SWRSToysD3d9Exist)
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
                                updateFileInfo.Icon = modInfo.Icon;
                            }
                            break;
                    }

                    Version currentVersion = GetCurrentVersion(updateFileInfo.LocalFileName);
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
                throw new Exception(Static.LanguageService.GetString("UpdatesManager-GetAvailableUpdateList-UpdatesFailed") + ": " + ex.Message);
            }
        }

        public static Version GetCurrentVersion(string fileName)
        {
            var fileVersionInfo = File.Exists(fileName) ? FileVersionInfo.GetVersionInfo(fileName) : null;

            Version currentVersion;
            if (fileVersionInfo?.FileVersion != null)
            {
                currentVersion = new Version(fileVersionInfo.FileVersion);
            }
            else
            {
                string modName = Path.GetFileNameWithoutExtension(fileName);
                string modDir = Path.GetDirectoryName(fileName);
                string modVersionFileName = Path.Combine(modDir, $"{modName}{MOD_VERSION_FILENAME_SUFFIX}");
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

            return currentVersion;
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
                        StatusChanged?.Invoke(string.Format(Static.LanguageService.GetString("UpdatesManager-DownloadAndExtractFile-Downloading"), updateFileInfo.Name) + $" {e.ProgressPercentage}%");
                    };
                    await client.DownloadFileTaskAsync(updateFileInfo.DownloadUrl, downloadToTempFilePath);
                }

                if (updateFileInfo.Compressed)
                {
                    StatusChanged?.Invoke(string.Format(Static.LanguageService.GetString("UpdatesManager-DownloadAndExtractFile-Extracting"), updateFileInfo.Name));
                    ZipFile.ExtractToDirectory(downloadToTempFilePath, updateFileDir);
                    File.Delete(downloadToTempFilePath);
                }

            }
            catch (Exception ex)
            {
                throw new Exception(string.Format(Static.LanguageService.GetString("UpdatesManager-DownloadAndExtractFile-Failed"), updateFileInfo.FileName) + ": " + ex.Message);
            }
        }

        public void CopyAndReplaceFile(UpdateFileInfoModel updateFileInfo)
        {
            StatusChanged?.Invoke(Static.LanguageService.GetString("UpdatesManager-Updating") + $" {updateFileInfo.Name}...");
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
                        File.WriteAllText(Path.Combine(updateFileDir, "replace.txt"), Static.SelfFileName);
                        File.WriteAllText(Path.Combine(updateFileDir, "args.txt"), string.Join(" ", Static.StartupArgs));
                        UpdateSokuLauncherFilePath = Path.Combine(updateFileDir, "Updater.exe");
                        File.Move(newVersionFileName, UpdateSokuLauncherFilePath);
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
                    throw new Exception(Static.LanguageService.GetString("UpdatesManager-CopyAndReplaceFile-CanNotCopyFile") + ": " + updateFileInfo.FileName);
                }
            }

            string modVersionFileName = Path.Combine(updateFileInfo.LocalFileDir, $"{Path.GetFileNameWithoutExtension(updateFileInfo.LocalFileName)}{MOD_VERSION_FILENAME_SUFFIX}");
            if (updateFileInfo.Version != "0.0.0.0")
            {
                File.WriteAllText(modVersionFileName, updateFileInfo.Version);
            }
            Directory.Delete(updateFileDir, true);
        }

        public void ClearVersionInfoJson()
        {
            VersionInfoJson = null;
        }

        private static void CopyDirectory(string sourceDir, string destinationDir, bool recursive = true)
        {
            var dir = new DirectoryInfo(sourceDir);
            if (!dir.Exists) throw new DirectoryNotFoundException(Static.LanguageService.GetString("UpdatesManager-CopyDirectory-SourceDirNotFound") + $": {dir.FullName}");

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

        public static async Task CheckSelfIsUpdating()
        {
            
            string updateSokuLauncherDir = Path.Combine(Static.TempDirPath, "Update", "SokuLauncher");

            if (Static.SelfFileDir == updateSokuLauncherDir)
            {
                string replaceTargetPath = File.ReadAllText("replace.txt");

                UpdatingWindow updatingWindow = new UpdatingWindow
                {
                    UpdatesManager = null,
                    IsIndeterminate = true,
                    Status = Static.LanguageService.GetString("UpdatesManager-CheckSelfIsUpdating-WaitProcessClose")
                };

                updatingWindow.Show();

                await Task.Delay(100);

                string sokuLauncherProcessName = Path.GetFileNameWithoutExtension(replaceTargetPath);

                bool hasSameProcess;
                do
                {
                    await Task.Delay(1000);
                    hasSameProcess = false;
                    var runningProcesses = Process.GetProcesses();
                    foreach (Process process in runningProcesses)
                    {
                        if (process.ProcessName == sokuLauncherProcessName)
                        {
                            process.Kill();
                            hasSameProcess = true;
                        }
                    }
                } while (hasSameProcess);

                updatingWindow.Status = Static.LanguageService.GetString("UpdatesManager-Updating") + "...";

                await Task.Run(() => {
                    string args = "";
                    try
                    {
                        args = File.ReadAllText("args.txt");
                    }
                    catch
                    { }
                    File.Copy(Static.SelfFileName, replaceTargetPath, true);

                    Directory.SetCurrentDirectory(Path.GetDirectoryName(replaceTargetPath));
                    Process.Start(new ProcessStartInfo { FileName = replaceTargetPath, UseShellExecute = true, Arguments = args});
                    Environment.Exit(0);
                });
            }
        }
    }
}
