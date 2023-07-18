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
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var currentProcess = Process.GetCurrentProcess();
            var currentExecutable = currentProcess.MainModule.FileName;

            var runningProcesses = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(currentExecutable));

            if (runningProcesses.Length > 1)
            {
                Current.Shutdown();
            }

            Static.TempDirPath = Path.Combine(Path.GetTempPath(), "SokuLauncher");
            
            MainWindow mainWindow = new MainWindow();

            //try
            //{
            //    Directory.CreateDirectory(Static.TempDirPath);
            //    Directory.CreateDirectory(Path.Combine(Static.TempDirPath, "Resources"));

            //    ResourcesManager resourcesManager = new ResourcesManager();
            //    resourcesManager.CopyVideoResources();
            //}
            //catch (Exception ex)
            //{
            //    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            //}

            try
            {
                Static.ConfigUtil = new ConfigUtil();
                Static.ConfigUtil.ReadConfig();

                Static.ModsManager = new ModsManager();
                Static.ModsManager.SearchModulesDir();
                Static.ModsManager.LoadSWRSToysSetting();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            Static.UpdatesManager = new UpdatesManager(Static.ConfigUtil, Static.ModsManager);

            mainWindow.Show();

            Task.Run(async () =>
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
