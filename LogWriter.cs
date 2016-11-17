using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Analizatior
{
    public static class LogWriter
    {
        static string logPath = "logFile.txt";
        static StreamWriter file;

        public static void Init()
        {
            file = new System.IO.StreamWriter(logPath, false);
        }

        public static void AddLog(string s)
        {
            file.WriteLine(DateTime.Now + " " + s);
            file.Flush();
        }
    }
}
