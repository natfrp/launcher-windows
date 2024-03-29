﻿using System.Collections.Generic;

using SakuraLibrary.Model;
using SakuraLibrary.Proto;
using TunnelStatus = SakuraLibrary.Proto.Tunnel.Types.Status;

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
                Status = TunnelStatus.Disabled,
                Description = "2333 -> 1.1.1.1:2333"
            },null, Nodes),
            new TunnelModel(new Tunnel()
            {
                Id = 1,
                Node = 2,
                Type = "UDP",
                Name = "SampleTunnel 2",
                Status = TunnelStatus.Pending,
                Description = "2333 -> 1.1.1.1:2333"
            }, null,Nodes),
            new TunnelModel(new Tunnel()
            {
                Id = 1,
                Node = 0,
                Type = "HTTP",
                Name = "SampleTunnel 3",
                Note = "super looooooooooooooooooooooooooooooooooooooong note",
                Status = TunnelStatus.Running,
                Description = "example.tld"
            },null, Nodes),
            new TunnelModel(new Tunnel()
            {
                Id = 114514,
                Node = 4,
                Type = "HTTP",
                Name = "LongTunnelNameThatWraps",
                Note = "super looooooooooooooooooooooooooooooooooooooong note",
                Status = TunnelStatus.Running,
                Description = "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"
            },null, Nodes),
        };
    }
}
