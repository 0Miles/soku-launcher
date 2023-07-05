using System;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows.Data;

namespace SokuLauncher.Converter
{
    public class RelativePathConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string relativePath = value as string;
            if (!string.IsNullOrEmpty(relativePath))
            {
                string exePath = Assembly.GetExecutingAssembly().Location;
                string exeDirectory = Path.GetDirectoryName(exePath);

                string absolutePath = Path.GetFullPath(Path.Combine(exeDirectory, relativePath));
                return absolutePath;
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
