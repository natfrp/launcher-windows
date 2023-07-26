using System.Linq;
using SakuraLibrary.Proto;
using TunnelState = SakuraLibrary.Proto.Tunnel.Types.State;

namespace SakuraLibrary.Model
{
    public class TunnelModel : ModelBase
    {
        public readonly LauncherModel Launcher;

        public Tunnel Proto { get => _proto; set => Set(out _proto, value); }
        private Tunnel _proto;

        [SourceBinding(nameof(Proto))]
        public int Id => Proto.Id;

        [SourceBinding(nameof(Proto))]
        public int Node => Proto.Node;

        [SourceBinding(nameof(Proto))]
        public string NodeName => Launcher?.Nodes.TryGetValue(Proto.Node, out var node) == true ? node.Name : "未知节点";

        [SourceBinding(nameof(Proto))]
        public string Name => Proto.Name;

        [SourceBinding(nameof(Proto))]
        public string Type => Proto.Type.ToUpper();

        [SourceBinding(nameof(Proto))]
        public string Description => Proto.Type switch
        {
            "tcp" or "udp" => $"{Proto.Remote} → {Proto.LocalIp}:{Proto.LocalPort}",
            "http" or "https" => $"{Proto.Type.ToUpper()} → {Proto.LocalIp}:{Proto.LocalPort}",
            "etcp" or "eudp" => $"{Proto.LocalIp}:{Proto.LocalPort}",
            _ => "-",
        };

        [SourceBinding(nameof(Proto))]
        public TunnelState State => Proto.State;

        [SourceBinding(nameof(Proto))]
        public bool Enabled
        {
            get => Proto.Enabled;
            set => Launcher.RPC.UpdateTunnel(new TunnelUpdate()
            {
                Action = TunnelUpdate.Types.Action.Update,
                Tunnel = new Tunnel()
                {
                    Id = Id,
                    Enabled = value,
                },
            });
        }

        [SourceBinding(nameof(Proto))]
        public string Note => Proto.Note;

        [SourceBinding(nameof(Note))]
        public bool NoteEmpty => string.IsNullOrEmpty(Note);

        public TunnelModel(Tunnel proto, LauncherModel launcher = null)
        {
            Proto = proto;
            Launcher = launcher;
        }
    }
}
