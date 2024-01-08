using Newtonsoft.Json;
using SokuLauncher.UpdateCenter.Controls;
using SokuLauncher.UpdateCenter.Models;
using SokuModManager;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using SokuModManager.Models;

namespace SokuLauncher.Utils
{
    public class UpdateMaster
    {
        private const string SELF_UPDATE_VERSION_INFO_URL = "https://soku.latte.today/version.json";
        private const string MOD_VERSION_FILENAME_SUFFIX = ".version.txt";

        private readonly string UpdateTempDirPath = Path.Combine(Path.GetTempPath(), "SokuModUpdate");

        public event Action<int> DownloadProgressChanged;
        public event Action<string> StatusChanged;

        public string UpdateSokuLauncherFilePath { get; private set; }

        public ConfigUtil CurrentConfigUtil { get; set; }
        public ModManager CurrentModManager { get; set; }
        public Updater CurrentUpdater { get; set; }
        public SourceManager CurrentSourceManager { get; set; }

        public UpdateMaster(ConfigUtil configUtil, ModManager modManager)
        {
            CurrentConfigUtil = configUtil;
            CurrentModManager = modManager;

            SourceConfigManager sourceConfigManager = new SourceConfigManager();
            sourceConfigManager.Refresh();
            CurrentSourceManager = new SourceManager(sourceConfigManager.ConfigList);
            CurrentSourceManager.SourceManagerStatusChanged += (sender, e) =>
            {
                DownloadProgressChanged?.Invoke(e.Progress ?? 0);

                switch (e.Status)
                {
                    case SourceManagerStatus.Fetching:
                        StatusChanged?.Invoke(Static.LanguageService.GetString("UpdatesManager-GetVersionInfoJson-Message"));
                        break;
                }
            };

            CurrentUpdater = new Updater(modManager);
            CurrentUpdater.UpdaterStatusChanged += (sender, e) =>
            {
                DownloadProgressChanged?.Invoke(e.Progress ?? 0);

                switch(e.Status)
                {
                    case UpdaterStatus.Downloading:
                        StatusChanged?.Invoke(string.Format(Static.LanguageService.GetString("UpdatesManager-DownloadAndExtractFile-Downloading"), e.Target) + $" {e.Progress}%");
                        break;
                    case UpdaterStatus.Extracting:
                        StatusChanged?.Invoke(string.Format(Static.LanguageService.GetString("UpdatesManager-DownloadAndExtractFile-Extracting"), e.Target));
                        break;
                    case UpdaterStatus.CopyingFiles:
                        StatusChanged?.Invoke(Static.LanguageService.GetString("UpdatesManager-Updating") + $" {e.Target}...");
                        break;
                    case UpdaterStatus.Pending:
                        StatusChanged?.Invoke(Static.LanguageService.GetString("Common-Pending"));
                        break;
                }
            };
        }

        public async Task<bool?> CheckForUpdates(string desc = null, string complatedMessage = null, bool isAutoUpdates = true, bool checkForUpdates = true, bool checkForInstallable = false, List<string> modsToCheckList = null, bool isShowUpdating = true, bool force = false)
        {
            try
            {
                List<UpdateFileInfoModel> updateList = new List<UpdateFileInfoModel>();
                var selfUpdateFileInfo = await GetAvailableSelfUpdateFileInfo();
                if (selfUpdateFileInfo != null)
                {
                    updateList.Add(selfUpdateFileInfo);
                }

                await CurrentSourceManager.FetchSourceList();
                foreach(var source in CurrentSourceManager.SourceList)
                {
                    List<UpdateFileInfoModel> sourceUpdateInfoList = Updater.GetUpdateFileInfosFromSource(source);
                    CurrentUpdater.RefreshAvailable(sourceUpdateInfoList, modsToCheckList);

                    if (checkForUpdates)
                    {
                        updateList = updateList.Concat(CurrentUpdater.AvailableUpdateList.Where(x => !updateList.Select(updateInfo => updateInfo.Name).Contains(x.Name))).ToList();
                    }
                    if (checkForInstallable)
                    {
                        updateList = updateList.Concat(CurrentUpdater.AvailableInstallList.Where(x => !updateList.Select(updateInfo => updateInfo.Name).Contains(x.Name))).ToList();
                    }
                }
             
                return await SelectAndUpdate(updateList, desc, complatedMessage, isAutoUpdates, isShowUpdating);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Static.LanguageService.GetString("UpdatesManager-MessageBox-Title"), MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }
        }

