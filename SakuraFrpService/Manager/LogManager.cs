using System.Threading;
using System.Collections.Generic;
using System.Collections.Concurrent;

using SakuraLibrary.Proto;
using SakuraLibrary.Helper;

namespace SakuraFrpService.Manager
{
    public class LogManager : ConcurrentQueue<Log>, IAsyncManager
    {
        public const int CATEGORY_FRPC = 0, CATEGORY_SERVICE_INFO = 1, CATEGORY_SERVICE_WARNING = 2, CATEGORY_SERVICE_ERROR = 3;

        public readonly MainService Main;
        public readonly AsyncManager AsyncManager;

        public int RotateSize;

        protected List<Log> newLog = new List<Log>();

        public LogManager(MainService main, int bufferSize)
        {
            Main = main;
            RotateSize = bufferSize;

            AsyncManager = new AsyncManager(Run);
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

        #region IAsyncManager

        public bool Running => AsyncManager.Running;

        public void Start() => AsyncManager.Start();

        public void Stop(bool kill = false) => AsyncManager.Stop(kill);

        #endregion
    }
}
