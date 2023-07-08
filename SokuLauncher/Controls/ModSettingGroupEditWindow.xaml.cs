using SokuLauncher.Models;
using SokuLauncher.ViewModels;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SokuLauncher.Controls
{
    public partial class ModSettingGroupEditWindow : Window
    {
        public ModSettingGroupEditWindowViewModel ViewModel { get; set; }
        public ModSettingGroupEditWindow(ModSettingGroupEditWindowViewModel viewModel = null)
        {
            ViewModel = viewModel;
            if (ViewModel == null)
            {
                ViewModel = new ModSettingGroupEditWindowViewModel();
            }
            DataContext = ViewModel;

            InitializeComponent();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.EnableMods = ViewModel.ModSettingInfoList.Where(x => x.Enabled == true).Select(x => x.Name).ToList();
            ViewModel.DisableMods = ViewModel.ModSettingInfoList.Where(x => x.Enabled == false).Select(x => x.Name).ToList();
            DialogResult = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SelectorListView.Focus();
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
        }
    }
}
