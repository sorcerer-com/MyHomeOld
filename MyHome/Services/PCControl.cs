using MyHome.TcpConnection;
using MyHome.Utils;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Input;
using System.Xml;

namespace MyHome.Services
{
    public class PCControl : Service
    {

        #region Embedded Functions

        // Keyboard
        private const uint KEYEVENTF_EXTENDEDKEY    = 0x0001;
        //private const uint KEYEVENTF_KEYUP        = 0x0002;

        // Mouse
        [StructLayout(LayoutKind.Sequential)]
        private struct Win32Point
        {
            public Int32 X;
            public Int32 Y;
        };

        private const uint MOUSEEVENTF_LEFTDOWN     = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP       = 0x0004;
        private const uint MOUSEEVENTF_RIGHTDOWN    = 0x0008;
        private const uint MOUSEEVENTF_RIGHTUP      = 0x0010;
        private const uint MOUSEEVENTF_MIDDLEDOWN   = 0x0020;
        private const uint MOUSEEVENTF_MIDDLEUP     = 0x0040;
        private const uint MOUSEEVENTF_WHEEL = 0x0800;

        // Camera
        [StructLayout(LayoutKind.Sequential)]
        private struct VIDEOHDR
        {
            public IntPtr lpData;
            public UInt32 dwBufferLength;
            public UInt32 dwBytesUsed;
            public UInt32 dwTimeCaptured;
            public IntPtr dwUser;
            public UInt32 dwFlags;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
            public UIntPtr[] dwReserved;
        }
        
        private const uint WM_USER = 0x0400; // 1024
        private const uint WM_CAP_START = WM_USER;
        private const uint WM_CAP_DRIVER_CONNECT = WM_CAP_START + 10;
        private const uint WM_CAP_DRIVER_DISCONNECT = WM_CAP_START + 11;
        private const uint WM_CAP_FILE_SAVEDIB = WM_CAP_START + 25;
        private const uint WM_CAP_SET_CALLBACK_FRAME = WM_CAP_START + 5;
        private const uint WM_CAP_GRAB_FRAME = WM_CAP_START + 60;
        private const uint WM_CAP_EDIT_COPY = WM_CAP_START + 30;


        // Keyboard functions
        [DllImport("user32.dll")]
        private static extern short VkKeyScan(char ch);

        [DllImport("user32.dll")]
        private static extern void keybd_event(byte bVk, byte bScan, uint dwFlags, uint dwExtraInfo);

