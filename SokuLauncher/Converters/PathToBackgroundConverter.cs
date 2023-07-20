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
                try
                {
                    var relativePathConverter = new RelativePathConverter();
                    string path = relativePathConverter.Convert(originPath, null, null, null) as string;

                    if (path.StartsWith("pack://application"))
                    {
                        return new ImageBrush()
                        {
                            ImageSource = new BitmapImage(new Uri(path)),
                            Stretch = Stretch.UniformToFill,
                        };
                    }
                    else
                    {
                        BitmapImage bitmap;
                        using (FileStream stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                        {
                            bitmap = new BitmapImage();

                            bitmap.BeginInit();
                            bitmap.CacheOption = BitmapCacheOption.OnLoad;
                            bitmap.StreamSource = stream;
                            bitmap.EndInit();

                            bitmap.Freeze();

                            stream.Close();
                        }

                        return new ImageBrush()
                        {
                            ImageSource = bitmap,
                            Stretch = Stretch.UniformToFill,
                        };
                    }
                }
                catch
                {

                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
