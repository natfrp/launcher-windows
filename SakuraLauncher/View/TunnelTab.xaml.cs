using System.Windows;
using System.Threading;
using System.Windows.Controls;

using SakuraLibrary.Proto;

using SakuraLauncher.Model;

namespace SakuraLauncher.View
{
    public partial class TunnelTab : UserControl
    {
        private readonly LauncherModel Model;

        public TunnelTab(LauncherModel main)
        {
            InitializeComponent();
            DataContext = Model = main;
        }

        private void ButtonCreate_Click(object sender, RoutedEventArgs e) => new CreateTunnelWindow(Model).ShowDialog();

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button).DataContext is TunnelModel tunnel)
            {
                if (App.ShowMessage(string.Format("确定要删除隧道 #{0} {1} 吗?", tunnel.Id, tunnel.Name), "操作确认", MessageBoxImage.Asterisk, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    IsEnabled = false;
                    ThreadPool.QueueUserWorkItem(s =>
                    {
                        try
                        {
                            var resp = Model.Pipe.Request(new RequestBase()
                            {
                                Type = MessageID.TunnelDelete,
                                DataId = tunnel.Id
                            });
                            if (!resp.Success)
                            {
                                App.ShowMessage(resp.Message, "操作失败", MessageBoxImage.Error, MessageBoxButton.OK);
                                return;
                            }
                        }
                        finally
                        {
                            Dispatcher.Invoke(() => IsEnabled = true);
                        }
                    });
                }
            }
        }
    }
}
