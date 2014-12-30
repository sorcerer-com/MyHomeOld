using MyHome.TcpConnection;
using MyHome.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;

namespace MyHome.Services
{
    public class TVControl : Service
    {

        #region Static Fuctionality

        public static List<string> VideoExtensions = new List<string> { "mpeg", "mpg", "m2v", "vob", "mp4", "m4v", "3gp", "mov", "wmv", "avi" };
        public static List<string> SubtitlesExtensions = new List<string> { "sub", "srt" };
        public static List<string> ImageExtensions = new List<string> { "bmp", "gif", "jpeg", "jpg", "png", "tiff", "tif" };

        public static List<string> GetVideos(string rootPath, bool recursive)
        {
            List<string> result = new List<string>();

            if (string.IsNullOrEmpty(rootPath) || !Directory.Exists(rootPath))
                return result;

            DirectoryInfo dirInfo = new DirectoryInfo(rootPath);
            List<FileInfo> fileInfos = new List<FileInfo>();

            SearchOption option = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            foreach (string videoExt in TVControl.VideoExtensions)
                fileInfos.AddRange(dirInfo.GetFiles("*." + videoExt, option));

            foreach (FileInfo fileInfo in fileInfos)
                result.Add(fileInfo.FullName);

            return result;
        }

        public static bool HasVideoSubtitles(string videoPath)
        {
            foreach (string subExt in TVControl.SubtitlesExtensions)
            {
                if (File.Exists(Path.ChangeExtension(videoPath, subExt)))
                    return true;
            }
            return false;
        }

        public static List<string> GetImages(string rootPath, bool recursive)
        {
            List<string> result = new List<string>();

            if (string.IsNullOrEmpty(rootPath) || !Directory.Exists(rootPath))
                return result;

            DirectoryInfo dirInfo = new DirectoryInfo(rootPath);
            List<FileInfo> fileInfos = new List<FileInfo>();

            SearchOption option = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            foreach (string imageExt in TVControl.ImageExtensions)
                fileInfos.AddRange(dirInfo.GetFiles("*." + imageExt, option));

            foreach (FileInfo fileInfo in fileInfos)
                result.Add(fileInfo.FullName);

            return result;
        }

        public static List<string> GetDirectories(string rootPath, bool recursive)
        {
            List<string> result = new List<string>();

            if (string.IsNullOrEmpty(rootPath) || !Directory.Exists(rootPath))
                return result;

            DirectoryInfo dirInfo = new DirectoryInfo(rootPath);
            SearchOption option = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;
            DirectoryInfo[] dirInfos = dirInfo.GetDirectories("*", option);

            foreach (DirectoryInfo dirInfo2 in dirInfos)
                result.Add(dirInfo2.FullName + Path.DirectorySeparatorChar);

            return result;
        }

        #endregion

        public class RemoteControlButton
        {
            public string Name { get; set; }
            public byte[] Signal { get; set; } // TODO: how it will be serialized? - add new type and serialize it like "1,2,3"
            // TODO: some kind of training functionality for signals

            public RemoteControlButton()
            {
                this.Name = "";
                this.Signal = new byte[0];
            }

            public RemoteControlButton(string name)
                : this()
            {
                this.Name = name;
                this.Signal = new byte[] { 5, 10, 23 };
            }
        }
        
        // TODO: may be create enum for Input
        public class Television
        {
            [XmlIdentifier]
            public int RoomId { get; set; }
            // TODO: add arduino connection - COM port and output pin (may be somekind of id)

            public string Name { get; set; }
            // TODO: how can we understand what should be the value
            public int Input { get; set; }
            public int Channel { get; set; }
            public int Volume { get; set; }
            public List<RemoteControlButton> RemoteControl { get; private set; }

            public Television()
            {
                this.RoomId = HomeControl.InvalidRoomId;

                this.Name = "Television";
                this.Input = 0;
                this.Channel = 0;
                this.Volume = 0;
                this.RemoteControl = new List<RemoteControlButton>();
            }

            public Television(int roomId, string name)
                : this()
            {
                this.RoomId = roomId;

                this.Name = name;
                this.Input = 0;
                this.Channel = 0;
                this.Volume = 0;
                this.RemoteControl = new List<RemoteControlButton>();

                string[] defaultButtonNames = { "Power", "Source", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "Mute", "Volume+", "Volume-", "Program+", "Program-" };
                foreach(string buttonName in defaultButtonNames)
                    this.RemoteControl.Add(new RemoteControlButton(buttonName));
            }

            public override string ToString()
            {
                return "" + this.RoomId + ": '" + this.Name + "', (" + this.Input + ", " + this.Channel + ", " + this.Volume + "), " + this.RemoteControl.Count + " Buttons";
            }
        }

        
        public List<Television> Televisions { get; private set; }

