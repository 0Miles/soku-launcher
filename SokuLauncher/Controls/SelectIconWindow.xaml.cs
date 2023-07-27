using SokuLauncher.Models;
using SokuLauncher.ViewModels;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SokuLauncher.Controls
{
    public partial class SelectIconWindow : Window
    {
        public SelectorWindowViewModel ViewModel { get; set; }
        public SelectIconWindow(SelectorWindowViewModel viewModel = null)
        {
            ViewModel = viewModel;
            if (ViewModel == null)
            {
                ViewModel = new SelectorWindowViewModel();
            }
            DataContext = ViewModel;

            InitializeComponent();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SelectorListView.SelectionMode = ViewModel.IsMutiSelect ? SelectionMode.Multiple : SelectionMode.Single;
            SelectorListView.Focus();
        }
    }
}
