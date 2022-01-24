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
        public MainWindow()
        {
            InitializeComponent();
        }

        private void CreateFolder_Click(object sender, RoutedEventArgs e)
        {
            StartGame("cryptographer");
            using (StreamReader reader = new StreamReader($@"\\{_friendIP}\sha1\playerData.txt"))
            {
                output.Text = reader.ReadToEnd();
            }

        }
        private void StartGame(string playerMode)
        {
            string folderName = System.Reflection.Assembly.GetEntryAssembly().GetName().Name + "_ShareFolder";
            string path = Environment.CurrentDirectory;
            string pathFolder = Environment.CurrentDirectory + "\\" + folderName;
            File.WriteAllText("./createData.bat",
                $"cd /d \"{path}\"\n" +
                $"md \"{folderName}\"\n" +
                $"echo host>>\"{pathFolder}\\playerData.txt\"\n" +
                "chcp 65001\n" +
                $"net share {folderName}=\"{pathFolder}\" /grant:\"Все\",Full\n" +
                $"icacls \"{pathFolder}\"  /grant \"Все\":F /T\n" +
                $"icacls \"{pathFolder}\\playerData.txt\"  /grant \"Все\":F \n");

            Process create = new Process();
            create.StartInfo.FileName = "createData.bat";
            create.StartInfo.Verb = "runas";
            create.StartInfo.UseShellExecute = false;
            create.StartInfo.CreateNoWindow = true;
            create.Start();
            System.Threading.Thread.Sleep(100);
            using (StreamWriter writer = new StreamWriter($@"./{folderName}\playerData.txt"))
            {
                writer.WriteLine("playerMode"+ "|" + playerMode);
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            string folderName = System.Reflection.Assembly.GetEntryAssembly().GetName().Name + "_ShareFolder";
            string path = Environment.CurrentDirectory;
            string pathFolder = Environment.CurrentDirectory + "\\" + folderName;
            File.WriteAllText("./deleteData.bat",
                $"cd /d \"{path}\"\n" +
                $"net share {folderName} /delete /y\n" +
                $"rd /S /Q \"{pathFolder}\"\n" +
                $"del \"{path}\\createData.bat\"\n" +
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
