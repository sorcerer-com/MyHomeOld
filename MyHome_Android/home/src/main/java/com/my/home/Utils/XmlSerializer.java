package com.my.home.Utils;

import android.content.Context;
import android.graphics.Point;

import com.my.home.TcpConnection.Client;

import org.w3c.dom.Document;
import org.w3c.dom.Element;
import org.w3c.dom.NamedNodeMap;
import org.w3c.dom.Node;
import org.w3c.dom.NodeList;

import java.lang.annotation.ElementType;
import java.lang.annotation.Retention;
import java.lang.annotation.RetentionPolicy;
import java.lang.annotation.Target;
import java.lang.reflect.Array;
import java.lang.reflect.Field;
import java.lang.reflect.Modifier;
import java.lang.reflect.ParameterizedType;
import java.util.Arrays;
import java.util.List;

import javax.xml.parsers.DocumentBuilder;
import javax.xml.parsers.DocumentBuilderFactory;
import javax.xml.transform.Result;
import javax.xml.transform.Transformer;
import javax.xml.transform.TransformerFactory;
import javax.xml.transform.dom.DOMSource;
import javax.xml.transform.stream.StreamResult;

/**
 * Created by Hristo on 28.6.2014 г..
 */
public class XmlSerializer {

    @Target({ElementType.FIELD, ElementType.TYPE})
    @Retention(RetentionPolicy.RUNTIME)
    public static @interface XmlIgnoreAttribute {
    }

    @Target(ElementType.FIELD)
    @Retention(RetentionPolicy.RUNTIME)
    public static @interface XmlIdentifierAttribute {
    }


    public static Document newDocument() {
        Document xmlDoc = null;
        try {
            DocumentBuilder builder = DocumentBuilderFactory.newInstance().newDocumentBuilder();
            xmlDoc = builder.newDocument();
        } catch (Exception e) {
			Client.log("Exception: " + e.getMessage()); 
        }
        return xmlDoc;
    }

    public static Document parseDocument(Context context, String fileName) {
        Document xmlDoc = null;
        try {
            DocumentBuilder builder = DocumentBuilderFactory.newInstance().newDocumentBuilder();
            xmlDoc = builder.parse(context.openFileInput(fileName));
        } catch (Exception e) {
            Client.log("Exception: " + e.getMessage());
        }
        return xmlDoc;
    }

    public static void saveDocument(Context context, String fileName, Document xmlDoc) {
        try {
            Transformer transformer = TransformerFactory.newInstance().newTransformer();
            Result result = new StreamResult(context.openFileOutput(fileName, Context.MODE_PRIVATE));
            transformer.transform(new DOMSource(xmlDoc), result);
        } catch (Exception e) {
            Client.log("Exception: " + e.getMessage());
        }
    }


    public static void serialize(Document xmlDoc, Object obj) throws IllegalAccessException {
        Element xmlRoot = xmlDoc.getDocumentElement();
        XmlSerializer.serialize(xmlRoot, obj);
    }

    public static void serialize(Element xmlRoot, Object obj) throws IllegalAccessException {
        Class type = obj.getClass();

        Document xmlDoc = xmlRoot.getOwnerDocument();
        Element xmlMain = xmlDoc.createElement(type.getSimpleName());
        xmlRoot.appendChild(xmlMain);

        if (type.getAnnotation(XmlIgnoreAttribute.class) != null)
            return;

        if (type.equals(String.class)) {
            xmlMain.setAttribute("value", obj.toString());
            return;
        }

        Field[] fields = type.getDeclaredFields();
        for (Field field : fields) {
            if (Modifier.isStatic(field.getModifiers()))
                continue;
             if (field.getAnnotation(XmlIgnoreAttribute.class) != null)
                continue;

            field.setAccessible(true);
			if (field.getType().isArray()) {
                Object array = field.get(obj);
                String value = "";
                for (int i = 0; i < Array.getLength(array); i++)
                    value += Array.get(array, i).toString() + ",";
                if (value.length() > 0)
                    value = value.substring(0, value.length() - 1);
                xmlMain.setAttribute(field.getName(), value);
            }
            else if (List.class.isAssignableFrom(field.getType()) && !field.getType().equals(String.class)) {
                Element xmlElement = xmlDoc.createElement(field.getName());
                xmlMain.appendChild(xmlElement);

                List list = (List)field.get(obj);
                for (Object elem : list)
                    XmlSerializer.serialize(xmlElement, elem);
            }
            else if (field.getType().equals(int.class) && field.getAnnotation(XmlIdentifierAttribute.class) != null) {
                int value = Math.abs((Integer) field.get(obj));
                xmlMain.setAttribute(field.getName(), String.format("%010d", value));
            }
            else if (field.getType().equals(Point.class)) {
                Point value = (Point) field.get(obj);
                xmlMain.setAttribute(field.getName(), value.x + "," + value.y);
            }
            else {
                Object value = field.get(obj);
                if (value != null)
                    xmlMain.setAttribute(field.getName(), value.toString());
            }
        }
    }


