using Grpc.Net.Client;
using SakuraLibrary.Helper;
using SakuraLibrary.Proto;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Pipes;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using NatfrpServiceClient = SakuraLibrary.Proto.NatfrpService.NatfrpServiceClient;
using UserStatus = SakuraLibrary.Proto.User.Types.Status;

namespace SakuraLibrary.Model
{
    public abstract class LauncherModel : ModelBase
    {
        static LauncherModel()
        {
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2Support", true);
            AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
        }

        public static Empty RpcEmpty = new();

        public readonly NatfrpServiceClient RPC = new(GrpcChannel.ForAddress("http://localhost", new GrpcChannelOptions
        {
            HttpHandler = new StandardSocketsHttpHandler()
            {
                ConnectCallback = async (context, cancellationToken) =>
                {
                    var pipe = new NamedPipeClientStream(".", Consts.PipeName, PipeDirection.InOut, PipeOptions.Asynchronous);
                    try
                    {
                        await pipe.ConnectAsync(cancellationToken);
                    }
                    catch
                    {
                        pipe.Dispose();
                    }
                    return pipe;
                }
            }
        }));
        public readonly DaemonHost Daemon;

        public readonly CancellationTokenSource CTS = new();

        public LauncherModel(bool forceDaemon = false)
        {
            Daemon = new DaemonHost(this, forceDaemon);
        }

        protected async void Run()
        {
            Daemon.Start();

            while (!CTS.IsCancellationRequested)
            {
                try
                {
                    var tasks = new Task[]
                    {
                        await RPC.StreamConfig(RpcEmpty).InitStream(c => Config = c, CTS.Token),
                        await RPC.StreamUser(RpcEmpty).InitStream(u => {
                            var us = UserInfo.Status != u.Status;
                            UserInfo = u;
                            // Make sure the SwitchTab don't get triggered by mistake
                            if (us)
                            {
                                Dispatcher.Invoke(() => RaisePropertyChanged("_Login_State"));
                            }
                        }, CTS.Token),
                        await RPC.StreamLog(RpcEmpty).InitStream(l =>
                        {
                            if (l.Category != Proto.Log.Types.Category.Unknown)
                            {
                                 Log(l);
                            }
                        }, CTS.Token),
                        await RPC.StreamNodes(RpcEmpty).InitStream(n => Nodes = n.Nodes, CTS.Token),
                        await RPC.StreamTunnels(RpcEmpty).InitStream(t => Dispatcher.Invoke(() =>
                        {
                            switch(t.Action)
                            {
                            case TunnelUpdate.Types.Action.Unknown: // dummy update
                                break;
                            case TunnelUpdate.Types.Action.Add:
                                Tunnels.Add(new TunnelModel(t.Tunnel, this));
                                break;
                            case TunnelUpdate.Types.Action.Update:
                                var find = Tunnels.FirstOrDefault(x => x.Id == t.Tunnel.Id);
                                if (find != null)
                                {
                                    find.Proto = t.Tunnel;
                                }
                                break;
                            case TunnelUpdate.Types.Action.Delete:
                                Tunnels.Remove(Tunnels.FirstOrDefault(x => x.Id == t.Tunnel.Id));
                                break;
                            case TunnelUpdate.Types.Action.Clear:
                                Tunnels.Clear();
                                break;
                            }
                        }), CTS.Token),
                    };

                    Connected = true;

                    await Task.WhenAll(tasks);
                }
                catch (Exception ex)
                {
                    Connected = false;
                    Console.WriteLine(ex);
                }

                // In case of disconnect, reset UI state
                UserInfo = new User { Status = UserStatus.Pending };
                Dispatcher.Invoke(() =>
                {
                    Nodes.Clear();
                    Tunnels.Clear();
                    ClearLog();
                });
                await Task.Delay(1000);
            }
        }

        #region ViewModel Abstraction

        public enum MessageMode
        {
            Info,
            Warning,
            Error,
            Confirm,
        }

        public abstract void Log(Log l, bool init = false);
        public abstract void ClearLog();
        public abstract bool ShowMessage(string message, string title, MessageMode mode);
        public abstract void Save();

