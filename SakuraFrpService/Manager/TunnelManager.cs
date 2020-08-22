using System.IO;
using System.Linq;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;

using SakuraLibrary;
using SakuraLibrary.Proto;
using SakuraLibrary.Helper;

using Tunnel = SakuraFrpService.Data.Tunnel;

namespace SakuraFrpService.Manager
{
    public class TunnelManager : Dictionary<int, Tunnel>, IAsyncManager
    {
        public const string FrpcExecutable = "frpc.exe";

        public readonly int PID = Process.GetCurrentProcess().Id;

        public readonly MainService Main;
        public readonly AsyncManager AsyncManager;

        public readonly string FrpcPath;

        public TunnelManager(MainService main)
        {
            Main = main;
            FrpcPath = Path.GetFullPath(FrpcExecutable);

            AsyncManager = new AsyncManager(Run);
        }

        public string GetArguments(int tunnel) => string.Format("-n -f {0}:{1} --watch {2}", Natfrp.Token, tunnel, PID);

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

        public Tunnel CreateFromApi(Natfrp.ApiTunnel j) => new Tunnel(this)
        {
            Id = j.Id,
            Node = j.Node,
            Name = j.Name,
            Type = j.Type,
            Note = j.Note,
            Description = j.Description
        };

        public async Task UpdateTunnels(bool loadEnabled = false)
        {
            var tunnels = await Natfrp.Request<Natfrp.GetTunnels>("get_tunnels");
            lock (this)
            {
                var tmp = new List<int>();
                foreach (var j in tunnels.Data)
                {
                    tmp.Add(j.Id);
                    if (!ContainsKey(j.Id))
                    {
                        this[j.Id] = CreateFromApi(j);
                    }
                }
                foreach (var k in Keys.Where(k => !tmp.Contains(k)))
                {
                    Remove(k);
                }
                if (loadEnabled && Properties.Settings.Default.EnabledTunnels != null)
                {
                    foreach (var i in Properties.Settings.Default.EnabledTunnels)
                    {
                        if (ContainsKey(i))
                        {
                            this[i].Enabled = true;
                        }
                    }
                }
            }
            Push();
        }

        public List<int> GetEnabledTunnels()
        {
            lock (this)
            {
                return this.Where(kv => kv.Value.Enabled).Select(kv => kv.Key).ToList();
            }
        }

        protected void Run()
        {
            bool first = true;
            int delayTicks = 0;
            while (!AsyncManager.StopEvent.WaitOne(50))
            {
                try
                {
                    if (delayTicks-- <= 0)
                    {
                        try
                        {
                            UpdateTunnels(first).Wait();
                            first = false;
                            delayTicks = 20 * 3600;
                        }
                        catch
                        {
                            delayTicks = 20 * 5;
                        }
                    }
                    foreach (var t in Values)
                    {
                        if (t.WaitTick > 0)
                        {
                            t.WaitTick--;
                            continue;
                        }
                        if (t.Enabled)
                        {
                            if (t.Running)
                            {
                                continue;
                            }
                            if (!t.Start())
                            {
                                if (++t.FailCount >= 3)
                                {
                                    t.Enabled = false;
                                    Main.LogManager.Log(LogManager.CATEGORY_SERVICE_ERROR, t.Name, "隧道持续启动失败, 已禁用该隧道");
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
                        }
                        else if (t.Running)
                        {
                            t.Stop();
                        }
                        else
                        {
                            continue;
                        }
                        Main.Save();
                        PushOne(t);
                    }
                }
                catch { }
            }
            StopAll();
        }

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

        #region IAsyncManager

        public bool Running => AsyncManager.Running;

        public void Start()
        {
            foreach (var p in Utils.SearchProcess("frpc", FrpcPath))
            {
                try
                {
                    p.Kill();
                }
                catch { }
            }
            AsyncManager.Start();
        }

        public void Stop(bool kill = false)
        {
            AsyncManager.Stop(kill);
            foreach (var t in Values)
            {
                try
                {
                    t.Stop();
                }
                catch { }
            }
        }

        #endregion
    }
}
