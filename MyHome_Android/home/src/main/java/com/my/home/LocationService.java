package com.my.home;

import android.app.IntentService;
import android.app.NotificationManager;
import android.app.PendingIntent;
import android.content.BroadcastReceiver;
import android.content.Context;
import android.content.Intent;
import android.content.IntentFilter;
import android.database.Cursor;
import android.location.Location;
import android.location.LocationListener;
import android.location.LocationManager;
import android.net.wifi.WifiManager;
import android.os.BatteryManager;
import android.os.Handler;
import android.os.Looper;
import android.preference.PreferenceManager;
import android.provider.CallLog;
import android.support.v4.app.NotificationCompat;
import android.telephony.SmsManager;
import android.telephony.TelephonyManager;
import android.widget.Toast;

import com.my.home.Activities.SettingsActivity;
import com.my.home.TcpConnection.Client;
import com.my.home.Utils.Utils;

public class LocationService extends IntentService implements LocationListener {

    private Thread thread;
	private boolean wasWifiEnabled;
	private Location lastLocation;
	private int smsCount;
	
	public LocationService() {
		super("LocationService");
	}
	
	@Override
	protected void onHandleIntent(Intent intent) {
        if (this.thread != null && this.thread.isAlive())
            return;
		this.thread = new Thread( new Runnable() {
			@Override
			public void run() {
                WifiManager wifiManager = (WifiManager) LocationService.this.getSystemService(Context.WIFI_SERVICE);
                LocationService.this.wasWifiEnabled = wifiManager.isWifiEnabled();
                if (MainActivity.SharedPreferences == null)
                    MainActivity.SharedPreferences = PreferenceManager.getDefaultSharedPreferences(LocationService.this);
				LocationService.this.lastLocation = null;
				LocationService.this.smsCount = 0;

                boolean enabled = true;
				while (enabled) {
                    enabled = MainActivity.SharedPreferences.getBoolean(SettingsActivity.KEY_PREF_LOCATION_ENABLED, true);

					boolean sending = MainActivity.SharedPreferences.getBoolean(SettingsActivity.KEY_PREF_LOCATION_SENDING, false);
					boolean missedCall = LocationService.this.checkMissedCalls();
					boolean changedSimCard = LocationService.this.checkSIMCard();
					if (missedCall || changedSimCard || sending) {
						// if it's for the first time wait 15 min and then start sending
                        if (sending && LocationService.this.lastLocation == null)
                            LocationService.this.lastLocation = new Location(LocationManager.NETWORK_PROVIDER);
						if (sending && LocationService.this.lastLocation != null) {
							String phoneNumber = MainActivity.SharedPreferences.getString(SettingsActivity.KEY_PREF_LOCATION_PHONE_NUMBER, "");
							String message = "(" + LocationService.this.smsCount + ") ";
							if (missedCall) message = "missed call, ";
							if (changedSimCard) message = "changed sim card, ";
							message += "long: " + LocationService.this.lastLocation.getLongitude();
							message += ", lat: " + LocationService.this.lastLocation.getLatitude();
							message += ", alt: " + LocationService.this.lastLocation.getAltitude();
							message += ", speed: " + LocationService.this.lastLocation.getSpeed();
							message += ", accuracy: " + LocationService.this.lastLocation.getAccuracy();
							//sendSMS(phoneNumber, message); // TODO: test may be set phoneNumber to settings too
							if (missedCall)
								showNotification(phoneNumber, message);

                            Client.log("Message was send (" + phoneNumber + "): " + message);
							LocationService.this.smsCount++;
						}
						else {
							// start getting location
                            LocationService.this.stopLocate();
                            LocationService.this.startLocate();
							
							// change sending settings to true
							MainActivity.SharedPreferences.edit().putBoolean(SettingsActivity.KEY_PREF_LOCATION_SENDING, true).commit();
						}
					}
					else {
						LocationService.this.stopLocate();
						LocationService.this.smsCount = 0;
					}
					
					int sleepTime = Utils.parseInt(MainActivity.SharedPreferences.getString(SettingsActivity.KEY_PREF_LOCATION_PERIOD, ""));
					Utils.Thread.sleep(sleepTime * 60 * 1000); // min
				}
				// when stop the service - stop sending and locate
				LocationService.this.stopLocate();
				MainActivity.SharedPreferences.edit().putBoolean(SettingsActivity.KEY_PREF_LOCATION_SENDING, false).commit();
			}
		});
        this.thread.setName("Location Service Thread");
        this.thread.start();
	}
	
