using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using SakuraLibrary;
using SakuraLibrary.Proto;

namespace SakuraFrpService.Manager
{
    public class NodeManager : Dictionary<int, Node>, IAsyncManager
    {
        public readonly MainService Main;
        public readonly AsyncManager AsyncManager;

        protected int FetchTicks = 0;

        public NodeManager(MainService main)
        {
            Main = main;

            AsyncManager = new AsyncManager(Run);
        }

        public async Task UpdateNodes()
        {
            PushMessageBase msg = new PushMessageBase()
            {
                Type = PushMessageID.UpdateNodes,
                DataNodes = new NodeList()
            };
            var nodes = await Natfrp.Request("get_nodes");
            lock (this)
            {
                Clear();
                foreach (Dictionary<string, dynamic> j in nodes["data"])
                {
                    var n = new Node()
                    {
                        Id = Utils.CastInt(j["id"]),
                        Name = (string)j["name"],
                        AcceptNew = j["accept_new"]
                    };
                    this[n.Id] = n;
                }
                msg.DataNodes.Nodes.Add(Values);
            }
            Main.Pipe.PushMessage(msg);
        }

        protected void Run()
        {
            while (!AsyncManager.StopEvent.WaitOne(0))
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

        #region IAsyncManager

        public void Start() => AsyncManager.Start();

        public void Stop(bool kill = false) => AsyncManager.Stop(kill);

        #endregion
    }
}
