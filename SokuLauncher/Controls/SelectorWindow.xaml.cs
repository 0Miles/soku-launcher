using SokuLauncher.Models;
using SokuLauncher.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SokuLauncher.Controls
{
    public partial class SelectorWindow : Window
    {
        public SelectorWindowViewModel ViewModel { get; set; }
        public SelectorWindow(SelectorWindowViewModel viewModel)
        {
            InitializeComponent();

            ViewModel = viewModel;
            if (ViewModel == null)
            {
                ViewModel = new SelectorWindowViewModel();
            }
            DataContext = ViewModel;
        }

        private void ListView_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (ViewModel.IsMutiSelect && e.LeftButton == MouseButtonState.Pressed)
            {
                ListViewItem clickedItem = GetListViewItemClicked(e.OriginalSource as DependencyObject);
                if (clickedItem != null)
                {
                    if (clickedItem.IsSelected)
                    {
                        clickedItem.IsSelected = false;
                    }
                    else
                    {
                        clickedItem.IsSelected = true;
                    }
                    (clickedItem.DataContext as SelectorNodeModel).Selected = clickedItem.IsSelected;
                }
            }
        }

        private ListViewItem GetListViewItemClicked(DependencyObject originalSource)
        {
            if (originalSource == null) return null;

            if (originalSource is ListViewItem listViewItem)
            {
                return listViewItem;
            }

            return GetListViewItemClicked(VisualTreeHelper.GetParent(originalSource));
        }

        private void OKButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ((e.AddedItems[0] as ListViewItem).DataContext as SelectorNodeModel).Selected = (e.AddedItems[0] as ListViewItem).IsSelected;
        }
    }
}