	private boolean checkMissedCalls() {
		final int sleepTime = Utils.parseInt(MainActivity.SharedPreferences.getString(SettingsActivity.KEY_PREF_LOCATION_PERIOD, ""));
		String phoneNumber = MainActivity.SharedPreferences.getString(SettingsActivity.KEY_PREF_LOCATION_PHONE_NUMBER, "");
		
		String[] projection = { CallLog.Calls.CACHED_NAME, CallLog.Calls.NUMBER, CallLog.Calls.DATE };
		String where = CallLog.Calls.TYPE + "=" + CallLog.Calls.MISSED_TYPE + " AND " + CallLog.Calls.NEW + "=1";
		Cursor c = this.getContentResolver().query(CallLog.Calls.CONTENT_URI, projection, where, null, null);
        if (c == null)
            return false;
		c.moveToLast();
		
		int numberColumnIdx = c.getColumnIndex(CallLog.Calls.NUMBER); 
		int dateColumnIdx = c.getColumnIndex(CallLog.Calls.DATE);
		if (numberColumnIdx == -1 || dateColumnIdx == -1)
			return false;
		while(!c.isBeforeFirst()) {
			String number = c.getString(numberColumnIdx);
			long callDate = c.getLong(dateColumnIdx);
			long delta = (System.currentTimeMillis() - callDate);
			if (number.endsWith(phoneNumber) && delta > sleepTime * 60 * 1000) {
                Handler handler = new Handler(Looper.getMainLooper());
                handler.post( new Runnable() {
                    @Override
                    public void run() {
                        Toast.makeText(LocationService.this, "After " + sleepTime + " min will be send sms", Toast.LENGTH_LONG).show();
                    }
                });
				return true;
			}
			c.moveToPrevious();
		}
		return false;
	}
	
	private boolean checkSIMCard() {
		String savedSimSerial = MainActivity.SharedPreferences.getString(SettingsActivity.KEY_PREF_LOCATION_SIM_SERIAL, "");
		
		TelephonyManager telMananger = (TelephonyManager) getSystemService(Context.TELEPHONY_SERVICE);
		String simSerialNumber = telMananger.getSimSerialNumber();
		if (!simSerialNumber.equals(savedSimSerial)) {
			// set new sim serial number to be valid
			MainActivity.SharedPreferences.edit().putString(SettingsActivity.KEY_PREF_LOCATION_SIM_SERIAL, simSerialNumber).commit();
			return true;
		}
		return false;
	}
	
	private void sendSMS(String phoneNumber, String message) {
		SmsManager smsManager = SmsManager.getDefault();
		smsManager.sendTextMessage(phoneNumber, null, message, null, null);
	}
	
	private void showNotification(String phoneNumber, String message) {
		int notifyID = 1;
		
		NotificationCompat.Builder builder = new NotificationCompat.Builder(this);
		builder.setSmallIcon(R.drawable.ic_launcher);
		builder.setContentTitle(phoneNumber);
		builder.setContentText(message);

        Intent intent = new Intent(this, MainActivity.class);
        PendingIntent pendingIntent = PendingIntent.getActivity(this, 0, intent, Intent.FLAG_ACTIVITY_NEW_TASK);
		builder.setContentIntent(pendingIntent);
		
		NotificationManager notificationManager = (NotificationManager) getSystemService(Context.NOTIFICATION_SERVICE);
		notificationManager.notify(notifyID, builder.build());
	}
	
	
	private void startLocate() {
		// start wifi just for case
		WifiManager wifiManager = (WifiManager) this.getSystemService(Context.WIFI_SERVICE);
		this.wasWifiEnabled = wifiManager.isWifiEnabled();
		if (!this.wasWifiEnabled)
			wifiManager.setWifiEnabled(true);
			
		// check battery status
		IntentFilter ifilter = new IntentFilter(Intent.ACTION_BATTERY_CHANGED);
		Intent batteryStatus = this.registerReceiver(null, ifilter);
        if (batteryStatus == null)
            return;
		int level = batteryStatus.getIntExtra(BatteryManager.EXTRA_LEVEL, -1);
		int scale = batteryStatus.getIntExtra(BatteryManager.EXTRA_SCALE, -1);
		final float batteryPct = level / (float)scale;
		
		// start getting location
        final LocationManager locationManager = (LocationManager)this.getSystemService(Context.LOCATION_SERVICE);
        Handler handler = new Handler(Looper.getMainLooper());
        handler.post( new Runnable() {
            @Override
            public void run() {
                if (locationManager.isProviderEnabled(LocationManager.NETWORK_PROVIDER))
                    locationManager.requestLocationUpdates(LocationManager.NETWORK_PROVIDER, 60 * 1000, 0, LocationService.this);
                if (locationManager.isProviderEnabled(LocationManager.GPS_PROVIDER) && batteryPct > 0.2f)
                    locationManager.requestLocationUpdates(LocationManager.GPS_PROVIDER, 60 * 1000, 0, LocationService.this);
            }
        });
        if (locationManager.isProviderEnabled(LocationManager.NETWORK_PROVIDER))
		     this.lastLocation = locationManager.getLastKnownLocation(LocationManager.NETWORK_PROVIDER);
        if (locationManager.isProviderEnabled(LocationManager.GPS_PROVIDER))
		    this.lastLocation = this.getBetterLocation(locationManager.getLastKnownLocation(LocationManager.GPS_PROVIDER), this.lastLocation);
	}
	
