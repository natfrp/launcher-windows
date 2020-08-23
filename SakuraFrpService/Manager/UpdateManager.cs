using System;
using System.Threading.Tasks;

using SakuraLibrary.Proto;
using SakuraLibrary.Helper;

namespace SakuraFrpService.Manager
{
    public class UpdateManager : IAsyncManager
    {
        public readonly MainService Main;
        public readonly AsyncManager AsyncManager;

        public UpdateStatus Status = new UpdateStatus();

        public int UpdateInterval { get => _updateInterval; set => _updateInterval = Math.Max(value, 3600); }
        private int _updateInterval;

        private DateTime LastCheck = DateTime.MinValue;

        public UpdateManager(MainService main)
        {
            Main = main;
            AsyncManager = new AsyncManager(Run);

            UpdateInterval = Properties.Settings.Default.UpdateInterval;
        }

        public async Task<UpdateStatus> CheckUpdate(bool loadEnabled = false)
        {
            var status = new UpdateStatus();
            lock (this)
            {
                // TODO
                // var result = await Natfrp.Request<>("get_version");
            }
            LastCheck = DateTime.Now;
            return status;
        }

        protected void Run()
        {
            do
            {
                lock (this)
                {
                    if ((DateTime.Now - LastCheck).TotalSeconds <= UpdateInterval)
                    {
                        continue;
                    }
                    Status = CheckUpdate().WaitResult();
                    if (Status.UpdateFrpc || Status.UpdateLauncher)
                    {
                        Main.Pipe.PushMessage(new PushMessageBase()
                        {
                            Type = PushMessageID.PushUpdate,
                            DataUpdate = Status
                        });
                    }
                }
            }
            while (!AsyncManager.StopEvent.WaitOne(60 * 1000));
        }

        #region IAsyncManager

        public bool Running => AsyncManager.Running;

        public void Start() => AsyncManager.Start();

        public void Stop(bool kill = false) => AsyncManager.Stop(kill);

        #endregion
    }
}
