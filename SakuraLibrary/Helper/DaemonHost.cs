using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Management;
using System.Diagnostics;
using System.ServiceProcess;

using SakuraLibrary.Model;
using SakuraLibrary.Proto;

namespace SakuraLibrary.Helper
{
    public class DaemonHost
    {
        public readonly bool Daemon;
        public readonly LauncherModel Launcher;

        public readonly string ServicePath;

        public Process BaseProcess = null;
        public ServiceController Controller = null;

        public Thread WorkerThread = null;
        public ManualResetEvent StopEvent = new(false);

        public DaemonHost(LauncherModel launcher, bool forceDaemon)
        {
            Controller = forceDaemon ? null : ServiceController.GetServices().FirstOrDefault(s => s.ServiceName == Consts.ServiceName);
            Daemon = Controller == null;
            Launcher = launcher;

            ServicePath = Path.GetFullPath(Consts.ServiceExecutable);

            if (!Daemon)
            {
                try
                {
                    using var searcher = new ManagementObjectSearcher("SELECT * FROM Win32_Service WHERE Name = '" + Consts.ServiceName + "'");
                    using var collection = searcher.Get();

                    var service = collection.OfType<ManagementObject>().FirstOrDefault();
                    if (service != null)
                    {
                        var oldPath = Path.GetFullPath((service.GetPropertyValue("PathName") as string).Trim('"'));
                        var newPath = Path.GetFullPath(Consts.ServiceExecutable);
                        if (oldPath != newPath)
                        {
                            NTAPI.MessageBox(0, "系统服务状态异常, 启动器可能无法正常运行\n请不要在安装系统服务后挪动启动器文件或在其他路径运行启动器\n\n如果无法正常连接到守护进程请点击 \"卸载服务\"\n如果无法正常连接到守护进程请点击 \"卸载服务\"\n如果无法正常连接到守护进程请点击 \"卸载服务\"\n\n服务路径:\n" + oldPath + "\n当前路径:\n" + newPath, "错误", 0x10);
                        }
                    }
                }
                catch (Exception e)
                {
                    NTAPI.MessageBox(0, "出现了一个神秘的错误, 建议截图此错误并联系管理员:\n" + e, "错误", 0x10);
                }
            }
        }

        public bool StartDaemon()
        {
            return false;
            //if (IsRunning())
            //{
            //    return true;
            //}
            //if (!Daemon)
            //{
            //    try
            //    {
            //        Controller.Start();
            //        return true;
            //    }
            //    catch { }
            //    return false;
            //}
            //BaseProcess = Process.Start(ServicePath, "--daemon");
            //return !BaseProcess.HasExited;
        }

        public bool Stop()
        {
            StopEvent.Set();
            return false;
            //if (!IsRunning())
            //{
            //    return true;
            //}
            //try
            //{
            //    if (!Daemon)
            //    {
            //        Controller.Stop();
            //        return true;
            //    }
            //    if (Launcher.Connected)
            //    {
            //        ThreadPool.QueueUserWorkItem(s => Launcher.RPC.Request(MessageID.ControlExit));
            //        BaseProcess.WaitForExit(10000);
            //    }
            //    if (!BaseProcess.HasExited)
            //    {
            //        BaseProcess.Kill();
            //        BaseProcess.WaitForExit(5000);
            //    }
            //    BaseProcess.Dispose();
            //    return true;
            //}
            //catch { }
            //return false;
        }

        public bool IsRunning()
        {
            return false;
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

        public void Start()
        {
            StopEvent.Reset();

            WorkerThread = new Thread(() =>
            {
                while (!StopEvent.WaitOne(500))
                {
                    if (!IsRunning())
                    {
                        StartDaemon();
                    }
                }
            })
            { IsBackground = true };
            WorkerThread.Start();
        }
    }
}
