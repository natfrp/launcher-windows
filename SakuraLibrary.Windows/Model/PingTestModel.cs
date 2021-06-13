using System.Collections.Generic;
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

        private ManualResetEvent stopTest = new ManualResetEvent(false);

        public PingTestModel(LauncherModel launcher) : base(launcher.Dispatcher)
        {
            Launcher = launcher;
            foreach (var n in launcher.Nodes.Where(n => n.Host != "").OrderBy(n => n.Id))
            {
                Nodes.Add(new NodePingModel(n, Dispatcher));
            }
        }

        public void Stop() => stopTest.Set();

        public void DoTest()
        {
            if (Testing)
            {
                Stop();
                return;
            }
            Testing = true;

            foreach (var node in Nodes)
            {
                node.TTL = "-";
                node.Ping = "-";
                node.Loss = "-";

                node.pingTime.Clear();

                node.complete = false;
                node.Sent = node.lossPackets = 0;
            }

            stopTest.Reset();
            ThreadPool.QueueUserWorkItem(_ =>
            {
                var ping = new Ping();
                var queue = new List<NodePingModel>(Nodes);
                while (!stopTest.WaitOne(1000) && queue.Count > 0)
                {
                    foreach (var node in queue)
                    {
                        if (stopTest.WaitOne(200))
                        {
                            break;
                        }
                        if (node.Sent++ == 0)
                        {
                            node.Ping = "测试中";
                        }
                        try
                        {
                            var result = ping.Send(node.Node.Host, 1000);
                            switch (result.Status)
                            {
                            case IPStatus.Success:
                                node.TTL = result.Options.Ttl.ToString();
                                node.pingTime.Add(result.RoundtripTime);
                                if (node.Sent >= 20)
                                {
                                    node.complete = true;
                                }
                                break;
                            case IPStatus.TimedOut:
                                node.lossPackets++;
                                break;
                            default:
                                node.Ping = "错误: " + result.Status;
                                node.Loss = "-";
                                node.complete = true;
                                continue;
                            }
                            node.Loss = ((float)node.lossPackets / node.Sent * 100).ToString("F1") + "%";
                            if (node.Sent > 3 && node.lossPackets >= node.Sent)
                            {
                                node.Ping = "超时";
                                node.complete = true;
                                continue;
                            }
                            if (node.pingTime.Count > 0)
                            {
                                node.Ping = (node.pingTime.Sum() / node.pingTime.Count).ToString();
                            }
                        }
                        catch
                        {
                            node.Ping = "未知错误";
                            node.Loss = "-";
                            node.complete = true;
                        }
                    }
                    queue.RemoveAll(n => n.complete);
                }
                Launcher.Dispatcher.BeginInvoke(() => Testing = false);
            });
        }
    }
}