        #endregion

        #region Main Window

        public bool Connected { get => _connected; set => Set(out _connected, value); }
        private bool _connected = false;

        public User UserInfo { get => _userInfo; set => SafeSet(out _userInfo, value ?? new User()); }
        private User _userInfo = new() { Status = UserStatus.Pending };

        public IDictionary<int, Node> Nodes { get => _nodes; set => SafeSet(out _nodes, value); }
        private IDictionary<int, Node> _nodes = new Dictionary<int, Node>();

        public ObservableCollection<TunnelModel> Tunnels { get; set; } = new ObservableCollection<TunnelModel>();

        #endregion

        #region RPC Wrappers

        public async Task RequestReloadNodesAsync() => await RPC.ReloadNodesAsync(RpcEmpty);

        public async Task RequestReloadTunnelsAsync() => await RPC.ReloadTunnelsAsync(RpcEmpty);

        public async Task<TunnelUpdate> RequestCreateTunnelAsync(string localIp, int localPort, string name, string note, string type, int remote, int node) => await RPC.UpdateTunnelAsync(new TunnelUpdate()
        {
            Action = TunnelUpdate.Types.Action.Add,
            Tunnel = new Tunnel()
            {
                Name = name,
                Note = note,
                Node = node,
                Type = type,
                Remote = remote.ToString(),
                LocalIp = localIp,
                LocalPort = localPort,
            },
        });

        public async Task RequestDeleteTunnelAsync(int id)
        {
            try
            {
                await RPC.UpdateTunnelAsync(new TunnelUpdate()
                {
                    Action = TunnelUpdate.Types.Action.Delete,
                    Tunnel = new Tunnel() { Id = id }
                });
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message, "删除失败", MessageMode.Error);
            }
        }

        public void RequestClearLog()
        {
            RPC.ClearLog(RpcEmpty);
            Dispatcher.Invoke(ClearLog);
        }

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

