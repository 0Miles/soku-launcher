using SokuLauncher.Utils;
using SokuLauncher.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SokuLauncher.Controls
{
    public partial class ConfigWindow : Window
    {
        ConfigWindowViewModel ViewModel;
        public ConfigWindow(ConfigWindowViewModel viewModel = null)
        {
            ViewModel = viewModel;
            if (ViewModel == null)
            {
                ViewModel = new ConfigWindowViewModel();
            }
            DataContext = ViewModel;
            InitializeComponent();
            GetSokuFileIcon();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            MainWindow mainWindow = new MainWindow
            {
                WindowState = WindowState.Normal,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };
            mainWindow.Show();
        }

        private void SokuDirButton_Click(object sender, RoutedEventArgs e)
        {
            string prev = ViewModel.SokuDirPath;
            FolderBrowserDialog folderDialog = new FolderBrowserDialog();
            folderDialog.SelectedPath = Path.GetFullPath(Path.Combine(Static.SelfFileDir, ViewModel.SokuDirPath));

            DialogResult result = folderDialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                string selectedFolder = folderDialog.SelectedPath;
                string relativePath = Static.GetRelativePath(selectedFolder, Static.SelfFileDir);
                if (!relativePath.StartsWith("../../"))
                {
                    selectedFolder = relativePath;
                }

                if (prev != selectedFolder)
                {
                    ViewModel.SokuDirPath = selectedFolder;
                    ViewModel.SokuFileName = ConfigUtil.SelectSokuFile(ViewModel.SokuDirPath);
                    GetSokuFileIcon();
                }
            }
        }

        private void SokuFileNameButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*";
            openFileDialog.InitialDirectory = Path.GetFullPath(Path.Combine(Static.SelfFileDir, ViewModel.SokuDirPath));
            openFileDialog.FileName = ViewModel.SokuFileName;

            DialogResult result = openFileDialog.ShowDialog();

            if (result == System.Windows.Forms.DialogResult.OK)
            {
                ViewModel.SokuFileName = Path.GetFileName(openFileDialog.FileName);
                GetSokuFileIcon();
            }
        }

        private void GetSokuFileIcon()
        {
            ViewModel.SokuFileIcon = Static.GetExtractAssociatedIcon(Path.GetFullPath(Path.Combine(ViewModel.SokuDirPath, ViewModel.SokuFileName ?? "")));
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
