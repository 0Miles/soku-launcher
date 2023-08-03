using SokuLauncher.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
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

            await UpdatesManager.CheckSelfIsUpdating();

            MainWindow mainWindow = new MainWindow();

            if (Static.StartupArgs.Length > 0)
            {
                foreach (string arg in Static.StartupArgs)
                {
                    if (File.Exists(arg))
                    {
                        Static.StartupArgs = Array.Empty<string>();
                        string filename = arg;
                        string ext = Path.GetExtension(filename).ToLower();

                        try
                        {
                            switch (ext)
                            {
                                case ".zip":
                                    mainWindow.ViewModel.UpdatesManager.GetVersionInfoJsonFromZip(filename);
                                    await mainWindow.ViewModel.UpdatesManager.CheckForUpdates(
                                        Static.LanguageService.GetString("UpdatesManager-InstallFromArchive-Desc"),
                                        Static.LanguageService.GetString("UpdatesManager-InstallFromArchive-Completed"),
                                        false,
                                        true,
                                        null,
                                        true,
                                        true);
                                    break;
                                default:
                                    throw new Exception(Static.LanguageService.GetString("Common-UnsupportedFormat"));
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, Static.LanguageService.GetString("Common-ErrorMessageBox-Title"), MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        Current.Shutdown();
                    }
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
                            await mainWindow.ViewModel.UpdatesManager.GetVersionInfoJson();
                            List<string> checkModes = settingGroup.EnableMods?.Select(x => x).ToList() ?? new List<string>();
                            checkModes.Add("SokuLauncher");
                            checkModes.Add("SokuModLoader");
                            await mainWindow.ViewModel.UpdatesManager.CheckForUpdates(
                                Static.LanguageService.GetString("UpdatesManager-CheckForUpdates-UpdateSelectionWindow-Desc"), 
                                null,
                                true,
                                mainWindow.ViewModel.ConfigUtil.Config.AutoCheckForInstallable,
                                checkModes,
                                true);
                            mainWindow.ViewModel.ModsManager.SearchModulesDir();
                            mainWindow.ViewModel.ModsManager.LoadSWRSToysSetting();
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, Static.LanguageService.GetString("UpdatesManager-MessageBox-Title"), MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    mainWindow.ViewModel.ModsManager.ApplyModSettingGroup(settingGroup);
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

            mainWindow.Show();

            if (mainWindow.ViewModel.ConfigUtil.Config.AutoCheckForUpdates)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await mainWindow.ViewModel.UpdatesManager.GetVersionInfoJson();
                        await Dispatcher.Invoke(() => mainWindow.ViewModel.UpdatesManager.CheckForUpdates(
                            Static.LanguageService.GetString("UpdatesManager-CheckForUpdates-UpdateSelectionWindow-Desc"),
                            Static.LanguageService.GetString("UpdatesManager-CheckForUpdates-Completed")
                            ));
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, Static.LanguageService.GetString("Common-ErrorMessageBox-Title"), MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                });
            }
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
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Exception ex = e.ExceptionObject as Exception;
            MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
