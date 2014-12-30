using MyHome.TcpConnection;
using MyHome.Utils;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Xml;

namespace MyHome.Services
{
    public static class ServiceManager
    {
        public const string SettingsFileName = "settings.xml";
        
        private static Server server;
        private static Dictionary<EServiceType, Service> services;


        public static void InitializeServices(Server server)
        {
            if (server == null)
                throw new ArgumentNullException("server");

            ServiceManager.server = server;

            ServiceManager.createServices();

            XmlDocument xmlDoc = new XmlDocument();
            if (System.IO.File.Exists(ServiceManager.SettingsFileName))
                xmlDoc.Load(ServiceManager.SettingsFileName);
            foreach (Service service in ServiceManager.services.Values)
            {
                service.Load(xmlDoc);
                ServiceManager.server.CommandReceived += service.ReceivedHandler;
                Logger.Log("ServiceManager", "Load " + service.Type + " Service");
            }

            ServiceManager.server.CommandReceived += ReceivedHandler;
        }

        public static void DeinitializeServices()
        {
            ServiceManager.server.CommandReceived -= ReceivedHandler;

            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.AppendChild(xmlDoc.CreateElement("MyHome"));
            foreach (Service service in ServiceManager.services.Values)
            {
                ServiceManager.server.CommandReceived -= service.ReceivedHandler;
                service.Save(xmlDoc);
                Logger.Log("ServiceManager", "Save " + service.Type + " Service");
            }
            xmlDoc.Save(ServiceManager.SettingsFileName);

            ServiceManager.services.Clear();
            ServiceManager.services = null;
            ServiceManager.server = null;
        }


        public static void AddService(Service service)
        {
            if (service == null)
                throw new ArgumentNullException("service");
            ServiceManager.services.Add(service.Type, service);
        }

        public static Service GetService(EServiceType type)
        {
            if (ServiceManager.services.ContainsKey(type))
                return ServiceManager.services[type];

            return null;
        }

        public static List<EServiceType> GetAvailableServices(int roomId)
        {
            List<EServiceType> availableServices = new List<EServiceType>();
            if (ServiceManager.services == null)
                return availableServices;

            foreach (Service service in ServiceManager.services.Values)
                if (service.IsAvailable(roomId))
                    availableServices.Add(service.Type);
            return availableServices;
        }


        private static void createServices()
        {
            ServiceManager.services = new Dictionary<EServiceType, Service>();

            AddService(new PCControl());
            AddService(new HomeControl());
            AddService(new TVControl());
        }


        public static void ReceivedHandler(Server server, Socket handler, Command command)
        {
            if (command.Type == ECommandType.GetAvailableServices)
            {
                int roomId = HomeControl.InvalidRoomId;
                if (command.Arguments.Count > 0)
                    roomId = (int)command.Arguments[0];
                List<EServiceType> availableServices = ServiceManager.GetAvailableServices(roomId);
                List<object> args = new List<object>();
                args.Add(roomId);
                foreach(EServiceType service in availableServices)
                    args.Add((int)service);

                Command cmd = new Command(ECommandType.GetAvailableServices, args);
                server.Send(handler, cmd);
            }
        }

    }
}
