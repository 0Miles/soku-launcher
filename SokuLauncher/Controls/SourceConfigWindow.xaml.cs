using SokuLauncher.Models;
using SokuLauncher.ViewModels;
using SokuModManager.Models.Source;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public partial class SourceConfigWindow : Window
    {

        public SourceConfigWindowViewModel ViewModel { get; set; }

        public SourceConfigWindow(SourceConfigWindowViewModel viewModel)
        {
            ViewModel = viewModel;
            if (ViewModel == null)
            {
                ViewModel = new SourceConfigWindowViewModel();
            }

            DataContext = ViewModel;

            InitializeComponent();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }

        private void SourceConfigAddButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Sources.Add(new SourceConfigModel
            {
                Name = "New Source",
                Url = "",
                PreferredDownloadLinkType = "github"
            });
        }

        private void DeleteSourceConfigButton_Click(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            SourceConfigModel selectedSourceConfig = (SourceConfigModel)button.DataContext;
            ViewModel.Sources.Remove(selectedSourceConfig);
        }
    }
}
