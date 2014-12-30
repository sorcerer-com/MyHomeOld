using MyHome.TcpConnection;
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

    public interface IService
    {
        EServiceType Type { get; }

        bool IsAvailable(int roomId);
        void Load(XmlDocument xmlDoc);
        void Save(XmlDocument xmlDoc);
        void Update();
        void ReceivedHandler(Server server, Socket handler, Command command);
    }
}
