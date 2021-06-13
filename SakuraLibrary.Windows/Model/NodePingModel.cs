using System.Collections.Generic;

using SakuraLibrary.Helper;

namespace SakuraLibrary.Model
{
    public class NodePingModel : ModelBase
    {
        public readonly NodeModel Node;

        public string Name => Node.ToString();
        public string AcceptNew => Node.AcceptNew ? "√" : "";

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

        public List<long> pingTime = new List<long>();

        public NodePingModel(NodeModel node, DispatcherWrapper dispatcher) : base(dispatcher)
        {
            Node = node;
        }
    }
}
