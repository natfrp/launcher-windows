using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using SakuraLibrary.Pipe;
using SakuraLibrary.Proto;
using SakuraLibrary.Helper;

using UserStatus = SakuraLibrary.Proto.User.Types.Status;

namespace SakuraLibrary.Model
{
    public abstract class LauncherModel : ModelBase, IAsyncManager
    {
        public readonly PipeClient Pipe = new PipeClient(Utils.InstallationPipeName);
        public readonly DaemonHost Daemon;
        public readonly AsyncManager AsyncManager;

        public LauncherModel(bool forceDaemon = false)
        {
            Daemon = new DaemonHost(this, forceDaemon);
            AsyncManager = new AsyncManager(Run);

            Pipe.ServerPush += Pipe_ServerPush;

            Daemon.Start();
            Start();
        }

        public abstract void Log(Log l, bool init = false);
        public abstract void ClearLog();

        public abstract void Save();

        protected void launcherError(string message) => Log(new Log()
        {
            Category = 3,
            Source = "Launcher",
            Data = message,
            Time = Utils.GetSakuraTime()
        });

        #region Daemon Sync

        public bool SyncUser()
        {
            var user = Pipe.Request(MessageID.UserInfo);
            if (user.Success)
            {
                UserInfo = user.DataUser;
            }
            return user.Success;
        }

        /// <summary>
        /// Sync all with daemon except UserInfo
        /// </summary>
        public virtual bool SyncAll()
        {
            if (!SyncLog() || !SyncConfig() || !SyncUpdate())
            {
                return false;
            }
            SyncNodes();
            SyncTunnels();
            return true;
        }

        public bool SyncLog()
        {
            var logs = Pipe.Request(MessageID.LogGet);
            if (logs.Success)
            {
                Dispatcher.Invoke(() =>
                {
                    ClearLog();
                    foreach (var l in logs.DataLog.Data)
                    {
                        Log(l, true);
                    }
                });
            }
            else
            {
                launcherError("SyncLog 失败: " + logs.Message);
            }
            return logs.Success;
        }

        public bool SyncConfig()
        {
            var config = Pipe.Request(MessageID.ControlConfigGet);
            if (config.Success)
            {
                Config = config.DataConfig;
            }
            else
            {
                launcherError("SyncConfig 失败: " + config.Message);
            }
            return config.Success;
        }

        public bool SyncUpdate()
        {
            var update = Pipe.Request(MessageID.ControlGetUpdate);
            if (update.Success)
            {
                Update = update.DataUpdate;
            }
            else
            {
                launcherError("SyncUpdate 失败: " + update.Message);
            }
            return update.Success;
        }

        public bool SyncNodes(bool reload = false)
        {
            if (reload)
            {
                if (!Pipe.Request(MessageID.NodeReload).Success)
                {
                    return false;
                }
            }
            var nodes = Pipe.Request(MessageID.NodeList);
            if (nodes.Success)
            {
                Dispatcher.Invoke(() =>
                {
                    Nodes.Clear();
                    foreach (var n in nodes.DataNodes.Nodes)
                    {
                        Nodes.Add(new NodeModel(n));
                    }
                });
            }
            return nodes.Success;
        }

        public bool SyncTunnels()
        {
            var tunnels = Pipe.Request(MessageID.TunnelList);
            if (tunnels.Success)
            {
                LoadTunnels(tunnels.DataTunnels);
            }
            return tunnels.Success;
        }

        #endregion

        #region IPC Handling

        protected void Run()
        {
            do
            {
                lock (Pipe)
                {
                    if (Pipe.Connected)
                    {
                        continue;
                    }
                    Connected = false;

                    if (!Pipe.Connect())
                    {
                        continue;
                    }
                    if (!SyncUser() || !SyncAll())
                    {
                        Pipe.Dispose();
                        continue;
                    }
                    Connected = true;
                }
            }
            while (!AsyncManager.StopEvent.WaitOne(500));
        }

        protected void Pipe_ServerPush(ServiceConnection connection, PushMessageBase msg)
        {
            try
            {
                switch (msg.Type)
                {
                case PushMessageID.UpdateUser:
                    UserInfo = msg.DataUser;
                    break;
                case PushMessageID.UpdateTunnel:
                    Dispatcher.Invoke(() =>
                    {
                        foreach (var t in Tunnels)
                        {
                            if (t.Id == msg.DataTunnel.Id)
                            {
                                t.Proto = msg.DataTunnel;
                                t.SetNodeName(Nodes.ToDictionary(k => k.Id, v => v.Name));
                                break;
                            }
                        }
                    });
                    break;
                case PushMessageID.UpdateTunnels:
                    LoadTunnels(msg.DataTunnels);
                    break;
                case PushMessageID.UpdateNodes:
                    Dispatcher.Invoke(() =>
                    {
                        Nodes.Clear();
                        var map = new Dictionary<int, string>();
                        foreach (var n in msg.DataNodes.Nodes)
                        {
                            Nodes.Add(new NodeModel(n));
                            map.Add(n.Id, n.Name);
                        }
                        foreach (var t in Tunnels)
                        {
                            t.SetNodeName(map);
                        }
                    });
                    break;
                case PushMessageID.AppendLog:
                    Dispatcher.Invoke(() =>
                    {
                        foreach (var l in msg.DataLog.Data)
                        {
                            Log(l);
                        }
                    });
                    break;
                case PushMessageID.PushUpdate:
                    Update = msg.DataUpdate;
                    break;
                case PushMessageID.PushConfig:
                    Config = msg.DataConfig;
                    break;
                }
            }
            catch { }
        }

