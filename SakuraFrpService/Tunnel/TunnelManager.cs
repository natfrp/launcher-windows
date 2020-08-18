using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

namespace SakuraFrpService.Tunnel
{
    public class TunnelManager : Dictionary<int, Tunnel>
    {
        public const string FrpcExecutable = "frpc.exe";

        public readonly MainService Main;

        public readonly string FrpcPath;
        public readonly Thread MainThread;

        protected string UserToken = null;
        protected ManualResetEvent stopEvent = new ManualResetEvent(false);

        public TunnelManager(MainService main)
        {
            Main = main;
            FrpcPath = Path.GetFullPath(FrpcExecutable);
            MainThread = new Thread(new ThreadStart(Run));
        }

        public string GetArguments(int tunnel) => "-n -f " + Main.UserToken + ":" + tunnel;

        public void AddJson(Dictionary<string, dynamic> json)
        {
            lock (this)
            {
                this[json["id"]] = new Tunnel(this)
                {
                    Id = json["id"],
                    Node = json["node"],
                    Name = json["name"],
                    Type = json["type"],
                    Description = json["description"]
                };
            }
        }

        public void StopAll()
        {
            lock (this)
            {
                foreach (var t in Values)
                {
                    t.Stop();
                }
            }
        }

        public void SetEnabledTunnels(List<int> ids)
        {
            lock (this)
            {
                foreach (var i in ids)
                {
                    if (ContainsKey(i))
                    {
                        this[i].Enabled = true;
                    }
                }
            }
        }

        public List<int> GetEnabledTunnels()
        {
            lock (this)
            {
                return this.Where(kv => kv.Value.Enabled).Select(kv => kv.Key).ToList();
            }
        }

        #region Async Work

        public void Start(string token)
        {
            if (MainThread.IsAlive)
            {
                return;
            }
            UserToken = token;
            stopEvent.Reset();
            MainThread.Start();
        }

        public void Stop() => stopEvent.Set();

        protected void Run()
        {
            while (!stopEvent.WaitOne(0))
            {
                try
                {
                    // TODO: Fetch tunnels periodically
                    // TODO: Control tunnel start / stop / retry status
                }
                catch { }
            }
            StopAll();
        }

        #endregion

        #region Dictionary Overload

        public new Tunnel this[int key]
        {
            get
            {
                lock (this)
                {
                    return base[key];
                }
            }
            set
            {
                lock (this)
                {
                    base[key] = value;
                }
            }
        }

        protected new void Add(int k, Tunnel v) => throw new NotImplementedException();

        #endregion
    }
}
