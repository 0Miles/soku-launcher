﻿using SokuLauncher.Controls;
using SokuLauncher.Utils;
using SokuLauncher.ViewModels;
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
                        MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                });
            }
        }
    }
}
