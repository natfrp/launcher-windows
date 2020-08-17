using System.Threading;
using System.ServiceProcess;

using SakuraLibrary.Proto;

using SakuraFrpService.Pipe;
using SakuraFrpService.Tunnel;

namespace SakuraFrpService
{
    public partial class MainService : ServiceBase
    {
        public bool Daemonize = false;

        public readonly PipeServer Pipe = null;
        public readonly TunnelManager TunnelManager = new TunnelManager();

        public MainService()
        {
            InitializeComponent();
            Pipe = new PipeServer("SakuraFrpLauncher_Service");
        }

        public void DaemonRun(string[] args)
        {
            Daemonize = true;
            OnStart(args);
            while (true)
            {
                Thread.Sleep(10);
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

            if (Properties.Settings.Default.FrpcPath == "")
            {

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
