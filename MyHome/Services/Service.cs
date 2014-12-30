using MyHome.TcpConnection;
using MyHome.Utils;
using System.Net.Sockets;
using System.Xml;

namespace MyHome.Services
{
    public enum EServiceType
    {
        Invalid = 0,
        PCControl = 1,
        HomeControl = 2,
        TVControl = 3
    }

    public abstract class Service
    {
        public abstract EServiceType Type { get; }


        public virtual bool IsAvailable(int roomId)
        {
            return true;
        }
        
        public virtual void Load(XmlDocument xmlDoc)
        {
            XmlSerializer.Deserialize(xmlDoc, this);
        }
        
        public virtual void Save(XmlDocument xmlDoc)
        {
            XmlSerializer.Serialize(xmlDoc, this);
        }

        public virtual void Update()
        {
        }

        public abstract void ReceivedHandler(Server server, Socket handler, Command command);
    }
}
