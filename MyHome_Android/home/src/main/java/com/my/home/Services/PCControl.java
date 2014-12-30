package com.my.home.Services;

import android.graphics.Bitmap;
import android.graphics.BitmapFactory;
import android.graphics.Point;

import com.my.home.MainActivity;
import com.my.home.TcpConnection.Client;
import com.my.home.TcpConnection.Command;
import com.my.home.TcpConnection.ECommandType;
import com.my.home.Utils.Enums;
import com.my.home.Utils.Utils;
import com.my.home.Utils.XmlSerializer;

import java.util.ArrayList;

/**
 * Created by Hristo on 14-02-04.
 */
@XmlSerializer.XmlIgnoreAttribute
public class PCControl extends Service {

	private Point mousePos;
    private Bitmap screenImage;
	private Bitmap cameraImage;
	
	public Point getMousePos() { return this.mousePos; }
	public void setMousePos(Point p) { PCControl.Send.setMousePosition(p); this.mousePos = p; PCControl.Send.getMousePosition(); }
	public Bitmap getScreenImage() { return this.screenImage; }
	public Bitmap getCameraImage() { return this.cameraImage; }
	
	public PCControl() {
		this.mousePos = new Point();
		this.screenImage = Bitmap.createBitmap(1, 1, Bitmap.Config.ARGB_8888);
		this.cameraImage = Bitmap.createBitmap(1, 1, Bitmap.Config.ARGB_8888);
	}
	

    @Override
	public EServiceType getType() { return EServiceType.PCControl; }

    @Override
    public void getData() {
        PCControl.Send.getMousePosition();
    }
	
    @Override
    public void onCommandReceived(Client client, Command command) {
		if (command == null)
			return;
	
        if (command.getType() == ECommandType.GetMousePosition && command.getArguments().size() == 2) {
            this.mousePos = PCControl.Receive.getMousePosition(command);
			Service.onPropertyChanged(this, "PCControl.MousePos");
        }
        else if (command.getType() == ECommandType.GetScreenImage && command.getArguments().size() > 0) {
            this.screenImage = PCControl.Receive.getScreenImage(command);
			Service.onPropertyChanged(this, "PCControl.ScreenImage");
        }
        else if (command.getType() == ECommandType.GetCameraImage && command.getArguments().size() > 0) {
            this.cameraImage = PCControl.Receive.getCameraImage(command);
			Service.onPropertyChanged(this, "PCControl.CameraImage");
        }
	}



	public static class Send {

		public static void setKey(int unicodeChar) {
			Command cmd = new Command(ECommandType.SetKey);
			cmd.getArguments().add(unicodeChar);
			MainActivity.Client.send(cmd);
		}

		public static void getMousePosition() {
			Command cmd = new Command(ECommandType.GetMousePosition);
			MainActivity.Client.send(cmd);
		}

		public static void setMousePosition(Point p) {
			Command cmd = new Command(ECommandType.SetMousePosition);
			cmd.getArguments().add(p.x);
			cmd.getArguments().add(p.y);
			MainActivity.Client.send(cmd);
		}
	
		public static void setMouseButton(Enums.EMouseButton mouseButton, Enums.EMouseButtonState mouseButtonState) {
			Command cmd = new Command(ECommandType.SetMouseButton);
			cmd.getArguments().add(mouseButton.getValue());
			cmd.getArguments().add(mouseButtonState.getValue());
			MainActivity.Client.send(cmd);
		}
	
		public static void setMouseWheel(int delta) {
			Command cmd = new Command(ECommandType.SetMouseWheel);
			cmd.getArguments().add(delta);
			MainActivity.Client.send(cmd);
		}
	
		public static void getScreenImage(int x, int y, int width, int height) {
            Command cmd = new Command(ECommandType.GetScreenImage);
            cmd.getArguments().add(x);
            cmd.getArguments().add(y);
            cmd.getArguments().add(width);
            cmd.getArguments().add(height);
            MainActivity.Client.send(cmd);
		}
	
		public static void getCameraImage() {
            Command cmd = new Command(ECommandType.GetCameraImage);
            MainActivity.Client.send(cmd);
		}
		
	}

	public static class Receive {

		public static Point getMousePosition(Command command) {
			Point res = new Point();
			if (command != null && command.getType() == ECommandType.GetMousePosition && command.getArguments().size() == 2) {
				res.x = (Integer)command.getArguments().get(0);
				res.y = (Integer)command.getArguments().get(1);
			}
			return res;
		}

		public static Bitmap getScreenImage(Command command) {
			if (command != null && command.getType() == ECommandType.GetScreenImage && command.getArguments().size() > 0) {
				ArrayList<Object> args = command.getArguments();
				// get in PNG format
				byte[] data = Utils.convert(args, 1, (Integer)args.get(0));
				return BitmapFactory.decodeByteArray(data, 0, data.length);
			}
			return null;
		}

		public static Bitmap getCameraImage(Command command) {
			if (command != null && command.getType() == ECommandType.GetCameraImage && command.getArguments().size() > 0) {
				ArrayList<Object> args = command.getArguments();
				// get in PNG format
				byte[] data = Utils.convert(args, 1, (Integer)args.get(0));
				return BitmapFactory.decodeByteArray(data, 0, data.length);
			}
			return null;
		}
	
	}
	
}
