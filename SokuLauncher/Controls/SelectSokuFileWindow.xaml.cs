using SokuLauncher.Models;
using SokuLauncher.ViewModels;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SokuLauncher.Controls
{
    public partial class SelectSokuFileWindow : Window
    {
        public SelectSokuFileWindowViewModel ViewModel { get; set; }
        public SelectSokuFileWindow(SelectSokuFileWindowViewModel viewModel = null)
        {
            ViewModel = viewModel;
            if (ViewModel == null)
            {
                ViewModel = new SelectSokuFileWindowViewModel();
            }
            DataContext = ViewModel;

            InitializeComponent();
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var selectorNode in ViewModel.SelectorNodeList)
            {
                selectorNode.Selected = false;
            }
            foreach (var selectedItem in SelectorListView.SelectedItems)
            {
                (selectedItem as SelectorNodeModel).Selected = true;
            }
            DialogResult = true;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            SelectorListView.SelectionMode = ViewModel.IsMutiSelect ? SelectionMode.Multiple : SelectionMode.Single;
            
            if (ViewModel.IsMutiSelect)
            {
                foreach (var selectorNode in ViewModel.SelectorNodeList.Where(x => x.Selected))
                {
                    SelectorListView.SelectedItems.Add(selectorNode);
                }
            }
            else
            {
                SelectorListView.SelectedItem = ViewModel.SelectorNodeList.FirstOrDefault(x => x.Selected);
            }
            SelectorListView.Focus();
        }
    }
}
