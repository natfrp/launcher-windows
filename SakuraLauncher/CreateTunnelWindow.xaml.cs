using System.Linq;
using System.Windows;
using System.Threading;
using System.Diagnostics;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;

using SakuraLibrary.Proto;

using SakuraLauncher.Model;
using SakuraLauncher.Helper;

namespace SakuraLauncher
{
    /// <summary>
    /// CreateTunnelWindow.xaml 的交互逻辑
    /// </summary>
    public partial class CreateTunnelWindow : Window
    {
        public Prop<int> LocalPort { get; set; } = new Prop<int>();
        public Prop<int> RemotePort { get; set; } = new Prop<int>();
        public Prop<string> Note { get; set; } = new Prop<string>("");
        public Prop<string> Type { get; set; } = new Prop<string>("");
        public Prop<string> TunnelName { get; set; } = new Prop<string>("");
        public Prop<string> LocalAddress { get; set; } = new Prop<string>("");

        public Prop<bool> Compression { get; set; } = new Prop<bool>(false);
        public Prop<bool> Encryption { get; set; } = new Prop<bool>(false);

        public Prop<bool> Loading { get; set; } = new Prop<bool>();
        public Prop<bool> Creating { get; set; } = new Prop<bool>();

        public ObservableCollection<NodeData> Nodes { get; } = null; // TODO: Get nodes
        public ObservableCollection<LocalProcessModel> Listening { get; set; } = new ObservableCollection<LocalProcessModel>();

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
                if (e.Data != null)
                {
                    var tokens = new List<string>(Regex.Split(e.Data.Trim(), "\\s+"));
                    if (tokens[0] == "UDP" && tokens.Count > 3 && tokens[2] == "*:*")
                    {
                        tokens.Insert(3, "LISTENING");
                    }
                    if ((tokens[0] == "TCP" || tokens[0] == "UDP") && tokens.Count > 4 && tokens[3] == "LISTENING")
                    {
                        var pname = "[拒绝访问]";
                        try
                        {
                            pname = Process.GetProcessById(int.Parse(tokens[4])).ProcessName;
                        }
                        catch { }
                        var spliter = tokens[1].LastIndexOf(':');
                        Dispatcher.Invoke(() => Listening.Add(new LocalProcessModel()
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
            if (Creating.Value)
            {
                return;
            }
            if (!(this.node.SelectedItem is NodeData node))
            {
                App.ShowMessage("请选择穿透服务器", "Oops", MessageBoxImage.Error);
                return;
            }
            /* TODO: IPC */
            new CreateTunnel()
            {
                Name = TunnelName.Value,
                Note = Note.Value,
                Node = node.ID,
                Type = Type.Value.ToLower(),
                RemotePort = RemotePort.Value,
                LocalPort = LocalPort.Value,
                LocalAddress = LocalAddress.Value
            };
        }

        private void ButtonReload_Click(object sender, RoutedEventArgs e)
        {
            if (!Loading.Value)
            {
                LoadListeningList();
            }
        }

        private void Listening_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 1 && e.AddedItems[0] is LocalProcessModel l)
            {
                Type.Value = l.Protocol;
                LocalPort.Value = int.Parse(l.Port);
                LocalAddress.Value = l.Address == "0.0.0.0" ? "127.0.0.1" : l.Address;
            }
        }
    }
}
