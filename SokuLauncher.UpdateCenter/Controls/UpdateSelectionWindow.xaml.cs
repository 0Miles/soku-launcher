using SokuLauncher.UpdateCenter.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SokuLauncher.UpdateCenter.Controls
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

        public ObservableCollection<UpdateSelectorNodeViewModel> UpdateSelectorNodeList { get; set; }
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

        private void SelectorListView_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;

            // 计算滚动的增量
            double delta = e.Delta / 3.0;

            // 获取外部 ScrollViewer
            ScrollViewer scrollViewer = FindScrollViewer(SelectorListView);

            // 调整滚动位置
            if (scrollViewer != null)
            {
                scrollViewer.ScrollToVerticalOffset(scrollViewer.VerticalOffset - delta);
            }
        }

        private ScrollViewer FindScrollViewer(DependencyObject depObj)
        {
            if (depObj == null) return null;

            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(depObj, i);

                if (child is ScrollViewer)
                {
                    return (ScrollViewer)child;
                }

                ScrollViewer result = FindScrollViewer(child);

                if (result != null)
                    return result;
            }

            return null;
        }
    }
}
