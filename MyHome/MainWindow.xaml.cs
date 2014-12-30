using MyHome.Controls;
using MyHome.Services;
using MyHome.TcpConnection;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Threading;

namespace MyHome
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private System.Windows.Forms.NotifyIcon notifyIcon;

        private Server server;


        public MainWindow()
        {
            InitializeComponent();
            this.Cursor = System.Windows.Input.Cursors.Wait;
            this.Title = "My Home - Loading...";

            DispatcherTimer timer = new DispatcherTimer();
            timer.Interval = new TimeSpan(0, 0, 1);
            timer.Tick += timer_Tick;

            this.setupNotifyIcon();

            this.server = new Server();
            (new System.Threading.Thread(() =>
                {
                    ServiceManager.InitializeServices(server);
                    this.server.Start();
                    this.Dispatcher.Invoke(() => { this.Cursor = null; this.timer_Tick(null, null); });
                    timer.Start();
                })).Start();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            this.Title = "My Home";
            this.Title += " - ";
            this.Title += this.server.ClientsCount + " Clients";
            this.notifyIcon.Text = this.Title;
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // wait to finish initialization
            while (!this.server.IsStarted) { System.Threading.Thread.Sleep(100); }

            this.server.Stop();

            ServiceManager.DeinitializeServices();

            this.notifyIcon.Dispose();
        }

        private void Window_StateChanged(object sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Minimized)
                this.Hide();
        }

        
        private bool openControl(Type controlType)
        {
            if (this.contentControl.Content != null && this.contentControl.Content.GetType() == controlType)
            {
                Label label = new Label();
                label.Content = "My Home";
                label.VerticalContentAlignment = VerticalAlignment.Center;
                label.HorizontalContentAlignment = HorizontalAlignment.Center;
                label.FontSize = 60;
                label.FontStyle = FontStyles.Italic;
                label.FontWeight = FontWeights.Bold;

                this.contentControl.Content = label;
                return false;
            }

            this.contentControl.Content = Activator.CreateInstance(controlType);
            return true;
        }


        private void openControlButton_Click(object sender, RoutedEventArgs e)
        {
            if (!this.server.IsStarted)
            {
                ToggleButton button = sender as ToggleButton;
                button.IsChecked = false;
                return;
            }

            Type type = null;
            if (sender == this.emulatorButton)
            {
                Window window = new Window();
                window.Owner = this;
                window.Width = 640;
                window.Height = 480;
                window.Content = new Emulator();
                window.Title = "Emulator";
                window.Icon = this.Icon;
                window.Show();
                window.Closing += (object sender2, System.ComponentModel.CancelEventArgs e2) =>
                    {
                        (window.Content as Emulator).Dispose();
                    };
            }
            else if (sender == this.logButton)
                type = typeof(Log);
            else if (sender == this.homeButton)
                type = typeof(Home);
            else if (sender == this.televisionButton)
                type = typeof(Television);

            if (type == null || !this.openControl(type))
            {
                ToggleButton button = sender as ToggleButton;
                button.IsChecked = false;
            }

        }


        private void setupNotifyIcon()
        {
            System.IO.Stream stream = Application.GetResourceStream(new Uri("pack://application:,,,/MyHome;component/Images/Main.ico")).Stream;
            this.notifyIcon = new System.Windows.Forms.NotifyIcon();
            this.notifyIcon.Icon = new System.Drawing.Icon(stream);
            this.notifyIcon.Visible = true;
            this.notifyIcon.DoubleClick += (object sender, EventArgs args) =>
            {
                if (this.IsVisible)
                {
                    this.Hide();
                    foreach (Window window in this.OwnedWindows)
                        window.Hide();
                }
                else
                {
                    this.Show();
                    foreach (Window window in this.OwnedWindows)
                        window.Show();
                    this.WindowState = WindowState.Normal;
                    this.Activate();
                }
            };
        }

    }
}
