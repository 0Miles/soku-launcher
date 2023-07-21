using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SokuLauncher.Models;

namespace SokuLauncher.Controls
{
    public partial class ModSettingGroupUserControl : UserControl
    {
        public ModSettingGroupUserControl()
        {
            InitializeComponent();
        }

        public event Action<object, SelectionChangedEventArgs> SelectionChanged;
        public event Action<object, RoutedEventArgs> ModDeleted;
        public event Action<object, RoutedEventArgs> ModUndeleted;

        public static DependencyProperty ModSettingGroupProperty =
            DependencyProperty.Register(
                "ModSettingGroup",
                typeof(ModSettingGroupModel),
                typeof(ModSettingGroupUserControl),
                new PropertyMetadata(new ModSettingGroupModel())
            );

        public ModSettingGroupModel ModSettingGroup
        {
            get
            {
                return (ModSettingGroupModel)(GetValue(ModSettingGroupProperty));
            }
            set
            {
                SetValue(ModSettingGroupProperty, value);
            }
        }

        private void CoverMediaElement_Loaded(object sender, RoutedEventArgs e)
        {
            MediaElement mediaElement = (MediaElement)sender;
            mediaElement.Position = TimeSpan.FromMilliseconds(1);
            mediaElement.Pause();
        }

        private void CoverMediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            MediaElement mediaElement = (MediaElement)sender;
            mediaElement.Position = TimeSpan.FromMilliseconds(1);
            mediaElement.Play();
        }

        private void WrapperGrid_MouseEnter(object sender, MouseEventArgs e)
        {
            if (IsVideoCover && CoverMediaElement != null)
            {
                CoverMediaElement.Position = TimeSpan.FromMilliseconds(100);
                CoverMediaElement.Play();
            }
        }

        private void WrapperGrid_MouseLeave(object sender, MouseEventArgs e)
        {
            if (IsVideoCover && CoverMediaElement != null)
            {
                CoverMediaElement.Position = TimeSpan.FromMilliseconds(1);
                CoverMediaElement.Stop();
            }
        }

        private bool IsVideoCover
        {
            get
            {
                if ((DataContext as ModSettingGroupModel)?.Cover != null && new string[] { "mp4", "avi", "wmv", "gif" }.Any(x => (DataContext as ModSettingGroupModel)?.Cover.ToLower().EndsWith(x) ?? false))
                {
                    return true;
                }
                return false;
            }
        }
    }
}
