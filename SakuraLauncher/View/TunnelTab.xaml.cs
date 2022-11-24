using System.Windows;
using System.Windows.Controls;

using SakuraLibrary.Model;

using SakuraLauncher.Model;
using SakuraLauncher.Helper;

namespace SakuraLauncher.View
{
    public partial class TunnelTab : UserControl
    {
        private readonly LauncherViewModel Model;
        private readonly TouchScrollHelper scrollHelper;

        public TunnelTab(LauncherViewModel main)
        {
            InitializeComponent();
            scrollHelper = new TouchScrollHelper(scrollViewer);

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

        private void ScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            var view = (ScrollViewer)sender;
            view.ScrollToVerticalOffset(view.VerticalOffset - e.Delta);
            e.Handled = true;
        }
    }
}
