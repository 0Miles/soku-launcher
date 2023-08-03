using Microsoft.Win32;
using SokuLauncher.Models;
using SokuLauncher.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace SokuLauncher.Controls
{
    public partial class ModListUserControl : UserControl
    {

        public ModListUserControl()
        {
            InitializeComponent();
            ((INotifyCollectionChanged)SelectorListView.Items).CollectionChanged += new NotifyCollectionChangedEventHandler(SelectorListViewCollectionChanged);
        }

        public event Action<object, SelectionChangedEventArgs> SelectionChanged;
        public event Action<object, RoutedEventArgs> ModDeleted;
        public event Action<object, RoutedEventArgs> ModUndeleted;
        public event Action<object, RoutedEventArgs> InstallButtonClick;
        private readonly Debouncer SearchDebouncer = new Debouncer(600);

        public static DependencyProperty ModInfoListProperty =
            DependencyProperty.Register(
                "ModInfoList",
                typeof(ObservableCollection<ModInfoModel>),
                typeof(ModListUserControl),
                new PropertyMetadata(new ObservableCollection<ModInfoModel>())
            );

        public ObservableCollection<ModInfoModel> ModInfoList
        {
            get
            {
                return (ObservableCollection<ModInfoModel>)(GetValue(ModInfoListProperty));
            }
            set
            {
                SetValue(ModInfoListProperty, value);
            }
        }


        public static DependencyProperty FilteredModInfoListProperty =
            DependencyProperty.Register(
                "FilteredModInfoList",
                typeof(ObservableCollection<ModInfoModel>),
                typeof(ModListUserControl),
                new PropertyMetadata(new ObservableCollection<ModInfoModel>())
            );

        public ObservableCollection<ModInfoModel> FilteredModInfoList
        {
            get
            {
                return (ObservableCollection<ModInfoModel>)(GetValue(FilteredModInfoListProperty));
            }
            set
            {
                SetValue(FilteredModInfoListProperty, value);
            }
        }

        public void SelectorListViewCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (ModInfoList != null && ModInfoList.Count > 0)
            {
                ModNotFoundTextBlock.Visibility = Visibility.Collapsed;
                foreach (var selectorNode in ModInfoList.Where(x => x.Enabled).ToList())
                {
                    SelectorListView.SelectedItems.Add(selectorNode);
                }
            }
            else
            {
                ModNotFoundTextBlock.Visibility = Visibility.Visible;
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            SearchMod();
            SelectorListView.Focus();
        }

        public void SearchMod()
        {
            string seachValue = SearchTextBox?.Text;

            if (string.IsNullOrWhiteSpace(seachValue))
            {
                FilteredModInfoList = new ObservableCollection<ModInfoModel>(ModInfoList);
            }
            else
            {
                FilteredModInfoList = new ObservableCollection<ModInfoModel>(ModInfoList.Where(x => (x.Name?.ToLower().Contains(seachValue.ToLower()) ?? false) || (x.RelativePath?.ToLower().Contains(seachValue.ToLower()) ?? false)));
            }

            if (FilteredModInfoList.Count == 0)
            {
                ModNotFoundTextBlock.Visibility = Visibility.Visible;
            }
            else
            {
                ModNotFoundTextBlock.Visibility = Visibility.Collapsed;
            }
        }

        private void SelectorListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SelectionChanged(sender, e);
        }

        private void OpenModFolder_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;
            var fullPath = (string)menuItem.Tag;
            Process.Start("explorer.exe", Path.GetDirectoryName(fullPath));
        }

        private void OpenModConfigFile_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;
            var fullPath = (string)menuItem.Tag;
            Process.Start(fullPath);
        }
        private void DeleteMod_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;
            var dirPath = (string)menuItem.Tag;

            foreach(var modInfo in ModInfoList.Where(x => x.DirName == dirPath))
            {
                modInfo.ToBeDeleted = true;
            }
            ModDeleted(dirPath, e);
        }
        
        private void UndeleteMod_Click(object sender, RoutedEventArgs e)
        {
            var menuItem = (MenuItem)sender;
            var dirPath = (string)menuItem.Tag;

            foreach(var modInfo in ModInfoList.Where(x => x.DirName == dirPath))
            {
                modInfo.ToBeDeleted = false;
            }
            ModUndeleted(dirPath, e);
        }

        private void SelectorListView_PreviewMouseRightButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void DeletedItemBorder_PreviewMouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private void SearchTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            SearchDebouncer.Debouce(() =>
            {
                Dispatcher.Invoke(SearchMod);
            });
        }

        private void SearchTextBox_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            switch (e.Key)
            {
                case System.Windows.Input.Key.Escape:
                    SearchTextBox.Text = "";
                    break;
            }
        }

        private void InsatllButton_Click(object sender, RoutedEventArgs e)
        {
            InstallButtonClick(sender, e);
        }

    }
}
