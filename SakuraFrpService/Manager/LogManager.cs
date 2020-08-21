using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;

using SakuraLibrary.Proto;

namespace SakuraFrpService.Manager
{
    public class LogManager : ConcurrentQueue<Log>
    {
        public const int CATEGORY_FRPC = 0, CATEGORY_SERVICE_INFO = 1, CATEGORY_SERVICE_WARNING = 2, CATEGORY_SERVICE_ERROR = 3;

        public readonly MainService Main;
        public readonly Thread MainThread;

        public int RotateSize;

        protected List<Log> newLog = new List<Log>();
        protected ManualResetEvent stopEvent = new ManualResetEvent(false);

        public LogManager(MainService main, int bufferSize)
        {
            Main = main;
            RotateSize = bufferSize;

            MainThread = new Thread(new ThreadStart(Run));
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
                    Data = data
                });
            }
        }

        #region Async Work

        public void Start()
        {
            if (MainThread.IsAlive)
            {
                MainThread.Abort(); // Shouldn't happen, just in case
            }
            stopEvent.Reset();
            MainThread.Start();
        }

        public void Stop(bool kill = false)
        {
            stopEvent.Set();
            try
            {
                if (kill)
                {
                    MainThread.Abort();
                    return;
                }
                MainThread.Join();
            }
            catch { }
        }

        protected void Run()
        {
            var msg = new PushMessageBase()
            {
                Type = PushMessageID.AppendLog,
                DataLog = new LogList()
            };
            while (!stopEvent.WaitOne(0))
            {
                Thread.Sleep(50);
                try
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
                            Enqueue(l);
                        }
                        newLog.Clear();
                    }
                    Main.Pipe.PushMessage(msg);
                    while (Count > RotateSize)
                    {
                        TryDequeue(out Log _);
                    }
                }
                catch { }
            }
        }

        #endregion
    }
}
