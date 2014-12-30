using MyHome.Utils;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Controls;

namespace MyHome.Controls
{
    /// <summary>
    /// Interaction logic for Logger.xaml
    /// </summary>
    public partial class Log : UserControl, INotifyPropertyChanged
    {
        public ObservableCollection<Logger.LogGridRow> Rows
        {
            get { lock (Logger.Rows) { return new ObservableCollection<Logger.LogGridRow>(Logger.Rows); } }
        }
        

        public Log()
        {
            InitializeComponent();

            this.DataContext = this;
            Logger.Rows.CollectionChanged += rows_CollectionChanged;
            if (Logger.Rows.Count > 0)
                this.logDataGrid.ScrollIntoView(Logger.Rows[Logger.Rows.Count - 1]);
        }

        private void rows_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            this.Dispatcher.InvokeAsync(() =>
                {
                    this.OnPropertyChanged("Rows");
                    this.logDataGrid.ScrollIntoView(Logger.Rows[Logger.Rows.Count - 1]);
                });
        }


        public event PropertyChangedEventHandler PropertyChanged;
		protected void OnPropertyChanged(string info)
		{
			PropertyChanged(this, new PropertyChangedEventArgs(info));
		}

    }
}
