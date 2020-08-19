using System.Windows;
using System.Windows.Controls;

using SakuraLauncher.Model;

namespace SakuraLauncher.View
{
    public partial class TunnelTab : UserControl
    {
        private LauncherModel Model => (LauncherModel)DataContext;

        public TunnelTab(LauncherModel main)
        {
            InitializeComponent();
            DataContext = main;
        }

        private void ButtonCreate_Click(object sender, RoutedEventArgs e) => new CreateTunnelWindow().ShowDialog();

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button).DataContext is TunnelModel tunnel)
            {
                if (App.ShowMessage(string.Format("确定要删除隧道 #{0} {1} 吗?", tunnel.Id, tunnel.Name), "操作确认", MessageBoxImage.Asterisk, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                {
                    IsEnabled = false;
                    // TODO: IPC
                    IsEnabled = true;
                }
            }
        }
    }
}
