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
        public readonly string ServicePath = Path.GetFullPath(Consts.ServiceExecutable);

        public readonly bool ForceDaemon;
        public readonly LauncherModel Launcher;

        public bool Daemon;

        public Process BaseProcess = null;
        public ServiceController Controller = null;

        public Thread WorkerThread = null;
        public ManualResetEvent StopEvent = new(false);

        public DaemonHost(LauncherModel launcher, bool forceDaemon)
        {
            ForceDaemon = forceDaemon;
            Launcher = launcher;

            DetectMode();
        }

        public void DetectMode()
        {
            Controller = ForceDaemon ? null : ServiceController.GetServices().FirstOrDefault(s => s.ServiceName == Consts.ServiceName);
            Daemon = Controller == null;

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

        public void Start()
        {
            StopEvent.Reset();

            WorkerThread = new Thread(() =>
            {
                while (!StopEvent.WaitOne(500))
                {
                    if (IsRunning())
                    {
                        continue;
                    }
                    try
                    {
                        if (!Daemon)
                        {
                            Controller.Start();
                        }
                        BaseProcess = Process.Start(new ProcessStartInfo(ServicePath, "--daemon")
                        {
                            UseShellExecute = false,
                            CreateNoWindow = true,
                        });
                    }
                    catch { }
                }
            })
            { IsBackground = true };
            WorkerThread.Start();
        }

        public bool Stop()
        {
            StopEvent.Set();
            WorkerThread.Join();

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
                    ThreadPool.QueueUserWorkItem(s =>
                    {
                        try
                        {
                            Launcher.RPC.Shutdown(new Empty());
                        }
                        catch { }
                    });
                    BaseProcess.WaitForExit(10000);
                }
                if (!BaseProcess.HasExited)
                {
                    BaseProcess.Kill();
                    BaseProcess.WaitForExit(5000);
                }
                BaseProcess.Dispose();
                BaseProcess = null;
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
                return Controller.Status == ServiceControllerStatus.Running || Controller.Status == ServiceControllerStatus.StartPending;
            }
            if (BaseProcess != null && !BaseProcess.HasExited)
            {
                return true;
            }
            var processes = Utils.SearchProcess(Consts.ServiceName, ServicePath);
            if (processes.Length == 0)
            {
                return false;
            }
            BaseProcess ??= processes[0];
            return true;
        }

        public bool SwitchMode()
        {
            if (ForceDaemon) return false;
            try
            {
                var p = Process.Start(new ProcessStartInfo(ServicePath, Daemon ? "--install" : "--uninstall")
                {
                    Verb = "runAs",
                    WindowStyle = ProcessWindowStyle.Hidden,
                });
                p.WaitForExit();
                return p.ExitCode == 0;
            }
            catch (Exception e)
            {
                NTAPI.MessageBox(0, e.ToString(), "运行模式切换失败", 0x10);
            }
            finally
            {
                DetectMode();
            }
            return false;
        }
    }
}