        #endregion

        #region Main Window

        public bool Connected { get => _connected; set => Set(out _connected, value); }
        private bool _connected = false;

        public User UserInfo
        {
            get => _userInfo;
            set
            {
                if (value == null)
                {
                    value = new User();
                }
                SafeSet(out _userInfo, value);
            }
        }
        private User _userInfo = new User();

        public ObservableCollection<NodeModel> Nodes { get; set; } = new ObservableCollection<NodeModel>();

        #endregion

        #region Tunnel

        public ObservableCollection<TunnelModel> Tunnels { get; set; } = new ObservableCollection<TunnelModel>();

        public void RequestReloadTunnels(Action<bool, string> callback) => ThreadPool.QueueUserWorkItem(s =>
         {
             try
             {
                 var resp = Pipe.Request(MessageID.TunnelReload);
                 callback(resp.Success, resp.Message);
             }
             catch (Exception e)
             {
                 callback(false, e.ToString());
             }
         });

        public void RequestDeleteTunnel(int id, Action<bool, string> callback) => ThreadPool.QueueUserWorkItem(s =>
        {
            try
            {
                var resp = Pipe.Request(new RequestBase()
                {
                    Type = MessageID.TunnelDelete,
                    DataId = id
                });
                callback(resp.Success, resp.Message);
            }
            catch (Exception e)
            {
                callback(false, e.ToString());
            }
        });

        protected void LoadTunnels(TunnelList list) => Dispatcher.Invoke(() =>
        {
            Tunnels.Clear();
            var map = Nodes.ToDictionary(k => k.Id, v => v.Name);
            foreach (var t in list.Tunnels)
            {
                Tunnels.Add(new TunnelModel(t, this, map));
            }
        });

        #endregion

        #region Settings - User Status

        [SourceBinding(nameof(UserInfo))]
        public string UserToken { get => UserInfo.Status != UserStatus.NoLogin ? "****************" : _userToken; set => SafeSet(out _userToken, value); }
        private string _userToken = "";

        [SourceBinding(nameof(UserInfo))]
        public bool LoggedIn => UserInfo.Status == UserStatus.LoggedIn;

        [SourceBinding(nameof(LoggingIn), nameof(LoggedIn))]
        public bool TokenEditable => !LoggingIn && !LoggedIn;

        [SourceBinding(nameof(UserInfo))]
        public bool LoggingIn { get => _loggingIn || UserInfo.Status == UserStatus.Pending; set => SafeSet(out _loggingIn, value); }
        private bool _loggingIn;

        public void RequestLogin(Action<bool, string> callback)
        {
            LoggingIn = true;
            ThreadPool.QueueUserWorkItem(s =>
            {
                try
                {
                    var resp = Pipe.Request(LoggedIn ? new RequestBase()
                    {
                        Type = MessageID.UserLogout
                    } : new RequestBase()
                    {
                        Type = MessageID.UserLogin,
                        DataUserLogin = new UserLogin()
                        {
                            Token = UserToken
                        }
                    });
                    callback(resp.Success, resp.Message);
                    SyncAll();
                }
                catch (Exception e)
                {
                    callback(false, e.ToString());
                }
                finally
                {
                    LoggingIn = false;
                }
            });
        }

        #endregion

        #region Settings - Launcher

        /// <summary>
        /// 0 = Show all
        /// 1 = Suppress all
        /// 2 = Suppress INFO
        /// </summary>
        public int NotificationMode { get => _notificationMode; set => Set(out _notificationMode, value); }
        private int _notificationMode;

        public bool LogTextWrapping { get => _logTextWrapping; set => Set(out _logTextWrapping, value); }
        private bool _logTextWrapping;

        #endregion

        #region Settings - Service

        public ServiceConfig Config { get => _config;
            set => SafeSet(out _config, value); }
        private ServiceConfig _config;

        [SourceBinding(nameof(Config))]
        public bool BypassProxy
        {
            get => Config != null && Config.BypassProxy;
            set
            {
                if (Config != null)
                {
                    Config.BypassProxy = value;
                    PushServiceConfig();
                }
                RaisePropertyChanged();
            }
        }

        public void PushServiceConfig()
        {
            var result = Pipe.Request(new RequestBase()
            {
                Type = MessageID.ControlConfigSet,
                DataConfig = Config
            });
            if (!result.Success)
            {
                launcherError("无法更新守护进程配置: " + result.Message);
            }
        }

        #endregion

        #region Settings - Auto Update

        [SourceBinding(nameof(Config))]
        public bool RemoteManagement
        {
            get => Config != null && Config.RemoteManagement;
            set
            {
                if (Config != null)
                {
                    if (!value || Config.RemoteKeySet)
                    {
                        Config.RemoteManagement = value;
                    }
                    PushServiceConfig();
                }
                RaisePropertyChanged();
            }
        }

