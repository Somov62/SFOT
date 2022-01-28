using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace OTSOS
{
    public class OtsosClient
    {
        private bool _IsGameRun;
        private string _friendIp;
        private readonly string _folderName;
        private readonly string _pathToCurrDir;
        private readonly string _pathToFolder;
        private readonly Thread _listenThread;
        private CancellationTokenSource _listenThreadToken;
        public delegate void Answer(object message);
        private Answer _answerSend;
        public OtsosClient(string friendIp, Answer answerListen, Answer answerSend)
        {
            _IsGameRun = false;
            ListenDelay = 500;

            _answerSend = answerSend;
            _listenThreadToken = new CancellationTokenSource();
            FriendIp = friendIp;

            _folderName = System.Reflection.Assembly.GetEntryAssembly().GetName().Name + "_ShareFolder";
            _pathToCurrDir = Environment.CurrentDirectory;
            _pathToFolder = Environment.CurrentDirectory + "\\" + _folderName;

            _listenThread = new Thread(() => { Listen(answerListen, _listenThreadToken.Token); });
            _listenThread.Start();
        }
        public string FriendIp
        {
            get
            {
                return _friendIp;
            }
            set
            {
                string[] segments = value.Split('.');
                if (segments.Length != 4) throw new Exception("Incorrect format Ip adress");
                foreach (var item in segments)
                {
                    if (!int.TryParse(item, out int segment) || segment > 255 || segment < 0) throw new Exception("Incorrect format Ip adress");
                }
                _friendIp = value;
            }
        }

        /// <summary>
        /// Delay listen stream in milliseconds
        /// </summary>
        public int ListenDelay { get; set; }
        public void Start(string startMessege)
        {
            if (_IsGameRun) throw new Exception("OtsosClient already started");
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
            _IsGameRun = true;
            Thread.Sleep(100);
            Send(startMessege);
        }
        public void Close()
        {
            if (!_IsGameRun) throw new Exception("OtsosClient already closed");
            _listenThreadToken.Cancel();
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

        private void Listen(Answer method, CancellationToken token)
        {
            bool IspreviewException = false;
            int previewLenght = 0;
            while (!token.IsCancellationRequested)
            {
                try
                {
                    string actualMessege = "";
                    using (StreamReader reader = new StreamReader($@"\\{FriendIp}\{_folderName}\playerData.txt"))
                    {
                        actualMessege = reader.ReadToEnd();
                    }
                    string[] messeges = actualMessege.Split("\r\n");
                    if (messeges.Length == previewLenght)
                    {
                        Thread.Sleep(ListenDelay);
                        continue;
                    }
                    string lastMessege = messeges[messeges.Length - 2];
                    previewLenght = messeges.Length;
                    IspreviewException = false;
                    method.DynamicInvoke(lastMessege);
                }
                catch
                {
                    if (IspreviewException) continue;
                    method.DynamicInvoke("Не удалось прочитать, поток занят");
                    IspreviewException = true;
                }
                Thread.Sleep(ListenDelay);
            }
        }
        public void Send(string messege)
        {
            try
            {
                using (StreamWriter writer = File.AppendText($@"{_pathToFolder}\playerData.txt"))
                {
                    writer.WriteLine(messege);
                    writer.Close();
                }
                _answerSend.DynamicInvoke(messege);
            }
            catch
            {
                _answerSend.DynamicInvoke("Не удалось отправить сообщение");
            }
        }
    }
}
