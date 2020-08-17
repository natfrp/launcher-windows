using System.Collections.Generic;

namespace SakuraFrpService.Tunnel
{
    public class TunnelManager : Dictionary<int, Tunnel>
    {
        public string FrpcPath;

        public TunnelManager()
        {

        }

        public string GetArguments(int tunnel) => "-n -f " + "/*TODO: User Token*/" + ":" + tunnel;
    }
}
