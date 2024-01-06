using SokuLauncher.Models;
using SokuModManager.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;

namespace SokuLauncher.Controls
{
    public partial class UpdateSelectionWindow : Window, INotifyPropertyChanged
    {
        public UpdateSelectionWindow()
        {
            InitializeComponent();
            DataContext = this;
        }
        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public List<UpdateFileInfoModel> AvailableUpdateList { get; set; }
        public string Desc { get; set; }

        public bool IsAutoCheckForUpdatesCheckBoxShow { get; set; } = false;

        public bool AutoCheckForUpdates { get; set; }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void CommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo { FileName = ((Uri)e.Parameter).AbsoluteUri, UseShellExecute = true });
            e.Handled = true;
        }
    }
}
