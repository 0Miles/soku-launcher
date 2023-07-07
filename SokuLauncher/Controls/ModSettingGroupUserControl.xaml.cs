using Newtonsoft.Json;
using SokuLauncher.Converters;
using SokuLauncher.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace SokuLauncher.Controls
{
    public partial class ModSettingGroupUserControl : UserControl
    {
        private MediaElement CoverMediaElement;
        private Border CoverBorder;
        private RelativePathConverter relativePathConverter = new RelativePathConverter();
        public ModSettingGroupUserControl()
        {
            InitializeComponent();
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

        private void WrapperBorder_MouseEnter(object sender, MouseEventArgs e)
        {
            if (IsVideoCover)
            {
                CoverMediaElement.Position = TimeSpan.FromMilliseconds(100);
                CoverMediaElement.Play();
            }
        }

        private void WrapperBorder_MouseLeave(object sender, MouseEventArgs e)
        {
            if (IsVideoCover)
            {
                CoverMediaElement.Position = TimeSpan.FromMilliseconds(1);
                CoverMediaElement.Stop();
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (IsVideoCover)
            {
                CoverMediaElement = new MediaElement()
                {
                    FlowDirection = FlowDirection.LeftToRight,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    Stretch = Stretch.UniformToFill,
                    LoadedBehavior = MediaState.Manual,
                    UnloadedBehavior = MediaState.Stop,
                    ScrubbingEnabled = true,
                };
                CoverMediaElement.SetBinding(MediaElement.SourceProperty, new Binding("Cover") { Converter = (IValueConverter)this.Resources["RelativePathConverter"] });
                CoverMediaElement.Loaded += CoverMediaElement_Loaded;
                CoverMediaElement.MediaEnded += CoverMediaElement_MediaEnded;
                CoverGrid.Children.Add(CoverMediaElement);
            }

            if (IsImageCover)
            {
                string coverPath = relativePathConverter.Convert((DataContext as ModSettingGroupModel).Cover, null, null, null) as string;
                CoverBorder = new Border()
                {
                    FlowDirection = FlowDirection.LeftToRight,
                    HorizontalAlignment = HorizontalAlignment.Stretch,
                    VerticalAlignment = VerticalAlignment.Stretch,
                    Background = new ImageBrush()
                    {
                        ImageSource = new BitmapImage(new Uri(coverPath)),
                        Stretch = Stretch.UniformToFill,
                    },
                };
                CoverGrid.Children.Add(CoverBorder);
            }
        }

        private bool IsImageCover
        {
            get
            {
                if ((DataContext as ModSettingGroupModel).Cover != null && new string[] { "png", "jpg", "jpeg", "bmp" }.Any(x => (DataContext as ModSettingGroupModel).Cover.ToLower().EndsWith(x)))
                {
                    return true;
                }
                return false;
            }
        }

        private bool IsVideoCover
        {
            get
            {
                if ((DataContext as ModSettingGroupModel).Cover != null && new string[] { "mp4", "avi", "wmv", "gif" }.Any(x => (DataContext as ModSettingGroupModel).Cover.ToLower().EndsWith(x)))
                {
                    return true;
                }
                return false;
            }
        }
    }
}
