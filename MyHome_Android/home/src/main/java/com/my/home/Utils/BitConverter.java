package com.my.home.Utils;

import java.nio.ByteBuffer;
import java.nio.ByteOrder;

/**
 * Created by Hristo on 14-02-05.
 */
public class BitConverter {

    public static byte[] getBytes(short s) {
        return ByteBuffer.allocate(2).order(ByteOrder.LITTLE_ENDIAN).putShort(s).array();
    }

    public static byte[] getBytes(int i) {
        return ByteBuffer.allocate(4).order(ByteOrder.LITTLE_ENDIAN).putInt(i).array();
    }

    public static byte[] getBytes(double d) {
        return ByteBuffer.allocate(4).order(ByteOrder.LITTLE_ENDIAN).putDouble(d).array();
    }

    public static short toInt16(byte[] data, int start) {
        return ByteBuffer.wrap(data, start, 2).order(ByteOrder.LITTLE_ENDIAN).getShort();
    }

    public static int toInt32(byte[] data, int start) {
        return ByteBuffer.wrap(data, start, 4).order(ByteOrder.LITTLE_ENDIAN).getInt();
    }

    public static double toDouble(byte[] data, int start) {
        return ByteBuffer.wrap(data, start, 8).order(ByteOrder.LITTLE_ENDIAN).getDouble();
    }
	
}
