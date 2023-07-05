using SokuLauncher.Utils;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
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

            try
            {
                ConfigUtil.ReadConfig();
                StaticVariable.ModsManager = new ModsManager();
                StaticVariable.ModsManager.SearchModulesDir();
                StaticVariable.ModsManager.LoadSWRSToysSetting();

                StaticVariable.UpdateUtil = new UpdateUtil();
                StaticVariable.UpdateUtil.CheckUpdate();
                if (StaticVariable.UpdateUtil.AvailableUpdateList.Count > 0)
                {
                    if (MessageBox.Show("Update detected. Would you like to download the new version?", "Update", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        foreach (var updateFileInfo in StaticVariable.UpdateUtil.AvailableUpdateList)
                        {
                            StaticVariable.UpdateUtil.DownloadAndExtractFile(updateFileInfo);
                            StaticVariable.UpdateUtil.ReplaceFile(updateFileInfo);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            MainWindow mainWindow = new MainWindow();
            mainWindow.Show();
        }
    }
}
