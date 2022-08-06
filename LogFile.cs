//Класс, необходимый для создания лог-файла службы
//(в такой файл будут записываться действия, происходящие со службой и внутри нее)

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LockingService
{
    class LogFile : System.IDisposable
    {
        StreamWriter file;

        public string FileName { get; private set; }

        public LogFile()
        {
            CreateFile();
        }

        ~LogFile()
        {
            Dispose();
        }

        public void Dispose()
        {
            if (file != null)
            {
                file.Close();
            }
        }

        private void CreateFile()
        {
            FileName = Path.GetTempFileName();
            file = File.CreateText(FileName);
        }

        public void Write(string text)
        {
            file.WriteLine(text);
        }

    }
}
