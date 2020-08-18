using System;
using System.Linq;
using System.Threading;
using System.ServiceProcess;
using System.Threading.Tasks;

using SakuraLibrary.Proto;
using UserStatus = SakuraLibrary.Proto.User.Types.Status;

using SakuraFrpService.Pipe;
using SakuraFrpService.Tunnel;

namespace SakuraFrpService
{
    public partial class MainService : ServiceBase
    {
        public string UserToken = "";
        public User UserInfo = new User();

        public readonly bool Daemonize;
        public readonly Thread TickThread = null;

        public readonly PipeServer Pipe;
        public readonly TunnelManager TunnelManager;

        public MainService(bool daemon)
        {
            Daemonize = daemon;
            if (!daemon)
            {
                TickThread = new Thread(new ThreadStart(Tick));
            }

            InitializeComponent();

            Pipe = new PipeServer("SakuraFrpLauncher_Service");
            TunnelManager = new TunnelManager(this);
        }

        public void Tick()
        {
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
            if (UserInfo.Status != UserStatus.NoLogin)
            {
                return "用户已登录";
            }
            UserInfo.Status = UserStatus.Pending;
            try
            {
                var user = await Natfrp.Request("get_user");
                if (!user["login"])
                {
                    return user["message"];
                }
                UserInfo.Id = user["data"]["id"];
                UserInfo.Name = user["data"]["name"];
                // string traffic, advanced_traffic

                UserToken = token;
                UserInfo.Status = UserStatus.LoggedIn;

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
            if (!force && UserInfo.Status != UserStatus.LoggedIn)
            {
                return UserInfo.Status == UserStatus.Pending ? "登录进行中, 请稍候" : null;
            }
            UserInfo.Status = UserStatus.Pending;

            try
            {
                TunnelManager.Stop(true);
            }
            catch (Exception e)
            {
                return "未知错误:\n" + e.ToString();
            }
            finally
            {
                UserInfo.Status = UserStatus.NoLogin;
            }
            return null;
        }

        protected override void OnStart(string[] args)
        {
            try
            {
                Pipe.Connected += Pipe_Connected;
                Pipe.DataReceived += Pipe_DataReceived;
                Pipe.Start();
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
            Pipe.Stop();
        }

        protected BasicResponse BasicResponse(bool success, string message = null) => new BasicResponse()
        {
            Success = success,
            Message = message
        };

        private void Pipe_Connected(PipeConnection connection)
        {
            // TODO: May authorize the client by signature, don't forget those compile from source users
        }

        private void Pipe_DataReceived(PipeConnection connection, int length)
        {
            try
            {
                var req = BasicRequest.Parser.ParseFrom(connection.Buffer, 0, length);
                switch (req.Type)
                {
                case MessageID.UserLogin:
                    {
                        var t = Login(req.USERLOGIN.Token);
                        t.Wait();
                        connection.SendProto(BasicResponse(t.Result == null, t.Result ?? "登录成功"));
                    }
                    break;
                case MessageID.UserLogout:
                    {
                        var result = Logout();
                        connection.SendProto(BasicResponse(result == null, result ?? "注销成功"));
                    }
                    break;
                case MessageID.UserInfo:
                    connection.SendProto(UserInfo);
                    break;
                case MessageID.TunnelList:
                    {
                        if (UserInfo.Status != UserStatus.LoggedIn)
                        {
                            connection.SendProto(BasicResponse(false, "用户未登录"));
                            break;
                        }
                        var resp = new GetTunnelListResponse()
                        {
                            Success = true,
                            Message = null
                        };
                        resp.Tunnels.Add(TunnelManager.Values.Select(t => t.CreateProto()));
                        connection.SendProto(resp);
                    }
                    break;
                case MessageID.TunnelReload:
                    {
                        if (UserInfo.Status != UserStatus.LoggedIn)
                        {
                            connection.SendProto(BasicResponse(false, "用户未登录"));
                            break;
                        }
                        var t = TunnelManager.UpdateTunnels();
                        t.Wait();
                        connection.SendProto(BasicResponse(t.Status == TaskStatus.RanToCompletion, t.Exception?.ToString()));
                    }
                    break;
                case MessageID.TunnelUpdate:
                    {
                        if (UserInfo.Status != UserStatus.LoggedIn)
                        {
                            connection.SendProto(BasicResponse(false, "用户未登录"));
                            break;
                        }
                        lock (TunnelManager)
                        {
                            if (!TunnelManager.ContainsKey(req.TUNNELUPDATE.Id))
                            {
                                connection.SendProto(BasicResponse(false, "隧道不存在"));
                                break;
                            }
                            TunnelManager[req.TUNNELUPDATE.Id].Enabled = req.TUNNELUPDATE.Status == 1;
                        }
                        connection.SendProto(BasicResponse(true));
                    }
                    break;
                case MessageID.TunnelLogGet:
                    // return GetTunnelLogResponse
                    break;
                case MessageID.TunnelLogClear:
                    break;
                }
            }
            catch { }
        }
    }
}
