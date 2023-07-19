using System.Collections.Generic;

using SakuraLibrary.Helper;
using SakuraLibrary.Proto;

namespace SakuraLibrary.Model
{
    public class NodePingModel : ModelBase
    {
        public readonly Node Node;

        public string Name => "#" + Node.Id + " " + Node.Name;
        public string AcceptNew => NodeFlags.AcceptNewTunnel(Node) ? "√" : "";

        public string Ping { get => _ping; set => SafeSet(out _ping, value); }
        private string _ping = "-";

        public string Loss { get => _loss; set => SafeSet(out _loss, value); }
        private string _loss = "-";

        public string TTL { get => _ttl; set => SafeSet(out _ttl, value); }
        private string _ttl = "-";

        public int Sent { get => _sent; set => SafeSet(out _sent, value); }
        private int _sent = 0;

        public bool complete = false;
        public int lossPackets = 0;

        public List<long> pingTime = new();

        public NodePingModel(Node node, DispatcherWrapper dispatcher) : base(dispatcher)
        {
            Node = node;
        }
    }
}
