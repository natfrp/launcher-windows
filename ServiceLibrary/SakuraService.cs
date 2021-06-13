using System;
using System.Text;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using SakuraLibrary.Proto;
using SakuraLibrary.Helper;
using UserStatus = SakuraLibrary.Proto.User.Types.Status;

using SakuraFrpService.Data;
using SakuraFrpService.Manager;
using SakuraFrpService.Provider;
using Tunnel = SakuraFrpService.Data.Tunnel;

namespace SakuraFrpService
{
    public class SakuraService
    {
        public const string Tag = "Service";

        public User UserInfo = new User()
        {
            Status = UserStatus.NoLogin
        };

        public readonly ICommunicationProvider Communication;

        public readonly LogManager LogManager;
        public readonly NodeManager NodeManager;
        public readonly TunnelManager TunnelManager;
        public readonly UpdateManager UpdateManager;
        public readonly RemoteManager RemoteManager;

        public readonly ISodiumProvider Sodium;
        public readonly IUtilsProvider ExtraUtils;
        public readonly IConfigProvider Config;

        protected bool AutoLogin = false;

        protected Thread TickThread = null;
        protected ManualResetEvent StopEvent = new ManualResetEvent(false);

        public SakuraService(IConfigProvider config, IUtilsProvider utils, ICommunicationProvider communication, ISodiumProvider sodium)
        {
            Config = config;
            Sodium = sodium;
            ExtraUtils = utils;

            Natfrp.Token = config.Token; // Prevent possible token lost
            Natfrp.BypassProxy = config.BypassProxy;

            AutoLogin = Natfrp.Token != null && Natfrp.Token.Length > 0;

            Communication = communication;
            Communication.DataReceived = Pipe_DataReceived;

            LogManager = new LogManager(this, 8192);
            NodeManager = new NodeManager(this);
            TunnelManager = new TunnelManager(this);
            UpdateManager = new UpdateManager(this);
            RemoteManager = new RemoteManager(this, Sodium)
            {
                Enabled = config.EnableRemote && config.RemoteKey != null && config.RemoteKey.Length > 0,
                EncryptKey = config.RemoteKey
            };
        }

        public void Start(bool blocking)
        {
            StopEvent.Reset();
            try
            {
                Communication.Start();
                LogManager.Start();
                if (!blocking)
                {
                    TickThread = new Thread(new ThreadStart(MainLoop));
                    TickThread.Start();
                }
                UpdateManager.Start();
            }
            catch
            {
                Environment.ExitCode = 1;
                Stop();
                return;
            }
            LogManager.Log(LogManager.CATEGORY_SERVICE_INFO, Tag, "守护进程启动成功");
            if (blocking)
            {
                MainLoop();
            }
        }

        public void Stop()
        {
            StopEvent.Set();
            try
            {
                Communication.Stop();
                RemoteManager.Stop(true);
                TunnelManager.Stop(true);
                NodeManager.Stop(true);
                if (TickThread != null)
                {
                    TickThread.Abort();
                }
                UpdateManager.Stop(true);
            }
            catch { }
            LogManager.Stop();
        }

        protected void MainLoop()
        {
            ulong ticks = 0;
            while (!StopEvent.WaitOne(50))
            {
                try
                {
                    if (ticks % 600 == 0)
                    {
                        lock (UserInfo)
                        {
                            if (UserInfo.Status == UserStatus.NoLogin && AutoLogin)
                            {
                                var _ = Login(Natfrp.Token, true);
                            }
                        }
                    }
                    if (ticks % 200 == 0)
                    {
                        if (!Communication.Running)
                        {
                            Communication.Start();
                        }
                    }
                }
                catch { }
                ticks++;
            }
        }

        protected void PushUserInfo()
        {
            lock (UserInfo)
            {
                Communication.PushMessage(new PushMessageBase()
                {
                    Type = PushMessageID.UpdateUser,
                    DataUser = UserInfo
                });
            }
        }

