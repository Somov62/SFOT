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

namespace Shared_folder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, Iotsosable
    {
        private string _userName = "misha";
        private readonly string _friendIP = "192.168.0.102";

        
        Thread listenThread;
        CancellationTokenSource cts = new CancellationTokenSource();

        public MainWindow()
        {
            InitializeComponent();
            
        }

        private void CreateFolder_Click(object sender, RoutedEventArgs e)
        {
            StartGame("cryptographer");
            listenThread = new Thread(() => { Listen(cts.Token); });
            listenThread.Start();
        }
        public void MessageReceived(object messege)
        {
            output.Items.Add(messege);
            scroll.ScrollToEnd();
        }

        
        private void Listen(CancellationToken token = default)
        {
            bool IspreviewException = false;
            int previewLenght = 0;
            while (!token.IsCancellationRequested)
            {
                try
                {
                    string actualMessege = "";
                    using (StreamReader reader = new StreamReader($@"\\{_friendIP}\{/*_folderName*/}\playerData.txt"))
                    {
                        actualMessege = reader.ReadToEnd();
                    }
                    string[] messeges = actualMessege.Split("\r\n");
                    if (messeges.Length == previewLenght)
                    {
                        Thread.Sleep(500);
                        continue;
                    }
                    string lastMessege = messeges[messeges.Length - 2];
                     previewLenght = messeges.Length;
                    IspreviewException = false;
                    this.Dispatcher.Invoke(() =>
                    {
                        MessageReceived(lastMessege);
                    });
                }
                catch 
                {
                    if (IspreviewException) continue;                    
                    this.Dispatcher.Invoke(() =>
                    {
                        MessageReceived("Не удалось прочитать, поток занят");
                    });
                    IspreviewException = true;
                }
                Thread.Sleep(500);
            }
        }
        private void StartGame(string playerMode)
        {
           
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            
        }

        private void Send_Click(object sender, RoutedEventArgs e)
        {
            string messege;
            messege = DateTime.Now.TimeOfDay.Hours.ToString();
            messege += ":" + DateTime.Now.TimeOfDay.Minutes.ToString();
            messege += " " + _userName;
            messege += "\n" + input.Text;
            try
            {
                using (StreamWriter writer = File.AppendText($@"{/*_pathToFolder*/}\playerData.txt"))
                {
                    writer.WriteLine(messege);
                    writer.Close();
                }
                output.Items.Add(messege);
                output.ScrollIntoView(output.Items[output.Items.Count - 1]);
                output.UpdateLayout();
                scroll.ScrollToEnd();
            }
            catch (IOException)
            {
                
                MessageReceived("Не удалось отправить, поток занят");
            
            }
            catch 
            {
                output.Items.Add("Не удалось отправить сообщение");
                scroll.ScrollToEnd();
            }
            
        }

        
    }
}
