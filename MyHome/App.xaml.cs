using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace MyHome
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

        public static void ShowHideGroupBoxContent(GroupBox groupBox)
        {
            Panel panel = groupBox.Content as Panel;
            if (panel == null)
                return;

            Action emptyDelegate = delegate() { };
            if (panel.Visibility == Visibility.Visible)
            {
                int height = (int)groupBox.ActualHeight;
                groupBox.Tag = (int)groupBox.ActualHeight;
                for (int i = height; i >= 25; i--)
                {
                    groupBox.Height = i;
                    groupBox.Dispatcher.Invoke(emptyDelegate, System.Windows.Threading.DispatcherPriority.Input);
                    Thread.Sleep(1);
                }
                groupBox.Height = double.NaN;

                panel.Visibility = Visibility.Collapsed;
                groupBox.FontWeight = FontWeights.Bold;
            }
            else
            {
                panel.Visibility = Visibility.Visible;
                groupBox.FontWeight = FontWeights.Normal;

                int height = (int)groupBox.Tag;
                groupBox.Tag = null;
                for (int i = 25; i < height; i++)
                {
                    groupBox.Height = i;
                    groupBox.Dispatcher.Invoke(emptyDelegate, System.Windows.Threading.DispatcherPriority.Input);
                    Thread.Sleep(1);
                }
                groupBox.Height = double.NaN;
            }
        }

    }
}
