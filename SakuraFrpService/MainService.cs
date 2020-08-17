using System;
using System.IO.Pipes;
using System.Threading;
using System.Diagnostics;
using System.ServiceProcess;

using SakuraLibrary.Proto;

using SakuraFrpService.Pipe;

namespace SakuraFrpService
{
    public partial class MainService : ServiceBase
    {
        public bool Daemonize = false;
        public PipeServer Pipe = null;

        public MainService()
        {
            InitializeComponent();
        }

        public void DaemonRun(string[] args)
        {
            Daemonize = true;
            OnStart(args);
            while (true)
                Thread.Sleep(10);
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
                Pipe = new PipeServer("SakuraFrpLauncher_Service");
                Pipe.Connected += Pipe_Connected;
                Pipe.DataReceived += Pipe_DataReceived;
                Pipe.Start();
            }
            catch
            {
                Pipe = null;
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
            if (Pipe == null) // Service start failed
            {
                return;
            }
            Pipe.Stop();
        }

        private void Pipe_Connected(PipeConnection connection)
        {
            // TODO: May authorize the client by signature, don't forget those compile from source users
        }

        private void Pipe_DataReceived(PipeConnection connection, int length)
        {
            var req = BasicRequest.Parser.ParseFrom(connection.Buffer, 0, length);
            switch (req.Type)
            {
            case MessageID.UserLogin:
                break;
            case MessageID.UserLogout:
                break;
            case MessageID.UserInfo:
                break;
            case MessageID.TunnelList:
                break;
            case MessageID.TunnelReload:
                break;
            case MessageID.TunnelUpdate:
                break;
            }
        }
    }
}
