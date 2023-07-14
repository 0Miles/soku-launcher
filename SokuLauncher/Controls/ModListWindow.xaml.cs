using SokuLauncher.Models;
using SokuLauncher.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SokuLauncher.Controls
{
    public partial class ModListWindow : Window, INotifyPropertyChanged
    {
        public ModListWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private ObservableCollection<ModInfoModel> _ModInfoList;
        public ObservableCollection<ModInfoModel> ModInfoList
        {
            get
            {
                return _ModInfoList;
            }
            set
            {
                if (_ModInfoList != value)
                {
                    _ModInfoList = value;
                    RaisePropertyChanged("ModInfoList");
                }
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
