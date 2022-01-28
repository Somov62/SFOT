using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Controls.Primitives;
using OTSOS;

namespace Shared_folder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string _userName = "misha";
        private readonly string _friendIP = "192.168.0.102";
        OtsosClient client;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void CreateFolder_Click(object sender, RoutedEventArgs e)
        {
            client = new OtsosClient(_friendIP, MessageReceived, MessageReceived);
            client.Start(_userName + " вошел в чат");
        }
        public void MessageReceived(object messege)
        {
            try
            {
                this.Dispatcher.Invoke(() =>
                {
                    output.Items.Add(messege);
                    scroll.ScrollToEnd();
                });
            }
            catch { } 
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (client == null) return;
            client.Close();
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            if (client == null) return;
            string messege;
            messege = DateTime.Now.TimeOfDay.Hours.ToString();
            messege += ":" + DateTime.Now.TimeOfDay.Minutes.ToString();
            messege += " " + _userName;
            messege += "\n" + input.Text;
            client.Send(messege);
        }
    }
}
