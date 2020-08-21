using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using SakuraLibrary.Proto;

namespace SakuraFrpService
{
    public class NodeManager : Dictionary<int, Node>
    {
        public readonly MainService Main;

        public readonly Thread MainThread;

        protected int FetchTicks = 0;
        protected ManualResetEvent stopEvent = new ManualResetEvent(false);

        public NodeManager(MainService main)
        {
            Main = main;
            MainThread = new Thread(new ThreadStart(Run));
        }

        public async Task UpdateNodes()
        {
            var nodes = await Natfrp.Request("get_nodes");
            lock (this)
            {
                Clear();
                foreach (Dictionary<string, dynamic> j in nodes["data"])
                {
                    this[(int)j["id"]] = new Node()
                    {
                        Id = (int)j["id"],
                        Name = j["name"] as string,
                        AcceptNew = j["accept_new"],
                        AcceptHttp = (int)j["accept_http"]
                    };
                }
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

        public void Stop(bool wait = false)
        {
            stopEvent.Set();
            if (wait)
            {
                try
                {
                    MainThread.Join();
                }
                catch { }
            }
        }

        protected void Run()
        {
            while (!stopEvent.WaitOne(0))
            {
                Thread.Sleep(50);
                if (FetchTicks-- <= 0)
                {
                    try
                    {
                        UpdateNodes().Wait();
                        FetchTicks = 20 * 3600 * 6;
                    }
                    catch
                    {
                        FetchTicks = 20 * 5;
                    }
                }
            }
        }

        #endregion
    }
}