        protected async Task<string> Login(string token, bool isAuto = false)
        {
            lock (UserInfo)
            {
                if (UserInfo.Status != UserStatus.NoLogin)
                {
                    return UserInfo.Status == UserStatus.Pending ? "操作进行中, 请稍候" : "用户已登录";
                }
                if (token.Length < 16)
                {
                    return "访问密钥无效, 请检查您的输入是否正确";
                }
                UserInfo.Status = UserStatus.Pending;
                PushUserInfo();
            }
            LogManager.Log(LogManager.CATEGORY_SERVICE_INFO, Tag, "开始登录, 访问密钥: " + token.Substring(0, 3) + "********" + token.Substring(token.Length - 3));
            try
            {
                Natfrp.Token = token;

                var user = await Natfrp.Request<Natfrp.GetUser>("get_user");
                if (!user.Data.Login)
                {
                    LogManager.Log(LogManager.CATEGORY_SERVICE_ERROR, Tag, "服务器拒绝登录: " + user.Message);
                    Logout(true);
                    return user.Message;
                }

                lock (UserInfo)
                {
                    UserInfo.Id = user.Data.Id;
                    UserInfo.Name = user.Data.Name;
                    UserInfo.Meta = user.Data.Meta;

                    UserInfo.Status = UserStatus.LoggedIn;

                    AutoLogin = false;
                }

                Config.Save();

                NodeManager.Clear();
                NodeManager.Start();

                TunnelManager.Clear();
                TunnelManager.Start();

                PushUserInfo();
                LogManager.Log(LogManager.CATEGORY_SERVICE_INFO, Tag, "用户登录成功");

                RemoteManager.Start();
            }
            catch (Exception e)
            {
                if (isAuto)
                {
                    LogManager.Log(LogManager.CATEGORY_SERVICE_ERROR, Tag, "自动登录失败, 将在稍后重试: " + e.ToString());
                    lock (UserInfo)
                    {
                        UserInfo.Status = UserStatus.NoLogin;
                    }
                    // Don't push here
                }
                else
                {
                    LogManager.Log(LogManager.CATEGORY_SERVICE_ERROR, Tag, "用户登录失败: " + e.ToString());
                    Logout(true);
                }
                return e.ToString();
            }
            return null;
        }

        protected string Logout(bool force = false)
        {
            lock (UserInfo)
            {
                if (!force && UserInfo.Status != UserStatus.LoggedIn)
                {
                    return UserInfo.Status == UserStatus.Pending ? "操作进行中, 请稍候" : null;
                }
                UserInfo.Status = UserStatus.Pending;
                PushUserInfo();
            }
            try
            {
                lock (UserInfo)
                {
                    Natfrp.Token = "";
                    UserInfo.Status = UserStatus.NoLogin;
                }
                Config.Save();

                TunnelManager.Stop();
                NodeManager.Stop();
                RemoteManager.Stop(true);
            }
            catch (Exception e)
            {
                return "未知错误:\n" + e.ToString();
            }
            finally
            {
                PushUserInfo();
                TunnelManager.Push();
            }
            if (!force)
            {
                LogManager.Log(LogManager.CATEGORY_SERVICE_INFO, Tag, "已退出登录");
            }
            return null;
        }

        #region IPC Handling

        public ServiceConfig GetConfig() => new ServiceConfig()
        {
            BypassProxy = Natfrp.BypassProxy,
            UpdateInterval = UpdateManager.UpdateInterval,
            RemoteManagement = RemoteManager.Enabled,
            RemoteKeySet = RemoteManager.EncryptKey != null && RemoteManager.EncryptKey.Length > 0
        };

        public void PushConfig() => Communication.PushMessage(new PushMessageBase()
        {
            Type = PushMessageID.PushConfig,
            DataConfig = GetConfig()
        });

        protected ResponseBase ResponseBase(bool success, string message = null) => new ResponseBase()
        {
            Success = success,
            Message = message ?? ""
        };