        public async Task<bool?> SelectAndUpdate(List<UpdateFileInfoModel> updateFileInfoList, string desc = null, string complatedMessage = null, bool isAutoUpdates = true, bool isShowUpdating = true)
        {
            if (updateFileInfoList.Count > 0)
            {
                UpdateSelectionWindow updateSelectionWindow = new UpdateSelectionWindow
                {
                    Desc = desc,
                    UpdateSelectorNodeList = new ObservableCollection<UpdateSelectorNodeModel>(updateFileInfoList.Select(
                        x => new UpdateSelectorNodeModel()
                        {
                            Name = x.Name,
                            Desc = x.Description,
                            Notes = x.Notes,
                            Version = x.Version,
                            LocalFileVersion = x.LocalFileVersion,
                            Installed = x.Installed,
                            Icon = x.Icon,
                            Selected = true
                        }
                    )),
                    IsAutoCheckForUpdatesCheckBoxShow = isAutoUpdates,
                    AutoCheckForUpdates = CurrentConfigUtil.Config.AutoCheckForUpdates
                };

                bool isRunUpdate = updateSelectionWindow.ShowDialog() == true;

                if (isAutoUpdates)
                {
                    if (CurrentConfigUtil.Config.AutoCheckForUpdates != updateSelectionWindow.AutoCheckForUpdates)
                    {
                        CurrentConfigUtil.Config.AutoCheckForUpdates = updateSelectionWindow.AutoCheckForUpdates;
                        CurrentConfigUtil.SaveConfig();
                    }
                }

                if (isRunUpdate)
                {

                    var selectedUpdates = updateSelectionWindow.UpdateSelectorNodeList.Where(x => x.Selected).Select(x => updateFileInfoList.First(updateFile => updateFile.Name == x.Name)).ToList();
                    await ExecuteUpdates(selectedUpdates, isShowUpdating, complatedMessage);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<string> GetSelfUpdateVersionInfoJson()
        {
            using (WebClient client = new WebClient())
            {
                client.Encoding = Encoding.UTF8;
                client.DownloadProgressChanged += (sender, e) =>
                {
                    DownloadProgressChanged?.Invoke(e.ProgressPercentage);
                    StatusChanged?.Invoke(Static.LanguageService.GetString("UpdatesManager-GetVersionInfoJson-Message") + $" {e.ProgressPercentage}%");
                };
                return await client.DownloadStringTaskAsync(string.IsNullOrWhiteSpace(CurrentConfigUtil.Config.VersionInfoUrl) ? SELF_UPDATE_VERSION_INFO_URL : CurrentConfigUtil.Config.VersionInfoUrl);
            }
        }

        public async Task ExecuteUpdates(List<UpdateFileInfoModel> selectedUpdates, bool isShowUpdating, string complatedMessage)
        {
            UpdatingWindow updatingWindow = new UpdatingWindow
            {
                UpdatesManager = this
            };

            var updateTask = Task.Run(async () =>
            {
                try
                {
                    foreach (var updateFileInfo in selectedUpdates)
                    {
                        await CurrentUpdater.DownloadAndExtractFile(updateFileInfo);

                        if (updateFileInfo.Name == "SokuLauncher")
                        {
                            File.WriteAllText(Path.Combine(UpdateTempDirPath, updateFileInfo.Name, "replace.txt"), Static.SelfFileName);
                            File.WriteAllText(Path.Combine(UpdateTempDirPath, updateFileInfo.Name, "args.txt"), string.Join(" ", Static.StartupArgs));
                            File.Move(Path.Combine(UpdateTempDirPath, updateFileInfo.Name, updateFileInfo.FileName), Path.Combine(UpdateTempDirPath, updateFileInfo.Name, "Updater.exe"));
                            UpdateSokuLauncherFilePath = Path.Combine(UpdateTempDirPath, updateFileInfo.Name, "Updater.exe");
                        }
                        else
                        {
                            CurrentUpdater.CopyAndReplaceFile(updateFileInfo);
                        }
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
                    if (complatedMessage != null)
                    {
                        MessageBox.Show(
                            complatedMessage,
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


        public async Task UpdateFromFile(string modPackagePath)
        {
            try
            {
                string ext = Path.GetExtension(modPackagePath).ToLower();
                List<UpdateFileInfoModel> updateFileInfoList;
                switch (ext)
                {
                    case ".zip":
                    case ".sokumod":
                        updateFileInfoList = Updater.GetUpdateFileInfosFromZip(modPackagePath);
                        break;
                    default:
                        throw new Exception(Static.LanguageService.GetString("Common-UnsupportedFormat"));
                }


                CurrentUpdater.RefreshAvailable(updateFileInfoList);

                await SelectAndUpdate(
                    CurrentUpdater.AvailableUpdateList.Concat(CurrentUpdater.AvailableInstallList).ToList(),
                    Static.LanguageService.GetString("UpdatesManager-InstallFromArchive-Desc"),
                    Static.LanguageService.GetString("UpdatesManager-InstallFromArchive-Completed"), 
                    false, 
                    true);

                CurrentModManager.Refresh();
                CurrentModManager.LoadSWRSToysSetting();

                if (updateFileInfoList.Any(x => x.Installed == false))
                {
                    if (MessageBox.Show(
                            Static.LanguageService.GetString("UpdatesManager-NewModInstalled"),
                            Static.LanguageService.GetString("UpdatesManager-MessageBox-Title"),
                            MessageBoxButton.YesNo,
                            MessageBoxImage.Question) == MessageBoxResult.Yes)
                    {
                        foreach (var mod in updateFileInfoList.Where(x => x.Installed == false).ToList())
                        {
                            CurrentModManager.ChangeModEnabled(mod.Name, true);
                        }
                        CurrentModManager.SaveSWRSToysIni();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, Static.LanguageService.GetString("UpdatesManager-MessageBox-Title"), MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task<UpdateFileInfoModel> GetAvailableSelfUpdateFileInfo()
        {
            string versionInfoJson = await GetSelfUpdateVersionInfoJson();
            List<UpdateFileInfoModel> selfUpdateVersionInfoList;
            try
            {
                selfUpdateVersionInfoList = JsonConvert.DeserializeObject<List<UpdateFileInfoModel>>(versionInfoJson);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    Static.LanguageService.GetString("UpdatesManager-GetAvailableUpdateList-ParsingVersionInfoFailed") + $": {ex}",
                    Static.LanguageService.GetString("UpdatesManager-MessageBox-Title"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                return null;
            }

            try
            {
                UpdateFileInfoModel updateFileInfo = selfUpdateVersionInfoList.FirstOrDefault(x => x.Name == "SokuLauncher");

                Version latestVersion = new Version(updateFileInfo.Version);

                updateFileInfo.LocalFileName = Static.SelfFileName;
                updateFileInfo.Icon = "%resources%/icon.ico";
                updateFileInfo.Installed = true;

                Version currentVersion = GetCurrentVersion(updateFileInfo.LocalFileName);
                updateFileInfo.LocalFileVersion = currentVersion.ToString();

                if (updateFileInfo.DescriptionI18n != null)
                {
                    string localDesc =
                        updateFileInfo.DescriptionI18n.FirstOrDefault(x => x.Language == CurrentConfigUtil.Config.Language)?.Content
                        ?? updateFileInfo.DescriptionI18n.FirstOrDefault(x => x.Language != null && x.Language.Split('-')[0] == CurrentConfigUtil.Config.Language.Split('-')[0])?.Content;
                    if (!string.IsNullOrWhiteSpace(localDesc))
                    {
                        updateFileInfo.Description = localDesc;
                    }
                }

                if (updateFileInfo.NotesI18n != null)
                {
                    string localDesc =
                        updateFileInfo.NotesI18n.FirstOrDefault(x => x.Language == CurrentConfigUtil.Config.Language)?.Content
                        ?? updateFileInfo.NotesI18n.FirstOrDefault(x => x.Language != null && x.Language.Split('-')[0] == CurrentConfigUtil.Config.Language.Split('-')[0])?.Content;
                    if (!string.IsNullOrWhiteSpace(localDesc))
                    {
                        updateFileInfo.Notes = localDesc;
                    }
                }
                return updateFileInfo;
            }
            catch (Exception ex)
            {
                throw new Exception(Static.LanguageService.GetString("UpdatesManager-GetAvailableUpdateList-UpdatesFailed") + ": " + ex.Message);
            }
        }

        public static Version GetCurrentVersion(string fileName)
        {
            var fileVersionInfo = File.Exists(fileName) ? FileVersionInfo.GetVersionInfo(fileName) : null;

            string modCurrentVersion = "0.0.0.0";

            if (fileVersionInfo?.FileVersion != null)
            {
                modCurrentVersion = fileVersionInfo.FileVersion;
            }
            else
            {
                string modName = Path.GetFileNameWithoutExtension(fileName);
                string modDir = Path.GetDirectoryName(fileName);
                string modVersionFileName = Path.Combine(modDir, $"{modName}{MOD_VERSION_FILENAME_SUFFIX}");
                
                if (File.Exists(modVersionFileName))
                {
                    modCurrentVersion = File.ReadAllText(modVersionFileName);
                }
            }

            var (major, minor, build, revision) = (0, 0, 0, 0);
            var m = Regex.Matches(modCurrentVersion, @"\d+");

            if (m.Count > 0)
            {
                int.TryParse(m[0]?.Value ?? "0", out major);
            }
            if (m.Count > 1)
            {
                int.TryParse(m[1]?.Value ?? "0", out minor);
            }
            if (m.Count > 2)
            {
                int.TryParse(m[2]?.Value ?? "0", out build);
            }
            if (m.Count > 3)
            {
                int.TryParse(m[3]?.Value ?? "0", out revision);
            }

            return new Version(major, minor, build, revision);
        }

        public static async Task CheckSelfIsUpdating()
        {
            string updateSokuLauncherDir = Path.Combine(Path.GetTempPath(), "SokuModUpdate", "SokuLauncher");

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
                    Process.Start(new ProcessStartInfo { FileName = replaceTargetPath, UseShellExecute = true, Arguments = args });
                    Environment.Exit(0);
                });
            }
        }
    }
}
