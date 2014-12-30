package com.my.home.Services;

import android.content.Context;
import android.os.Handler;
import android.os.Looper;

import com.my.home.TcpConnection.Client;
import com.my.home.Utils.XmlSerializer;

import org.w3c.dom.Document;

import java.util.ArrayList;

/**
 * Created by Hristo on 14-03-13.
 */
public abstract class Service implements Client.ReceivedListener {

    public interface PropertyChangedListener {
        public void onPropertyChanged(Service service, String property);
    }

    private static ArrayList<PropertyChangedListener> propertyChangedListeners = new ArrayList<PropertyChangedListener>();
    public static void registerPropertyChangedListener(PropertyChangedListener listener) {
		if (!Service.propertyChangedListeners.contains(listener))
			Service.propertyChangedListeners.add(listener);
    }

    public static void unregisterPropertyChangedListener(PropertyChangedListener listener) {
        if (Service.propertyChangedListeners.contains(listener))
            Service.propertyChangedListeners.remove(listener);
    }

    protected static void onPropertyChanged(final Service service, final String property) {
		Handler handler = new Handler(Looper.getMainLooper());
        handler.post( new Runnable() {
            @Override
            public void run() {
				for (PropertyChangedListener listener : Service.propertyChangedListeners)
					listener.onPropertyChanged(service, property);
            }
        });
    }

    public static void onPropertyChanged(final EServiceType serviceType, final String property) {
        final Service service = ServiceManager.getService(serviceType);
        Service.onPropertyChanged(service, property);
    }

		
	public abstract EServiceType getType();
	public boolean getIsAvailable(int roomId) { return ServiceManager.getAvailableServices(roomId).contains(this.getType()); }

    public void load(Context context, Document xmlDoc) {
        if (xmlDoc == null)
            return;

        try {
            XmlSerializer.deserialize(xmlDoc, this);
        } catch (Exception e) {
            Client.log("Exception: " + e.getMessage());
        }
    }

    public void save(Context context, Document xmlDoc) {
        try {
            XmlSerializer.serialize(xmlDoc, this);
        } catch (Exception e) {
            Client.log("Exception: " + e.getMessage());
        }
    }

    public void getData() { }

}
