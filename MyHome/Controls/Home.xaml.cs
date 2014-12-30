using Microsoft.Win32;
using MyHome.Services;
using MyHome.Utils;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace MyHome.Controls
{
    /// <summary>
    /// Interaction logic for Home.xaml
    /// </summary>
    public partial class Home : UserControl, INotifyPropertyChanged
    {
        private HomeControl homeControl;

        public ImageSource Layout
        {
            get
            {
                if (this.SelectedRoom == null || this.SelectedRoom.Id == HomeControl.InvalidRoomId)
                    return Utils.Utils.Convert(this.homeControl.Layout);

                System.Drawing.Bitmap bmp = Utils.Utils.Grayscale(this.homeControl.Layout, 0.7);
                for (int j = (int)this.SelectedRoom.Min.Y; j < (int)this.SelectedRoom.Max.Y; j++)
                    for (int i = (int)this.SelectedRoom.Min.X; i < (int)this.SelectedRoom.Max.X; i++)
                    {
                        System.Drawing.Color c = this.homeControl.Layout.GetPixel(i, j);
                        if (c.ToArgb() == this.SelectedRoom.Color.ToArgb())
                            bmp.SetPixel(i, j, c);
                    }
                return Utils.Utils.Convert(bmp);
            }
        }

        public ObservableCollection<HomeControl.Room> Rooms
        {
            get
            {
                return new ObservableCollection<HomeControl.Room>(this.homeControl.Rooms);
            }
        }

        public HomeControl.Room SelectedRoom { get; set; }
        

        // TODO: ScrollViewer template
        public Home()
        {
            InitializeComponent();

            this.homeControl = ServiceManager.GetService(EServiceType.HomeControl) as HomeControl;
            if (this.homeControl == null)
                throw new NullReferenceException("Connot find Home Control Service");

            this.DataContext = this;
            if (this.Rooms.Count > 0)
                this.SelectedRoom = this.Rooms[0];
        }


        private void GroupBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            GroupBox groupBox = e.Source as GroupBox;
            if (groupBox != null)
                App.ShowHideGroupBoxContent(groupBox);
        }


        private void importLayout_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Picture Files (*.bmp, *.jpg, *.gif, *.png, *.tiff)|*.bmp;*.jpg;*.gif;*.png;*.tiff|All Files (*.*)|*.*";
            openFileDialog.DefaultExt = "png";
            openFileDialog.RestoreDirectory = true;
            if (openFileDialog.ShowDialog() == true)
            {
                System.Drawing.Bitmap layout = new System.Drawing.Bitmap(openFileDialog.FileName);
                layout = new System.Drawing.Bitmap(layout, 960, 540);
                layout.Save(HomeControl.LayoutFileName, System.Drawing.Imaging.ImageFormat.Png);
                this.homeControl.ReloadLayout();
                this.OnPropertyChanged("Layout");
                this.OnPropertyChanged("Rooms");
                Logger.Log("Home", "Import Layout: " + openFileDialog.FileName);
            }
        }

        private void roomsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.OnPropertyChanged("SelectedRoom");
            this.OnPropertyChanged("Layout");
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string info)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(info));
        }

    }
}
