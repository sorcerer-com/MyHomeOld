package com.my.home.Activities;

import android.content.Intent;
import android.content.SharedPreferences;
import android.os.Bundle;
import android.preference.Preference;
import android.preference.PreferenceActivity;
import android.view.MenuItem;

import com.my.home.LocationService;
import com.my.home.MainActivity;
import com.my.home.R;
import com.my.home.Utils.Utils;

import java.util.Set;

public class SettingsActivity extends PreferenceActivity implements SharedPreferences.OnSharedPreferenceChangeListener {

    public static final String KEY_PREF_SERVER_ADDRESS 			= "pref_server_address";
    public static final String KEY_PREF_SERVER_PORT 			= "pref_server_port";

    public static final String KEY_PREF_LOCATION_ENABLED 		= "pref_location_enabled";
    public static final String KEY_PREF_LOCATION_SENDING 		= "pref_location_sending";
    public static final String KEY_PREF_LOCATION_PERIOD 		= "pref_location_period";
    public static final String KEY_PREF_LOCATION_PHONE_NUMBER	= "pref_location_phone_number";
    public static final String KEY_PREF_LOCATION_SIM_SERIAL		= "pref_location_sim_serial";

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        //setContentView(R.layout.activity_settings);
        addPreferencesFromResource(R.xml.preferences);
    }

    @Override
    protected void onResume() {
        super.onResume();
        // Set up a listener whenever a key changes
        getPreferenceScreen().getSharedPreferences()
                .registerOnSharedPreferenceChangeListener(this);
    }

    @Override
    protected void onPause() {
        super.onPause();
        // Unregister the listener whenever a key changes
        getPreferenceScreen().getSharedPreferences()
                .unregisterOnSharedPreferenceChangeListener(this);
    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        switch (item.getItemId()) {
            case android.R.id.home:
                onBackPressed();
                return true;
        }

        return super.onOptionsItemSelected(item);
    }

	@Override
	public void onSharedPreferenceChanged(SharedPreferences sharedPreferences, String key) {
		// reconnect if client if address or port of the server is changed
        if (key.equals(SettingsActivity.KEY_PREF_SERVER_ADDRESS) || key.equals(SettingsActivity.KEY_PREF_SERVER_PORT)) {
			String serverAddress = sharedPreferences.getString(SettingsActivity.KEY_PREF_SERVER_ADDRESS, "");
			int serverPort = Utils.parseInt(sharedPreferences.getString(SettingsActivity.KEY_PREF_SERVER_PORT, ""));
		
			MainActivity.Client.disconnect();
			Utils.Thread.sleep(100);
            MainActivity.Client.setAddress(serverAddress);
            MainActivity.Client.setPort(serverPort);
            MainActivity.Client.connect();
        }
        else if (key.equals(SettingsActivity.KEY_PREF_LOCATION_ENABLED) && sharedPreferences.getBoolean(SettingsActivity.KEY_PREF_LOCATION_ENABLED, true)) {
            Intent newIntent = new Intent(this, LocationService.class);
            this.startService(newIntent);
        }
    }
}
