using System.Collections.Generic;

namespace SakuraFrpService.Provider
{
    public class ConfigProvider : IConfigProvider
    {
        private SakuraService Main;

        public void Init(SakuraService main)
        {
            Main = main;
        }

        public string FrpcExecutable => "frpc.exe";

        public string Token { get => Settings.Token; set => Settings.Token = value; }

        public bool BypassProxy { get => Settings.BypassProxy; set => Settings.BypassProxy = value; }
        public int UpdateInterval { get => Settings.UpdateInterval; set => Settings.UpdateInterval = value; }

        public byte[] RemoteKey { get => Settings.RemoteKey; set => Settings.RemoteKey = value; }
        public bool EnableRemote { get => Settings.EnableRemote; set => Settings.EnableRemote = value; }

        public List<int> EnabledTunnels { get => Settings.EnabledTunnels; set => Settings.EnabledTunnels = value; }

        private Properties.Settings Settings = Properties.Settings.Default;

        public ConfigProvider()
        {
            if (!Settings.UpgradeRequired)
            {
                return;
            }
            Settings.Upgrade();
            Settings.UpgradeRequired = false;
            Settings.Save();
        }

        public void Save()
        {
            Token = Natfrp.Token;
            if (Main.TunnelManager.Running)
            {
                EnabledTunnels = Main.TunnelManager.GetEnabledTunnels();
            }

            Settings.Save();
        }
    }
}
