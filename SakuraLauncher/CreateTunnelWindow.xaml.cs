using System.Text;
using System.Windows;
using System.Threading;
using System.Diagnostics;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

using SakuraLauncher.Data;
using SakuraLauncher.Helper;

namespace SakuraLauncher
{
    /// <summary>
    /// CreateTunnelWindow.xaml 的交互逻辑
    /// </summary>
    public partial class CreateTunnelWindow : Window
    {
        public Prop<bool> Creating { get; set; } = new Prop<bool>();
        public Prop<int> RemotePort { get; set; } = new Prop<int>();
        public Prop<string> TunnelName { get; set; } = new Prop<string>("");
        public Prop<string> Protocol { get; set; } = new Prop<string>("");

        public Prop<bool> Compression { get; set; } = new Prop<bool>(true);
        public Prop<bool> Encryption { get; set; } = new Prop<bool>(false);

        public Prop<int> LocalPort { get; set; } = new Prop<int>();
        public Prop<string> LocalAddress { get; set; } = new Prop<string>("");

        public Prop<bool> Loading { get; set; } = new Prop<bool>();

        public ObservableCollection<NodeData> Nodes => MainWindow.Instance.Nodes;
        public ObservableCollection<ListeningData> Listening { get; set; } = new ObservableCollection<ListeningData>();

        public CreateTunnelWindow()
        {
            InitializeComponent();

            DataContext = this;

            LoadListeningList();
        }

        public void LoadListeningList()
        {
            Loading.Value = true;
            Listening.Clear();
            var process = Process.Start(new ProcessStartInfo("netstat.exe", "-ano")
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true
            });
            process.OutputDataReceived += (s, e) =>
            {
                if(e.Data != null)
                {
                    var tokens = new List<string>(Regex.Split(e.Data.Trim(), "\\s+"));
                    if(tokens[0] == "UDP" && tokens.Count > 3 && tokens[2] == "*:*")
                    {
                        tokens.Insert(3, "LISTENING");
                    }
                    if((tokens[0] == "TCP" || tokens[0] == "UDP") && tokens.Count > 4 && tokens[3] == "LISTENING")
                    {
                        var pname = "[拒绝访问]";
                        try
                        {
                            pname = Process.GetProcessById(int.Parse(tokens[4])).ProcessName;
                        }
                        catch { }
                        var spliter = tokens[1].LastIndexOf(':');
                        Dispatcher.Invoke(() => Listening.Add(new ListeningData()
                        {
                            Protocol = tokens[0],
                            Address = tokens[1].Substring(0, spliter),
                            Port = tokens[1].Substring(spliter + 1),
                            PID = tokens[4],
                            ProcessName = pname
                        }));
                    }
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
                Loading.Value = false;
            });
        }

        private void ButtonCreate_Click(object sender, RoutedEventArgs e)
        {
            if(Creating.Value)
            {
                return;
            }
            if(!(node.SelectedItem is NodeData s))
            {
                MessageBox.Show("请选择穿透服务器", "Oops", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            Creating.Value = true;
            App.ApiRequest("create_tunnel", new StringBuilder("type=").Append(Protocol.Value.ToLower())
                .Append("&name=").Append(TunnelName.Value)
                .Append("&local_ip=").Append(LocalAddress.Value)
                .Append("&local_port=").Append(LocalPort.Value)
                .Append("&encryption=").Append(Encryption.Value ? "true" : "false")
                .Append("&compression=").Append(Compression.Value ? "true" : "false")
                .Append("&remote_port=").Append(RemotePort.Value).ToString()).ContinueWith(t =>
            {
                Creating.Value = false;
                var json = t.Result;
                if(json == null)
                {
                    return;
                }
                Dispatcher.Invoke(() => MainWindow.Instance.AddTunnel(json["data"], true));
                if(MessageBox.Show("是否继续创建?", "创建成功", MessageBoxButton.YesNo, MessageBoxImage.Information) == MessageBoxResult.Yes)
                {
                    Dispatcher.Invoke(() =>
                    {
                        TunnelName.Value = "";
                        node.SelectedItem = null;
                        listening.SelectedItem = null;
                    });
                }
                else
                {
                    Dispatcher.Invoke(Close);
                }
            });
        }

        private void ButtonReload_Click(object sender, RoutedEventArgs e)
        {
            if(!Loading.Value)
            {
                LoadListeningList();
            }
        }

        private void Listening_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(e.AddedItems.Count == 1 && e.AddedItems[0] is ListeningData l)
            {
                Protocol.Value = l.Protocol;
                LocalPort.Value = int.Parse(l.Port);
                LocalAddress.Value = l.Address == "0.0.0.0" ? "127.0.0.1" : l.Address;
            }
        }
    }
}
