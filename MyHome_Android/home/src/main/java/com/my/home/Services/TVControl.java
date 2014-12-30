package com.my.home.Services;

import com.my.home.MainActivity;
import com.my.home.TcpConnection.Client;
import com.my.home.TcpConnection.Command;
import com.my.home.TcpConnection.ECommandType;
import com.my.home.Utils.XmlSerializer;

import java.lang.reflect.Array;
import java.util.ArrayList;

/**
 * Created by Hristo on 14-05-16.
 */
public class TVControl extends Service {

    public static class RemoteControlButton {

        public String name;
        public byte[] signal;

        public RemoteControlButton() {
            this.name = "";
            this.signal = new byte[0];
        }
    }
	
	public static class Television {

        @XmlSerializer.XmlIdentifierAttribute
		public int roomId;
        // TODO: add arduino connection - COM port and output pin (may be somekind of id)

		public String name;
		public int input;
		public int channel;
		public int volume;
		public ArrayList<RemoteControlButton> remoteControl;

		public Television()	{
			this.roomId = HomeControl.InvalidRoomId;

			this.name = "";
			this.input = 0;
			this.channel = 0;
			this.volume = 0;
			this.remoteControl = new ArrayList<RemoteControlButton>();
		}
		
	}

	
	private ArrayList<Television> televisions;
	private ArrayList<String> movies;
	private ArrayList<String> images;
	
	public ArrayList<Television> getTelevisions() { return this.televisions; }
	public ArrayList<String> getMovies() { return this.movies; }
	public ArrayList<String> getImages() { return this.images; }
	
	
	public TVControl() {
		this.televisions = new ArrayList<Television>();
		this.movies = new ArrayList<String>();
		this.images = new ArrayList<String>();
	}


	@Override
	public EServiceType getType() { return EServiceType.TVControl; }

    @Override
    public void getData() {
        TVControl.Send.getTelevisions();
        TVControl.Send.getMovies();
        TVControl.Send.getImages(" ");
    }
	
    @Override
    public void onCommandReceived(Client client, Command command) {
		if (command == null)
			return;
	
		if (command.getType() == ECommandType.GetTelevisions && command.getArguments().size() > 0) {
			this.televisions = TVControl.Receive.getTelevisions(command);
			Service.onPropertyChanged(this, "TVControl.Televisions");
		}
		else if (command.getType() == ECommandType.GetMovies) {
			if (command.getArguments().size() == 0) // if receive "start" command
				this.movies.clear();
			else
				this.movies.addAll(TVControl.Receive.getMovies(command));
			Service.onPropertyChanged(this, "TVControl.Movies");
		}
		else if (command.getType() == ECommandType.GetImages) {
			if (command.getArguments().size() == 0) // if receive "start" command
				this.images.clear();
			else
				this.images.addAll(TVControl.Receive.getImages(command));
			Service.onPropertyChanged(this, "TVControl.Images");
		}
	}
	
	
	
	public static class Send {

		public static void getTelevisions() {
			Command cmd = new Command(ECommandType.GetTelevisions);
			MainActivity.Client.send(cmd);
		}

		public static void setRemoteControlButton(int roomId, int buttonIdx) {
			Command cmd = new Command(ECommandType.SetRemoteControlButton);
            cmd.getArguments().add(roomId);
            cmd.getArguments().add(buttonIdx);
			MainActivity.Client.send(cmd);
		}

		public static void getMovies() {
			Command cmd = new Command(ECommandType.GetMovies);
			MainActivity.Client.send(cmd);
		}

		public static void setMovie(String command, String movieName) {
			Command cmd = new Command(ECommandType.SetMovie);
            cmd.getArguments().add(command);
            cmd.getArguments().add(movieName);
			MainActivity.Client.send(cmd);
		}

		public static void getImages(String path) {
			Command cmd = new Command(ECommandType.GetImages);
			cmd.getArguments().add(path);
			MainActivity.Client.send(cmd);
		}

		public static void setImage(String command, String imagePath) {
			Command cmd = new Command(ECommandType.SetImage);
            cmd.getArguments().add(command);
            cmd.getArguments().add(imagePath);
			MainActivity.Client.send(cmd);
		}
		
	}

	public static class Receive {
		
		public static ArrayList<Television> getTelevisions(Command command) {
			if (command != null && command.getType() == ECommandType.GetTelevisions && command.getArguments().size() > 0) {
				ArrayList<Object> args = command.getArguments();
				int idx = 0;
				int numTelevisions = (Integer)args.get(idx);
				idx++;
				ArrayList<Television> televisions = new ArrayList<Television>(numTelevisions);
				for (int i = 0; i < numTelevisions; i++) {
					Television television = new Television();

                    television.roomId = (Integer)args.get(idx);
                    idx++;

					television.name = (String)args.get(idx);
					idx++;
                    television.input = (Integer)args.get(idx);
                    idx++;
                    television.channel = (Integer)args.get(idx);
                    idx++;
                    television.volume = (Integer)args.get(idx);
                    idx++;

					int buttonsCount = (Integer)args.get(idx);
					idx++;
					for (int j = 0; j < buttonsCount; j++) {
						RemoteControlButton button = new RemoteControlButton();

                        button.name = (String)args.get(idx);
						idx++;
                        int size = (Integer)args.get(idx);
                        idx++;
                        button.signal = new byte[size];
                        for (int k = 0; k < size; k++) {
                            button.signal[k] = (Byte)args.get(idx);
                            idx++;
                        }
						
						television.remoteControl.add(button);
					}
					
					televisions.add(television);
				}
				return televisions;
			}
			return null;
		}
		
		public static ArrayList<String> getMovies(Command command) {
			if (command != null && command.getType() == ECommandType.GetMovies && command.getArguments().size() > 0) {
				ArrayList<Object> args = command.getArguments();
				int idx = 0;
				int numMovies = (Integer)args.get(idx);
				idx++;
				ArrayList<String> movies = new ArrayList<String>(numMovies);
				for (int i = 0; i < numMovies; i++) {
					String movieName = (String)args.get(idx);
					idx++;
                    boolean hasSubtitles = (Byte)args.get(idx) == 1;
                    idx++;
					
					if (hasSubtitles)
						movieName = movieName + " (sub)";
					movies.add(movieName);
				}
				return movies;
			}
			return null;
		}
		
		public static ArrayList<String> getImages(Command command) {
			if (command != null && command.getType() == ECommandType.GetImages && command.getArguments().size() > 0) {
				ArrayList<Object> args = command.getArguments();
				int idx = 0;
				int numImages = (Integer)args.get(idx);
				idx++;
				ArrayList<String> images = new ArrayList<String>(numImages);
				for (int i = 0; i < numImages; i++) {
					String imageRelativePath = (String)args.get(idx);
					idx++;
					
					images.add(imageRelativePath);
				}
				return images;
			}
			return null;
		}
	
	}
	
}