        [SourceBinding(nameof(Config), nameof(LoggedIn))]
        public bool CanEnableRemoteManagement => LoggedIn && Config != null && Config.RemoteKeySet;

        [SourceBinding(nameof(Config))]
        public bool EnableTLS
        {
            get => Config != null && Config.EnableTls;
            set
            {
                if (Config != null)
                {
                    Config.EnableTls = value;
                    PushServiceConfig();
                }
                RaisePropertyChanged();
            }
        }

        public UpdateStatus Update { get => _update; set => SafeSet(out _update, value); }
        private UpdateStatus _update;

        [SourceBinding(nameof(Update))]
        public bool HaveUpdate => Update != null && Update.UpdateManagerRunning && Update.UpdateAvailable;

        [SourceBinding(nameof(Update))]
        public string UpdateText
        {
            get
            {
                if (Update == null || !Update.UpdateAvailable)
                {
                    return "";
                }
                if (Update.UpdateReadyDir != "")
                {
                    return "更新准备完成, 点此进行更新";
                }
                return "下载更新中... " + Math.Round(Update.DownloadCurrent / 1048576f, 2) + " MiB/" + Math.Round(Update.DownloadTotal / 1048576f, 2) + " MiB";
            }
        }

        [SourceBinding(nameof(Config), nameof(Update))]
        public bool CheckUpdate
        {
            get => Config != null && Config.UpdateInterval != -1 && Update != null && Update.UpdateManagerRunning;
            set
            {
                if (Config != null)
                {
                    Config.UpdateInterval = value ? 86400 : -1;
                    PushServiceConfig();
                }
                if (!value)
                {
                    Update = null;
                }
                RaisePropertyChanged();
            }
        }

        public void RequestUpdateCheck() => Pipe.Request(MessageID.ControlCheckUpdate);

        public void ConfirmUpdate(bool legacy, Action<bool, string> callback, Func<string, bool> confirm, Func<string, bool> warn)
        {
            if (Update.UpdateReadyDir == "")
            {
                return;
            }
            if (!confirm(Update.Note))
            {
                return;
            }
            if (NTAPI.GetSystemMetrics(SystemMetric.SM_REMOTESESSION) != 0 && !warn("检测到当前正在使用远程桌面连接，若您正在通过 SakuraFrp 连接计算机，请勿进行更新\n进行更新时启动器和所有 frpc 将彻底退出并且需要手动确认操作，这会造成远程桌面断开并且无法恢复\n是否继续?"))
            {
                return;
            }
            Daemon.Stop();
            try
            {
                Process.Start(new ProcessStartInfo(Consts.ServiceExecutable, "--update \"" + Update.UpdateReadyDir.TrimEnd('\\') + "\" " + (legacy ? "legacy" : "wpf"))
                {
                    Verb = "runas"
                });
            }
            finally
            {
                Environment.Exit(0);
            }
        }

        #endregion

        #region Settings - Working Mode

        public bool IsDaemon => Daemon.Daemon;

        public string WorkingMode => Daemon.Daemon ? "守护进程" : "系统服务";

        public bool SwitchingMode { get => _switchingMode; set => SafeSet(out _switchingMode, value); }
        private bool _switchingMode;

        public void SwitchWorkingMode(Action<bool, string> callback, Func<string, bool> confirm)
        {
            if (SwitchingMode)
            {
                return;
            }
            if (LoggingIn || LoggedIn)
            {
                callback(false, "请先登出当前账户");
                return;
            }
            if (!confirm("确定要切换运行模式吗?\n如果您不知道该操作的作用, 请不要切换运行模式\n如果您不知道该操作的作用, 请不要切换运行模式\n如果您不知道该操作的作用, 请不要切换运行模式\n\n注意事项:\n1. 切换运行模式后不要移动启动器到其他目录, 否则会造成严重错误\n2. 如需移动或卸载启动器, 请先切到 \"守护进程\" 模式来避免文件残留\n3. 切换过程可能需要十余秒, 请耐心等待, 不要做其他操作\n4. 切换操作即为 安装/卸载 系统服务, 需要管理员权限\n5. 切换完成后需要重启启动器"))
            {
                return;
            }
            SwitchingMode = true;
            ThreadPool.QueueUserWorkItem(s =>
            {
                try
                {
                    Daemon.Stop();
                    if (!Daemon.InstallService(!Daemon.Daemon))
                    {
                        callback(false, "运行模式切换失败, 请检查您是否有足够的权限 安装/卸载 服务.\n由于发生严重错误, 启动器即将退出.");
                    }
                    else
                    {
                        callback(true, "运行模式已切换, 启动器即将退出");
                    }
                    Environment.Exit(0);
                }
                finally
                {
                    SwitchingMode = false;
                }
            });
        }

        #endregion

        #region IAsyncManager

        public bool Running => AsyncManager.Running;

        public void Start() => AsyncManager.Start(true);

        public void Stop(bool kill = false) => AsyncManager.Stop(kill);

        #endregion
    }
}
