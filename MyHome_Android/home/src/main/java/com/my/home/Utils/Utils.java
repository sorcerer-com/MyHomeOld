package com.my.home.Utils;

import android.content.Context;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.graphics.Canvas;
import android.graphics.ColorMatrix;
import android.graphics.ColorMatrixColorFilter;
import android.graphics.Paint;
import android.graphics.Point;
import android.graphics.drawable.BitmapDrawable;
import android.graphics.drawable.Drawable;
import android.widget.ImageView;

import com.my.home.TcpConnection.Client;

import java.io.FileInputStream;
import java.io.FileOutputStream;
import java.util.ArrayList;
import java.util.Arrays;

/**
 * Created by Hristo on 14-02-06.
 */
public class Utils {

    public static <T> T as(Object o, Class<T> clazz) {
        if(clazz.isInstance(o)) {
            return clazz.cast(o);
        }
        return null;
    }

	// convert object[] to byte[]
	public static byte[] convert(ArrayList<Object> data) {
		return convert(data, 0, data.size());
	}

	public static byte[] convert(ArrayList<Object> data, int start)	{
		return convert(data, start, data.size() - start);
	}

	public static byte[] convert(ArrayList<Object> data, int start, int length) {
		if (start + length > data.size())
			length = data.size() - start;
			
		byte[] bytes = new byte[length];
		for (int i = 0; i < length; i++)
			bytes[i] = (Byte)data.get(start + i);
		return bytes;
	}

    // convert byte to short
    public static short convert(byte b) {
        return (short)(b & 0xff);
    }
	
	
	// parse functions
	public static int parseInt(String str) {
		int result = 0;
		try {
			result = Integer.parseInt(str);
		} catch(Exception e) {
			Client.log("Exception: " + e.getMessage()); 
		}
		return result;
	}
	
	public static double parseDouble(String str) {
		double result = 0;
		try {
			result = Double.parseDouble(str);
		} catch(Exception e) { 
			Client.log("Exception: " + e.getMessage()); 
		}
		return result;
	}
	
	public static Point parsePoint(String str) {
		Point result = new Point();
		try {
			String[] strings = str.split(",");
			result  = new Point(Integer.parseInt(strings[0]), Integer.parseInt(strings[1]));
		} catch(Exception e) { 
			Client.log("Exception: " + e.getMessage()); 
		}
		return result;
	}
	
	
	public static Object parse(String str, Class type)
	{
		if (type.equals(byte.class))
		{
			return (byte)Utils.parseInt(str);
		}
		else if (type.equals(int.class))
		{
			return Utils.parseInt(str);
		}
		else if (type.equals(double.class))
		{
			return Utils.parseDouble(str);
		}
		else if (type.equals(Point.class))
		{
			return Utils.parsePoint(str);
		}
		return str;
	}
	

	public static Bitmap getBitmap(ImageView imageView) {
        // return original image
		Drawable drawable = imageView.getDrawable();
        if (drawable == null)
            return null;
		if (drawable instanceof BitmapDrawable)
			return ((BitmapDrawable)drawable).getBitmap();
		
		Bitmap bitmap = Bitmap.createBitmap(drawable.getIntrinsicWidth(), drawable.getIntrinsicHeight(), Bitmap.Config.ARGB_8888);
		Canvas canvas = new Canvas(bitmap);
		drawable.draw(canvas);
		return bitmap;
	}

    public static Bitmap setSaturation(Bitmap src, float saturation) {
        Bitmap bitmapResult = Bitmap.createBitmap(src.getWidth(), src.getHeight(), Bitmap.Config.ARGB_8888);
        Canvas canvasResult = new Canvas(bitmapResult);
        Paint paint = new Paint();
        ColorMatrix colorMatrix = new ColorMatrix();
        colorMatrix.setSaturation(saturation);
        ColorMatrixColorFilter filter = new ColorMatrixColorFilter(colorMatrix);
        paint.setColorFilter(filter);
        canvasResult.drawBitmap(src, 0, 0, paint);

        return bitmapResult;
    }
	
	
	
	public static class Thread {
	
		public static void sleep(int time) {
            try {
                java.lang.Thread.sleep(time);
            } catch (Exception e) {
				Client.log("Exception: " + e.getMessage());
            }
		}
		
		public static void join(java.lang.Thread thread) {
			try {
				thread.join();
			} catch (Exception e) {
				Client.log("Exception: " + e.getMessage());
			}
		}
		
	}
	
	
	public static class InternalStorage {

        public static Bitmap loadBitmap(Context context, String fileName) {
            Bitmap result = null;
            if (Arrays.asList(context.fileList()).contains(fileName)) {
                try {
                    FileInputStream in = context.openFileInput(fileName);
                    result = BitmapFactory.decodeStream(in);
                    in.close();
                } catch (Exception e) {
                    Client.log("Exception: " + e.getMessage());
                }
            }
            return result;
        }
	
		public static void saveBitmap(Context context, String fileName, Bitmap bitmap) {
            try {
                FileOutputStream out = context.openFileOutput(fileName, Context.MODE_PRIVATE);
                bitmap.compress(Bitmap.CompressFormat.PNG, 100, out);
                out.close();
            } catch (Exception e) {
                Client.log("Exception: " + e.getMessage());
            }
		}

    }
	
}
