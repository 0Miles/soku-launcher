using SokuLauncher.Controls;
using SokuLauncher.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
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
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var currentProcess = Process.GetCurrentProcess();
            var currentExecutable = currentProcess.MainModule.FileName;

            var runningProcesses = Process.GetProcessesByName(Path.GetFileNameWithoutExtension(currentExecutable));

            if (runningProcesses.Length > 1)
            {
                Application.Current.Shutdown();
            }


            MainWindow mainWindow = new MainWindow();

            try
            {
                Static.TempDirPath = Path.Combine(Path.GetTempPath(), "SokuLauncher");
                Directory.CreateDirectory(Static.TempDirPath);
                Directory.CreateDirectory(Path.Combine(Static.TempDirPath, "Resources"));

                ResourcesManager resourcesManager = new ResourcesManager();
                resourcesManager.CopyVideoResources();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            try
            {
                Static.ConfigUtil = new ConfigUtil();
                Static.ConfigUtil.ReadConfig();

                Static.ModsManager = new ModsManager();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            Task.Run(async () =>
            {
                try
                {
                    Static.UpdatesManager = new UpdatesManager(Static.ConfigUtil, Static.ModsManager);
                    if (Static.ConfigUtil.Config.AutoCheckForUpdates)
                    {
                        await Static.UpdatesManager.GetVersionInfoJson();

                        Dispatcher.Invoke(() =>
                        {
                            Static.UpdatesManager.CheckForUpdates();
                        });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            });

            mainWindow.Show();
        }
    }
}
