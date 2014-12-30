package com.my.home;

import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.content.SharedPreferences;
import android.graphics.Bitmap;
import android.graphics.ColorMatrix;
import android.graphics.ColorMatrixColorFilter;
import android.graphics.Matrix;
import android.graphics.Rect;
import android.net.wifi.WifiManager;
import android.os.Bundle;
import android.preference.PreferenceManager;
import android.view.GestureDetector;
import android.view.MotionEvent;
import android.view.View;
import android.widget.ImageView;
import android.widget.LinearLayout;
import android.widget.TextView;

import com.my.home.Activities.SettingsActivity;
import com.my.home.Services.EServiceType;
import com.my.home.Services.HomeControl;
import com.my.home.Services.Service;
import com.my.home.Services.ServiceManager;
import com.my.home.TcpConnection.Client;
import com.my.home.Utils.Animation;
import com.my.home.Utils.Utils;
import com.my.home.Utils.WorkerThread;

import java.util.ArrayList;

public class MainActivity extends Activity implements View.OnTouchListener, Client.IsConnectedListener, Service.PropertyChangedListener {

    public static SharedPreferences SharedPreferences;
    public static Client Client;
    public static HomeControl.Room SelectedRoom;
	
	private boolean wasWifiEnabled;
	private Thread reconnectionThread;
    private GestureDetector gestureDetector;
	
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

		// TODO: is there new values in Server side - command, service types

        // TODO: add connection icon to every activities

        PreferenceManager.setDefaultValues(this, R.xml.preferences, false);
        MainActivity.SharedPreferences = PreferenceManager.getDefaultSharedPreferences(this);

		WifiManager wifiManager = (WifiManager) this.getSystemService(Context.WIFI_SERVICE);
        this.wasWifiEnabled = wifiManager.isWifiEnabled();
		if (!this.wasWifiEnabled)
			wifiManager.setWifiEnabled(true);

        if (MainActivity.Client == null) {
            String serverAddress = MainActivity.SharedPreferences.getString(SettingsActivity.KEY_PREF_SERVER_ADDRESS, "");
            int serverPort = Utils.parseInt(MainActivity.SharedPreferences.getString(SettingsActivity.KEY_PREF_SERVER_PORT, ""));
            
			MainActivity.Client = new Client(serverAddress,serverPort);
			MainActivity.Client.registerIsConnectedListener(this);
			MainActivity.Client.connect();

            ServiceManager.initializeServices(this);
            Service.registerPropertyChangedListener(this);

            // show saved layout
            Service.onPropertyChanged(EServiceType.HomeControl, "HomeControl.Rooms");
        }

