using System;
using System.Threading;
using System.ServiceProcess;
using System.Threading.Tasks;
using System.Collections.Generic;

using SakuraLibrary.Proto;
using UserStatus = SakuraLibrary.Proto.User.Types.Status;

using SakuraFrpService.Pipe;
using SakuraFrpService.Tunnel;

namespace SakuraFrpService
{
    public partial class MainService : ServiceBase
    {
        public string UserToken = "";
        public UserStatus LoginStatus = UserStatus.NoLogin; // TODO: Thread safety?

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
            if (LoginStatus != UserStatus.NoLogin)
            {
                return "用户已登录";
            }
            LoginStatus = UserStatus.Pending;
            try
            {
                var nodes = await Natfrp.Request("get_nodes");
                /*
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

                var tunnels = await Natfrp.Request("get_tunnels");
                foreach (Dictionary<string, dynamic> j in tunnels["data"])
                {
                    TunnelManager.AddJson(j);
                }

                LoginStatus = UserStatus.LoggedIn;

                TunnelManager.SetEnabledTunnels(Properties.Settings.Default.EnabledTunnels);
                TunnelManager.Start();
            }
            catch (NatfrpException e)
            {
                return e.Message + (e.InnerException == null ? "" : e.InnerException.ToString());
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
                    var t = Login(req.USERLOGIN.Token);
                    t.Wait();
                    connection.SendProto(new BasicResponse()
                    {
                        Success = t.Result == null,
                        Message = t.Result ?? "登录成功"
                    });
                    break;
                case MessageID.UserLogout:
                    break;
                case MessageID.UserInfo:
                    // return User
                    break;
                case MessageID.TunnelList:
                    // return GetTunnelListResponse
                    break;
                case MessageID.TunnelReload:
                    break;
                case MessageID.TunnelUpdate:
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
