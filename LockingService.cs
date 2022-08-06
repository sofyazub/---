//Главный класс, отвечающий за работу с серийным портом,
//обработку пришедшей информации, вынесение решения о блокировки компьютера

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace LockingService
{
    public partial class LockingService : ServiceBase
    {
        private delegate void LineReceivedEvent(string reading);
        private StreamWriter file;
        string check = "Card UID: ";
        bool isBlock = true;

        public LockingService()
        {
            InitializeComponent();
            this.CanStop = true;             // службу можно остановить
            this.CanPauseAndContinue = true; // службу можно приостановить и затем продолжить
            this.AutoLog = true;             // служба может вести запись в лог
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                file = new StreamWriter(new FileStream("LockingService.log",
                                System.IO.FileMode.Append, System.IO.FileAccess.Write));
                this.file.WriteLine("Служба LockingService стартовала в " + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
                this.file.Flush();
                port.PortName = "COM6";
                port.BaudRate = 9600;
                OpenPort();
            }
            catch (IOException ex)
            {
                this.file.WriteLine(ex.Message);
                this.file.Flush();
                System.Diagnostics.Process.GetCurrentProcess().Kill();
            }
        }

        protected override void OnContinue()
        {
            WriteLog("LockingService возобновление");
        }

        protected override void OnPause()
        {
            WriteLog("LockingService пауза");
        }

        protected override void OnStop()
        {
            WriteLog("LockingService остановка");
            if (file != null)
            {
                this.file.Close();
            }
        }

        private void serialPort1_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            OpenPort();
            while (!port.IsOpen)
            {
                WriteLog("port is close");
                isBlock = true;
                Block();
            }
            try
            {
                string reading = port.ReadLine();
                LineReceived(reading);
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
        }


        private void LineReceived(string reading)
        {
            if (IsFindCard(reading))
            {
                isBlock = false;
                Block();
                return;
            }
            else
            {
                isBlock = true;
                Block();
            }
        }

        public bool IsFindCard(string reading)
        {
            if (reading.Contains("Card UID: "))
            {
                reading = reading.Replace(check, "");
                reading = reading.Substring(0, 11);
                Console.WriteLine(reading);
                if (rightcard(reading))
                {
                    WriteLog("Right card");

                    return true;
                }
                else
                {
                    WriteLog("Wrong card");
                    return false;
                }
            }
            else Console.WriteLine("No card");
            return false;
        }

        public void Block()
        {
            if (isBlock)
            {
                LockWorkStation();

                WriteLog("Block");
            }
            else
            {
                WriteLog("No block");
            }
        }

        private void LockWorkStation()
        {
            try { 
            String applicationName = @"C:\\Users\\HONOR\\source\\repos\\LockingApp\\LockingApp\\bin\\Debug\\LockingApp.exe block";
            ApplicationLoader.PROCESS_INFORMATION procInfo;
            ApplicationLoader.StartProcessAndBypassUAC(applicationName, out procInfo);
            }
             catch (Exception e)
            {
                 WriteLog(e.Message);
            }
        }

        private void OpenPort()
        {
            while (!port.IsOpen)
            {
                try
                {
                    port.Open();
                }
                catch (Exception ex)
                {
                    WriteLog("Ошибка открытия Com порта: " + ex.Message);
                    isBlock = true;
                    Block();
                }
            }
        }

        private bool rightcard(string code)
        {
            string shifrnames = "C:\\Users\\HONOR\\source\\repos\\LockingService\\LockingService\\decrypt.txt";
            string names = "codes.txt";
            //CryptoHelper.EncryptFile(one, two);
            CryptoHelper.DecryptFile(shifrnames, names);
            string[] readtext = File.ReadAllLines(names, Encoding.UTF8);
            foreach (string s in readtext)
            {
                if (code == s)
                {
                    return true;
                }
            }
            return false;
        }

        private void WriteLog(string s)
        {
            if (file != null)
            {
                this.file.WriteLine(s);
                this.file.Flush();
            }
        }
    }
}
