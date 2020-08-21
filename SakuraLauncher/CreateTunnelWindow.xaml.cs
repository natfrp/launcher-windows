using System.Windows;
using System.Windows.Controls;

using SakuraLibrary.Proto;

using SakuraLauncher.Model;
using System.Threading;

namespace SakuraLauncher
{
    /// <summary>
    /// CreateTunnelWindow.xaml 的交互逻辑
    /// </summary>
    public partial class CreateTunnelWindow : Window
    {
        public readonly CreateTunnelModel Model;

        public CreateTunnelWindow(LauncherModel launcher)
        {
            InitializeComponent();
            DataContext = Model = new CreateTunnelModel(this, launcher);

            Model.ReloadListening();
        }

        private void ButtonCreate_Click(object sender, RoutedEventArgs e)
        {
            if (Model.Creating)
            {
                return;
            }
            if (!(this.node.SelectedItem is NodeModel node))
            {
                App.ShowMessage("请选择穿透服务器", "Oops", MessageBoxImage.Error);
                return;
            }
            Model.Creating = true;
            ThreadPool.QueueUserWorkItem(s =>
            {
                try
                {
                    var resp = Model.Launcher.Pipe.Request(new RequestBase()
                    {
                        Type = MessageID.TunnelCreate,
                        DataCreateTunnel = new CreateTunnel()
                        {
                            Name = Model.TunnelName,
                            Note = Model.Note,
                            Node = node.Id,
                            Type = Model.Type.ToLower(),
                            RemotePort = Model.RemotePort,
                            LocalPort = Model.LocalPort,
                            LocalAddress = Model.LocalAddress
                        }
                    });
                    if (!resp.Success)
                    {
                        App.ShowMessage(resp.Message, "操作失败", MessageBoxImage.Error, MessageBoxButton.OK);
                        return;
                    }
                    Dispatcher.Invoke(() =>
                    {
                        Model.Launcher.Tunnels.Add(new TunnelModel(resp.DataTunnel));
                        Model.LocalPort = 0;
                        Model.RemotePort = 0;
                        Model.TunnelName = "";
                        listening.SelectedItem = null;
                    });
                    if (App.ShowMessage(string.Format("成功创建隧道 #{0} {1}\n是否继续创建?", resp.DataTunnel.Id, resp.DataTunnel.Name), "创建成功", MessageBoxImage.Question, MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                    {
                        Close();
                        return;
                    }
                }
                finally
                {
                    Model.Creating = false;
                }
            });
        }

        private void ButtonReload_Click(object sender, RoutedEventArgs e) => Model.ReloadListening();

        private void Listening_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count == 1 && e.AddedItems[0] is LocalProcessModel l)
            {
                Model.Type = l.Protocol;
                Model.LocalPort = int.Parse(l.Port);
                Model.LocalAddress = l.Address == "0.0.0.0" ? "127.0.0.1" : l.Address;
            }
        }
    }
}
