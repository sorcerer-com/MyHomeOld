using MyHome.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace MyHome.Dialogs
{
    /// <summary>
    /// Interaction logic for RoomSelectorDialog.xaml
    /// </summary>
    public partial class RoomSelectorDialog : Window
    {
        public HomeControl.Room SelectedRoom { get; set; }

        public ObservableCollection<HomeControl.Room> Rooms
        {
            get
            {
                HomeControl homeControl = ServiceManager.GetService(EServiceType.HomeControl) as HomeControl;
                return new ObservableCollection<HomeControl.Room>(homeControl.Rooms);
            }
        }


        public RoomSelectorDialog()
        {
            InitializeComponent();

            this.DataContext = this;
        }

        public RoomSelectorDialog(int selectedRoomId)
            : this()
        {
            foreach (HomeControl.Room room in this.Rooms)
            {
                if (room.Id == selectedRoomId)
                {
                    this.SelectedRoom = room;
                    break;
                }
            }
        }


        private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                this.DialogResult = false;
                this.Close();
            }
        }

        private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (this.IsActive)
            {
                this.DialogResult = true;
                this.Close();
            }
        }

    }
}
