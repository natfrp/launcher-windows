﻿using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

using SakuraLibrary;
using SakuraLibrary.Proto;

using Tunnel = SakuraFrpService.Data.Tunnel;

namespace SakuraFrpService.Manager
{
    public class TunnelManager : Dictionary<int, Tunnel>
    {
        public const string FrpcExecutable = "frpc.exe";

        public readonly MainService Main;

        public readonly string FrpcPath;
        public readonly Thread MainThread;

        protected bool FirstFetch = true;
        protected int FetchTicks = 0;
        protected string UserToken = null;
        protected ManualResetEvent stopEvent = new ManualResetEvent(false);

        public TunnelManager(MainService main)
        {
            Main = main;
            FrpcPath = Path.GetFullPath(FrpcExecutable);
            MainThread = new Thread(new ThreadStart(Run));
        }

        public string GetArguments(int tunnel) => "-n -f " + Natfrp.Token + ":" + tunnel;

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

        public void Push()
        {
            PushMessageBase msg = new PushMessageBase()
            {
                Type = PushMessageID.UpdateTunnels,
                DataTunnels = new TunnelList()
            };
            lock (this)
            {
                msg.DataTunnels.Tunnels.Add(Values.Select(t => t.CreateProto()));
            }
            Main.Pipe.PushMessage(msg);
        }

        public void PushOne(Tunnel t) => Main.Pipe.PushMessage(new PushMessageBase()
        {
            Type = PushMessageID.UpdateTunnel,
            DataTunnel = t.CreateProto()
        });

        public Tunnel ParseJson(dynamic j) => new Tunnel(this)
        {
            Id = Utils.CastInt(j["id"]),
            Node = Utils.CastInt(j["node"]),
            Name = (string)j["name"],
            Type = (string)j["type"],
            Note = (string)j["note"],
            Description = (string)j["description"]
        };

        public async Task UpdateTunnels()
        {
            var tunnels = await Natfrp.Request("get_tunnels");
            lock (this)
            {
                var tmp = new List<int>();
                foreach (Dictionary<string, dynamic> j in tunnels["data"])
                {
                    var id = Utils.CastInt(j["id"]);
                    tmp.Add(id);
                    if (!ContainsKey(id))
                    {
                        this[id] = ParseJson(j);
                    }
                }
                foreach (var k in Keys.Where(k => !tmp.Contains(k)))
                {
                    Remove(k);
                }
                if (FirstFetch)
                {
                    FirstFetch = false;
                    SetEnabledTunnels(Properties.Settings.Default.EnabledTunnels);
                }
                // TODO: We might need to update EnabledTunnels each time we fetch
            }
            Push();
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
                MainThread.Abort(); // Shouldn't happen, just in case
            }

            foreach (var p in Utils.SearchProcess("frpc", FrpcPath)) // Get rid of leftover processes
            {
                try
                {
                    p.Kill();
                }
                catch { }
            }

            UserToken = token;
            FirstFetch = true;

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
            while (!stopEvent.WaitOne(0))
            {
                Thread.Sleep(50);
                try
                {
                    if (FetchTicks-- <= 0)
                    {
                        try
                        {
                            UpdateTunnels().Wait();
                            FetchTicks = 20 * 3600;
                        }
                        catch
                        {
                            FetchTicks = 20 * 5;
                        }
                    }
                    foreach (var t in Values)
                    {
                        if (t.Enabled)
                        {
                            if (t.Running)
                            {
                                continue;
                            }
                            if (t.WaitTick > 0)
                            {
                                t.WaitTick--;
                                continue;
                            }
                            if (!t.Start())
                            {
                                if (++t.FailCount >= 3)
                                {
                                    t.Enabled = false;
                                    Main.LogManager.Log(t.Name, "隧道持续启动失败, 已禁用该隧道");
                                }
                                else
                                {
                                    t.WaitTick = 20 * 5 * t.FailCount;
                                }
                            }
                            else
                            {
                                t.FailCount = 0;
                            }
                            PushOne(t);
                        }
                        else if (t.Running)
                        {
                            t.Stop();
                            PushOne(t);
                        }
                    }
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

        public new void Clear()
        {
            lock (this)
            {
                StopAll();
                base.Clear();
            }
        }

        public new void Remove(int k)
        {
            lock (this)
            {
                if (!ContainsKey(k))
                {
                    return;
                }
                this[k].Stop();
                base.Remove(k);
            }
        }

        #endregion
    }
}