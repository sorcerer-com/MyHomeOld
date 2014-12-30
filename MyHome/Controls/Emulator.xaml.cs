using MyHome.Services;
using MyHome.TcpConnection;
using MyHome.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MyHome.Controls
{
    /// <summary>
    /// Interaction logic for Emulator.xaml
    /// </summary>
    public partial class Emulator : UserControl, IDisposable
    {
        private Client client;

        public Emulator()
        {
            InitializeComponent();

            this.client = new Client("127.0.0.1", 1100);
            this.client.Connect();
            this.client.CommandReceived += client_CommandReceived;
        }

        public void Dispose()
        {
            this.client.Dispose();
            GC.SuppressFinalize(this);
        }

        
        private void client_CommandReceived(Client client, Command command)
        {
            if (command.Type == ECommandType.GetAvailableServices)
                this.availableServicesReceived(client, command);
            else if (command.Type == ECommandType.GetScreenImage || command.Type == ECommandType.GetCameraImage || 
                command.Type == ECommandType.GetLayout)
                this.imageReceived(client, command);
            else if (command.Type == ECommandType.GetRooms)
                this.roomsReceived(client, command);
            else if (command.Type == ECommandType.GetTelevisions)
                this.televisionsReceived(client, command);
            else if (command.Type == ECommandType.GetMovies)
                this.moviesReceived(client, command);
            else if (command.Type == ECommandType.GetImages)
                this.imagesReceived(client, command);
        }

        private void availableServicesReceived(Client client, Command command)
        {
            string message = "Available services for room: ";
            message += (int)command.Arguments[0] + " - ";
            for (int i = 1; i < command.Arguments.Count; i++)
                message += (EServiceType)command.Arguments[i] + ", ";
            message = message.Substring(0, message.Length - 2);
            Logger.Log("Emulator", message);
        }

        private void imageReceived(Client client, Command command)
        {
            // get in PNG format
            byte[] data = Utils.Utils.Convert(command.Arguments.ToArray(), 1, (int)command.Arguments[0]);
            using (var ms = new MemoryStream(data))
            {
                using (var screenBmp = System.Drawing.Bitmap.FromStream(ms))
                {
                    //screenBmp.Save("1.bmp");
                    this.Dispatcher.InvokeAsync(() =>
                    {
                        System.Windows.Forms.Clipboard.SetImage(screenBmp);

                        Window preview = new Window();
                        preview.Width = screenBmp.Width;
                        preview.Height = screenBmp.Height;
                        preview.Background = new ImageBrush(BitmapFrame.Create(ms));
                        preview.WindowStyle = WindowStyle.ToolWindow;
                        preview.KeyDown += (sender, e) => { if (e.Key == Key.Escape) preview.Close(); };
                        preview.ShowDialog();
                    });
                    Logger.Log("Emulator", "Image was copied to clipboard");
                    Thread.Sleep(100);
                }
            }
        }

        private void roomsReceived(Client client, Command command)
        {
            List<HomeControl.Room> rooms = new List<HomeControl.Room>();
            int idx = 0;
            int numRooms = (int)command.Arguments[idx];
            idx++;
            for (int i = 0; i < numRooms; i++)
            {
                HomeControl.Room room = new HomeControl.Room();

                room.Id = (int)command.Arguments[idx];
                idx++;

                room.Color = System.Drawing.Color.FromArgb((byte)command.Arguments[idx + 3], (byte)command.Arguments[idx + 0], (byte)command.Arguments[idx + 1], (byte)command.Arguments[idx + 2]);
                idx += 4;

                room.Name = (string)command.Arguments[idx];
                idx++;

                int x = (int)command.Arguments[idx];
                idx++;
                int y = (int)command.Arguments[idx];
                idx++;
                room.Min = new System.Drawing.Point(x, y);

                x = (int)command.Arguments[idx];
                idx++;
                y = (int)command.Arguments[idx];
                idx++;
                room.Max = new System.Drawing.Point(x, y);

                rooms.Add(room);
                Logger.Log("Emulator", "Room: " + room);
            }

            foreach (HomeControl.Room room in rooms)
            {
                Command cmd = new Command(ECommandType.GetAvailableServices, new List<object> { room.Id });
                client.Send(cmd);
            }
        }

        private void televisionsReceived(Client client, Command command)
        {
            int idx = 0;
            int numTelevisions = (int)command.Arguments[idx];
            idx++;
            for (int i = 0; i < numTelevisions; i++)
            {
                TVControl.Television television = new TVControl.Television();

                television.Name = (string)command.Arguments[idx];
                idx++;
                television.Input = (int)command.Arguments[idx];
                idx++;
                television.Channel = (int)command.Arguments[idx];
                idx++;
                television.Volume = (int)command.Arguments[idx];
                idx++;

                int buttonsCount = (int)command.Arguments[idx];
                idx++;
                for (int j = 0; j < buttonsCount; j++)
                {
                    string buttonName = (string)command.Arguments[idx];
                    television.RemoteControl.Add(new TVControl.RemoteControlButton(buttonName));
                    idx++;
                }

                Logger.Log("Emulator", "Television: " + television);
            }
        }

        private void moviesReceived(Client client, Command command)
        {
            if (command.Arguments.Count == 0)
            {
                Logger.Log("Emulator", "Clear movies list");
                return;
            }

            int idx = 0;
            int numMovies = (int)command.Arguments[idx];
            idx++;
            for (int i = 0; i < numMovies; i++)
            {
                string movieName = (string)command.Arguments[idx];
                idx++;
                bool hasSubtitles = (byte)command.Arguments[idx] == 1;
                idx++;

                if (hasSubtitles)
                    Logger.Log("Emulator", "Movie: " + movieName + ", hasSubtitles");
                else
                    Logger.Log("Emulator", "Movie: " + movieName);
            }
        }

        private void imagesReceived(Client client, Command command)
        {
            if (command.Arguments.Count == 0)
            {
                Logger.Log("Emulator", "Clear images list");
                return;
            }

            List<string> items = new List<string>();
            int idx = 0;
            int numImages = (int)command.Arguments[idx];
            idx++;
            for (int i = 0; i < numImages; i++)
            {
                string imageRelativePath = (string)command.Arguments[idx];
                idx++;

                Logger.Log("Emulator", "Image: " + imageRelativePath);
                items.Add(imageRelativePath);
            }
        }


        #region Service Manager

        private void getAvailableServicesButton_Click(object sender, RoutedEventArgs e)
        {
            Command cmd = new Command(ECommandType.GetAvailableServices);
            client.Send(cmd);
        }

        #endregion

        #region PC Control

        private void sendKeyButton_Click(object sender, RoutedEventArgs e)
        {
            int unicode = Char.ConvertToUtf32("R", 0);
            Command cmd = new Command(ECommandType.SetKey, new List<object> { unicode });
            client.Send(cmd);
        }

        private void getMousePositionButton_Click(object sender, RoutedEventArgs e)
        {
            Command cmd = new Command(ECommandType.GetMousePosition);
            client.Send(cmd);
        }

        private void setMousePositionButton_Click(object sender, RoutedEventArgs e)
        {
            System.Drawing.Point mousePos = MyHome.Services.PCControl.GetMousePosition();
            Command cmd = new Command(ECommandType.SetMousePosition, new List<object> { mousePos.X + 10, mousePos.Y });
            client.Send(cmd);
        }

        private void setMouseButtonButton_Click(object sender, RoutedEventArgs e)
        {
            Command cmd = new Command(ECommandType.SetMouseButton, new List<object> { (int)MouseButton.Right, (int)MouseButtonState.Pressed });
            client.Send(cmd);
            cmd = new Command(ECommandType.SetMouseButton, new List<object> { (int)MouseButton.Right, (int)MouseButtonState.Released });
            client.Send(cmd);
        }

        private void setMouseWheelButton_Click(object sender, RoutedEventArgs e)
        {
            Command cmd = new Command(ECommandType.SetMouseWheel, new List<object> { (int)500 });
            client.Send(cmd);
        }

        private void getScreenImageButton_Click(object sender, RoutedEventArgs e)
        {
            System.Drawing.Point mousePos = MyHome.Services.PCControl.GetMousePosition();
            Command cmd = new Command(ECommandType.GetScreenImage, new List<object> { mousePos.X - 150, mousePos.Y - 150, 300, 300 });
            client.Send(cmd);
        }

        private void getCameraImageButton_Click(object sender, RoutedEventArgs e)
        {
            Command cmd = new Command(ECommandType.GetCameraImage);
            client.Send(cmd);
        }

        #endregion

        #region Home Control

        private void getLayoutButton_Click(object sender, RoutedEventArgs e)
        {
            Command cmd = new Command(ECommandType.GetLayout);
            client.Send(cmd);
        }

        private void getRoomsButton_Click(object sender, RoutedEventArgs e)
        {
            Command cmd = new Command(ECommandType.GetRooms);
            client.Send(cmd);
        }

        #endregion

        #region TV Control

        private void getTelevisionsButton_Click(object sender, RoutedEventArgs e)
        {
            Command cmd = new Command(ECommandType.GetTelevisions);
            client.Send(cmd);
        }

        private void setRemoteControlButtonButton_Click(object sender, RoutedEventArgs e)
        {
            Command cmd = new Command(ECommandType.SetRemoteControlButton, new List<object> { 0, 0 });
            client.Send(cmd);
        }

        private void getMoviesButton_Click(object sender, RoutedEventArgs e)
        {
            Command cmd = new Command(ECommandType.GetMovies);
            client.Send(cmd);
        }

        private void setMoviesButton_Click(object sender, RoutedEventArgs e)
        {
            TVControl tvControl = ServiceManager.GetService(EServiceType.TVControl) as TVControl;
            string moviePath = TVControl.GetVideos(tvControl.MoviesRootPath, true)[0];
            string movieName = Path.GetFileNameWithoutExtension(moviePath);

            Command cmd = new Command(ECommandType.SetMovie, new List<object> { "open", movieName });
            client.Send(cmd);

            Thread.Sleep(3000);

            cmd = new Command(ECommandType.SetMovie, new List<object> { "pause" });
            client.Send(cmd);

            Thread.Sleep(1000);

            cmd = new Command(ECommandType.SetMovie, new List<object> { "stop" });
            client.Send(cmd);
        }

        private void getImagesButton_Click(object sender, RoutedEventArgs e)
        {
            Command cmd = new Command(ECommandType.GetImages, new List<object> { "\\" });
            client.Send(cmd);
        }

        private void setImagesButton_Click(object sender, RoutedEventArgs e)
        {
            TVControl tvControl = ServiceManager.GetService(EServiceType.TVControl) as TVControl;
            string imagePath = TVControl.GetImages(tvControl.ImagesRootPath, true)[0];
            string imageRelativePath = imagePath.Remove(0, tvControl.ImagesRootPath.Length);

            Command cmd = new Command(ECommandType.SetImage, new List<object> { "open", imageRelativePath });
            client.Send(cmd);

            Thread.Sleep(3000);

            cmd = new Command(ECommandType.SetImage, new List<object> { "pause" });
            client.Send(cmd);

            Thread.Sleep(1000);

            cmd = new Command(ECommandType.SetImage, new List<object> { "next" });
            client.Send(cmd);

            Thread.Sleep(1000);

            cmd = new Command(ECommandType.SetImage, new List<object> { "play" });
            client.Send(cmd);

            Thread.Sleep(1000);

            cmd = new Command(ECommandType.SetImage, new List<object> { "prev" });
            client.Send(cmd);

            Thread.Sleep(1000);

            cmd = new Command(ECommandType.SetImage, new List<object> { "stop" });
            client.Send(cmd);
        }

        #endregion

    }
}
