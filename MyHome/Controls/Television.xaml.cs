using MyHome.Dialogs;
using MyHome.Services;
using MyHome.Utils;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Input;

namespace MyHome.Controls
{
    /// <summary>
    /// Interaction logic for Television.xaml
    /// </summary>
    public partial class Television : UserControl, INotifyPropertyChanged
    {
        private TVControl tvControl;

        public ObservableCollection<TVControl.Television> Televisions
        {
            get
            {
                return new ObservableCollection<TVControl.Television>(this.tvControl.Televisions);
            }
        }

        public TVControl.Television SelectedTelevision { get; set; }

        public string MoviesRootPath
        {
            get { return this.tvControl.MoviesRootPath; }
            set { if (value != null) this.tvControl.MoviesRootPath = value; }
        }

        public string ImagesRootPath
        {
            get { return this.tvControl.ImagesRootPath; }
            set { if (value != null) this.tvControl.ImagesRootPath = value; }
        }


        public Television()
        {
            InitializeComponent();

            this.tvControl = ServiceManager.GetService(EServiceType.TVControl) as TVControl;
            if (this.tvControl == null)
                throw new NullReferenceException("Connot find TV Control Service");

            this.DataContext = this;
            if (this.Televisions.Count > 0)
                this.SelectedTelevision = this.Televisions[0];
        }


        private void GroupBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            GroupBox groupBox = e.Source as GroupBox;
            if (groupBox != null)
                App.ShowHideGroupBoxContent(groupBox);
        }


        private void addTelevisionButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            AddDialog addDialog = new AddDialog(true);
            System.Windows.Point pos = this.PointToScreen(Mouse.GetPosition(null));
            addDialog.Left = pos.X - 100;
            addDialog.Top = pos.Y;
            if (addDialog.ShowDialog() == true)
            {
                if (string.IsNullOrEmpty(addDialog.SelectedName))
                    addDialog.SelectedName = "Television";

                this.tvControl.Televisions.Add(new TVControl.Television(addDialog.SelectedRoom.Id, addDialog.SelectedName));
                this.OnPropertyChanged("Televisions");
                Logger.Log("Television", "Add television '" + addDialog.SelectedName + "' to " + addDialog.SelectedRoom.Name + "(" + addDialog.SelectedRoom.Id + ")");
            }
        }

        private void removeTelevisionImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TVControl.Television television = null;
            if (sender is Image)
                television = (sender as Image).DataContext as TVControl.Television;

            if (television != null)
                this.tvControl.Televisions.Remove(television);
            this.OnPropertyChanged("Televisions");

            this.SelectedTelevision = null;
            if (this.Televisions.Count > 0)
                this.SelectedTelevision = this.Televisions[0];
            this.OnPropertyChanged("SelectedTelevision");
        }

        private void televisionsListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.OnPropertyChanged("SelectedTelevision");
        }

        private void roomIdTextBox_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TextBox textbox = sender as TextBox;
            if (textbox == null)
                return;

            int selectedRoomId = string.IsNullOrEmpty(textbox.Text) ? -1 : Utils.Utils.ParseInt(textbox.Text);
            RoomSelectorDialog roomSelectorDialog = new RoomSelectorDialog(selectedRoomId);
            System.Windows.Point pos = this.PointToScreen(Mouse.GetPosition(null));
            roomSelectorDialog.Left = pos.X - 100;
            roomSelectorDialog.Top = pos.Y;
            if (roomSelectorDialog.ShowDialog() == true)
            {
                textbox.Text = roomSelectorDialog.SelectedRoom.Id.ToString();
            }
        }

        private void addRemoteControlButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if (this.SelectedTelevision == null)
                return;

            AddDialog addDialog = new AddDialog(false);
            System.Windows.Point pos = this.PointToScreen(Mouse.GetPosition(null));
            addDialog.Left = pos.X - 100;
            addDialog.Top = pos.Y;
            if (addDialog.ShowDialog() == true)
            {
                if (string.IsNullOrEmpty(addDialog.SelectedName))
                    addDialog.SelectedName = "Button";

                this.SelectedTelevision.RemoteControl.Add(new TVControl.RemoteControlButton(addDialog.SelectedName));
                this.OnPropertyChanged("SelectedTelevision"); // TODO: doesn't refresh buttons list
                Logger.Log("Television", "Add button '" + addDialog.SelectedName + "' to remotecontrol of " + this.SelectedTelevision.Name);
            }
        }

        private void removeRemoteControlImage_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TVControl.RemoteControlButton button = null;
            if (sender is Image)
                button = (sender as Image).DataContext as TVControl.RemoteControlButton;

            if (button != null)
                this.SelectedTelevision.RemoteControl.Remove(button);
            this.OnPropertyChanged("SelectedTelevision"); // TODO: doesn't refresh buttons list
        }

        
        private void rootPathTextBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox == null || e.ChangedButton != MouseButton.Left)
                return;

            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            fbd.SelectedPath = textBox.Text;
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                textBox.Text = fbd.SelectedPath;

        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string info)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(info));
        }

    }
}
