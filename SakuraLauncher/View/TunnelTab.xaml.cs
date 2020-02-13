using System.Windows;
using System.Windows.Controls;

using SakuraLauncher.Data;

namespace SakuraLauncher.View
{
    public partial class TunnelTab : UserControl
    {
        private readonly MainWindow Main = null;

        public TunnelTab(MainWindow main)
        {
            InitializeComponent();
            DataContext = Main = main;
        }

        private void ButtonAddListener_Click(object sender, RoutedEventArgs e) => new CreateTunnelWindow().ShowDialog();

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            if((sender as Button).DataContext is Tunnel tunnel && tunnel.IsReal)
            {
                if(MessageBox.Show("是否确定删除隧道 " + tunnel.Name + "?\n该操作不可撤销.", "Confirm", MessageBoxButton.OKCancel, MessageBoxImage.Asterisk) != MessageBoxResult.OK)
                {
                    return;
                }
                IsEnabled = false;
                App.ApiRequest("deletetunnel", "name=" + tunnel.Real.Name + "&server=" + tunnel.Real.ServerID).ContinueWith(t => Dispatcher.Invoke(() =>
                {
                    IsEnabled = true;
                    var json = t.Result;
                    if(json == null)
                    {
                        return;
                    }
                    Main.Tunnels.Remove(tunnel);
                }));
            }
        }
    }
}
