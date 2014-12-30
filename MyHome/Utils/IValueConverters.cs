using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace MyHome.Utils
{
    public class DrawingColorToMediaColor : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            System.Drawing.Color color = (System.Drawing.Color)value;
            System.Windows.Media.Color result = System.Windows.Media.Color.FromArgb(color.A, color.R, color.G, color.B);
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            System.Windows.Media.Color color = (System.Windows.Media.Color)value;
            System.Drawing.Color result = System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
            return result;
        }
    }


    public class ICollectionToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ICollection collection = value as ICollection;
            if (collection == null)
                return null;

            string result = "";
            foreach (var item in collection)
                result += item.ToString() + ", ";
            result = result.Substring(0, result.Length - 2);
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string str = value as string;
            if (str == null)
                return null;

            string[] result = str.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries);
            return new List<string>(result);
        }
    }
}