	private void stopLocate() {
		// stop wifi
		WifiManager wifiManager = (WifiManager) this.getSystemService(Context.WIFI_SERVICE);
		if (!this.wasWifiEnabled && wifiManager.isWifiEnabled())
			wifiManager.setWifiEnabled(false);
		
		LocationManager locationManager = (LocationManager)this.getSystemService(Context.LOCATION_SERVICE);
		locationManager.removeUpdates(this);
	}

    @Override
    public void onLocationChanged(Location location) {
		this.lastLocation = this.getBetterLocation(location, this.lastLocation);
		if (this.lastLocation == null)
            return;

		if (this.lastLocation.getAccuracy() < 100.0 && this.lastLocation.getSpeed() < 7.0) {
			this.stopLocate();
		}
    }

    @Override
    public void onStatusChanged(java.lang.String s, int i, android.os.Bundle bundle) {
    }

    @Override
    public void onProviderEnabled(java.lang.String s) {
    }

    @Override
    public void onProviderDisabled(java.lang.String s) {
    }
	
	private Location getBetterLocation(Location location, Location currentBestLocation) {
		final int TWO_MINUTES = 2 * 60 * 1000;
		
		if (currentBestLocation == null) {
			// A new location is always better than no location
			return location;
		} else if (location == null) {
            return currentBestLocation;
        }

		// Check whether the new location fix is newer or older
		long timeDelta = location.getTime() - currentBestLocation.getTime();
		boolean isSignificantlyNewer = timeDelta > TWO_MINUTES;
		boolean isSignificantlyOlder = timeDelta < -TWO_MINUTES;
		boolean isNewer = timeDelta > 0;

		// If it's been more than two minutes since the current location, use the new location
		// because the user has likely moved
		if (isSignificantlyNewer) {
			return location;
		// If the new location is more than two minutes older, it must be worse
		} else if (isSignificantlyOlder) {
			return currentBestLocation;
		}

		// Check whether the new location fix is more or less accurate
		int accuracyDelta = (int) (location.getAccuracy() - currentBestLocation.getAccuracy());
		boolean isLessAccurate = accuracyDelta > 0;
		boolean isMoreAccurate = accuracyDelta < 0;
		boolean isSignificantlyLessAccurate = accuracyDelta > 200;

		// Check if the old and new location are from the same provider
		boolean isFromSameProvider = isSameProvider(location.getProvider(),
				currentBestLocation.getProvider());

		// Determine location quality using a combination of timeliness and accuracy
		if (isMoreAccurate) {
			return location;
		} else if (isNewer && !isLessAccurate) {
			return location;
		} else if (isNewer && !isSignificantlyLessAccurate && isFromSameProvider) {
			return location;
		}
		return currentBestLocation;
	}

	// Checks whether two providers are the same
	private boolean isSameProvider(String provider1, String provider2) {
		if (provider1 == null) {
			return provider2 == null;
		}
		return provider1.equals(provider2);
	}
	
	
	public static class Receiver extends BroadcastReceiver {
	
		@Override
		public void onReceive(Context context, Intent intent) {
			Intent newIntent = new Intent(context, LocationService.class);
			context.startService(newIntent);
		}
		
	}
	
}