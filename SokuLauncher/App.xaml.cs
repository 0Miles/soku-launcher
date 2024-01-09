using SokuLauncher.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace SokuLauncher
{
    public partial class App : Application
    {
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            Static.StartupArgs = e.Args;

            LanguageService_OnChangeLanguage(ConfigUtil.GetLanguageCode(CultureInfo.CurrentCulture.Name));

            await UpdateManager.CheckSelfIsUpdating();

            MainWindow mainWindow = new MainWindow();

            if (Static.StartupArgs.Length > 0)
            {
                List<string> paths = Static.StartupArgs.Where(x => File.Exists(x)).ToList();

                if (paths.Count > 0)
                {
                    Static.StartupArgs = Array.Empty<string>();
                    await mainWindow.ViewModel.UpdateManager.UpdateFromFile(paths[0]);
                    Current.Shutdown();
                }
            }

            if (Static.StartupArgs.Length > 1 && Static.StartupArgs[0] == "-s")
            {
                try
                {
                    string modSettingGroupId = Static.StartupArgs[1];

                    string sokuFile = Path.Combine(mainWindow.ViewModel.ConfigUtil.SokuDirFullPath, mainWindow.ViewModel.ConfigUtil.Config.SokuFileName);

                    if (!File.Exists(sokuFile))
                    {
                        throw new Exception(string.Format(Static.LanguageService.GetString("MainWindow-SokuFileNotFound"), mainWindow.ViewModel.ConfigUtil.Config.SokuFileName));
                    }

                    var settingGroup = mainWindow.ViewModel.ConfigUtil.Config.SokuModSettingGroups.FirstOrDefault(x => x.Id.ToLower() == modSettingGroupId.ToLower() || x.Name.ToLower() == modSettingGroupId.ToLower()) ?? throw new Exception(string.Format(Static.LanguageService.GetString("App-ModSettingGroupNotFound"), modSettingGroupId));

                    if (mainWindow.ViewModel.ConfigUtil.Config.AutoCheckForUpdates)
                    {
                        try
                        {
                            List<string> checkModes = settingGroup.EnableMods?.Select(x => x).ToList() ?? new List<string>();
                            checkModes.Add("SokuLauncher");
                            checkModes.Add("SokuModLoader");
                            var updateList = await mainWindow.ViewModel.UpdateManager.CheckForUpdates(
                                true,
                                mainWindow.ViewModel.ConfigUtil.Config.AutoCheckForInstallable,
                                checkModes
                            );
                            if (updateList?.Count > 0)
                            {
                                await mainWindow.ViewModel.UpdateManager.SelectAndUpdate(updateList, Static.LanguageService.GetString("UpdateManager-CheckForUpdates-UpdateSelectionWindow-Desc"));
                            }
                            mainWindow.ViewModel.ModManager.Refresh();
                            mainWindow.ViewModel.ModManager.LoadSWRSToysSetting();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, Static.LanguageService.GetString("UpdateManager-MessageBox-Title"), MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    mainWindow.ViewModel.ModManager.ApplyModSettingGroup(
                        new SokuModManager.Models.Mod.ModSettingGroupModel
                        {
                            Id = settingGroup.Id,
                            Name = settingGroup.Name,
                            EnableMods = settingGroup.EnableMods,
                            DisableMods = settingGroup.DisableMods
                        }
                    );
                    Directory.SetCurrentDirectory(mainWindow.ViewModel.ConfigUtil.SokuDirFullPath);
                    Process.Start(sokuFile);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, Static.LanguageService.GetString("Common-ErrorMessageBox-Title"), MessageBoxButton.OK, MessageBoxImage.Error);
                }
                Current.Shutdown();
            }

            var currentProcess = Process.GetCurrentProcess();
            var currentExecutable = currentProcess.MainModule.FileName;
            var runningProcesses = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(currentExecutable));
            if (runningProcesses.Length > 1)
            {
                Current.Shutdown();
            }

            if (mainWindow.ViewModel.ConfigUtil.Config.AutoCheckForUpdates)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await Dispatcher.Invoke(async () =>
                        {
                            var updateList = await mainWindow.ViewModel.UpdateManager.CheckForUpdates();
                            if (updateList?.Count > 0)
                            {
                                await mainWindow.ViewModel.UpdateManager.SelectAndUpdate(
                                    updateList,
                                    Static.LanguageService.GetString("UpdateManager-CheckForUpdates-UpdateSelectionWindow-Desc"),
                                    Static.LanguageService.GetString("UpdateManager-CheckForUpdates-Completed")
                                );
                            }
                        });
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, Static.LanguageService.GetString("Common-ErrorMessageBox-Title"), MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                });
            }

            mainWindow.Show();
        }

        public App()
        {
            Static.LanguageService = new LanguageService();
            Static.LanguageService.OnChangeLanguage += LanguageService_OnChangeLanguage;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            Current.DispatcherUnhandledException += Application_DispatcherUnhandledException;
        }

        private void LanguageService_OnChangeLanguage(string languageCode)
        {
            switch (languageCode)
            {
                case "zh-Hant":
                    Resources.MergedDictionaries[0].Source = new Uri("pack://application:,,,/Resources/Languages/zh-Hant.xaml");
                    break;
                case "zh-Hans":
                    Resources.MergedDictionaries[0].Source = new Uri("pack://application:,,,/Resources/Languages/zh-Hans.xaml");
                    break;
                default:
                    Resources.MergedDictionaries[0].Source = new Uri("pack://application:,,,/Resources/Languages/en.xaml");
                    break;
            }
        }

        private void Application_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show(e.Exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
            Environment.Exit(0);
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Environment.Exit(0);
        }
    }
}