        // Mouse functions
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GetCursorPos(ref Win32Point pt);
        
        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int X, int Y);

        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, int dx, int dy, int dwData, uint dwExtraInfo);

        // Camera functions
        [DllImport("avicap32.dll")]
        private static extern int capCreateCaptureWindowA(string lpszWindowName, int dwStyle, int x, int y, 
            int nWidth, int nHeight, int hWndParent, int nID);

        private delegate IntPtr capVideoStreamCallback_t(UIntPtr hWnd, ref VIDEOHDR lpVHdr);
        [DllImport("user32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessage(int hWnd, uint Msg, int wParam, capVideoStreamCallback_t routine);
        
        [DllImport("user32.dll")]
        private static extern bool DestroyWindow(int hWnd);

        #endregion

        #region Static Fuctionality

        // Keyboard functions
        public static void SetKey(Key key)
        {
            int keyCode = KeyInterop.VirtualKeyFromKey(key);
            keybd_event((byte)keyCode, 0, KEYEVENTF_EXTENDEDKEY | 0, 0);
        }

        // Mouse functions
        public static Point GetMousePosition()
        {
            Win32Point w32Mouse = new Win32Point();
            GetCursorPos(ref w32Mouse);
            return new Point(w32Mouse.X, w32Mouse.Y);
        }

        public static void SetMousePosition(Point p)
        {
            SetCursorPos(p.X, p.Y);
        }

        public static void SetMouseButton(MouseButton button, MouseButtonState state)
        {
            uint flag = 0;
            if (button == MouseButton.Left && state == MouseButtonState.Pressed)
                flag = MOUSEEVENTF_LEFTDOWN;
            else if (button == MouseButton.Left && state == MouseButtonState.Released)
                flag = MOUSEEVENTF_LEFTUP;
            if (button == MouseButton.Right && state == MouseButtonState.Pressed)
                flag = MOUSEEVENTF_RIGHTDOWN;
            else if (button == MouseButton.Right && state == MouseButtonState.Released)
                flag = MOUSEEVENTF_RIGHTUP;
            if (button == MouseButton.Middle && state == MouseButtonState.Pressed)
                flag = MOUSEEVENTF_MIDDLEDOWN;
            else if (button == MouseButton.Middle && state == MouseButtonState.Released)
                flag = MOUSEEVENTF_MIDDLEUP;

            mouse_event(flag, 0, 0, 0, 0);
        }

        public static void SetMouseWheel(int delta)
        {
            mouse_event(MOUSEEVENTF_WHEEL, 0, 0, delta, 0);
        }

        // Camera functions
        public static Bitmap GetCameraImage()
        {
            // http://stackoverflow.com/questions/16184659/how-to-copy-image-without-using-the-clipboard
            Bitmap bmp = new Bitmap(640, 480, PixelFormat.Format32bppArgb);
            int hWnd = capCreateCaptureWindowA("ccWebCam", 0, 0, 0, bmp.Width, bmp.Height, 0, 0);

            SendMessage(hWnd, WM_CAP_DRIVER_CONNECT, 0, null);
            Thread.Sleep(500);
            /*SendMessage(hWnd, WM_CAP_SET_CALLBACK_FRAME, 0, (UIntPtr hWnd1, ref VIDEOHDR lpVHdr) =>
            {
                BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.WriteOnly, bmp.PixelFormat);
                byte[] imageTemp = new byte[lpVHdr.dwBufferLength];
                Marshal.Copy(lpVHdr.lpData, imageTemp, 0, (int)lpVHdr.dwBufferLength);
                Marshal.Copy(imageTemp, 0, bmpData.Scan0, imageTemp.Length);
                bmp.UnlockBits(bmpData);

                //if (bmp != null)
                //    bmp.RotateFlip(RotateFlipType.Rotate180FlipNone);

                return IntPtr.Zero;
            });*/
            SendMessage(hWnd, WM_CAP_GRAB_FRAME, 0, null);
            //Thread.Sleep(1500);
            SendMessage(hWnd, WM_CAP_EDIT_COPY, 0, null);
            SendMessage(hWnd, WM_CAP_DRIVER_DISCONNECT, 0, null);

            if (System.Windows.Forms.Clipboard.ContainsImage())
            {
                Image img = System.Windows.Forms.Clipboard.GetImage();
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    g.Clear(Color.White);
                    g.DrawImage(img, 0, 0);
                }
            }

            DestroyWindow(hWnd);

            return bmp;
        }

        #endregion


        public PCControl()
        {
        }


        #region IService

        [XmlIgnore]
        public override EServiceType Type
        {
            get { return EServiceType.PCControl; }
        }


        public override void ReceivedHandler(Server server, Socket handler, Command command)
        {
            if (command.Type == ECommandType.SetKey && command.Arguments.Count == 1)
            {
                char c = char.ConvertFromUtf32((int)command.Arguments[0])[0];
                int keyCode = VkKeyScan(c);
                keybd_event((byte)keyCode, 0, KEYEVENTF_EXTENDEDKEY | 0, 0);
            }
            else if (command.Type == ECommandType.GetMousePosition)
            {
                Point p = PCControl.GetMousePosition();
                Command cmd = new Command(ECommandType.GetMousePosition, new List<object> { (int)p.X, (int)p.Y });
                server.Send(handler, cmd);
            }
            else if (command.Type == ECommandType.SetMousePosition && command.Arguments.Count == 2)
            {
                int x = (int)command.Arguments[0];
                int y = (int)command.Arguments[1];
                PCControl.SetMousePosition(new Point(x, y));
            }
            else if (command.Type == ECommandType.SetMouseButton && command.Arguments.Count == 2)
            {
                MouseButton button = (MouseButton)command.Arguments[0];
                MouseButtonState state = (MouseButtonState)command.Arguments[1];
                PCControl.SetMouseButton(button, state);
            }
            else if (command.Type == ECommandType.SetMouseWheel && command.Arguments.Count == 1)
            {
                int delta = (int)command.Arguments[0];
                PCControl.SetMouseWheel(delta);
            }
            else if (command.Type == ECommandType.GetScreenImage && command.Arguments.Count == 4)
            {
                int x = (int)command.Arguments[0];
                int y = (int)command.Arguments[1];
                int width = (int)command.Arguments[2];
                int height = (int)command.Arguments[3];

                using (var screenBmp = new Bitmap(width, height, PixelFormat.Format32bppArgb))
                {
                    using (var bmpGraphics = Graphics.FromImage(screenBmp))
                    {
                        bmpGraphics.CopyFromScreen(x, y, 0, 0, screenBmp.Size);

                        // send in PNG format
                        Command cmd = new Command(ECommandType.GetScreenImage);
                        using (var ms = new MemoryStream())
                        {
                            screenBmp.Save(ms, ImageFormat.Png);
                            byte[] data = ms.ToArray();
                            cmd.Arguments.Add(data.Length);
                            cmd.Arguments.AddRange(Utils.Utils.Convert(data));
                        }

                        server.Send(handler, cmd);
                    }
                }
            }
            else if (command.Type == ECommandType.GetCameraImage)
            {
                using (var cameraBmp = PCControl.GetCameraImage())
                {
                    // send in PNG format
                    Command cmd = new Command(ECommandType.GetCameraImage);
                    using (var ms = new MemoryStream())
                    {
                        cameraBmp.Save(ms, ImageFormat.Png);
                        byte[] data = ms.ToArray();
                        cmd.Arguments.Add(data.Length);
                        cmd.Arguments.AddRange(Utils.Utils.Convert(data));
                    }

                    server.Send(handler, cmd);
                }
            }
        }

        #endregion

    }
}
