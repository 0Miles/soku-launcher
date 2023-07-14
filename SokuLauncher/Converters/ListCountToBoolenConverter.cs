using System;
using System.Collections;
using System.Globalization;
using System.Windows.Data;

namespace SokuLauncher.Converters
{
    public class ListCountToBoolenConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null || (value as ICollection).Count == 0)
            {
                return false;
            }
            else
            {
                return true;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
