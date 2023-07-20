using System;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

using SakuraLibrary.Proto;

namespace SakuraLibrary.Model
{
    public class CreateTunnelModel : ModelBase
    {
        public readonly LauncherModel Launcher;

        public string Type { get => _type; set => Set(out _type, value); }
        private string _type = "";

        public string TunnelName { get => _tunnelName; set => Set(out _tunnelName, value); }
        private string _tunnelName = "";

        public int RemotePort { get => _remotePort; set => Set(out _remotePort, value); }
        private int _remotePort;

        public int LocalPort { get => _localPort; set => Set(out _localPort, value); }
        private int _localPort;

        public string LocalAddress { get => _localAddress; set => Set(out _localAddress, value); }
        private string _localAddress = "";

        public string Note { get; set; } = "";

        public bool Loading { get => _loading; set => Set(out _loading, value); }
        private bool _loading = false;

        public bool Creating { get => _creating; set => SafeSet(out _creating, value); }
        private bool _creating = false;

        public List<Node> Nodes { get; set; } = new List<Node>();

        public ObservableCollection<LocalProcessModel> Listening { get; set; } = new ObservableCollection<LocalProcessModel>();

        public CreateTunnelModel(LauncherModel launcher) : base(launcher.Dispatcher)
        {
            Launcher = launcher;

            var groups = new Dictionary<string, List<Node>>()
            {
                { "private", new List<Node>() },
                { "vip3", new List<Node>() },
                { "vip4", new List<Node>() },
                { "china", new List<Node>() },
                { "normal", new List<Node>() },
            };
            foreach (var n in launcher.Nodes.Values)
            {
                if (!NodeFlags.AcceptNewTunnel(n))
                {
                    continue;
                }
                var key = NodeFlags.IsPrivate(n) ? "private" : n.Vip > 0 ? $"vip{n.Vip}" : NodeFlags.IsChinese(n) ? "china" : "normal";
                if (!groups.ContainsKey(key))
                {
                    groups.Add(key, new List<Node>() { n });
                }
                else
                {
                    groups[key].Add(n);
                }
            }
            foreach (var g in groups)
            {
                if (g.Value.Count == 0)
                {
                    continue;
                }
                Nodes.Add(new Node()
                {
                    Enabled = false,
                    Name = g.Key switch
                    {
                        "private" => "私有节点",
                        "vip3" => "青铜节点",
                        "vip4" => "白银节点",
                        "china" => "普通节点 (内地)",
                        "normal" => "普通节点 (其他)",
                        _ => g.Key,
                    },
                });
                Nodes.AddRange(g.Value);
            }
        }

        public void RequestCreate(int node, Action<bool> callback)
        {
            Creating = true;
            Launcher.RequestCreateTunnelAsync(LocalAddress, LocalPort, TunnelName, Note, Type.ToLower(), RemotePort, node).ContinueWith(r => Dispatcher.Invoke(() =>
            {
                Creating = false;
                if (r.Exception != null)
                {
                    Launcher.ShowMessage(r.Exception.ToString(), "创建失败", LauncherModel.MessageMode.Error);
                    callback(false);
                    return;
                }
                callback(Launcher.ShowMessage($"成功创建隧道 #{r.Result.Tunnel.Id} {r.Result.Tunnel.Name}\n\n按 \"取消\" 继续创建, \"确定\" 关闭创建隧道窗口", "创建成功", LauncherModel.MessageMode.Confirm));
            }));
        }

        public void ReloadListening()
        {
            if (Loading)
            {
                return;
            }
            Loading = true;
            Listening.Clear();
            var process = Process.Start(new ProcessStartInfo("netstat.exe", "-ano")
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true
            });
            var processNames = new Dictionary<string, string>();
            process.OutputDataReceived += (s, e) =>
            {
                if (e.Data != null)
                {
                    var tokens = e.Data.Split(new char[0], StringSplitOptions.RemoveEmptyEntries);
                    if (tokens.Length < 3 || (tokens[0] != "TCP" && tokens[0] != "UDP") || tokens[1][0] == '[')
                    {
                        return;
                    }

                    string pid;
                    if (tokens[0] == "UDP")
                    {
                        if (tokens.Length < 4 || tokens[2] != "*:*")
                        {
                            return;
                        }
                        pid = tokens[3];
                    }
                    else
                    {
                        if (tokens.Length < 5 || tokens[3] != "LISTENING")
                        {
                            return;
                        }
                        pid = tokens[4];
                    }

                    if (!processNames.ContainsKey(pid))
                    {
                        processNames[pid] = "[拒绝访问]";
                        try
                        {
                            processNames[pid] = Process.GetProcessById(int.Parse(pid)).ProcessName;
                        }
                        catch { }
                    }

                    var spliter = tokens[1].Split(':');
                    Launcher.Dispatcher.BeginInvoke(() => Listening.Add(new LocalProcessModel()
                    {
                        Protocol = tokens[0],
                        Address = spliter[0],
                        Port = spliter[1],
                        PID = pid,
                        ProcessName = processNames[pid]
                    }));
                }
            };
            process.BeginOutputReadLine();
            ThreadPool.QueueUserWorkItem(s =>
            {
                try
                {
                    process.WaitForExit(3000);
                    process.Kill();
                }
                catch { }
                Launcher.Dispatcher.BeginInvoke(() => Loading = false);
            });
        }
    }
}
