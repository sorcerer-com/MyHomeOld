using System;
using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Xml;

namespace MyHome.Utils
{
    [AttributeUsage(AttributeTargets.Property)]
    public class XmlIgnoreAttribute : Attribute
    {
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class XmlIdentifierAttribute : Attribute
    {
    }


    public static class XmlSerializer
    {

        public static void Serialize(XmlDocument xmlDoc, Object obj)
        {
            XmlElement xmlRoot = xmlDoc.DocumentElement;
            XmlSerializer.Serialize(xmlRoot, obj);
        }

        public static void Serialize(XmlElement xmlRoot, Object obj)
        {
            Type type = obj.GetType();

            XmlDocument xmlDoc = xmlRoot.OwnerDocument;
            XmlElement xmlMain = xmlDoc.CreateElement(type.Name);
            xmlRoot.AppendChild(xmlMain);

            if (type == typeof(string))
            {
                xmlMain.SetAttribute("Value", obj.ToString());
                return;
            }

            PropertyInfo[] pis = type.GetProperties();
            foreach (PropertyInfo pi in pis)
            {
                if (pi.GetCustomAttribute(typeof(XmlIgnoreAttribute)) != null)
                    continue;

                if (pi.PropertyType.IsArray)
                {
                    Array array = pi.GetValue(obj) as Array;
                    string value = "";
                    foreach (Object elem in array)
                        value += Utils.toInvariantString(elem) + ",";
                    if (value.Length > 0)
                        value = value.Substring(0, value.Length - 1);
                    xmlMain.SetAttribute(pi.Name, value);
                }
                else if (typeof(IList).IsAssignableFrom(pi.PropertyType) && pi.PropertyType != typeof(string))
                {
                    XmlElement xmlElement = xmlDoc.CreateElement(pi.Name);
                    xmlMain.AppendChild(xmlElement);

                    IList list = pi.GetValue(obj) as IList;
                    foreach (Object elem in list)
                        XmlSerializer.Serialize(xmlElement, elem);
                }
                else if (pi.PropertyType == typeof(int) && pi.GetCustomAttribute(typeof(XmlIdentifierAttribute)) != null)
                {
                    int value = Math.Abs((int)pi.GetValue(obj));
                    xmlMain.SetAttribute(pi.Name, value.ToString("D10", CultureInfo.InvariantCulture));
                }
                else if (pi.PropertyType == typeof(System.Drawing.Color))
                {
                    System.Drawing.Color value = (System.Drawing.Color)pi.GetValue(obj);
                    xmlMain.SetAttribute(pi.Name, Utils.toInvariantString(value));
                }
                else
                {
                    Object value = pi.GetValue(obj);
                    if (value != null)
                        xmlMain.SetAttribute(pi.Name, Utils.toInvariantString(value));
                }
            }
        }


        public static void Deserialize(XmlDocument xmlDoc, Object obj)
        {
            XmlElement xmlRoot = xmlDoc.DocumentElement;
            if (xmlRoot == null) return;
            XmlElement xmlMain = xmlRoot.GetElementsByTagName(obj.GetType().Name)[0] as XmlElement;
            if (xmlMain == null) return;

            XmlSerializer.Deserialize(xmlMain, obj);
        }

        public static void Deserialize(XmlElement xmlMain, Object obj)
        {
            Type type = obj.GetType();

            foreach (XmlAttribute xmlAttribute in xmlMain.Attributes)
            {
                PropertyInfo pi = type.GetProperty(xmlAttribute.Name);
                if (pi == null)
                    continue;

                if (pi.PropertyType.IsArray)
                {
                    string[] values = xmlAttribute.Value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                    Type elementType = pi.PropertyType.GetElementType();
                    Array array = Array.CreateInstance(elementType, values.Length);
                    for (int i = 0; i < values.Length; i++)
                        array.SetValue(Utils.Parse(values[i], elementType), i);
                    pi.SetValue(obj, array);
                }
                else
                {
                    pi.SetValue(obj, Utils.Parse(xmlAttribute.Value, pi.PropertyType));
                }
            }

            foreach (XmlNode xmlNode in xmlMain.ChildNodes)
            {
                XmlElement xmlElement = xmlNode as XmlElement;
                if (xmlElement == null)
                    continue;

                PropertyInfo pi = type.GetProperty(xmlElement.Name);
                if (pi == null || !typeof(IList).IsAssignableFrom(pi.PropertyType))
                    continue;

                IList list = pi.GetValue(obj) as IList;
                list.Clear();
                Type[] genTypes = pi.PropertyType.GenericTypeArguments;
                if (genTypes != null && genTypes.Length > 0)
                {
                    foreach (XmlNode xmlNode2 in xmlElement.ChildNodes)
                    {
                        XmlElement xmlElement2 = xmlNode2 as XmlElement;
                        if (xmlElement2 == null)
                            continue;

                        if (genTypes[0] == typeof(string))
                            list.Add(xmlElement2.GetAttribute("Value"));
                        else
                        {
                            Object newObj = genTypes[0].GetConstructor(new Type[0]).Invoke(null);
                            XmlSerializer.Deserialize(xmlElement2, newObj);
                            list.Add(newObj);
                        }
                    }
                }
            }
        }

    }
}
