using System.Collections.Generic;

using SakuraLibrary.Proto;
using TunnelStatus = SakuraLibrary.Proto.Tunnel.Types.Status;

namespace SakuraLauncher.Model
{
    public interface ITunnelModel
    {
        bool IsReal { get; }
        TunnelModel Real { get; }
    }

    public class TunnelModel : ModelBase, ITunnelModel
    {
        public bool IsReal => true;
        public TunnelModel Real => this;

        public Tunnel Proto { get => _proto; set => Set(out _proto, value); }
        private Tunnel _proto;

        [SourceBinding(nameof(Proto))]
        public int Id => Proto.Id;

        [SourceBinding(nameof(Proto))]
        public int Node => Proto.Node;

        [SourceBinding(nameof(Proto))]
        public string Name => Proto.Name;

        [SourceBinding(nameof(Proto))]
        public string Type => Proto.Type;

        [SourceBinding(nameof(Proto))]
        public string Description => Proto.Description;

        [SourceBinding(nameof(Proto))]
        public bool NotPending => Proto.Status != TunnelStatus.Pending;

        [SourceBinding(nameof(Proto))]
        public bool Enabled
        {
            get => Proto.Status != TunnelStatus.Disabled;
            set
            {
                // TODO: Update status
            }
        }

        public string NodeName { get => _nodeName; set => Set(out _nodeName, value); }
        private string _nodeName;

        public TunnelModel(Tunnel proto, Dictionary<int, string> nodes = null)
        {
            Proto = proto;
            SetNodeName(nodes);
        }

        public void SetNodeName(Dictionary<int, string> nodes) => NodeName = nodes != null && nodes.ContainsKey(Node) ? nodes[Node] : string.Format("#{0} 未知节点", Node);
    }

    public class FakeTunnelModel : ITunnelModel
    {
        public bool IsReal => false;
        public TunnelModel Real => null;
    }
}
