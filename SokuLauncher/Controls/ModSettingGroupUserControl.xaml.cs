using SokuLauncher.Converters;
using SokuLauncher.Models;
using SokuLauncher.ViewModels;
using System;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SokuLauncher.Controls
{
    public partial class ModSettingGroupUserControl : UserControl, INotifyPropertyChanged
    {

        public ModSettingGroupUserControl()
        {
            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
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