        this.gestureDetector = new GestureDetector(this, new TouchGestureDetector());
    }

    @Override
    protected void onDestroy() {
		// TODO: add onOptionsItemSelected on every activity for home button, to fix call this on activity change
		// TODO: orientation changed problem on new androids
        if (this.isFinishing()) {
			Service.unregisterPropertyChangedListener(this);
			ServiceManager.deinitializeServices(this);
			
			WorkerThread.stop();
			Utils.Thread.join(this.reconnectionThread);
			
            MainActivity.Client.unregisterIsConnectedListener(this);
            MainActivity.Client.finalize();
			MainActivity.Client = null;
			
			// stop wifi
			WifiManager wifiManager = (WifiManager) this.getSystemService(Context.WIFI_SERVICE);
			if (!this.wasWifiEnabled && wifiManager.isWifiEnabled())
				wifiManager.setWifiEnabled(false);
        }
        super.onDestroy();
    }


    public void showMenuImageView_Click(View view) {
        final LinearLayout menuLinearLayout = (LinearLayout) findViewById(R.id.main_menu_linearlayout);
        final ImageView buttonImageView = (ImageView) view;
		final boolean open = menuLinearLayout.getVisibility() == View.GONE;

		if (open)
			buttonImageView.setImageResource(R.drawable.ic_main_minus);
		else
			buttonImageView.setImageResource(R.drawable.ic_main_plus);
			
		Animation.openMenu(menuLinearLayout, open, 100);
    }

    public void menuItemImageView_Click(View view) {
		Object tag = view.getTag();
		if (!isServiceAvailable(tag))
			return;
        Intent intent = new Intent();
        intent.setClassName(this, "com.my.home.Activities." + tag + "Activity");
        startActivity(intent);
    }
	
    @Override
    public boolean onTouch(View view, MotionEvent motionEvent) {
        gestureDetector.onTouchEvent(motionEvent);
		return true;
	}

    private class TouchGestureDetector extends GestureDetector.SimpleOnGestureListener {
        @Override
        public boolean onDoubleTap(MotionEvent motionEvent) {
            ImageView imageView = (ImageView)findViewById(R.id.main_imageview);

            float[] eventXY = new float[] { motionEvent.getX(), motionEvent.getY() };
            Matrix invertMatrix = new Matrix();
            imageView.getImageMatrix().invert(invertMatrix);
            invertMatrix.mapPoints(eventXY);

            Bitmap bitmap = Utils.getBitmap(imageView);
            if (eventXY[0] >= 0 && eventXY[0] < bitmap.getWidth() &&
                eventXY[1] >= 0 && eventXY[1] < bitmap.getHeight()) {
                int color = bitmap.getPixel((int)eventXY[0], (int)eventXY[1]);

                HomeControl homeControl = Utils.as(ServiceManager.getService(EServiceType.HomeControl), HomeControl.class);
                if (homeControl != null) {
                    ArrayList<HomeControl.Room> rooms = homeControl.getRooms();
                    for (HomeControl.Room room : rooms) {
                        if (room.color == color) {
							MainActivity.this.selectRoom(room);
                            break;
                        }
                    }
                }
            }
            return true;
        }
    }
	

    @Override
    public void onIsConnectedChanged(boolean isConnected) {
		int resId = isConnected ? R.drawable.ic_connected : R.drawable.ic_disconnected;
        final ImageView statusImageView = (ImageView) findViewById(R.id.main_statusbar_imageview);
        statusImageView.setImageResource(resId);

		if (!isConnected && (this.reconnectionThread == null || !this.reconnectionThread.isAlive())) {		
			this.reconnectionThread = new Thread( new Runnable() {
				@Override
				public void run() {
					while(!MainActivity.Client.isConnected() && WorkerThread.isAlive()) {
                        MainActivity.Client.connect();
						Animation.fade(statusImageView, 1000, true);
						Utils.Thread.sleep(1500);
						Animation.fade(statusImageView, 1000, false);
						Utils.Thread.sleep(1500);
					}
				}
			});
			this.reconnectionThread.setName("Reconnection Thread");
			this.reconnectionThread.start();
		}
		else if (isConnected) {
            ServiceManager.getServicesData();

			TextView statusbarTextView = (TextView)findViewById(R.id.main_statusbar_textview);
            statusbarTextView.setText("Loading.");
			Animation.switchText(statusbarTextView, 500, "Loading.", "Loading..", "Loading...");
		}
    }

    @Override
    public void onPropertyChanged(Service service, String property) {
		final HomeControl homeControl = Utils.as(service, HomeControl.class);
        if (homeControl == null)
            return;
		
		if (property.equals("HomeControl.Rooms")) {
			ImageView imageView = (ImageView)findViewById(R.id.main_imageview);
			imageView.setImageBitmap(homeControl.getLayout());
			ColorMatrix cm = new ColorMatrix();
			cm.setSaturation(0);
			ColorMatrixColorFilter filter = new ColorMatrixColorFilter(cm);
			imageView.setColorFilter(filter);
            imageView.setAlpha(0);
			Animation.fade(imageView, 1000, false);
			imageView.setOnTouchListener(this);
			
			TextView statusbarTextView = (TextView)findViewById(R.id.main_statusbar_textview);
            statusbarTextView.setText("");

            if (MainActivity.SelectedRoom != null)
                this.selectRoom(MainActivity.SelectedRoom);
		}
	}
	
	
	private boolean selectRoom(HomeControl.Room room) {
		HomeControl homeControl = Utils.as(ServiceManager.getService(EServiceType.HomeControl), HomeControl.class);
		if (homeControl == null)
			return false;
		
		// TODO: what color will use the selected room
		// TODO: may be add label over room with the name when the hole apartment is shown
		// TODO: may be make interface to expand like (IronMan), when click to give more information
		// TODO: may be combine some of the service like TVControl and MusicControl to Entertaiment - with submenu
		TextView statusbarTextView = (TextView)findViewById(R.id.main_statusbar_textview);
		Rect lastRect = new Rect(0, 0, homeControl.getLayout().getWidth(), homeControl.getLayout().getHeight());
		if (MainActivity.SelectedRoom != null)
			lastRect = new Rect((int)(MainActivity.SelectedRoom.min.x * 0.7), (int)(MainActivity.SelectedRoom.min.y * 0.7),
					(int)(MainActivity.SelectedRoom.max.x * 1.3), (int)(MainActivity.SelectedRoom.max.y * 1.3));

		Rect rect = new Rect(0, 0, homeControl.getLayout().getWidth(), homeControl.getLayout().getHeight());
		if (room.id != 0 && room != MainActivity.SelectedRoom) {
			rect = new Rect((int)(room.min.x * 0.7), (int)(room.min.y * 0.7), (int)(room.max.x * 1.3), (int)(room.max.y * 1.3));
			MainActivity.SelectedRoom = room;
			statusbarTextView.setText(room.name);
		}
		else {
			MainActivity.SelectedRoom = null;
			statusbarTextView.setText("");
		}
		
		LinearLayout menuLinearLayout = (LinearLayout) findViewById(R.id.main_menu_linearlayout);
		if (menuLinearLayout.getVisibility() == View.VISIBLE)
			Animation.openMenu(menuLinearLayout, true, 100);

		ImageView imageView = (ImageView)findViewById(R.id.main_imageview);
		Animation.zoom(imageView, homeControl.getLayout(), 500, lastRect, rect);
        return true;
	}

	
	public static boolean isServiceAvailable(Object obj) {
		if (obj == null || obj.toString().isEmpty())
			return true;
		String service = obj.toString();

		EServiceType serviceType = EServiceType.Invalid;
		for (EServiceType type : EServiceType.values()) {
			if (type.name().equals(service)) {
				serviceType = type;
				break;
			}
		}

        if (serviceType == EServiceType.Invalid) // if isn't a service
            return true;

        ArrayList<EServiceType> availableServices = new ArrayList<EServiceType>();
        if (MainActivity.SelectedRoom != null && ServiceManager.getAvailableServices(MainActivity.SelectedRoom.id) != null) // selected room services
            availableServices.addAll(ServiceManager.getAvailableServices(MainActivity.SelectedRoom.id));
        else if (ServiceManager.getAvailableServices(HomeControl.InvalidRoomId) != null) // global services
            availableServices.addAll(ServiceManager.getAvailableServices(HomeControl.InvalidRoomId));
		
		if (availableServices.contains(serviceType))
			return true;
		return false;
	}

}
