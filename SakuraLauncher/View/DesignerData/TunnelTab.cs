using System.Collections.Generic;

using SakuraLibrary.Model;
using SakuraLibrary.Proto;
using static SakuraLibrary.Proto.Tunnel.Types;

namespace SakuraLauncher.View.DesignerData
{
    public class TunnelTab
    {
        public static Dictionary<int, string> Nodes = new Dictionary<int, string>()
        {
            { 1, "PA47 Node" },
            { 2, "LMAO BGP" },
            { 4, "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA" },
        };

        public List<TunnelModel> Tunnels { get; } = new List<TunnelModel>()
        {
            new TunnelModel(new Tunnel()
            {
                Id = 1,
                Node = 1,
                Type = "TCP",
                Name = "SampleTunnel1",
                Note = "yay=louder",
                Enabled = false,
                State = State.Idle,
                Description = "2333 -> 1.1.1.1:2333"
            },null, Nodes),
            new TunnelModel(new Tunnel()
            {
                Id = 1,
                Node = 2,
                Type = "UDP",
                Name = "SampleTunnel 2",
                State = State.Started,
                Description = "2333 -> 1.1.1.1:2333"
            }, null,Nodes),
            new TunnelModel(new Tunnel()
            {
                Id = 1,
                Node = 0,
                Type = "HTTP",
                Name = "SampleTunnel 3",
                Note = "super looooooooooooooooooooooooooooooooooooooong note",
                State = State.Running,
                Description = "example.tld"
            },null, Nodes),
            new TunnelModel(new Tunnel()
            {
                Id = 114514,
                Node = 4,
                Type = "HTTP",
                Name = "LongTunnelNameThatWraps",
                Note = "super looooooooooooooooooooooooooooooooooooooong note",
                State = State.Running,
                Description = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"
            },null, Nodes),
        };
    }
}
