using SokuLauncher.Models;
using SokuLauncher.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows;

namespace SokuLauncher.Controls
{
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
            UpdatesManager.DownloadProgressChanged += (progress) => Progress = progress;
            UpdatesManager.StatusChanged += (status) => Status = status;
        }
    }
}
