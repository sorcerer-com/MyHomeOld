using MyHome.TcpConnection;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;

namespace MyHome.Windows
{
    /// <summary>
    /// Interaction logic for ServerLogWindow.xaml
    /// </summary>
    public partial class ServerLogWindow : Window, ILogger
    {
        private const string logFile = "server.log";

        public struct LogGridRow
        {
            public string Category { get; set; }
            public string Log { get; set; }
            
            public LogGridRow(string category, string log) : this()
            {
                this.Category = category;
                this.Log = log;
            }
        }

        private static ObservableCollection<LogGridRow> rows = new ObservableCollection<LogGridRow>();
        public ObservableCollection<LogGridRow> Rows
        {
            get { return rows; }
        }


        public ServerLogWindow()
        {
            InitializeComponent();
            File.CreateText(logFile).Close();

            this.DataContext = this;
        }

        public void Log(string category, string log)
        {
            this.Dispatcher.InvokeAsync(() =>
                {
                    File.AppendAllText(logFile, "[" + category + "] " + log + "\r\n");
                    this.Rows.Add(new LogGridRow(category, log));
                    this.logDataGrid.ScrollIntoView(this.Rows[this.Rows.Count - 1]);
                });
        }
    }
}
