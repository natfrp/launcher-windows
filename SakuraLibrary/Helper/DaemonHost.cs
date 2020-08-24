using System.IO;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.ServiceProcess;

using SakuraLibrary.Model;
using SakuraLibrary.Proto;

namespace SakuraLibrary.Helper
{
    public class DaemonHost : IAsyncManager
    {
        public readonly bool Daemon;
        public readonly AsyncManager AsyncManager;
        public readonly LauncherModel Launcher;

        public readonly string ServicePath;

        public Process BaseProcess = null;
        public ServiceController Controller = null;

        public DaemonHost(LauncherModel launcher)
        {
            Controller = ServiceController.GetServices().FirstOrDefault(s => s.ServiceName == Consts.ServiceName);
            Daemon = Controller == null;
            Launcher = launcher;

            ServicePath = Path.GetFullPath(Consts.ServiceExecutable);

            AsyncManager = new AsyncManager(Run);
        }

        public bool StartDaemon()
        {
            if (IsRunning())
            {
                return true;
            }
            if (!Daemon)
            {
                try
                {
                    Controller.Start();
                    return true;
                }
                catch { }
                return false;
            }
            BaseProcess = Process.Start(ServicePath, "--daemon");
            return !BaseProcess.HasExited;
        }

        public bool StopDaemon()
        {
            if (!IsRunning())
            {
                return true;
            }
            try
            {
                if (!Daemon)
                {
                    Controller.Stop();
                    return true;
                }
                if (Launcher.Connected)
                {
                    ThreadPool.QueueUserWorkItem(s => Launcher.Pipe.Request(MessageID.ControlExit));
                    BaseProcess.WaitForExit(10000);
                }
                if (!BaseProcess.HasExited)
                {
                    BaseProcess.Kill();
                }
                BaseProcess.Dispose();
                return true;
            }
            catch { }
            return false;
        }

        public bool IsRunning()
        {
            if (!Daemon)
            {
                Controller.Refresh();
                return Controller.Status == ServiceControllerStatus.Running;
            }
            if (BaseProcess != null && !BaseProcess.HasExited)
            {
                return true;
            }
            var processes = Utils.SearchProcess("SakuraFrpService", ServicePath);
            if (processes.Length == 0)
            {
                return false;
            }
            if (BaseProcess == null)
            {
                BaseProcess = processes[0];
            }
            return true;
        }

        public bool InstallService(bool uninstall = false)
        {
            try
            {
                var p = Process.Start(new ProcessStartInfo(ServicePath, uninstall ? "--uninstall" : "--install")
                {
                    Verb = "runAs"
                });
                p.WaitForExit();
                return p.ExitCode == 0;
            }
            catch { }
            return false;
        }

        protected void Run()
        {
            while (!AsyncManager.StopEvent.WaitOne(500))
            {
                if (!IsRunning())
                {
                    StartDaemon();
                }
            }
        }

        #region IAsyncManager

        public bool Running => AsyncManager.Running;

        public void Start() => AsyncManager.Start(true);

        public void Stop(bool kill = false)
        {
            AsyncManager.Stop(kill);
            StopDaemon();
        }

        #endregion
    }
}
