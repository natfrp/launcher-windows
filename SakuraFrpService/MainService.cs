using System;
using System.Linq;
using System.Threading;
using System.ServiceProcess;
using System.Threading.Tasks;

using SakuraLibrary;
using SakuraLibrary.Pipe;
using SakuraLibrary.Proto;
using UserStatus = SakuraLibrary.Proto.User.Types.Status;

using SakuraFrpService.Tunnel;

namespace SakuraFrpService
{
    public partial class MainService : ServiceBase
    {
        public User UserInfo = new User()
        {
            Status = UserStatus.NoLogin
        };

        public readonly bool Daemonize;
        public readonly Thread TickThread = null;

        public readonly PipeServer Pipe;
        public readonly LogManager LogManager;
        public readonly TunnelManager TunnelManager;

        public MainService(bool daemon)
        {
            Daemonize = daemon;
            if (!daemon)
            {
                TickThread = new Thread(new ThreadStart(Tick));
            }

            InitializeComponent();

            Pipe = new PipeServer(Consts.PipeName);

            LogManager = new LogManager(8192);
            TunnelManager = new TunnelManager(this);
        }

        public void Save()
        {
            var settings = Properties.Settings.Default;

            lock (UserInfo)
            {
                settings.Token = Natfrp.Token;
                settings.LoggedIn = UserInfo.Status == UserStatus.LoggedIn;
            }

            settings.Save();
        }

        public void Tick()
        {
            try
            {
                lock (UserInfo)
                {
                    if (UserInfo.Status == UserStatus.NoLogin && Properties.Settings.Default.LoggedIn)
                    {
                        var _ = Login(Properties.Settings.Default.Token);
                    }
                }
            }
            catch { }
            Thread.Sleep(50);
        }

        public void DaemonRun(string[] args)
        {
            if (!Daemonize)
            {
                throw new InvalidOperationException();
            }
            OnStart(args);
            while (true)
            {
                Tick();
            }
        }

        public new void Stop()
        {
            if (Daemonize)
            {
                OnStop();
                return;
            }
            base.Stop();
        }

        protected async Task<string> Login(string token)
        {
            lock (UserInfo)
            {
                if (UserInfo.Status != UserStatus.NoLogin)
                {
                    return "用户已登录";
                }
                if (token.Length != 16)
                {
                    return "Token  无效";
                }
                UserInfo.Status = UserStatus.Pending;
            }
            try
            {
                Natfrp.Token = token;

                var user = await Natfrp.Request("get_user");
                if (!user["login"])
                {
                    return user["message"];
                }

                lock (UserInfo)
                {
                    UserInfo.Id = (int)user["data"]["uid"];
                    UserInfo.Name = user["data"]["name"];
                    // string traffic, advanced_traffic

                    UserInfo.Status = UserStatus.LoggedIn;
                }

                /*
                var nodes = await Natfrp.Request("get_nodes");
                Nodes.Clear();
                foreach (Dictionary<string, dynamic> j in nodes["data"])
                {
                    Nodes.Add(new NodeData()
                    {
                        ID = (int)j["id"],
                        Name = (string)j["name"],
                        AcceptNew = (bool)j["accept_new"]
                    });
                }
                */

                TunnelManager.Clear();
                TunnelManager.Start(token);

                Save();
            }
            catch (NatfrpException e)
            {
                Logout(true);
                return e.Message + (e.InnerException == null ? "" : "\n" + e.InnerException.ToString());
            }
            catch (Exception e)
            {
                Logout(true);
                return "未知错误:\n" + e.ToString();
            }
            return null;
        }

