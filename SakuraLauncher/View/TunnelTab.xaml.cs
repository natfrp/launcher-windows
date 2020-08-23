using System.Windows;
using System.Windows.Controls;

using SakuraLibrary.Model;

using SakuraLauncher.Model;

namespace SakuraLauncher.View
{
    public partial class TunnelTab : UserControl
    {
        private readonly LauncherViewModel Model;

        public TunnelTab(LauncherViewModel main)
        {
            InitializeComponent();
            DataContext = Model = main;
        }

        private void ButtonCreate_Click(object sender, RoutedEventArgs e) => new CreateTunnelWindow(Model).ShowDialog();

        private void ButtonReload_Click(object sender, RoutedEventArgs e)
        {
            IsEnabled = false;
            Model.RequestReloadTunnels((a, b) =>
            {
                Dispatcher.Invoke(() => IsEnabled = true);
                Model.SimpleFailureHandler(a, b);
            });
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button).DataContext is TunnelModel tunnel)
            {
                if (App.ShowMessage(string.Format("确定要删除隧道 #{0} {1} 吗?", tunnel.Id, tunnel.Name), "操作确认", MessageBoxImage.Asterisk, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    IsEnabled = false;
                    Model.RequestDeleteTunnel(tunnel.Id, (a, b) =>
                    {
                        Dispatcher.Invoke(() => IsEnabled = true);
                        Model.SimpleFailureHandler(a, b);
                    });
                }
            }
        }
    }
}
