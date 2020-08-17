using System.Threading;
using System.Collections.Generic;

namespace SakuraFrpService.Tunnel
{
    public class TunnelManager : Dictionary<int, Tunnel>
    {
        public string FrpcPath;

        public readonly Thread MainThread;

        protected ManualResetEvent stopEvent = new ManualResetEvent(false);

        public TunnelManager()
        {
            MainThread = new Thread(new ThreadStart(Run))
            {
                IsBackground = false
            };
        }

        public string GetArguments(int tunnel) => "-n -f " + "/*TODO: User Token*/" + ":" + tunnel;

        public void Start()
        {
            if (MainThread.IsAlive)
            {
                return;
            }
            stopEvent.Reset();
            MainThread.Start();
        }

        public void Stop() => stopEvent.Set();

        protected void Run()
        {
            while (!stopEvent.WaitOne(0))
            {

            }
        }
    }
}
