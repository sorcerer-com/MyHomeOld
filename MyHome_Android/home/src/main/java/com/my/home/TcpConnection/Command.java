package com.my.home.TcpConnection;

import com.my.home.Utils.BitConverter;
import com.my.home.Utils.Flags;
import com.my.home.Utils.Utils;

import java.io.ByteArrayOutputStream;
import java.util.ArrayList;

/**
 * Created by Hristo on 13-12-28.
 */
public class Command {
    public static final int MinBytes = 8;

    private ECommandType type;
    private ArrayList<Object> arguments;


    public Command() {
        this.type = ECommandType.Invalid;
        this.arguments = new ArrayList<Object>();
    }

    public Command(ECommandType type) {
        this();
        this.type = type;
    }

    public Command(ECommandType type, ArrayList<Object> arguments) {
        this();
        this.type = type;
        if (arguments != null)
            this.arguments = new ArrayList<Object>(arguments);
    }

    public Command(Command cmd) {
        this(cmd.type, cmd.arguments);
    }


    public ECommandType getType() {
        return type;
    }

    public void setType(ECommandType type) {
        this.type = type;
    }

    public ArrayList<Object> getArguments() {
        return arguments;
    }


    public byte[] serialize() {
		int size = this.arguments.size();
		ByteArrayOutputStream buffer = new ByteArrayOutputStream(4 + 4 + size + size * 8); // max length = type(4) + size(4) + size * flag(1) +  size * double(8)

		buffer.write(BitConverter.getBytes(this.type.getValue()), 0, 4);
		buffer.write(BitConverter.getBytes(size), 0, 4);
		
		int count = 0;
		for (int i = 0; i < size; i++) {
			Object arg = this.arguments.get(i);
			if (count == 0) { // count arguments with equal types
				while (i + count < size && arg.getClass() == this.arguments.get(i + count).getClass())
					count++;

				Flags flags = new Flags();
				if (arg.getClass() == Byte.class) 
					flags.setFlag(1, true);
				else if (arg.getClass() == Integer.class) 
					flags.setFlag(2, true);
				else if (arg.getClass() == Double.class) 
					flags.setFlag(3, true);
				else if (arg.getClass() == String.class) 
					flags.setFlag(4, true);
				
				if (count > 255) 
					flags.setFlag(5, true);
				
				buffer.write(flags.getByte());
				if (count > 255)
					buffer.write(BitConverter.getBytes(count), 0, 4);
				else
					buffer.write((byte)count);
			}
			
            if (arg.getClass() == Byte.class)
				buffer.write((Byte)arg);
            else if (arg.getClass() == Integer.class)
				buffer.write(BitConverter.getBytes((Integer)arg), 0, 4);
            else if (arg.getClass() == Double.class)
				buffer.write(BitConverter.getBytes((Double)arg), 0, 4);
            else if (arg.getClass() == String.class) {
                byte[] bytes = new byte[0];
                try { bytes = ((String)arg).getBytes("UTF-16LE"); }
                catch (Exception e) { Client.log("Exception: " + e.getMessage()); }
				buffer.write(BitConverter.getBytes(bytes.length), 0, 4);
                for(byte b : bytes)
					buffer.write(b);
            }
			
			count--;
        }
		
        return buffer.toByteArray();
    }

    public byte[] deSerialize(byte[] data) {
        int start = 0;

        this.type = ECommandType.values()[BitConverter.toInt32(data, start)];
        start += 4;
        int size = BitConverter.toInt32(data, start);
        start += 4;
        this.arguments = new ArrayList<Object>(size);
		
		int count = 0;
		Flags flags = new Flags();
        for (int i = 0; i < size; i++) {
			if (count == 0) { // get count of arguments with equal types
				flags = new Flags(data[start]);
				start++;
				if (flags.getFlag(5)) {
					count = BitConverter.toInt32(data, start);
					start += 4;
				}
				else {
					count = Utils.convert(data[start]);
					start++;
				}
			}
			
            if (flags.getFlag(1)) {
                this.arguments.add(data[start]);
                start += 1;
            }
            else if (flags.getFlag(2)) {
                this.arguments.add(BitConverter.toInt32(data, start));
                start += 4;
            }
            else if (flags.getFlag(3)) {
                this.arguments.add(BitConverter.toDouble(data, start));
                start += 8;
            }
            else if (flags.getFlag(4)) {
				int len = BitConverter.toInt32(data, start);
                start += 4;
				String str = "";
				try { str = new String(data, start, len, "UTF-16LE"); } 
				catch (Exception e) { Client.log("Exception: " + e.getMessage()); }
				start += len;
				
				this.arguments.add(str);
            }
			
			count--;
        }

		byte[] temp = new byte[data.length - start];
		System.arraycopy(data, start, temp, 0, data.length - start);
		return temp;
    }


    @Override
    public String toString() {
        String res = this.type +": ";
        if (this.arguments.size() < 100) {
            for (Object arg : this.arguments)
				res += arg + ", ";
        }
        else
            res += "a lot of arguments";
        return res;
    }

}
