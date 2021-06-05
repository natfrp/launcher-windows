using System.Collections.ObjectModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading;

namespace SakuraLibrary.Model
{
    public class PingTestModel : ModelBase
    {
        public readonly LauncherModel Launcher;

        public bool Testing { get => _testing; set => Set(out _testing, value); }
        private bool _testing = false;

        public ObservableCollection<NodePingModel> Nodes { get => _nodes; set => Set(out _nodes, value); }
        private ObservableCollection<NodePingModel> _nodes = new ObservableCollection<NodePingModel>();

        public PingTestModel(LauncherModel launcher) : base(launcher.Dispatcher)
        {
            Launcher = launcher;
            foreach (var n in launcher.Nodes.Where(n => n.Host != "").OrderBy(n => n.Id))
            {
                Nodes.Add(new NodePingModel(n, Dispatcher));
            }
        }

        public void DoTest()
        {
            if (Testing)
            {
                return;
            }
            Testing = true;

            foreach (var node in Nodes)
            {
                node.TTL = "-";
                node.Ping = "-";
            }
            ThreadPool.QueueUserWorkItem(s =>
            {
                var ping = new Ping();
                foreach (var node in Nodes)
                {
                    node.Ping = "测试中";
                    try
                    {
                        var result = ping.Send(node.Node.Host, 5000);
                        switch (result.Status)
                        {
                        case IPStatus.Success:
                            node.TTL = result.Options.Ttl.ToString();
                            node.Ping = result.RoundtripTime + "ms";
                            break;
                        case IPStatus.TimedOut:
                            node.Ping = "超时";
                            break;
                        default:
                            node.Ping = "错误: " + result.Status;
                            break;
                        }
                    }
                    catch
                    {
                        node.Ping = "错误";
                    }
                }
                Launcher.Dispatcher.BeginInvoke(() => Testing = false);
            });
        }
    }
}
