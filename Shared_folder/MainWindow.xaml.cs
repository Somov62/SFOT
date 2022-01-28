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

namespace Shared_folder
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly string _friendIP = "25.35.108.130";
        private string _folderName;
        private string _pathToCurrDir;
        private string _pathToFolder;
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
            using (StreamReader reader = new StreamReader($@"\\{_friendIP}\{_folderName}\playerData.txt"))
            {
                output.Text = reader.ReadToEnd();
            }

        }
        private void StartGame(string playerMode)
        {
            File.WriteAllText("./createData.bat",
                $"cd /d \"{_pathToCurrDir}\"\n" +
                $"md \"{_folderName}\"\n" +
                $"echo host>>\"{_pathToFolder}\\playerData.txt\"\n" +
                "chcp 65001\n" +
                $"net share {_folderName}=\"{_pathToFolder}\" /grant:\"Все\",Full\n" +
                $"icacls \"{_pathToFolder}\"  /grant \"Все\":F /T\n" +
                $"icacls \"{_pathToFolder}\\playerData.txt\"  /grant \"Все\":F \n");

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
