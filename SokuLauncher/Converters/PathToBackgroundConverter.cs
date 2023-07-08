using SokuLauncher.Models;
using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Linq;

namespace SokuLauncher.Converters
{
    public class PathToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string originPath && new string[] { "png", "jpg", "jpeg", "bmp" }.Any(x => originPath.ToLower().EndsWith(x)))
            {
                var relativePathConverter = new RelativePathConverter();
                string path = relativePathConverter.Convert(originPath, null, null, null) as string;

                return new ImageBrush()
                {
                    ImageSource = new BitmapImage(new Uri(path)),
                    Stretch = Stretch.UniformToFill,
                };
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
