using System;
using System.Collections;
using System.Globalization;
using System.Windows.Data;

namespace MyHome.Utils
{
    public class ICollectionToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ICollection collection = value as ICollection;
            string result = "";
            foreach (var item in collection)
                result += item.ToString() + ", ";
            result = result.Substring(0, result.Length - 2);
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
