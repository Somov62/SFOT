using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;

namespace OTSOS
{
    /// <summary>
    /// Тип использумой локальной сети
    /// </summary>
    public enum IpType
    {
        /// <summary>
        /// Вы используете Hamachi и ваш ip начинается на 25.
        /// </summary>
        Hamachi = 25,
        /// <summary>
        /// Вы используете обычную LAN и ваш ip начинается на 192.
        /// </summary>
        Standart = 19
    }
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
        public OtsosClient(string friendIp, IpType ipType, Answer answerListen, Answer answerSend)
        {
            _IsGameRun = false;
            ListenDelay = 500;

            _answerSend = answerSend;
            _listenThreadToken = new CancellationTokenSource();

            List<IPAddress> ipAdress = Dns.GetHostByName(Dns.GetHostName()).AddressList.ToList();
            string personalIp = string.Empty;
            if (IpType.Hamachi == ipType)
            {
                personalIp = ipAdress.Where(p => p.ToString().Substring(0, 3) == "25.").FirstOrDefault().ToString();
            }
            if (IpType.Standart == ipType)
            {
                personalIp = ipAdress.Where(p => p.ToString().Substring(0, 3) == "25.").FirstOrDefault().ToString(); 
            }
            if (personalIp is null || personalIp == string.Empty) throw new Exception("The selected internet adapter was not found");
            PersonalIp = personalIp;
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
            private set
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
        public string PersonalIp { get; private set; }

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
                $"echo host>>\"{_pathToFolder}\\{FriendIp}.txt\"\n" +
                "chcp 65001\n" +
                $"            net share {_folderName}=\"{_pathToFolder}\" /grant:\"Все\",Full\n" +
                $"            icacls \"{_pathToFolder}\"  /grant \"Все\":F /T\n" +
                $"            icacls \"{_pathToFolder}\\{FriendIp}.txt\"  /grant \"Все\":F \n");

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
        public void Close(bool sleepDisconnect = true)
        {
            if (!_IsGameRun) throw new Exception("OtsosClient already closed");
            _listenThreadToken.Cancel();
            if (sleepDisconnect)
            {
                Send("<close>");
                Thread.Sleep(3000);
            }
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
                    using (StreamReader reader = new StreamReader($@"\\{FriendIp}\{_folderName}\{PersonalIp}.txt"))
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
                    if (lastMessege == "<close>")
                    {
                        method.DynamicInvoke("Пользователь оффлайн, канал закрыт");
                        this.Close(false);
                    }
                    method.DynamicInvoke(lastMessege);
                }
                catch
                {
                    if (IspreviewException) continue;
                    method.DynamicInvoke("Пользователь оффлайн или ip неверен");
                    IspreviewException = true;
                }
                Thread.Sleep(ListenDelay);
            }
        }
        public void Send(string messege)
        {
            try
            {
                using (StreamWriter writer = File.AppendText($@"{_pathToFolder}\{FriendIp}.txt"))
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
