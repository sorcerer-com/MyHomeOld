using MyHome.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;

namespace MyHome.Dialogs
{
    /// <summary>
    /// Interaction logic for AddDialog.xaml
    /// </summary>
    public partial class AddDialog : Window
    {
        public string SelectedName { get; set; }
        public HomeControl.Room SelectedRoom { get; set; }

        public ObservableCollection<HomeControl.Room> Rooms
        {
            get
            {
                HomeControl homeControl = ServiceManager.GetService(EServiceType.HomeControl) as HomeControl;
                return new ObservableCollection<HomeControl.Room>(homeControl.Rooms);
            }
        }

        public AddDialog(bool showRooms)
        {
            InitializeComponent();

            this.DataContext = this;
            this.nameTextBox.Focus();
            if (this.Rooms.Count > 0)
                this.SelectedRoom = this.Rooms[0];

            if (!showRooms)
                this.roomsStackPanel.Visibility = System.Windows.Visibility.Collapsed;
        }


        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                this.Button_Click(this.okButton, null);
            else if (e.Key == Key.Escape)
                this.Button_Click(this.cancelButton, null);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (sender == this.okButton)
                this.DialogResult = true;
            else
                this.DialogResult = false;
            this.Close();
        }


    }
}