    public static void deserialize(Document xmlDoc, Object obj) throws IllegalAccessException {
        Element xmlRoot = xmlDoc.getDocumentElement();
        if (xmlRoot == null) return;
        Element xmlMain = (Element)xmlRoot.getElementsByTagName(obj.getClass().getSimpleName()).item(0);
        if (xmlMain == null) return;

        XmlSerializer.deserialize(xmlMain, obj);
    }

    public static void deserialize(Element xmlMain, Object obj) throws IllegalAccessException {
        Class type = obj.getClass();

        NamedNodeMap attributes = xmlMain.getAttributes();
        for (int i = 0; i < attributes.getLength(); i++) {
            Node xmlAttribute = attributes.item(i);
            Field field = null;
            try {
                field = type.getDeclaredField(xmlAttribute.getNodeName());
            } catch (Exception e) { 
				Client.log("Exception: " + e.getMessage()); 
			}
            if (field == null)
                continue;

            field.setAccessible(true);
			if (field.getType().isArray()) {
                String[] values = xmlAttribute.getNodeValue().split(",");
                Class elementType = field.getType().getComponentType();
                Object array = Array.newInstance(elementType, values.length);
                for (int j = 0; j < values.length; j++)
                    Array.set(array, j, Utils.parse(values[j], elementType));
                field.set(obj, array);
			}
			else {
                field.set(obj, Utils.parse(xmlAttribute.getNodeValue(), field.getType()));
			}
        }

        NodeList childNodes = xmlMain.getChildNodes();
        for (int i = 0; i < childNodes.getLength(); i++) {
            Element xmlElement = Utils.as(childNodes.item(i), Element.class);
            if (xmlElement == null)
                continue;

            Field field = null;
            try {
                field = type.getDeclaredField(xmlElement.getNodeName());
            } catch (Exception e) {
				Client.log("Exception: " + e.getMessage());
			}
            if (field == null || !List.class.isAssignableFrom(field.getType()))
                continue;

            field.setAccessible(true);
            List list = (List)field.get(obj);
            list.clear();

            ParameterizedType genType = Utils.as(field.getGenericType(), ParameterizedType.class);
            if (genType != null && genType.getActualTypeArguments().length > 0) {
                NodeList childNodes2 = xmlElement.getChildNodes();
                for (int j = 0; j < childNodes2.getLength(); j++) {
                    Element xmlElement2 = Utils.as(childNodes2.item(j), Element.class);
                    if (xmlElement2 == null)
                        continue;

                    if (((Class) genType.getActualTypeArguments()[0]).equals(String.class))
                        list.add(xmlElement2.getAttribute("value"));
                    else {
                        Object newObj = null;
                        try {
                            newObj = ((Class) genType.getActualTypeArguments()[0]).newInstance();
                        } catch (Exception e) {
                            Client.log("Exception: " + e.getMessage());
                        }
                        XmlSerializer.deserialize(xmlElement2, newObj);
                        list.add(newObj);
                    }
                }
            }
        }
    }

}
