using MyHome.Utils;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace MyHome.TcpConnection
{
    public class Client : IDisposable
    {
        private string address;
        private int port;

        private Thread thread;
        private Socket socket;

        public delegate void ReceivedHandler(Client client, Command command);
        public event ReceivedHandler CommandReceived;


        public bool IsConnected
        {
            get { return this.socket != null && this.socket.Connected; }
        }


        public Client(string address, int port)
        {
            this.address = address;
            this.port = port;

            this.thread = new Thread(new ThreadStart(doReceive));
            this.thread.Name = "Client Receiver Thread";
            this.thread.IsBackground = true;
            this.thread.Start();

            this.socket = null;
        }

        public void Dispose()
        {
            this.thread.Abort();
            this.Disconnect();
            GC.SuppressFinalize(this);
        }

        
        public void Connect()
        {
            if (this.socket != null && this.socket.Connected)
                return;

            this.socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                IPAddress ipAddress = Utils.Utils.ParseIPAddress(this.address);
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, this.port);

                this.socket.Connect(remoteEP);
                Logger.Log("Client", "Socket connected to " + this.socket.RemoteEndPoint.ToString());
            }
            catch (Exception e)
            {
                Logger.Log("Client", "Unexpected exception: " + e.ToString());
            }
        }

        public void Disconnect()
        {
            if (this.socket == null)
                return;
            try
            {
                this.socket.Shutdown(SocketShutdown.Both);
                this.socket.Close();
                this.socket.Dispose();
                Logger.Log("Client", "Disconnect");
            }
            catch (Exception e)
            {
                Logger.Log("Client", "Unexpected exception: " + e.ToString());
            }
        }


        public bool Send(Command command)
        {
            if (this.socket == null || !this.socket.Connected || this.socket.Poll(1000, SelectMode.SelectRead))
            {
                this.Connect();
                if (this.socket == null || !this.socket.Connected)
                    return false;
            }

            try
            {
                byte[] bytes = command.Serialize().ToArray();
                this.socket.Send(bytes);
                Logger.Log("Client", "Send command: " + command.ToString());
            }
            catch (Exception e)
            {
                Logger.Log("Client", "Unexpected exception: " + e.ToString());
            }
            return true;
        }

        private void doReceive()
        {
            while (this.thread.IsAlive)
            {
                if (this.socket == null || !this.socket.Connected || this.socket.Available < Command.MinBytes)
                {
                    Thread.Sleep(100);
                    continue;
                }

                try
                {
                    List<byte> data = new List<byte>();
                    while (this.socket.Available > 0)
                    {
                        byte[] bytes = new byte[this.socket.Available];
                        int bytesRec = this.socket.Receive(bytes);
                        if (bytesRec > 0)
                        {
                            data.AddRange(bytes);
                            data.RemoveRange(data.Count - (bytes.Length - bytesRec), bytes.Length - bytesRec);
                        }
                        Thread.Sleep(100); // to be equivalent with android version
                    }
                        
                    while (data.Count > 0)
                    {
                        Command cmd = new Command();
                        cmd.DeSerialize(data);
                        Logger.Log("Client", "Received command: " + cmd.ToString());

                        this.OnCommandReceived(cmd);
                    }
                }
                catch (Exception e)
                {
                    Logger.Log("Client", "Unexpected exception: " + e.ToString());
                }
            }
        }


        protected virtual void OnCommandReceived(Command cmd)
        {
            if (this.CommandReceived != null)
                this.CommandReceived(this, cmd);
        }

    }
}
