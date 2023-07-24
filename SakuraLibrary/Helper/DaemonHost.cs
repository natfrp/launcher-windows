using SakuraLibrary.Model;
using SakuraLibrary.Proto;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.ServiceProcess;
using System.Text;
using System.Threading;

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
                            Launcher.ShowMessage("系统服务状态异常, 启动器可能无法正常运行\n请不要在安装系统服务后挪动启动器文件或在其他路径运行启动器\n\n【如果无法正常连接到守护进程请点击 \"卸载服务\"】\n【如果无法正常连接到守护进程请点击 \"卸载服务\"】\n\n服务路径:\n" + oldPath + "\n预期路径:\n" + newPath, "警告", LauncherModel.MessageMode.Warning);
                        }
                    }
                }
                catch (Exception e)
                {
                    Launcher.ShowMessage("无法检查系统服务安装是否正确, 启动器可能无法正常工作\n\n【如果无法正常连接到守护进程请点击 \"卸载服务\"】\n【如果无法正常连接到守护进程请点击 \"卸载服务\"】\n\n错误信息：\n" + e.ToString(), "警告", LauncherModel.MessageMode.Error);
                }
            }
        }

        public void Start()
        {
            // Don't put this in constructer, the dispatcher is not initialized yet
            DetectMode();

            StopEvent.Reset();

            WorkerThread = new Thread(() =>
            {
                var counter = 0;
                var suppress = false;
                do
                {
                    if (Launcher.Connected || IsRunning())
                    {
                        continue;
                    }
                    try
                    {
                        if (!Daemon)
                        {
                            Controller.Start();
                            continue;
                        }
                        var p = Process.Start(new ProcessStartInfo(ServicePath, "--daemon")
                        {
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            RedirectStandardError = true,
                            StandardErrorEncoding = Encoding.UTF8,
                        });
                        p.Exited += (s, e) =>
                        {
                            if (p.ExitCode == 0 || suppress)
                            {
                                return;
                            }
                            var msg = p.StandardError.ReadToEnd().Trim();
                            if (msg == string.Empty)
                            {
                                return;
                            }
                            switch (Launcher.ShowMessage("按 \"忽略\" 屏蔽此提示, \"终止\" 退出启动器\n\n错误信息:\n" + msg.ToString(), "守护进程异常退出", LauncherModel.MessageMode.AbortRetryIgnore | LauncherModel.MessageMode.Error))
                            {
                            case LauncherModel.MessageResult.Abort:
                                Environment.Exit(1);
                                break;
                            case LauncherModel.MessageResult.Ignore:
                                suppress = true;
                                break;
                            }
                        };
                        BaseProcess = p;
                    }
                    catch (Exception e)
                    {
                        if (suppress || counter++ <= 3)
                        {
                            continue;
                        }
                        switch (Launcher.ShowMessage("按 \"忽略\" 屏蔽此提示, \"终止\" 退出启动器\n\n屏蔽此提示后, 可以在 WPF 启动器中切换运行模式来尝试修复此问题\n若切换运行模式无法解决, 请尝试添加启动器目录到杀软白名单并重装启动器\n\n错误信息:\n" + e.ToString(), "守护进程启动失败", LauncherModel.MessageMode.AbortRetryIgnore | LauncherModel.MessageMode.Error))
                        {
                        case LauncherModel.MessageResult.Abort:
                            Environment.Exit(1);
                            break;
                        case LauncherModel.MessageResult.Ignore:
                            suppress = true;
                            break;
                        }
                    }
                }
                while (!StopEvent.WaitOne(1000));
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
                // The daemon is not managed by this host, but it do exists, request exit anyway
                if (Launcher.Connected)
                {
                    try
                    {
                        Launcher.RPC.Shutdown(new Empty());
                    }
                    catch { }
                }
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
                Launcher.ShowMessage(e.ToString(), "运行模式切换失败", LauncherModel.MessageMode.Error);
            }
            finally
            {
                DetectMode();
            }
            return false;
        }
    }
}
