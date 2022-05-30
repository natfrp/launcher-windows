using SakuraLibrary;
using SakuraLibrary.Helper;
using SakuraLibrary.Proto;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace SakuraFrpService.Manager
{
    public class LogManager : ConcurrentQueue<Log>, IAsyncManager
    {
        public const int CATEGORY_FRPC = 0,
            CATEGORY_SERVICE_INFO = 1, CATEGORY_SERVICE_WARNING = 2, CATEGORY_SERVICE_ERROR = 3,
            CATEGORY_NOTICE_INFO = 4, CATEGORY_NOTICE_WARNING = 5, CATEGORY_NOTICE_ERROR = 6;

        public static readonly string LogDirectory = Path.Combine(Consts.WorkingDirectory, "Logs");

        public readonly MainService Main;
        public readonly AsyncManager AsyncManager;

        public int RotateSize;

        protected StreamWriter logWriter = null;
        protected List<Log> newLog = new List<Log>();

        public LogManager(MainService main, int bufferSize)
        {
            Main = main;
            RotateSize = bufferSize;

            AsyncManager = new AsyncManager(Run);

            Directory.CreateDirectory(LogDirectory);
        }

        public void Clear()
        {
            while (Count > 0)
            {
                TryDequeue(out Log _);
            }
        }

        public void Log(int category, string source, string data)
        {
            if (data == null)
            {
                return;
            }
            lock (newLog)
            {
                newLog.Add(new Log()
                {
                    Category = category,
                    Source = source,
                    Data = data,
                    Time = Utils.GetSakuraTime()
                });
            }
        }

        protected void Run()
        {
            var msg = new PushMessageBase()
            {
                Type = PushMessageID.AppendLog,
                DataLog = new LogList()
            };
            while (!AsyncManager.StopEvent.WaitOne(50))
            {
                try
                {
                    lock (this)
                    {
                        lock (newLog)
                        {
                            if (newLog.Count == 0)
                            {
                                continue;
                            }
                            msg.DataLog.Data.Clear();
                            msg.DataLog.Data.Add(newLog);
                            foreach (var l in newLog)
                            {
                                if (l.Category < CATEGORY_NOTICE_INFO)
                                {
                                    Enqueue(l);
                                    WriteFileLog(l);
                                }
                            }
                            newLog.Clear();
                        }
                        Main.Pipe.PushMessage(msg);
                        while (Count > RotateSize)
                        {
                            TryDequeue(out Log _);
                        }
                    }
                }
                catch { }
            }
        }

        private void WriteFileLog(Log l)
        {
            try
            {
                logWriter?.WriteLine(l.Category == 0 ?
                    string.Format("frpc[{0}] {1}", l.Source, l.Data) :
                    string.Format("{0} {1} {2} {3}",
                        Utils.ParseSakuraTime(l.Time).ToString("yyyy/MM/dd HH:mm:ss"),
                        l.Category == CATEGORY_SERVICE_ERROR ? "E" : (l.Category == CATEGORY_SERVICE_WARNING ? "W" : "I"),
                        l.Source,
                        l.Data
                    )
                );
            }
            catch { }
        }

        #region IAsyncManager

        public bool Running => AsyncManager.Running;

        public void Start()
        {
            var logs = Path.Combine(LogDirectory, "SakuraFrpService.log");
            if (File.Exists(logs))
            {
                var last = Path.Combine(LogDirectory, "SakuraFrpService.last.log");
                File.Delete(last);
                File.Move(logs, last);
            }
            logWriter = new StreamWriter(File.Open(logs, FileMode.Create, FileAccess.Write, FileShare.Read), Encoding.UTF8, 4096, false)
            {
                AutoFlush = true
            };

            AsyncManager.Start();
        }

        public void Stop(bool kill = false)
        {
            logWriter.Close();

            AsyncManager.Stop(kill);
        }

        #endregion
    }
}
