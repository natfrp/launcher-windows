using System.Collections.Generic;

namespace SakuraFrpService.Provider
{
    public interface IConfigProvider
    {
        void Init(SakuraService main);

        string FrpcExecutable { get; }

        string Token { get; set; }

        bool BypassProxy { get; set; }
        int UpdateInterval { get; set; }

        byte[] RemoteKey { get; set; }
        bool EnableRemote { get; set; }

        List<int> EnabledTunnels { get; set; }

        /// <summary>
        /// Do the following sync when called:
        /// <see cref="Token"/> = <see cref="Natfrp.Token"/>;
        /// <see cref="EnabledTunnels"/> = <see cref="Manager.TunnelManager.GetEnabledTunnels"/>;
        /// Save the config afterwards.
        /// </summary>
        void Save();
    }
}