        protected string Logout(bool force = false)
        {
            lock (UserInfo)
            {
                if (!force && UserInfo.Status != UserStatus.LoggedIn)
                {
                    return UserInfo.Status == UserStatus.Pending ? "登录进行中, 请稍候" : null;
                }
                UserInfo.Status = UserStatus.Pending;
            }
            try
            {
                Properties.Settings.Default.Token = "";
                Properties.Settings.Default.LoggedIn = false;
                TunnelManager.Stop(true);
            }
            catch (Exception e)
            {
                return "未知错误:\n" + e.ToString();
            }
            finally
            {
                lock (UserInfo)
                {
                    UserInfo.Status = UserStatus.NoLogin;
                }
                Save();
            }
            return null;
        }

        #region Service Implemention

        protected override void OnStart(string[] args)
        {
            try
            {
                Pipe.Connected += Pipe_Connected;
                Pipe.DataReceived += Pipe_DataReceived;
                Pipe.Start();
                if (!Daemonize)
                {
                    TickThread.Start();
                }
            }
            catch
            {
                ExitCode = 1;
                Stop();
                return;
            }
        }

        protected override void OnStop()
        {
            try
            {
                Pipe.Stop();
                TickThread.Abort(); // Hmm, should we use ResetEvent?
            }
            catch { }
        }

        #endregion

        #region IPC Handling

        protected ResponseBase ResponseBase(bool success, string message = null) => new ResponseBase()
        {
            Success = success,
            Message = message ?? ""
        };

        private void Pipe_Connected(PipeConnection connection)
        {
            // TODO: May authorize the client by signature, don't forget those compile from source users
        }

        private void Pipe_DataReceived(PipeConnection connection, int count)
        {
            try
            {
                var req = RequestBase.Parser.ParseFrom(connection.Buffer, 0, count);
                var resp = ResponseBase(true);
                switch (req.Type)
                {
                case MessageID.UserLogin:
                    {
                        var t = Login(req.DataUserLogin.Token);
                        t.Wait();
                        if (t.Result != null)
                        {
                            resp.Success = false;
                            resp.Message = t.Result;
                            break;
                        }
                    }
                    goto USERINFO;
                case MessageID.UserLogout:
                    {
                        var result = Logout();
                        if (result != null)
                        {
                            resp.Success = false;
                            resp.Message = result;
                            break;
                        }
                    }
                    goto USERINFO;
                case MessageID.UserInfo:
                USERINFO:
                    resp.DataUser = UserInfo;
                    break;
                case MessageID.TunnelList:
                    if (UserInfo.Status != UserStatus.LoggedIn)
                    {
                        resp.Success = false;
                        resp.Message = "用户未登录";
                        break;
                    }
                    resp.DataTunnelList = new TunnelList();
                    resp.DataTunnelList.Tunnels.Add(TunnelManager.Values.Select(t => t.CreateProto()));
                    break;
                case MessageID.TunnelReload:
                    {
                        if (UserInfo.Status != UserStatus.LoggedIn)
                        {
                            resp.Success = false;
                            resp.Message = "用户未登录";
                            break;
                        }
                        var t = TunnelManager.UpdateTunnels();
                        t.Wait();
                        if (t.Status != TaskStatus.RanToCompletion)
                        {
                            resp.Success = false;
                            resp.Message = t.Exception?.ToString() ?? "未知错误";
                        }
                    }
                    break;
                case MessageID.TunnelUpdate:
                    if (UserInfo.Status != UserStatus.LoggedIn)
                    {
                        resp.Success = false;
                        resp.Message = "用户未登录";
                        break;
                    }
                    lock (TunnelManager)
                    {
                        if (!TunnelManager.ContainsKey(req.DataUpdateTunnel.Id))
                        {
                            resp.Success = false;
                            resp.Message = "隧道不存在";
                            break;
                        }
                        TunnelManager[req.DataUpdateTunnel.Id].Enabled = req.DataUpdateTunnel.Status == 1;
                    }
                    break;
                case MessageID.LogGet:
                    // ?
                    break;
                case MessageID.LogClear:
                    LogManager.Clear();
                    break;
                }
                connection.SendProto(resp);
            }
            catch (Exception e)
            {
                connection.SendProto(ResponseBase(false, e.ToString()));
            }
        }

        #endregion
    }
}
