using SokuLauncher.Models;
using SokuLauncher.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace SokuLauncher.Controls
{
    /// <summary>
    /// UpdatingWindow.xaml 的互動邏輯
    /// </summary>
    public partial class UpdatingWindow : Window, INotifyPropertyChanged
    {
        public UpdatingWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public bool Stillness { get; set; } = true;

        public UpdatesManager UpdatesManager { get; set; }

        public List<UpdateFileInfoModel> AvailableUpdateList { get; set; }

        private int _Progress = 0;
        public int Progress
        {
            get
            {
                return _Progress;
            }
            set
            {
                if (value != _Progress)
                {
                    _Progress = value;
                    RaisePropertyChanged("Progress");
                }
            }
        }

        private string _Status = "";
        public string Status
        {
            get
            {
                return _Status;
            }
            set
            {
                if (value != _Status)
                {
                    _Status = value;
                    RaisePropertyChanged("Status");
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            foreach (var updateFileInfo in AvailableUpdateList)
            {
                UpdatesManager.DownloadProgressChanged += (progress) => Progress = progress;
                UpdatesManager.DownloadFileNameChanged += (status) => Status = status;
                UpdatesManager.DownloadAndExtractFile(updateFileInfo);
                UpdatesManager.CopyAndReplaceFile(updateFileInfo);
            }
            Close();
            if (!Stillness)
            {
                MessageBox.Show("All updates completed", "Updates", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
