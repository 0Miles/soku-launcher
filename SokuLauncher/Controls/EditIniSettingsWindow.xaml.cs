using SokuLauncher.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using System.Windows.Shapes;

namespace SokuLauncher.Controls
{
    /// <summary>
    /// EditIniSettingsWindow.xaml 的互動邏輯
    /// </summary>
    public partial class EditIniSettingsWindow : Window, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public EditIniSettingsWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private ObservableCollection<IniSettingModel> _IniSettings = new ObservableCollection<IniSettingModel>();
        public ObservableCollection<IniSettingModel> IniSettings
        {
            get { return _IniSettings; }
            set
            {
                _IniSettings = value;
                OnPropertyChanged("IniSettings");
            }
        }

        public string DefaultIniFileName { get; set; }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            IniSettings.Add(new IniSettingModel
            {
                FileName = DefaultIniFileName,
                Enabled = true
            });
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (IniSettingDataGrid.SelectedItem != null && IniSettingDataGrid.SelectedItem is IniSettingModel selectedItem)
            {
                IniSettings.Remove(selectedItem);
            }
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
