using System;
using System.Threading;

namespace SakuraLibrary.Helper
{
    public interface IAsyncManager
    {
        bool Running { get; }

        void Start();
        void Stop(bool kill);
    }

    public class AsyncManager : IAsyncManager
    {
        public Action Run;

        public Thread MainThread = null;
        public ManualResetEvent StopEvent = new ManualResetEvent(false);

        public bool Running => MainThread != null && MainThread.IsAlive;

        public AsyncManager(Action run)
        {
            Run = run;
        }

        public void Start() => Start(false);

        public void Start(bool background)
        {
            if (MainThread != null && MainThread.IsAlive)
            {
                MainThread.Abort(); // Shouldn't happen, just in case
            }
            StopEvent.Reset();

            MainThread = new Thread(new ThreadStart(Run))
            {
                IsBackground = background
            };
            MainThread.Start();
        }

        public void Stop(bool kill)
        {
            StopEvent.Set();
            if (!Running)
            {
                return;
            }
            try
            {
                if (kill)
                {
                    MainThread.Abort();
                    return;
                }
                MainThread.Join();
            }
            finally
            {
                MainThread = null;
            }
        }
    }
}
