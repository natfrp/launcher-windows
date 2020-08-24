using System;
using System.Text;
using System.Reflection;
using System.Diagnostics;
using System.Threading.Tasks;

using SakuraLibrary.Proto;
using SakuraLibrary.Helper;

namespace SakuraFrpService.Manager
{
    public class UpdateManager : IAsyncManager
    {
        public readonly MainService Main;
        public readonly AsyncManager AsyncManager;

        public UpdateStatus Status = new UpdateStatus();

        public int UpdateInterval
        {
            get => _updateInterval;
            set
            {
                _updateInterval = value <= 0 ? -1 : Math.Max(value, 3600);
                if (_updateInterval == -1)
                {
                    lock (this)
                    {
                        Status.UpdateFrpc = Status.UpdateLauncher = false;
                    }
                }
            }
        }
        private int _updateInterval;

        private float FrpcSakura = 0;
        private Version FrpcVersion;

        private DateTime LastCheck = DateTime.MinValue;

        public UpdateManager(MainService main)
        {
            Main = main;
            AsyncManager = new AsyncManager(Run);

            UpdateInterval = Properties.Settings.Default.UpdateInterval;
        }

        public bool LoadFrpcVersion()
        {
            try
            {
                using (var p = Process.Start(new ProcessStartInfo(Main.TunnelManager.FrpcPath, "-v")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    StandardOutputEncoding = Encoding.UTF8
                }))
                {
                    if (!p.WaitForExit(100))
                    {
                        p.Kill();
                    }
                    return TryParseFrpcVersion(p.StandardOutput.ReadLine(), out FrpcVersion, out FrpcSakura);
                }
            }
            catch (Exception e)
            {
                Main.LogManager.Log(2, "Service", e.ToString());
                return false;
            }
        }

        public bool TryParseFrpcVersion(string data, out Version version, out float sakura)
        {
            sakura = 0;
            var temp = data.Trim().Split(new string[] { "-sakura-" }, StringSplitOptions.RemoveEmptyEntries);
            if (temp.Length == 0 || temp[0].Length == 0)
            {
                version = null;
                return false;
            }
            if (temp[0][0] == 'v')
            {
                temp[0] = temp[0].Substring(1);
            }
            return Version.TryParse(temp[0], out version) && (temp.Length == 1 || float.TryParse(temp[1], out sakura));
        }

        public async Task<UpdateStatus> CheckUpdate(bool loadEnabled = false)
        {
            var result = await Natfrp.Request<Natfrp.GetVersion>("get_version");
            var status = new UpdateStatus
            {
                UpdateLauncher = Version.TryParse(result.Launcher.Version, out Version launcher) && launcher > Assembly.GetExecutingAssembly().GetName().Version,
                UpdateFrpc = TryParseFrpcVersion(result.Frpc.Version, out Version frpc, out float sakura) && (frpc > FrpcVersion || (FrpcSakura != 0 && sakura > FrpcSakura))
            };
            lock (this)
            {
                LastCheck = DateTime.Now;
            }
            return status;
        }

        protected void Run()
        {
            do
            {
                lock (this)
                {
                    if (UpdateInterval <= 0 || (DateTime.Now - LastCheck).TotalSeconds <= UpdateInterval)
                    {
                        continue;
                    }
                }
                try
                {
                    Status = CheckUpdate().WaitResult();
                    if (Status.UpdateFrpc || Status.UpdateLauncher)
                    {
                        Main.Pipe.PushMessage(new PushMessageBase()
                        {
                            Type = PushMessageID.PushUpdate,
                            DataUpdate = Status
                        });
                    }
                }
                catch (AggregateException e) when (e.InnerExceptions.Count == 1)
                {
                    Main.LogManager.Log(2, "Service", "UpdateManager: 更新检查失败, " + e.InnerExceptions[0].ToString());
                }
                catch (Exception e)
                {
                    Main.LogManager.Log(2, "Service", e.ToString());
                }
            }
            while (!AsyncManager.StopEvent.WaitOne(60 * 1000));
        }

        #region IAsyncManager

        public bool Running => AsyncManager.Running;

        public void Start() => AsyncManager.Start();

        public void Stop(bool kill = false) => AsyncManager.Stop(kill);

        #endregion
    }
}
