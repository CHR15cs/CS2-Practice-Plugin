using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSPracc
{
    public class Logging
    {
        public static Logging Instance { get; private set; }
        private static FileInfo _logFile = null;
        public Logging(FileInfo loggignFile) 
        {
            _logFile = loggignFile;
            Instance = this;
        }

        public static void LogMessage(string message) 
        { 
            if(_logFile != null)
            {
                using (StreamWriter sw = File.AppendText(_logFile.FullName))
                {
                    sw.WriteLine($"{DateTime.Now} - {message}");
                }
            }

        }

    }
}
