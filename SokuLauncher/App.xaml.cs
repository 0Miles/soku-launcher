using SokuLauncher.Utils;
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

                Static.ResourcesManager = new ResourceManager();
                Static.ResourcesManager.CopyVideoResources();

                Static.ConfigUtil = new ConfigUtil();
                Static.ConfigUtil.ReadConfig();

                Static.ModsManager = new ModsManager();
                Static.ModsManager.SearchModulesDir();
                Static.ModsManager.LoadSWRSToysSetting();

                Static.UpdateUtil = new UpdateUtil();
                Static.UpdateUtil.CheckUpdate();
                if (Static.UpdateUtil.AvailableUpdateList.Count > 0)
                {
                    if (MessageBox.Show("Update detected. Would you like to download the new version?", "Update", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
                    {
                        foreach (var updateFileInfo in Static.UpdateUtil.AvailableUpdateList)
                        {
                            Static.UpdateUtil.DownloadAndExtractFile(updateFileInfo);
                            Static.UpdateUtil.ReplaceFile(updateFileInfo);
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
