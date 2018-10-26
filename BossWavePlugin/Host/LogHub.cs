using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Hubs;
using nxaXIO.PlugKit.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Timers;

namespace BossWavePlugin.Host
{
    [HubName("logHub")]
    public class LogHub : Hub
    {
        public static List<string> memoryLog = new List<string>();
        public void ActivateLogging()
        {
            if (BossWavePlugin.Instance != null)
            {
                string id = Context.ConnectionId;
                string date = DateTime.Now.ToString("M-d-yyyy");
                string dirpath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\etc\\log\\" + DateTime.Now.ToString("M-d-yyyy");
                string fullpath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "\\etc\\log\\" + DateTime.Now.ToString("M-d-yyyy") + "\\" + id + ".txt";
                try
                {
                    if (!Directory.Exists(dirpath))
                    {
                        Directory.CreateDirectory(dirpath);
                    }
                    using (FileStream fs = File.Create(fullpath))
                    {
                        fs.Write(Encoding.UTF8.GetBytes("! Client : " + id + " has connected."), 0, Encoding.UTF8.GetBytes("! Client : " + id + " has connected.").Length);
                    }
                    new Thread( () => new LogWriter().Run(id, dirpath, fullpath)).Start();
                    BossWavePlugin.Instance.host.WriteLog(LogLevel.Info, "Log activated. (Notice: \\etc\\log\\conectionID)");
                } catch (Exception e)
                {
                    BossWavePlugin.Instance.host.WriteLog(LogLevel.Error, e.Message);
                }
            }
        }

        public static void AddToLog(string line)
        {
            memoryLog.Add(line);
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
                            fs.Write(line);
                        }

                        memoryLog.Clear();
                        
                    }
                }
                catch (Exception e)
                {
                    BossWavePlugin.Instance.host.WriteLog(LogLevel.Error, e.Message);
                }
            }
        }
    }

    class LogWriter{
        static string id;
        static string dirpath;
        static string fullpath;


        public void Run(string id, string dirpath, string fullpath)
        {
            LogWriter.id = id;
            LogWriter.dirpath = dirpath;
            LogWriter.fullpath = fullpath;

            // Create a timer with a two second interval.
            var aTimer = new System.Timers.Timer(20000);
            // Hook up the Elapsed event for the timer. 
            aTimer.Elapsed += OnTimedEvent;
            aTimer.AutoReset = true;
            aTimer.Enabled = true;
        
            
        }
        private static void OnTimedEvent(Object source, ElapsedEventArgs e)
        {
            if (BossWavePlugin.Instance != null)
            {
                try
                {
                    LogHub.WriteLog(id, dirpath, fullpath);
                    BossWavePlugin.Instance.host.WriteLog(LogLevel.Info, "Log updated. (Notice: \\etc\\log\\conectionID)");
                    
                }
                catch (Exception ex)
                {
                    BossWavePlugin.Instance.host.WriteLog(LogLevel.Error, ex.Message);
                }
            }
        }

        public static void Stop()
        {
            LogHub.WriteLog(id, dirpath, fullpath);
            BossWavePlugin.Instance.host.WriteLog(LogLevel.Info, "Committing last log. (Notice: \\etc\\log\\conectionID)");
        }
    }

}
