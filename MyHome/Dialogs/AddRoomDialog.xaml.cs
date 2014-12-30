using System.Windows;
using System.Windows.Input;

namespace MyHome.Dialogs
{
    /// <summary>
    /// Interaction logic for AddRoomDialog.xaml
    /// </summary>
    public partial class AddRoomDialog : Window
    {
        public string RoomName { get; set; }

        public AddRoomDialog()
        {
            InitializeComponent();

            this.DataContext = this;
            this.roomNameTextBox.Focus();
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