        public string MoviesRootPath { get; set; }
        public string ImagesRootPath { get; set; }
        

        public TVControl()
        {
            this.Televisions = new List<Television>();
            this.MoviesRootPath = Environment.GetFolderPath(Environment.SpecialFolder.MyVideos);
            this.ImagesRootPath = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        }

        public Television GetTelevision(int roomId)
        {
            foreach (Television television in this.Televisions)
                if (television.RoomId == roomId)
                    return television;
            return null;
        }


        #region IService

        [XmlIgnore]
        public override EServiceType Type
        {
            get { return EServiceType.TVControl; }
        }


        public override bool IsAvailable(int roomId)
        {
            Television television = this.GetTelevision(roomId);
            return roomId != HomeControl.InvalidRoomId && television != null;  // TODO: implement - check is connected arduino is ok
        }

        public override void ReceivedHandler(Server server, Socket handler, Command command)
        {
            if (command.Type == ECommandType.GetTelevisions)
            {
                Command cmd = new Command(ECommandType.GetTelevisions);
                cmd.Arguments.Add(this.Televisions.Count);
                foreach (Television television in this.Televisions)
                {
                    cmd.Arguments.Add(television.RoomId);

                    cmd.Arguments.Add(television.Name);
                    cmd.Arguments.Add(television.Input);
                    cmd.Arguments.Add(television.Channel);
                    cmd.Arguments.Add(television.Volume);

                    cmd.Arguments.Add(television.RemoteControl.Count);
                    foreach (RemoteControlButton button in television.RemoteControl)
                    {
                        cmd.Arguments.Add(button.Name);
                        cmd.Arguments.Add(button.Signal.Length);
                        foreach (byte b in button.Signal)
                            cmd.Arguments.Add(b);
                    }
                }

                server.Send(handler, cmd);
            }
            else if (command.Type == ECommandType.SetRemoteControlButton && command.Arguments.Count == 2)
            {
                int roomId = (int)command.Arguments[0];
                int buttonIdx = (int)command.Arguments[1];

                Television television = this.GetTelevision(roomId);
                if (television != null)
                {
                    RemoteControlButton button = television.RemoteControl[buttonIdx];

                    // TODO: send IR signal to arduino
                }
            }

            else if (command.Type == ECommandType.GetMovies)
            {
                const int maxCount = 100;
                List<string> moviesPaths = TVControl.GetVideos(this.MoviesRootPath, true);

                Command cmd = new Command(ECommandType.GetMovies);
                server.Send(handler, cmd); // send empty command for "start" of transfer
                if (moviesPaths.Count > maxCount)
                    cmd.Arguments.Add(maxCount);
                else
                    cmd.Arguments.Add(moviesPaths.Count);
                foreach (string moviePath in moviesPaths)
                {
                    cmd.Arguments.Add(Path.GetFileNameWithoutExtension(moviePath));
                    bool hasSubtitles = TVControl.HasVideoSubtitles(moviePath);
                    cmd.Arguments.Add((byte)(hasSubtitles ? 1 : 0));

                    if ((cmd.Arguments.Count - 1) / 2 >= maxCount)
                    {
                        server.Send(handler, cmd);
                        cmd.Arguments.Clear();
                        int count = Math.Min(moviesPaths.Count - moviesPaths.IndexOf(moviePath) - 1, maxCount);
                        cmd.Arguments.Add(count);
                    }
                }

                if (cmd.Arguments.Count - 1 > 0)
                    server.Send(handler, cmd);
            }
            else if (command.Type == ECommandType.SetMovie && command.Arguments.Count > 0)
            {
                if ((string)command.Arguments[0] == "open" && command.Arguments.Count == 2) // open command
                {
                    string movieName = (string)command.Arguments[1];
                    List<string> moviesPaths = TVControl.GetVideos(this.MoviesRootPath, true);
                    foreach (string moviePath in moviesPaths)
                    {
                        if (moviePath.Contains("\\" + movieName + "."))
                        {
                            ProcessStartInfo startInfo = new ProcessStartInfo("wmplayer");
                            startInfo.Arguments = "\"" + moviePath + "\" /fullscreen";
                            Process.Start(startInfo);
                            break;
                        }
                    }
                }
                else if ((string)command.Arguments[0] == "play" || (string)command.Arguments[0] == "pause")
                {
                    PCControl.SetKey(System.Windows.Input.Key.MediaPlayPause);
                }
                else if ((string)command.Arguments[0] == "stop")
                {
                    PCControl.SetKey(System.Windows.Input.Key.MediaStop);

                    Process[] processes = Process.GetProcessesByName("wmplayer");
                    foreach (Process process in processes)
                    {
                        if (process.MainWindowTitle == "Windows Media Player")
                        {
                            process.Kill();
                            break;
                        }
                    }
                }
                else if ((string)command.Arguments[0] == "prev")
                {
                    PCControl.SetKey(System.Windows.Input.Key.MediaPreviousTrack);
                }
                else if ((string)command.Arguments[0] == "next")
                {
                    PCControl.SetKey(System.Windows.Input.Key.MediaNextTrack);
                }
            }

            else if (command.Type == ECommandType.GetImages && command.Arguments.Count == 1)
            {
                const int maxCount = 100;
                string path = (string)command.Arguments[0];
                List<string> imagesPaths = TVControl.GetDirectories(this.ImagesRootPath + path, false);
                imagesPaths.AddRange(TVControl.GetImages(this.ImagesRootPath + path, false));

                Command cmd = new Command(ECommandType.GetImages);
                server.Send(handler, cmd); // send empty command for "start" of transfer
                if (imagesPaths.Count > maxCount)
                    cmd.Arguments.Add(maxCount);
                else
                    cmd.Arguments.Add(imagesPaths.Count);
                foreach (string imagePath in imagesPaths)
                {
                    string imageRelativePath = imagePath.Remove(0, this.ImagesRootPath.Length + path.Length - 1);
                    cmd.Arguments.Add(imageRelativePath);

                    if (cmd.Arguments.Count - 1 >= maxCount)
                    {
                        server.Send(handler, cmd);
                        cmd.Arguments.Clear();
                        int count = Math.Min(imagesPaths.Count - imagesPaths.IndexOf(imagePath) - 1, maxCount);
                        cmd.Arguments.Add(count);
                    }

                    /* TODO Thumbnail
                    using (var originalBmp = new System.Drawing.Bitmap(imagePath))
                    {
                        int newWidth = 128, newHeight = 128;
                        if (originalBmp.Width > originalBmp.Height)
                            newHeight = (int)(newWidth * ((float)originalBmp.Height / originalBmp.Width));
                        else
                            newWidth = (int)(newHeight * ((float)originalBmp.Width / originalBmp.Height));
                        using (var thumbnailBmp = new System.Drawing.Bitmap(originalBmp, newWidth, newHeight))
                        {
                            using (var ms = new MemoryStream())
                            {
                                thumbnailBmp.Save(ms, System.Drawing.Imaging.ImageFormat.Png);
                                byte[] data = ms.ToArray();
                                cmd.Arguments.Add(data.Length);
                                cmd.Arguments.AddRange(Utils.Utils.Convert(data));
                            }
                        }
                    }*/
                }

                if (cmd.Arguments.Count - 1 > 0)
                    server.Send(handler, cmd);
            }
            else if (command.Type == ECommandType.SetImage && command.Arguments.Count > 0)
            {
                if ((string)command.Arguments[0] == "open" && command.Arguments.Count == 2) // open command
                {
                    Process[] processes = Process.GetProcessesByName("rundll32");
                    foreach (Process process in processes)
                    {
                        if (process.MainWindowTitle == "Photo Viewer Slide Show" || process.MainWindowTitle.Contains("Windows Photo Viewer"))
                        {
                            process.Kill();
                            break;
                        }
                    }

                    string imageRelativePath = (string)command.Arguments[1];
                    ProcessStartInfo startInfo = new ProcessStartInfo("rundll32");
                    startInfo.Arguments = "\"C:\\Program Files\\Windows Photo Viewer\\PhotoViewer.dll\", ImageView_Fullscreen " + this.ImagesRootPath + imageRelativePath;
                    Process.Start(startInfo);
                    
                    System.Threading.Thread.Sleep(200);
                    PCControl.SetKey(System.Windows.Input.Key.F11); // start slideshow
                }
                else if ((string)command.Arguments[0] == "play")
                {
                    PCControl.SetKey(System.Windows.Input.Key.F11); // start slideshow
                }
                else if ((string)command.Arguments[0] == "pause")
                {
                    PCControl.SetKey(System.Windows.Input.Key.Escape); // stop slideshow
                }
                else if ((string)command.Arguments[0] == "stop")
                {
                    Process[] processes = Process.GetProcessesByName("rundll32");
                    foreach (Process process in processes)
                    {
                        if (process.MainWindowTitle == "Photo Viewer Slide Show" || process.MainWindowTitle.Contains("Windows Photo Viewer"))
                        {
                            process.Kill();
                            break;
                        }
                    }
                }
                else if ((string)command.Arguments[0] == "prev")
                {
                    PCControl.SetKey(System.Windows.Input.Key.Left);
                }
                else if ((string)command.Arguments[0] == "next")
                {
                    PCControl.SetKey(System.Windows.Input.Key.Right);
                }
            }
        }

        #endregion
    }
}
