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
#pragma warning disable IDE0052
        private readonly TouchScrollHelper scrollHelper;
#pragma warning restore IDE0052

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
            Model.RequestReloadTunnelsAsync().ContinueWith(_ => Dispatcher.Invoke(() => IsEnabled = true));
        }

        private void ButtonDelete_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button).DataContext is TunnelModel tunnel)
            {
                if (Model.ShowMessage(string.Format("确定要删除隧道 #{0} {1} 吗?", tunnel.Id, tunnel.Name), "操作确认", LauncherModel.MessageMode.OkCancel | LauncherModel.MessageMode.Confirm) == LauncherModel.MessageResult.Ok)
                {
                    IsEnabled = false;
                    Model.RequestDeleteTunnelAsync(tunnel.Id).ContinueWith(_ => Dispatcher.Invoke(() => IsEnabled = true));
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
