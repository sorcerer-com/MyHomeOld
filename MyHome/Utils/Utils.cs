using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MyHome.Utils
{

    public static class Utils
    {

        public static Bitmap Grayscale(Bitmap bmp, double alpha)
        {
            Bitmap newBitmap = new Bitmap(bmp.Width, bmp.Height);
            using (Graphics g = Graphics.FromImage(newBitmap))
            {
                //create the grayscale ColorMatrix
                ColorMatrix colorMatrix = new ColorMatrix(
                   new float[][]
                  {
                     new float[] {.3f, .3f, .3f, 0, 0},
                     new float[] {.59f, .59f, .59f, 0, 0},
                     new float[] {.11f, .11f, .11f, 0, 0},
                     new float[] {0, 0, 0, (float)alpha, 0},
                     new float[] {0, 0, 0, 0, 1}
                  });
                
                ImageAttributes attributes = new ImageAttributes();
                attributes.SetColorMatrix(colorMatrix);
                g.DrawImage(bmp, new Rectangle(0, 0, bmp.Width, bmp.Height), 0, 0, bmp.Width, bmp.Height, GraphicsUnit.Pixel, attributes);
            }
            return newBitmap;
        }


        public static ImageSource Convert(Bitmap bmp)
        {
            return Imaging.CreateBitmapSourceFromHBitmap(bmp.GetHbitmap(), IntPtr.Zero,
                Int32Rect.Empty, BitmapSizeOptions.FromWidthAndHeight(bmp.Width, bmp.Height));
        }


        public static string toInvariantString(System.Drawing.Color color)
        {
            return "#" + color.ToArgb().ToString("X", CultureInfo.InvariantCulture);
        }

        public static string toInvariantString(object obj)
        {
            return obj is IConvertible ? ((IConvertible)obj).ToString(CultureInfo.InvariantCulture)
                : obj is IFormattable ? ((IFormattable)obj).ToString(null, CultureInfo.InvariantCulture)
                : obj.ToString();
        }


        #region Parse

        public static IPAddress ParseIPAddress(string str)
        {
            IPAddress result = null;
            IPAddress.TryParse(str, out result);
            return result;
        }

        public static int ParseInt(string str)
        {
            return Utils.ParseInt(str, NumberStyles.Integer);
        }

        public static int ParseInt(string str, NumberStyles numberStyle)
        {
            int result = 0;
            int.TryParse(str, numberStyle, CultureInfo.InvariantCulture, out result);
            return result;
        }

        public static double ParseDouble(string str)
        {
            return Utils.ParseDouble(str, NumberStyles.Float);
        }

        public static double ParseDouble(string str, NumberStyles numberStyle)
        {
            double result = 0;
            double.TryParse(str, numberStyle, CultureInfo.InvariantCulture, out result);
            return result;
        }

        public static System.Drawing.Color ParseColor(string str)
        {
            string hex = str.Substring(1);
            int argb = Utils.ParseInt(hex, System.Globalization.NumberStyles.HexNumber);
            return System.Drawing.Color.FromArgb(argb);
        }

        public static object Parse(string str, Type type)
        {
            if (type == typeof(byte))
            {
                return (byte)Utils.ParseInt(str);
            }
            else if (type == typeof(int))
            {
                return Utils.ParseInt(str);
            }
            else if (type == typeof(double))
            {
                return Utils.ParseDouble(str);
            }
            else if (type == typeof(System.Drawing.Color))
            {
                return Utils.ParseColor(str);
            }
            return str;
        }

        #endregion


        #region Convert byte[] to object[]

        public static object[] Convert(byte[] bytes)
        {
            return Convert(bytes, 0, bytes.Length);
        }

        public static object[] Convert(byte[] bytes, int start)
        {
            return Convert(bytes, start, bytes.Length - start);
        }

        public static object[] Convert(byte[] bytes, int start, int length)
        {
            if (start + length > bytes.Length)
                length = bytes.Length - start;

            object[] data = new object[length];
            for (int i = 0; i < length; i++)
                data[i] = bytes[start + i];
            return data;
        }

        #endregion

        #region Convert object[] to byte[]

        public static byte[] Convert(object[] data)
        {
            return Convert(data, 0, data.Length);
        }

        public static byte[] Convert(object[] data, int start)
        {
            return Convert(data, start, data.Length - start);
        }

        public static byte[] Convert(object[] data, int start, int length)
        {
            if (start + length > data.Length)
                length = data.Length - start;

            byte[] bytes = new byte[length];
            for (int i = 0; i < length; i++)
                bytes[i] = (byte)data[start + i];
            return bytes;
        }

        #endregion

    }
}
