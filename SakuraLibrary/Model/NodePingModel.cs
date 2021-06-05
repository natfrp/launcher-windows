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

        public string TTL { get => _ttl; set => SafeSet(out _ttl, value); }
        private string _ttl = "-";

        public NodePingModel(NodeModel node, DispatcherWrapper dispatcher) : base(dispatcher)
        {
            Node = node;
        }
    }
}
