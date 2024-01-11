using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using SokuLauncher.ViewModels;

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
                typeof(ModSettingGroupViewModel),
                typeof(ModSettingGroupUserControl),
                new PropertyMetadata(new ModSettingGroupViewModel())
            );

        public ModSettingGroupViewModel ModSettingGroup
        {
            get
            {
                return (ModSettingGroupViewModel)(GetValue(ModSettingGroupProperty));
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
                if ((DataContext as ModSettingGroupViewModel)?.Cover != null && new string[] { "mp4", "avi", "wmv", "gif" }.Any(x => (DataContext as ModSettingGroupViewModel)?.Cover.ToLower().EndsWith(x) ?? false))
                {
                    return true;
                }
                return false;
            }
        }
    }
}
