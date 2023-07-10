using SokuLauncher.Models;
using System.Collections.Generic;
using System.Windows;
namespace SokuLauncher.Controls
{
    public partial class UpdateSelectionWindow : Window
    {
        public UpdateSelectionWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        public List<UpdateFileInfoModel> AvailableUpdateList { get; set; }
        public string Desc { get; set; }

        public bool AutoUpdates { get; set; }

        public bool AutoCheckForUpdates { get; set; }

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
