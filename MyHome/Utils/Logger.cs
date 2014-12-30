using System;
using System.Collections.ObjectModel;
using System.IO;

namespace MyHome.Utils
{
    public static class Logger
    {
        public const string LogFile = "log.txt";

        public struct LogGridRow
        {
            public int Number { get; set; }
            public DateTime Time { get; set; }
            public string Category { get; set; }
            public string Log { get; set; }

            public LogGridRow(int number, string category, string log) : this()
            {
                this.Number = number;
                this.Time = DateTime.Now;
                this.Category = category;
                this.Log = log;
            }
        }


        public static ObservableCollection<LogGridRow> Rows { get; private set; }


        static Logger()
        {
            Rows = new ObservableCollection<LogGridRow>();
        }


        public static void Log(string category, string log)
        {
            lock (Rows)
            {
                if (Rows.Count == 0 && File.Exists(Logger.LogFile))
                {
                    const int maxCount = 1; // 10
                    string file = Path.GetFileNameWithoutExtension(Logger.LogFile);
                    for (int i = maxCount - 1; i > 0; i--)
                    {
                        if (File.Exists(file + i + ".txt"))
                            File.Move(file + i + ".txt", file + (i + 1) + ".txt");
                    }

                    if (File.Exists(file + maxCount + ".txt"))
                        File.Delete(file + maxCount + ".txt");

                    File.Move(Logger.LogFile, file + "1.txt");
                }
                File.AppendAllText(Logger.LogFile, DateTime.Now.ToString() + " [" + category + "] " + log + "\r\n");
                Rows.Add(new LogGridRow(Rows.Count + 1, category, log));
            }
        }

    }
}
