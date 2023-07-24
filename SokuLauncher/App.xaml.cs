using SokuLauncher.Utils;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
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

            LanguageService_OnChangeLanguage(ConfigUtil.GetLanguageCode(CultureInfo.CurrentCulture.Name));

            await UpdatesManager.CheckSelfIsUpdating();

            var currentProcess = Process.GetCurrentProcess();
            var currentExecutable = currentProcess.MainModule.FileName;

            var runningProcesses = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(currentExecutable));

            if (runningProcesses.Length > 1)
            {
                Current.Shutdown();
            }

            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
            if (mainWindow.ViewModel.ConfigUtil.Config.AutoCheckForUpdates)
            {
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await mainWindow.ViewModel.UpdatesManager.GetVersionInfoJson();
                        Dispatcher.Invoke(() => mainWindow.ViewModel.UpdatesManager.CheckForUpdates());
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
