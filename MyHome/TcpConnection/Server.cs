using MyHome.Utils;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace MyHome.TcpConnection
{

    public class Server : IDisposable
    {
        private Thread threadListener;
        private Thread threadReceiver;
        private bool shouldStop;
        private List<Socket> handlers;

        public delegate void ReceivedHandler(Server server, Socket handler, Command command);
        public event ReceivedHandler CommandReceived;


        public bool IsStarted
        {
            get { return this.threadListener == null ? false : (this.threadListener.IsAlive && !this.shouldStop); }
        }

        public int ClientsCount
        {
            get { return this.handlers.Count; }
        }


        public Server()
        {
            this.threadListener = new Thread(new ThreadStart(doListen));
            this.threadListener.Name = "Server Listener Thread";
            this.threadListener.IsBackground = true;

            this.threadReceiver = new Thread(new ThreadStart(doReceive));
            this.threadReceiver.Name = "Server Receiver Thread";
            this.threadReceiver.IsBackground = true;

            this.shouldStop = false;
            this.handlers = new List<Socket>();
        }

        public void Dispose()
        {
            this.Stop();
            GC.SuppressFinalize(this);
        }


        public void Start()
        {
            this.shouldStop = false;
            this.threadListener.Start();
            this.threadReceiver.Start();
        }

        public void Stop()
        {
            this.shouldStop = true;
        }

        public void Send(Socket handler, Command command)
        {
            try
            {
                byte[] data = command.Serialize().ToArray();
                int bytesSent = handler.Send(data);
                if (data.Length == bytesSent)
                    Logger.Log("Server", "Sended command: " + command.ToString());
                else
                    Logger.Log("Server", "Error: command " + command.Type + " isn't sended");
            }
            catch (Exception e)
            {
                Logger.Log("Server", "Unexpected exception: " + e.ToString());
            }
        }


        private void doListen()
        {
            IPEndPoint localEndPoint = new IPEndPoint(IPAddress.Any, 1100); // TODO: add this to settings
            Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(10);
            }
            catch (Exception e)
            {
                Logger.Log("Server", "Unexpected exception: " + e.ToString());
                System.Windows.MessageBox.Show("Cannot open server port: " + localEndPoint.ToString(), "Server", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }

            while (this.threadListener.IsAlive && !this.shouldStop)
            {
                try
                {
                    Logger.Log("Server", "Waiting for a connection...");
                    Socket handler = listener.Accept();
                    handler.SendTimeout = 1000;
                    handler.ReceiveTimeout = 1000;
                    this.handlers.Add(handler);
                    Logger.Log("Server", "Connection accepted from: " + handler.RemoteEndPoint.ToString());
                        
                }
                catch (Exception e)
                {
                    Logger.Log("Server", "Unexpected exception: " + e.ToString());
                }
            }
            listener.Disconnect(true);
        }

        private void doReceive()
        {
            while (this.threadListener.IsAlive && !this.shouldStop)
            {
                foreach (Socket handler in handlers)
                {
                    // TODO: may be from time to time to drop unactive sockets
                    if (!handler.Connected)// || (handler.Available == 0 && handler.Poll(1000, SelectMode.SelectRead)))
                    {
                        Logger.Log("Server", "Client " + handler.RemoteEndPoint.ToString() + " was disconnected");
                        handler.Shutdown(SocketShutdown.Both);
                        handler.Close();
                        this.handlers.Remove(handler);
                        break;
                    }

                    if (handler.Available < Command.MinBytes)
                        continue;

                    try
                    {
                        List<byte> data = new List<byte>();
                        while (handler.Available > 0)
                        {
                            byte[] bytes = new byte[handler.Available];
                            int bytesRec = handler.Receive(bytes);
                            if (bytesRec > 0)
                            {
                                data.AddRange(bytes);
                                data.RemoveRange(data.Count - (bytes.Length - bytesRec), bytes.Length - bytesRec);
                            }
                            Thread.Sleep(1);
                        }

                        while (data.Count > 0)
                        {
                            Command cmd = new Command();
                            cmd.DeSerialize(data);
                            Logger.Log("Server", "Received command: " + cmd.ToString());

                            this.OnCommandReceived(handler, cmd);
                        }
                    }
                    catch (Exception e)
                    {
                        Logger.Log("Server", "Unexpected exception: " + e.ToString());
                    }
                }
                Thread.Sleep(10);
            }

            foreach (Socket handler in handlers)
                handler.Disconnect(true);
        }


        protected virtual void OnCommandReceived(Socket handler, Command cmd)
        {
            if (this.CommandReceived != null)
                this.CommandReceived(this, handler, cmd);
        }
    
    }
}
