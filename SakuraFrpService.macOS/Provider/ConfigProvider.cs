using System.Collections.Generic;
using System.Linq;

using Foundation;

namespace SakuraFrpService.Provider
{
    public class ConfigProvider : IConfigProvider
    {
        public string Token { get; set; }

        public bool BypassProxy { get; set; }
        public int UpdateInterval { get; set; }

        public byte[] RemoteKey { get; set; }
        public bool EnableRemote { get; set; }

        public List<int> EnabledTunnels { get; set; } = new List<int>();

        private readonly NSUserDefaults Settings = NSUserDefaults.StandardUserDefaults;

        public ConfigProvider()
        {
            Token = "";

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
            Settings.SetBool(BypassProxy, "BypassProxy");
            Settings.SetBool(EnableRemote, "EnableRemote");
            Settings.SetInt(UpdateInterval, "UpdateInterval");

            // TODO:
            //if(EnabledTunnels != null && EnabledTunnels.Length != 0)
            //{
            //    Settings.SetValueForKey(NSObject.FromObject(EnabledTunnels.ToArray()), new NSString("EnabledTunnels"));
            //}
        }
    }
}
