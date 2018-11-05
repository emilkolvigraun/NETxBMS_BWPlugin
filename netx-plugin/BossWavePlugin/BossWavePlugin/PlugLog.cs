using Newtonsoft.Json.Linq;
using nxaXIO.PlugKit.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Timers;

namespace BossWavePlugin
{
    public static class PlugLog
    {
        public static List<string> memoryLog { get; set; } // The log is written in memory and emptied every 20'th second

        public static void EnableLogging(int time)
        {
            if (BossWavePlugin.Instance != null)
            {
                memoryLog = new List<string>(); // First instantiate the memory log

                string id = DateTime.Now.ToString("HH:mm").Replace(":", " "); // Hour:Minute
                string date = DateTime.Now.ToString("M-d-yyyy"); // Month:Day:Year

                string dirpath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\etc\\log\\" + date; // Path to current Log folder
                string fullpath = dirpath + "\\" + id + ".txt"; // Path to current file
                try
                {
                    if (!Directory.Exists(dirpath)) // If the Directory doesnt exist...
                    {
                        Directory.CreateDirectory(dirpath); // ...create it
                    }
                    using (FileStream fs = File.Create(fullpath))
                    {
                        fs.Write(Encoding.UTF8.GetBytes("INFO, Session begun at, " + DateTime.Now.ToString() + "\n"), 0, Encoding.UTF8.GetBytes("INFO, Session begun at, " + DateTime.Now.ToString() + "\n").Length);
                    }
                    new Thread(() => new LogWriter().Run(id, dirpath, fullpath, time)).Start();
                    BossWavePlugin.Instance.host.WriteLog(LogLevel.Info, "Log activated. (Notice: \\etc\\log\\HH:mm)");
                }
                catch (Exception e)
                {
                    BossWavePlugin.Instance.host.WriteLog(LogLevel.Error, e.Message);
                }
            }
        }

        public static void WriteLog(string id, string dirpath, string fullpath)
        {
            if (BossWavePlugin.Instance != null)
            {
                try
                {
                    if (!Directory.Exists(dirpath))
                    {
                        Directory.CreateDirectory(dirpath);
                    }
                    using (StreamWriter fs = File.AppendText(fullpath))
                    {
                        foreach (string line in memoryLog)
                        {
                            fs.Write(line + "\n");
                        }
                    }

                    memoryLog.Clear();
                }
                catch (Exception e)
                {
                    BossWavePlugin.Instance.host.WriteLog(LogLevel.Error, e.Message);
                }
            }
        }
    }


    public class LogWriter
    {
        static string id;
        static string dirpath;
        static string fullpath;
        
        public void Run(string id, string dirpath, string fullpath, int time)
        {
            LogWriter.id = id;
            LogWriter.dirpath = dirpath;
            LogWriter.fullpath = fullpath;
                    
            var aTimer = new System.Timers.Timer(time); // A timer with a twenty second interval.
            
            aTimer.Elapsed += OnTimedEvent; // Hook up the Elapsed event for the timer. 
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        }

        private static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            if (BossWavePlugin.Instance != null)
            {
                PlugLog.WriteLog(id, dirpath, fullpath);
                BossWavePlugin.Instance.host.WriteLog(LogLevel.Warning, "Log updated. (Notice: \\etc\\log\\HH:mm)");
            }
        }

        public static void Stop()
        {
            PlugLog.WriteLog(id, dirpath, fullpath);
            BossWavePlugin.Instance.host.WriteLog(LogLevel.Info, "Committing last log. (Notice: \\etc\\log\\HH:mm)");
        }
    }
}
