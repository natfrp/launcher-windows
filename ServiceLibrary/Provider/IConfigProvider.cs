using System.Collections.Generic;

namespace SakuraFrpService.Provider
{
    public interface IConfigProvider
    {
        string Token { get; set; }

        bool BypassProxy { get; set; }
        int UpdateInterval { get; set; }

        byte[] RemoteKey { get; set; }
        bool EnableRemote { get; set; }

        List<int> EnabledTunnels { get; set; }

        void Save();
    }
}
