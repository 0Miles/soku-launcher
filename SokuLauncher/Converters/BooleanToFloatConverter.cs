using System;
using System.Globalization;
using System.Windows.Data;

namespace SokuLauncher.Converters
{
    public class BooleanToFloatConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string[] values = (parameter as string).Split(',');
            if (values.Length > 1)
            {
                return (bool)value ? float.Parse(values[1]) : float.Parse(values[0]);
            }
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
