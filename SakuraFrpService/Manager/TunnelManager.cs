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
        public const string Tag = "Service/TunnelManager";

        public const string FrpcExecutable = "frpc.exe";

        public readonly int PID = Process.GetCurrentProcess().Id;

        public readonly MainService Main;
        public readonly AsyncManager AsyncManager;

        public readonly string FrpcPath;
        public readonly string FrpcWorkingDirectory;

        public TunnelManager(MainService main)
        {
            Main = main;
            FrpcPath = Path.GetFullPath(FrpcExecutable);
            FrpcWorkingDirectory = Path.Combine(main.WorkingDirectory, "FrpcWorkingDirectory");

            Directory.CreateDirectory(FrpcWorkingDirectory);

            AsyncManager = new AsyncManager(Run);
        }

        public string GetArguments(int tunnel) => string.Format("-n -f {0}:{1} --watch {2} --report", Natfrp.Token, tunnel, PID);

        public void StopAllAndClear()
        {
            lock (this)
            {
                Parallel.ForEach(Values, t => t.Stop());
                Clear();
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
                bool changed = false;

                // Add new tunnels
                var tmp = new List<int>();
                foreach (var j in tunnels.Data)
                {
                    tmp.Add(j.Id);
                    if (!ContainsKey(j.Id))
                    {
                        changed = true;
                        this[j.Id] = CreateFromApi(j);
                    }
                }

                // Remove deleted tunnels
                var remove = Keys.Where(k => !tmp.Contains(k)).ToList();
                foreach (var k in remove)
                {
                    changed = true;
                    Remove(k);
                }

                // Update tunnel details and set the changed flag accordingly
                void update<T>(ref T dst, T src)
                {
                    if (Comparer<T>.Default.Compare(dst, src) != 0)
                    {
                        changed = true;
                        dst = src;
                    }
                }
                foreach (var j in tunnels.Data)
                {
                    var r = this[j.Id];
                    update(ref r.Id, j.Id);
                    update(ref r.Node, j.Node);
                    update(ref r.Name, j.Name);
                    update(ref r.Type, j.Type);
                    update(ref r.Note, j.Note);
                    update(ref r.Description, j.Description);
                }

                if (changed)
                {
                    Main.LogManager.Log(LogManager.CATEGORY_SERVICE_INFO, Tag, "隧道列表已更新");
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
                            if (!t.Running)
                            {
                                if (t.StartState == 1)
                                {
                                    t.FailAndCleanup(); // Pending start, crash confirmed
                                }
                                else
                                {
                                    if (!t.Start())
                                    {
                                        t.FailAndCleanup();
                                    }
                                    else
                                    {
                                        // Wait for report
                                        t.WaitTick = 20 * 60;
                                    }
                                }
                            }
                            else if (t.StartState != 2)
                            {
                                // No report for 60s
                                t.Stop();
                                t.FailAndCleanup();
                            }
                            else
                            {
                                continue;
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
            StopAllAndClear();
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
            var killed = new List<Process>();
            foreach (var p in Utils.SearchProcess("frpc", FrpcPath))
            {
                try
                {
                    Main.LogManager.Log(LogManager.CATEGORY_SERVICE_INFO, Tag, "正在结束残留 frpc 进程: #" + p.Id);
                    p.Kill();
                    killed.Add(p);
                }
                catch { }
            }
            Parallel.ForEach(killed, p =>
            {
                try
                {
                    if (!p.WaitForExit(1000))
                    {
                        Main.LogManager.Log(LogManager.CATEGORY_SERVICE_ERROR, Tag, "无法结束残留 frpc 进程: #" + p.Id);
                    }
                }
                catch { }
            });
            AsyncManager.Start();
        }

        public void Stop(bool kill = false)
        {
            AsyncManager.Stop(kill);
            StopAllAndClear();
        }

        #endregion
    }
}
