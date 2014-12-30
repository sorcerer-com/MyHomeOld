using MyHome.TcpConnection;
using MyHome.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Sockets;
using System.Xml;

namespace MyHome.Services
{
    public class HomeControl : Service
    {
        public const string LayoutFileName = "layout.png";
        public const int InvalidRoomId = 0;

        public class Room
        {
            [XmlIdentifier]
            public int Id { get; set; }
            public Color Color { get; set; }
            public string Name { get; set; }
            [XmlIgnore]
            public Point Min { get; set; }
            [XmlIgnore]
            public Point Max { get; set; }
            [XmlIgnore]
            public List<EServiceType> Services { get { return ServiceManager.GetAvailableServices(this.Id); } }

            public Room()
            {
                this.Id = HomeControl.InvalidRoomId;
                this.Color = Color.Black;
                this.Name = "";
                this.Min = new Point(int.MaxValue, int.MaxValue);
                this.Max = new Point(int.MinValue, int.MinValue);
            }

            public Room(Color color, string name)
                : this()
            {
                this.Id = (int)(DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond);
                this.Color = color;
                this.Name = name;
            }

            public override string ToString()
            {
                return "" + this.Id + ": " + this.Color + ", '" + this.Name + "', (" + this.Min + "), (" + this.Max + ")";
            }
        }


        [XmlIgnore]
        public Bitmap Layout { get; private set; }
        public List<Room> Rooms { get; private set; }


        public HomeControl()
        {
            this.ReloadLayout();
        }


        public void ReloadLayout()
        {
            this.Layout = new Bitmap(960, 540);
            if (File.Exists(HomeControl.LayoutFileName))
            {
                using (var stream = new FileStream(HomeControl.LayoutFileName, FileMode.Open))
                {
                    this.Layout = new Bitmap(stream);
                }
            }

            this.Rooms = new List<Room>();
            this.Rooms.Add(new Room(Color.Black, "Walls"));
            this.Rooms.Add(new Room(Color.White, "Doors"));
            this.Rooms.Add(new Room(Color.White, "Windows"));
            foreach (Room room in this.Rooms)
                room.Id = HomeControl.InvalidRoomId;
        }

        public void UpdateRooms()
        {
            if (this.Rooms == null || this.Layout == null)
                return;

            foreach (Room room in this.Rooms)
            {
                room.Min = new Point(this.Layout.Width, this.Layout.Height);
                room.Max = new Point(0, 0);
            }

            for (int j = 0; j < this.Layout.Height; j++)
            {
                for (int i = 0; i < this.Layout.Width; i++)
                {
                    Color color = this.Layout.GetPixel(i, j);
                    if (color.A == 0)
                        continue;

                    bool found = false;
                    foreach (Room room in this.Rooms)
                    {
                        if (color.ToArgb() == room.Color.ToArgb())
                        {
                            Point min = new Point();
                            min.X = Math.Min(room.Min.X, i);
                            min.Y = Math.Min(room.Min.Y, j);
                            Point max = new Point();
                            max.X = Math.Max(room.Max.X, i);
                            max.Y = Math.Max(room.Max.Y, j);

                            room.Min = min;
                            room.Max = max;

                            found = true;
                        }
                    }

                    if (!found)
                    {
                        HomeControl.Room room = new HomeControl.Room(color, "Room" + this.Rooms.Count);
                        room.Min = new Point(i, j);
                        room.Max = new Point(i, j);
                        this.Rooms.Add(room);
                        System.Threading.Thread.Sleep(100);
                    }
                }
            }
        }

        public Room GetRoom(int roomId)
        {
            foreach (Room room in this.Rooms)
                if (room.Id == roomId)
                    return room;
            return null;
        }


        #region IService

        [XmlIgnore]
        public override EServiceType Type
        {
            get { return EServiceType.HomeControl; }
        }

        
        public override void Load(XmlDocument xmlDoc)
        {
            base.Load(xmlDoc);

            this.UpdateRooms();
        }

        public override void ReceivedHandler(Server server, Socket handler, Command command)
        {
            if (command.Type == ECommandType.GetLayout)
            {
                // send in PNG format
                Command cmd = new Command(ECommandType.GetLayout);
                using (var ms = new MemoryStream())
                {
                    this.Layout.Save(ms, ImageFormat.Png);
                    byte[] data = ms.ToArray();
                    cmd.Arguments.Add(data.Length);
                    cmd.Arguments.AddRange(Utils.Utils.Convert(data));
                }

                server.Send(handler, cmd);
            }
            else if (command.Type == ECommandType.GetRooms)
            {
                Command cmd = new Command(ECommandType.GetRooms);
                cmd.Arguments.Add(this.Rooms.Count);
                foreach (Room room in this.Rooms)
                {
                    cmd.Arguments.Add(room.Id);

                    cmd.Arguments.Add(room.Color.R);
                    cmd.Arguments.Add(room.Color.G);
                    cmd.Arguments.Add(room.Color.B);
                    cmd.Arguments.Add(room.Color.A);

                    cmd.Arguments.Add(room.Name);

                    cmd.Arguments.Add((int)room.Min.X);
                    cmd.Arguments.Add((int)room.Min.Y);

                    cmd.Arguments.Add((int)room.Max.X);
                    cmd.Arguments.Add((int)room.Max.Y);
                }

                server.Send(handler, cmd);
            }
        }

        #endregion

    }
}
