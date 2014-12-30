package com.my.home.Services;

import android.content.Context;
import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.graphics.Color;
import android.graphics.Point;

import com.my.home.MainActivity;
import com.my.home.TcpConnection.Client;
import com.my.home.TcpConnection.Command;
import com.my.home.TcpConnection.ECommandType;
import com.my.home.Utils.Utils;
import com.my.home.Utils.XmlSerializer;

import org.w3c.dom.Document;
import org.w3c.dom.Element;

import java.util.ArrayList;

/**
 * Created by Hristo on 14-02-25.
 */
public class HomeControl extends Service {

    public static final String LayoutFilename = "layout.png";
	public static final int InvalidRoomId = 0;

	public static class Room {

        @XmlSerializer.XmlIdentifierAttribute
		public int id;
		public int color;
		public String name;
		public Point min;
		public Point max;
		
		public Room() {
			this.id = HomeControl.InvalidRoomId;
			this.color = Color.BLACK;
			this.name = "";
			this.min = new Point(Integer.MAX_VALUE, Integer.MAX_VALUE);
			this.max = new Point(Integer.MIN_VALUE, Integer.MIN_VALUE);
		}
		
	}


    @XmlSerializer.XmlIgnoreAttribute
	private Bitmap layout;
	private ArrayList<Room> rooms;
	
	public Bitmap getLayout() { return this.layout; }
	public ArrayList<Room> getRooms() { return this.rooms; }
	
	
	public HomeControl() {
		this.layout = Bitmap.createBitmap(1, 1, Bitmap.Config.ARGB_8888);
		this.rooms = new ArrayList<Room>();
	}
	

    @Override
	public EServiceType getType() { return EServiceType.HomeControl; }

    @Override
    public void load(Context context, Document xmlDoc) {
        Bitmap layout = Utils.InternalStorage.loadBitmap(context, HomeControl.LayoutFilename);
        if (layout != null)
            this.layout = layout;

        super.load(context, xmlDoc);
    }

    @Override
    public void save(Context context, Document xmlDoc) {
        Utils.InternalStorage.saveBitmap(context, HomeControl.LayoutFilename, this.layout);

        super.save(context, xmlDoc);
    }

    @Override
    public void getData() {
        HomeControl.Send.getLayout();
        HomeControl.Send.getRooms();
    }
	
    @Override
    public void onCommandReceived(Client client, Command command) {
		if (command == null)
			return;
	
		if (command.getType() == ECommandType.GetLayout && command.getArguments().size() > 0) {
			this.layout = HomeControl.Receive.getLayout(command);
			Service.onPropertyChanged(this, "HomeControl.Layout");
		}
		else if (command.getType() == ECommandType.GetRooms && command.getArguments().size() > 0) {
			this.rooms = HomeControl.Receive.getRooms(command);
			Service.onPropertyChanged(this, "HomeControl.Rooms");
		}
	}
	
	
	
	public static class Send {

		public static void getLayout() {
			Command cmd = new Command(ECommandType.GetLayout);
			MainActivity.Client.send(cmd);
		}

		public static void getRooms() {
			Command cmd = new Command(ECommandType.GetRooms);
			MainActivity.Client.send(cmd);
		}
		
	}

	public static class Receive {

		public static Bitmap getLayout(Command command) {
			if (command != null && command.getType() == ECommandType.GetLayout && command.getArguments().size() > 0) {
				ArrayList<Object> args = command.getArguments();
				// get in PNG format
				byte[] data = Utils.convert(args, 1, (Integer)args.get(0));
				return BitmapFactory.decodeByteArray(data, 0, data.length);
			}
			return null;
		}
		
		public static ArrayList<Room> getRooms(Command command) {
			if (command != null && command.getType() == ECommandType.GetRooms && command.getArguments().size() > 0) {
				ArrayList<Object> args = command.getArguments();
				int idx = 0;
				int numRooms = (Integer)args.get(idx);
				idx++;
				ArrayList<Room> rooms = new ArrayList<Room>(numRooms);
				for (int i = 0; i < numRooms; i++) {
					Room room = new Room();

                    room.id = (Integer)args.get(idx);
                    idx++;
					
                    room.color = Color.argb(Utils.convert((Byte)args.get(idx + 3)), Utils.convert((Byte)args.get(idx + 0)),
                            Utils.convert((Byte)args.get(idx + 1)), Utils.convert((Byte)args.get(idx + 2)));
					idx += 4;
					
					room.name = (String)args.get(idx);
					idx++;

                    room.min.x = (Integer)args.get(idx);
                    idx++;
                    room.min.y = (Integer)args.get(idx);
                    idx++;

                    room.max.x = (Integer)args.get(idx);
                    idx++;
                    room.max.y = (Integer)args.get(idx);
                    idx++;
					
					rooms.add(room);
					
					if (ServiceManager.getAvailableServices(room.id) == null)
						ServiceManager.Send.getAvailableServices(room.id);
				}
				return rooms;
			}
			return null;
		}
	
	}
	
}
