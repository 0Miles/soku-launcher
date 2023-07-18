using SokuLauncher.Controls;
using SokuLauncher.Utils;
using System;
using System.Diagnostics;
using System.IO;
using System.Net;
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

            await UpdatesManager.CheckSelfIsUpdating();

            var currentProcess = Process.GetCurrentProcess();
            var currentExecutable = currentProcess.MainModule.FileName;

            var runningProcesses = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(currentExecutable));

            if (runningProcesses.Length > 1)
            {
                Current.Shutdown();
            }

            try
            {
                Static.ConfigUtil = new ConfigUtil();
                Static.ConfigUtil.ReadConfig();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            try
            {
                Static.ModsManager = new ModsManager();
                Static.ModsManager.SearchModulesDir();
                Static.ModsManager.LoadSWRSToysSetting();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            Static.UpdatesManager = new UpdatesManager(Static.ConfigUtil, Static.ModsManager);

            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();

            _ = Task.Run(async () =>
            {
                try
                {
                    if (Static.ConfigUtil.Config.AutoCheckForUpdates)
                    {
                        await Static.UpdatesManager.GetVersionInfoJson();
                        Dispatcher.Invoke(() => Static.UpdatesManager.CheckForUpdates());
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });
        }
    }
}
