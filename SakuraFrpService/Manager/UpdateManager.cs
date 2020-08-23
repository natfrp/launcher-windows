using System;
using System.Threading.Tasks;

using SakuraLibrary.Helper;

namespace SakuraFrpService.Manager
{
    public class UpdateManager : IAsyncManager
    {
        public readonly MainService Main;
        public readonly AsyncManager AsyncManager;

        public int UpdateInterval { get => _updateInterval; set => _updateInterval = Math.Max(value, 3600); }
        private int _updateInterval;

        private DateTime LastCheck = DateTime.MinValue;

        public UpdateManager(MainService main)
        {
            Main = main;
            AsyncManager = new AsyncManager(Run);

            UpdateInterval = Properties.Settings.Default.UpdateInterval;
        }

        public async Task CheckUpdate(bool loadEnabled = false)
        {
            lock (this)
            {
                // var result = await Natfrp.Request<>("get_version");
            }
            LastCheck = DateTime.Now;
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
                    CheckUpdate().Wait();
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
