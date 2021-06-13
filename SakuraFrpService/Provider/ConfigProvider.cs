using System.Collections.Generic;

using SakuraLibrary;

namespace SakuraFrpService.Provider
{
    public class ConfigProvider : IConfigProvider
    {
        public string PipeName => UtilsWindows.InstallationPipeName;

        public string Token { get => Settings.Token; set => Settings.Token = value; }

        public bool BypassProxy { get => Settings.BypassProxy; set => Settings.BypassProxy = value; }
        public int UpdateInterval { get => Settings.UpdateInterval; set => Settings.UpdateInterval = value; }

        public byte[] RemoteKey { get => Settings.RemoteKey; set => Settings.RemoteKey = value; }
        public bool EnableRemote { get => Settings.EnableRemote; set => Settings.EnableRemote = value; }

        public List<int> EnabledTunnels { get => Settings.EnabledTunnels; set => Settings.EnabledTunnels = value; }

        private Properties.Settings Settings => Properties.Settings.Default;

        public ConfigProvider()
        {
            if (Settings.UpgradeRequired)
            {
                Settings.Upgrade();
                Settings.UpgradeRequired = false;
                Settings.Save();
            }
        }

        public void Save()
        {
            Settings.Save();
        }
    }
}
