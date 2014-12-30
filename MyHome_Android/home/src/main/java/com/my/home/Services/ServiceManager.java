package com.my.home.Services;

import android.content.Context;
import android.util.SparseArray;

import com.my.home.MainActivity;
import com.my.home.TcpConnection.Client;
import com.my.home.TcpConnection.Command;
import com.my.home.TcpConnection.ECommandType;
import com.my.home.Utils.Utils;
import com.my.home.Utils.XmlSerializer;

import org.w3c.dom.Document;
import org.w3c.dom.Element;
import org.w3c.dom.Node;
import org.w3c.dom.NodeList;

import java.util.ArrayList;
import java.util.EnumMap;

/**
 * Created by Hristo on 14-03-13.
 */
public class ServiceManager {

    private static final String SettingsFileName = "settings.xml";

	private static CommandReceiver commandReceiver;
	private static EnumMap<EServiceType, Service> services;
	private static SparseArray<ArrayList<EServiceType>> availableServices;
	
	
	public static void initializeServices(Context context) {
		ServiceManager.createServices();

        ServiceManager.availableServices = new SparseArray<ArrayList<EServiceType>>();

        Document xmlDoc = XmlSerializer.parseDocument(context, ServiceManager.SettingsFileName);
        ServiceManager.load(context, xmlDoc);
        for(Service service : ServiceManager.services.values()) {
            MainActivity.Client.registerCommandReceivedListener(service);
            service.load(context, xmlDoc);
        }
		
		ServiceManager.commandReceiver = new CommandReceiver();
		MainActivity.Client.registerCommandReceivedListener(commandReceiver);
	}

	public static void deinitializeServices(Context context) {
		MainActivity.Client.unregisterCommandReceivedListener(commandReceiver);
		ServiceManager.commandReceiver = null;

        Document xmlDoc = XmlSerializer.newDocument();
        xmlDoc.appendChild(xmlDoc.createElement("MyHome"));
        ServiceManager.save(context, xmlDoc);
        for(Service service : ServiceManager.services.values()) {
            MainActivity.Client.unregisterCommandReceivedListener(service);
            service.save(context, xmlDoc);
        }
        XmlSerializer.saveDocument(context, ServiceManager.SettingsFileName, xmlDoc);

        ServiceManager.availableServices.clear();
        ServiceManager.availableServices = null;

        ServiceManager.services.clear();
		ServiceManager.services = null;
	}

    public static void getServicesData() {
		ServiceManager.Send.getAvailableServices(HomeControl.InvalidRoomId);
		
        for(Service service : ServiceManager.services.values())
            service.getData();
    }
	

	public static void addService(Service service) {
		if (service == null)
			return;

		ServiceManager.services.put(service.getType(), service);
	}

	public static Service getService(EServiceType type) {
		if (ServiceManager.services.containsKey(type))
			return ServiceManager.services.get(type);

		return null;
	}

	public static ArrayList<EServiceType> getAvailableServices(int roomId) {
		return ServiceManager.availableServices.get(roomId);
	}
	
	
	private static void createServices() {
		ServiceManager.services = new EnumMap<EServiceType, Service>(EServiceType.class);
		
		ServiceManager.addService(new PCControl());
		ServiceManager.addService(new HomeControl());
		ServiceManager.addService(new TVControl());
	}

    private static void load(Context context, Document xmlDoc) {
        if (xmlDoc == null)
            return;

        Node node = xmlDoc.getElementsByTagName(ServiceManager.class.getSimpleName()).item(0);
        Element xmlMain = Utils.as(node, Element.class);

        if (xmlMain != null) {
            node = xmlMain.getElementsByTagName("availableServices").item(0);
            Element xmlRoot = Utils.as(node, Element.class);

            if (xmlRoot != null) {
                NodeList nodes = xmlRoot.getChildNodes();
                for (int i = 0; i < nodes.getLength(); i++) {
                    Element xmlElement = Utils.as(nodes.item(i), Element.class);

                    int roomId = Utils.parseInt(xmlElement.getAttribute("roomId"));
                    String[] services = xmlElement.getAttribute("services").split(",");
                    ArrayList<EServiceType> servicesList = new ArrayList<EServiceType>(services.length);
                    for (String service : services)
                        servicesList.add(EServiceType.values()[Utils.parseInt(service)]);
                    ServiceManager.availableServices.put(roomId, servicesList);
                }
            }
        }
    }

    private static void save(Context context, Document xmlDoc) {
        Element xmlMain = xmlDoc.createElement(ServiceManager.class.getSimpleName());
        xmlDoc.getDocumentElement().appendChild(xmlMain);

        Element xmlRoot = xmlDoc.createElement("availableServices");
        xmlMain.appendChild(xmlRoot);
        SparseArray<ArrayList<EServiceType>> availableServices = ServiceManager.availableServices;
        for (int i = 0; i < availableServices.size(); i++) {
            int roomId = availableServices.keyAt(i);

            Element xmlElement = xmlDoc.createElement("availableService");
            xmlElement.setAttribute("roomId", String.format("%010d", roomId));
            String services = "";
            ArrayList<EServiceType> servicesList = availableServices.get(roomId);
            for(EServiceType type : servicesList)
                services += type.getValue() + ",";
            services = services.substring(0, services.length() - 1);
            xmlElement.setAttribute("services", services);
            xmlRoot.appendChild(xmlElement);
        }
    }
	
	
	private static class CommandReceiver extends Service {
	
		public EServiceType getType() { return EServiceType.Invalid; }
	
		@Override
		public void onCommandReceived(Client client, Command command) {
            if (command.getType() == ECommandType.GetAvailableServices) {
				ArrayList<EServiceType> result = new ArrayList<EServiceType>();
				int roomId = ServiceManager.Receive.getAvailableServices(command, result);
				if (ServiceManager.availableServices.get(roomId) != null)
					ServiceManager.availableServices.remove(roomId);
				ServiceManager.availableServices.put(roomId, result);
                Service.onPropertyChanged(this, "ServiceManager.AvailableServices(" + roomId + ")");
                Service.onPropertyChanged(this, "ServiceManager.AvailableServices");
            }
		}
	
	}
	
	
	
	public static class Send {

		public static void getAvailableServices(int roomId) {
			Command cmd = new Command(ECommandType.GetAvailableServices);
            cmd.getArguments().add(roomId);
			MainActivity.Client.send(cmd);
		}
		
	}
	
	public static class Receive {

		public static int getAvailableServices(Command command, ArrayList<EServiceType> result) {
            if (command != null && command.getType() == ECommandType.GetAvailableServices && result != null) {
				result.clear();
				for (int i = 1; i < command.getArguments().size(); i++) {
					Object o = command.getArguments().get(i);
					EServiceType serviceType = EServiceType.values()[(Integer)o];
					result.add(serviceType);
				}
				return (Integer)command.getArguments().get(0);
            }
			return 0;
		}
		
	}

}
