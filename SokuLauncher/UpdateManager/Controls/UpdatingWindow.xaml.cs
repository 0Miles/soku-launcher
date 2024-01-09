using SokuLauncher.Models;
using SokuLauncher.Utils;
using SokuModManager.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

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

        public UpdateManager UpdateManager { get; set; }

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

        private string _Status = Static.LanguageService.GetString("Common-Pending") + "...";
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
        private bool _IsIndeterminate = false;
        public bool IsIndeterminate
        {
            get
            {
                return _IsIndeterminate;
            }
            set
            {
                if (value != _IsIndeterminate)
                {
                    _IsIndeterminate = value;
                    RaisePropertyChanged("IsIndeterminate");
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (UpdateManager != null)
            {
                UpdateManager.DownloadProgressChanged += (progress) => Progress = progress;
                UpdateManager.StatusChanged += (status) => Status = status;
            }
        }
    }
}
