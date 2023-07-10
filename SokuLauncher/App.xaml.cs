﻿using SokuLauncher.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace SokuLauncher
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

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
                MessageBox.Show(ex.Message);
            }

            try
            {
                Static.ConfigUtil = new ConfigUtil();
                Static.ConfigUtil.ReadConfig();

                Static.ModsManager = new ModsManager();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            try
            {
                UpdateManager updateManager = new UpdateManager();
                if (Static.ConfigUtil.Config.AutoCheckUpdate)
                {
                    updateManager.CheckUpdate();
                    if (updateManager.AvailableUpdateList.Count > 0)
                    {
                        if (MessageBox.Show("Update detected. Would you like to download the new version?", "Update", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                        {
                            foreach (var updateFileInfo in updateManager.AvailableUpdateList)
                            {
                                updateManager.DownloadAndExtractFile(updateFileInfo);
                                updateManager.ReplaceFile(updateFileInfo);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            mainWindow.Show();
        }
    }
}
