﻿using System;
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

namespace Shared_folder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly string _friendIP = "192.168.0.102";
        private string _folderName;
        private string _pathToCurrDir;
        private string _pathToFolder;
        Thread listenThread;
        CancellationTokenSource cts = new CancellationTokenSource();

        public MainWindow()
        {
            InitializeComponent();
            _folderName = System.Reflection.Assembly.GetEntryAssembly().GetName().Name + "_ShareFolder";
            _pathToCurrDir = Environment.CurrentDirectory;
            _pathToFolder = Environment.CurrentDirectory + "\\" + _folderName;
        }

        private void CreateFolder_Click(object sender, RoutedEventArgs e)
        {
            StartGame("cryptographer");
            listenThread = new Thread(() => { Listen(cts.Token); });
            listenThread.Start();
        }
        private void Listen(CancellationToken token = default)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    using (StreamReader reader = new StreamReader($@"\\{_friendIP}\{_folderName}\playerData.txt"))
                    {
                        this.Dispatcher.Invoke(() =>
                        {
                            output.Text = reader.ReadToEnd();
                        });

                    }
                }
                catch (ThreadAbortException)
                {
                    break;
                }
                catch
                {
                    this.Dispatcher.Invoke(() =>
                    {
                        output.Text += "Ошибка чтения данных удалённого компьютера\n";
                    });
                }
                Thread.Sleep(500); 
            }
        }
        private void StartGame(string playerMode)
        {
            File.WriteAllText("./createData.bat",
                $"cd /d \"{_pathToCurrDir}\"\n" +
                $"md \"{_folderName}\"\n" +
                $"echo host>>\"{_pathToFolder}\\playerData.txt\"\n" +
                "chcp 65001\n" +
                $"            net share {_folderName}=\"{_pathToFolder}\" /grant:\"Все\",Full\n" +
                $"            icacls \"{_pathToFolder}\"  /grant \"Все\":F /T\n" +
                $"            icacls \"{_pathToFolder}\\playerData.txt\"  /grant \"Все\":F \n");

            Process create = new Process();
            create.StartInfo.FileName = "createData.bat";
            create.StartInfo.Verb = "runas";
            create.StartInfo.UseShellExecute = false;
            create.StartInfo.CreateNoWindow = true;
            create.Start();
            System.Threading.Thread.Sleep(100);
            using (StreamWriter writer = new StreamWriter($@"./{_folderName}\playerData.txt"))
            {
                writer.WriteLine("playerMode"+ "|" + playerMode);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            cts.Cancel();
            File.WriteAllText("./deleteData.bat",
                $"cd /d \"{_pathToCurrDir}\"\n" +
                $"net share {_folderName} /delete /y\n" +
                $"rd /S /Q \"{_pathToFolder}\"\n" +
                $"del \"{_pathToCurrDir}\\createData.bat\"\n" +
                "del %0");

            Process create = new Process();
            create.StartInfo.FileName = "deleteData.bat";
            create.StartInfo.Verb = "runas";
            create.StartInfo.UseShellExecute = false;
            create.StartInfo.CreateNoWindow = true;
            create.Start();
        }
    }
}
