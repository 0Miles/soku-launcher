using System;
using System.Globalization;
using System.IO;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Linq;

namespace SokuLauncher.Shared.Converters
{
    public class PathToImageSourceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string originPath && new string[] { "png", "jpg", "jpeg", "bmp", "ico" }.Any(x => originPath.ToLower().EndsWith(x)))
            {
                try
                {
                    var relativePathConverter = new RelativePathConverter();
                    string path = relativePathConverter.Convert(originPath, null, null, null) as string;

                    if (path.StartsWith("pack://application"))
                    {
                        return new BitmapImage(new Uri(path));
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

                        return bitmap;
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
            return null;
        }
    }
}
