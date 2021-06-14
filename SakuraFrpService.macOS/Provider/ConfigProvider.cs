using System.Collections.Generic;
using System.Linq;

using Foundation;

using SakuraFrpService.Helper;

namespace SakuraFrpService.Provider
{
    public class ConfigProvider : IConfigProvider
    {
        public const string KEYCHAIN_TOKEN = KeychainHelper.Service + ".Token",
            KEYCHAIN_REMOTEKEY = KeychainHelper.Service + ".RemoteKey";

        private SakuraService Main;

        public void Init(SakuraService main)
        {
            Main = main;
        }

        public string FrpcExecutable => "frpc";

        public string Token { get; set; }

        public bool BypassProxy { get; set; }
        public int UpdateInterval { get; set; }

        public byte[] RemoteKey { get; set; }
        public bool EnableRemote { get; set; }

        public List<int> EnabledTunnels { get; set; } = new List<int>();

        private readonly NSUserDefaults Settings = NSUserDefaults.StandardUserDefaults;

        public ConfigProvider()
        {
            Token = KeychainHelper.LoadString(KEYCHAIN_TOKEN) ?? "";
            RemoteKey = KeychainHelper.Load(KEYCHAIN_REMOTEKEY);

            BypassProxy = Settings.BoolForKey("BypassProxy");
            EnableRemote = Settings.BoolForKey("EnableRemote");
            UpdateInterval = (int)Settings.IntForKey("UpdateInterval");

            var t = Settings.ArrayForKey("EnabledTunnels");
            if (t != null)
            {
                EnabledTunnels.AddRange(t.Select(o => (o as NSNumber).Int32Value));
            }
        }

        public void Save()
        {
            KeychainHelper.Save(KEYCHAIN_TOKEN, Token = Natfrp.Token);
            KeychainHelper.Save(KEYCHAIN_REMOTEKEY, RemoteKey);

            Settings.SetBool(BypassProxy, "BypassProxy");
            Settings.SetBool(EnableRemote, "EnableRemote");
            Settings.SetInt(UpdateInterval, "UpdateInterval");

            if (Main.TunnelManager.Running)
            {
                Settings.SetValueForKey(NSObject.FromObject(Main.TunnelManager.GetEnabledTunnels()), new NSString("EnabledTunnels"));
            }
        }
    }
}
