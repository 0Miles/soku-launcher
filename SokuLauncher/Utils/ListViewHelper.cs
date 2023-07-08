using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;

namespace SokuLauncher.Utils
{
    public static class ListViewHelper
    {
        public static WrapPanel GetWrapPanel(ListView listView)
        {
            ItemsPresenter itemsPresenter = FindVisualChild<ItemsPresenter>(listView);
            if (itemsPresenter != null)
            {
                ItemsPanelTemplate itemsPanelTemplate = listView.ItemsPanel;
                if (itemsPanelTemplate != null)
                {
                    return itemsPanelTemplate.FindName("PART_WrapPanel", itemsPresenter) as WrapPanel;
                }
            }
            return null;
        }

        private static T FindVisualChild<T>(DependencyObject parent) where T : DependencyObject
        {
            int childCount = VisualTreeHelper.GetChildrenCount(parent);
            for (int i = 0; i < childCount; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(parent, i);
                if (child is T typedChild)
                {
                    return typedChild;
                }
                T result = FindVisualChild<T>(child);
                if (result != null)
                {
                    return result;
                }
            }
            return null;
        }
    }
}