        public async Task LoginOrLogout()
        {
            LoggingIn = true;
            try
            {
                if (LoggedIn)
                {
                    await RPC.LogoutAsync(RpcEmpty).ConfigureAwait(false);
                }
                else
                {
                    await RPC.LoginAsync(new LoginRequest() { Token = UserToken }).ConfigureAwait(false);
                }
            }
            catch (Exception ex)
            {
                ShowMessage(ex.Message, "登录失败", MessageMode.Error);
            }
            finally
            {
                LoggingIn = false;
            }
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

        public ServiceConfig Config
        {
            get => _config;
            set => SafeSet(out _config, value);
        }
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
            }
        }

        [SourceBinding(nameof(Config))]
        public bool RemoteManagement
        {
            get => Config?.RemoteManagement ?? false;
            set
            {
                if (Config == null) return;
                Config.RemoteManagement = value && !string.IsNullOrEmpty(Config.RemoteManagementKey);
                PushServiceConfig();
            }
        }

        [SourceBinding(nameof(Config), nameof(LoggedIn))]
        public bool CanEnableRemoteManagement => LoggedIn && !string.IsNullOrEmpty(Config?.RemoteManagementKey);

        [SourceBinding(nameof(Config))]
        public bool EnableTLS
        {
            get => Config?.FrpcForceTls ?? false;
            set
            {
                if (Config == null) return;
                Config.FrpcForceTls = value;
                PushServiceConfig();
            }
        }

        public void PushServiceConfig(bool blocking = false)
        {
            if (blocking)
            {
                RPC.UpdateConfig(Config);
            }
            else
            {
                _ = RPC.UpdateConfigAsync(Config);
            }
        }

        #endregion

        #region Settings - Auto Update

        public string License => Properties.Resources.LICENSE;

        public string LauncherVersion => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        [SourceBinding(nameof(Config))]
        public string ServiceVersion => Config?.Update?.ServiceVersion ?? "-";

        [SourceBinding(nameof(Config))]
        public string FrpcVersion => Config?.Update?.FrpcVersion ?? "-";

        [SourceBinding(nameof(Config))]
        public bool HaveUpdate => false; // TODO Config.Update != null && Update.UpdateEnabled && Update.UpdateAvailable;

        [SourceBinding(nameof(Config))]
        public string UpdateText
        {
            get
            {
                // TODO
                return "";
                //if (Update == null || !Update.UpdateAvailable)
                //{
                //    return "";
                //}
                //if (Update.UpdateReadyDir != "")
                //{
                //    return "更新准备完成, 点此进行更新";
                //}
                //return "下载更新中... " + Math.Round(Update.DownloadCurrent / 1048576f, 2) + " MiB/" + Math.Round(Update.DownloadTotal / 1048576f, 2) + " MiB";
            }
        }

        [SourceBinding(nameof(Config))]
        public bool CheckUpdate
        {
            get => Config != null && Config.UpdateInterval != -1;
            set
            {
                if (Config != null)
                {
                    Config.UpdateInterval = value ? 86400 : -1;
                    PushServiceConfig();
                }
                RaisePropertyChanged();
            }
        }

        public void RequestUpdateCheck()
        {
            // TODO
        }

        public void ConfirmUpdate(bool legacy)
        {
            // TODO
            //if (Update.UpdateReadyDir == "")
            //{
            //    return;
            //}
            //if (!confirm(Update.Note))
            //{
            //    return;
            //}
            //if (NTAPI.GetSystemMetrics(SystemMetric.SM_REMOTESESSION) != 0 && !warn("检测到当前正在使用远程桌面连接，若您正在通过 SakuraFrp 连接计算机，请勿进行更新\n进行更新时启动器和所有 frpc 将彻底退出并且需要手动确认操作，这会造成远程桌面断开并且无法恢复\n是否继续?"))
            //{
            //    return;
            //}
            //Daemon.Stop();
            //try
            //{
            //    Process.Start(new ProcessStartInfo(Consts.ServiceExecutable, "--update \"" + Update.UpdateReadyDir.TrimEnd('\\') + "\" " + (legacy ? "legacy" : "wpf"))
            //    {
            //        Verb = "runas"
            //    });
            //}
            //finally
            //{
            //    Environment.Exit(0);
            //}
        }

        #endregion

        #region Settings - Working Mode

        public bool IsDaemon => Daemon.Daemon;

        [SourceBinding(nameof(IsDaemon))]
        public string WorkingMode => Daemon.Daemon ? "守护进程" : "系统服务";

        public bool SwitchingMode { get => _switchingMode; set => SafeSet(out _switchingMode, value); }
        private bool _switchingMode;

        public void SwitchWorkingMode()
        {
            if (SwitchingMode)
            {
                return;
            }
            if (!ShowMessage("确定要切换运行模式吗?\n如果您不知道该操作的作用, 请不要切换运行模式\n如果您不知道该操作的作用, 请不要切换运行模式\n如果您不知道该操作的作用, 请不要切换运行模式\n\n注意事项:\n1. 切换运行模式后不要移动启动器到其他目录, 否则会造成严重错误\n2. 如需移动或卸载启动器, 请先切到 \"守护进程\" 模式来避免文件残留\n3. 切换过程可能需要十余秒, 请耐心等待, 不要做其他操作\n4. 切换操作即为 安装/卸载 系统服务, 需要管理员权限", "提示", MessageMode.Confirm))
            {
                return;
            }
            SwitchingMode = true;
            ThreadPool.QueueUserWorkItem(s =>
            {
                try
                {
                    Daemon.Stop();

                    var result = Daemon.SwitchMode();

                    Dispatcher.Invoke(() => RaisePropertyChanged(nameof(IsDaemon)));
                    Daemon.Start();

                    if (result)
                    {
                        ShowMessage("运行模式已切换, 正在重新初始化 Daemon", "提示", MessageMode.Info);
                    }
                }
                catch (Exception ex)
                {
                    ShowMessage(ex.ToString(), "错误", MessageMode.Error);
                }
                finally
                {
                    SwitchingMode = false;
                }
            });
        }

        #endregion
    }
}