        public void Pipe_DataReceived(ServiceConnection connection, int count)
        {
            try
            {
                var req = RequestBase.Parser.ParseFrom(connection.Buffer, 0, count);
                var resp = ResponseBase(true);
                var isRemote = connection is RemotePipeConnection;

                switch (req.Type)
                {
                case MessageID.UserLogin:
                    {
                        if (isRemote)
                        {
                            connection.RespondFailure("远程控制无法执行该操作");
                            return;
                        }
                        var result = Login(req.DataUserLogin.Token).WaitResult();
                        if (result != null)
                        {
                            connection.RespondFailure(result);
                            return;
                        }
                    }
                    break;
                case MessageID.UserLogout:
                    {
                        if (isRemote)
                        {
                            connection.RespondFailure("远程控制无法执行该操作");
                            return;
                        }
                        var result = Logout();
                        if (result != null)
                        {
                            connection.RespondFailure(result);
                            return;
                        }
                    }
                    break;
                case MessageID.UserInfo:
                    lock (UserInfo)
                    {
                        if (AutoLogin)
                        {
                            resp.DataUser = UserInfo.Clone();
                            resp.DataUser.Status = UserStatus.Pending;
                        }
                        else
                        {
                            resp.DataUser = UserInfo;
                        }
                    }
                    break;
                case MessageID.LogGet:
                    resp.DataLog = new LogList();
                    lock (LogManager)
                    {
                        resp.DataLog.Data.Add(isRemote ? LogManager.Skip(LogManager.Count - 80) : LogManager);
                    }
                    break;
                case MessageID.LogClear:
                    LogManager.Clear();
                    break;
                case MessageID.ControlExit:
                    if (isRemote)
                    {
                        connection.RespondFailure("远程控制无法执行该操作");
                        return;
                    }
                    Stop();
                    return;
                case MessageID.ControlConfigGet:
                    if (isRemote)
                    {
                        connection.RespondFailure("远程控制无法执行该操作");
                        return;
                    }
                    resp.DataConfig = GetConfig();
                    break;
                case MessageID.ControlConfigSet:
                    if (isRemote)
                    {
                        connection.RespondFailure("远程控制无法执行该操作");
                        return;
                    }
                    if (req.DataConfig.RemoteKeyNew != "")
                    {
                        RemoteManager.EncryptKey = Sodium.ArgonHashBinary(Encoding.UTF8.GetBytes(req.DataConfig.RemoteKeyNew), RemoteManager.SALT, 3, 268435456, 32);
                    }
                    RemoteManager.Enabled = req.DataConfig.RemoteManagement;
                    if (RemoteManager.Enabled && UserInfo.Status == UserStatus.LoggedIn)
                    {
                        RemoteManager.Start();
                    }
                    else
                    {
                        RemoteManager.Stop();
                    }
                    Natfrp.BypassProxy = req.DataConfig.BypassProxy;
                    UpdateManager.UpdateInterval = req.DataConfig.UpdateInterval;
                    Config.Save();
                    PushConfig();
                    break;
                case MessageID.ControlCheckUpdate:
                    if (isRemote)
                    {
                        connection.RespondFailure("远程控制无法执行该操作");
                        return;
                    }
                    UpdateManager.IssueUpdateCheck();
                    break;
                case MessageID.ControlGetUpdate:
                    if (isRemote)
                    {
                        connection.RespondFailure("远程控制无法执行该操作");
                        return;
                    }
                    resp.DataUpdate = UpdateManager.Status;
                    break;
                default:
                    // Login required ↓
                    lock (UserInfo)
                    {
                        if (UserInfo.Status != UserStatus.LoggedIn)
                        {
                            connection.RespondFailure("用户未登录");
                            return;
                        }
                    }
                    switch (req.Type)
                    {
                    case MessageID.TunnelList:
                        resp.DataTunnels = new TunnelList();
                        resp.DataTunnels.Tunnels.Add(TunnelManager.Values.Select(t => t.CreateProto()));
                        break;
                    case MessageID.TunnelReload:
                        TunnelManager.UpdateTunnels().Wait();
                        break;
                    case MessageID.TunnelUpdate:
                        lock (TunnelManager)
                        {
                            if (!TunnelManager.TryGetValue(req.DataUpdateTunnel.Id, out Tunnel t))
                            {
                                connection.RespondFailure("隧道不存在");
                                return;
                            }
                            t.Enabled = req.DataUpdateTunnel.Status == 1;
                            TunnelManager.PushOne(t);
                        }
                        break;
                    case MessageID.TunnelDelete:
                        Natfrp.Request<Natfrp.ApiResponse>("delete_tunnel", "tunnel=" + req.DataId).WaitResult();
                        lock (TunnelManager)
                        {
                            TunnelManager.Remove(req.DataId);
                            TunnelManager.Push();
                        }
                        break;
                    case MessageID.TunnelCreate:
                        {
                            var result = Natfrp.Request<Natfrp.CreateTunnel>("create_tunnel", new StringBuilder()
                                .Append("type=").Append(req.DataCreateTunnel.Type)
                                .Append("&name=").Append(req.DataCreateTunnel.Name)
                                .Append("&note=").Append(req.DataCreateTunnel.Note)
                                .Append("&node=").Append(req.DataCreateTunnel.Node)
                                .Append("&local_ip=").Append(req.DataCreateTunnel.LocalAddress)
                                .Append("&local_port=").Append(req.DataCreateTunnel.LocalPort)
                                .Append("&remote_port=").Append(req.DataCreateTunnel.RemotePort).ToString()).WaitResult();
                            var t = TunnelManager.CreateFromApi(result.Data);
                            lock (TunnelManager)
                            {
                                TunnelManager[t.Id] = t;
                                TunnelManager.Push();
                            }
                            resp.Message = "#" + t.Id + " " + t.Name;
                        }
                        break;
                    case MessageID.NodeList:
                        resp.DataNodes = new NodeList();
                        resp.DataNodes.Nodes.Add(NodeManager.Values);
                        break;
                    case MessageID.NodeReload:
                        NodeManager.UpdateNodes().Wait();
                        break;
                    }
                    break;
                }
                connection.SendProto(resp);
            }
            catch (AggregateException e) when (e.InnerExceptions.Count == 1)
            {
                connection.SendProto(ResponseBase(false, e.InnerExceptions[0].ToString()));
            }
            catch (Exception e)
            {
                connection.SendProto(ResponseBase(false, e.ToString()));
            }
        }

        #endregion
    }
}
