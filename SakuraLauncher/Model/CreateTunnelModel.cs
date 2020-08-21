using System;
using System.Linq;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace SakuraLauncher.Model
{
    public class CreateTunnelModel : ModelBase
    {
        public readonly CreateTunnelWindow View;
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
        public bool Encryption { get; set; }
        public bool Compression { get; set; }

        public bool Loading { get => _loading; set => Set(out _loading, value); }
        private bool _loading = false;

        public bool Creating { get => _creating; set => SafeSet(out _creating, value, View.Dispatcher); }
        private bool _creating = false;

        public IEnumerable<NodeModel> Nodes { get => _nodes; set => Set(out _nodes, value); }
        private IEnumerable<NodeModel> _nodes;

        public ObservableCollection<LocalProcessModel> Listening { get; set; } = new ObservableCollection<LocalProcessModel>();

        public CreateTunnelModel(CreateTunnelWindow view, LauncherModel launcher)
        {
            View = view;
            Launcher = launcher;
            launcher.Nodes.CollectionChanged += LauncherNodes_CollectionChanged;
            LauncherNodes_CollectionChanged();
        }

        ~CreateTunnelModel()
        {
            Launcher.Nodes.CollectionChanged -= LauncherNodes_CollectionChanged;
        }

        private void LauncherNodes_CollectionChanged(object sender = null, NotifyCollectionChangedEventArgs e = null) => Nodes = Launcher.Nodes.Where(t => t.AcceptNew);

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
                    View.Dispatcher.InvokeAsync(() => Listening.Add(new LocalProcessModel()
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
                View.Dispatcher.InvokeAsync(() => Loading = false);
            });
        }
    }
}
