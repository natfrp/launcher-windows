using SakuraLauncher.Helper;
using SakuraLauncher.Model;
using SakuraLibrary.Model;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

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

            var sd = main.TunnelsView.SortDescriptions[0];
            foreach (Control child in sortMenu.Items)
            {
                if (child is not MenuItem mi || mi.Tag is not string tag) continue;
                mi.IsCheckable = true;
                mi.IsChecked = tag == sd.PropertyName || tag == "_" && sd.Direction == ListSortDirection.Descending;
                mi.Click += MenuItem_SortClicked;
            }

            DataContext = Model = main;
        }

        private void ButtonCreate_Click(object sender, RoutedEventArgs e)
        {
            if (Model.LegacyCreateTunnel || Model.WebView2Environment == null)
            {
                Model.ShowDialog(new CreateTunnelWindow(Model), this);
            }
            else
            {
                Model.ShowDialog(new CreateTunnelWindow2(Model), this);
            }
        }

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

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            if (Model.WebView2Environment == null)
            {
                Model.ShowMessage("WebView2 初始化失败，该功能不可用。如果已安装 WebView2 请重启启动器。", "错误", LauncherModel.MessageMode.Ok | LauncherModel.MessageMode.Error);
                return;
            }
            if ((sender as Button).DataContext is TunnelModel tunnel)
            {
                Model.ShowDialog(new CreateTunnelWindow2(Model, tunnel.Proto), this);
            }
        }

        private void ButtonSort_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button b) b.ContextMenu.IsOpen = true;
        }

        private void MenuItem_SortClicked(object sender, RoutedEventArgs e)
        {
            if (sender is not MenuItem mi || Model == null) return;

            var sd = Model.TunnelsView.SortDescriptions[0];
            sd = new SortDescription(sd.PropertyName, sd.Direction);
            if ((string)mi.Tag == "_")
            {
                sd.Direction = mi.IsChecked ? ListSortDirection.Descending : ListSortDirection.Ascending;

                // Update direction in other sort descs
                for (var i = 1; i < Model.TunnelsView.SortDescriptions.Count; i++)
                {
                    var sdSec = Model.TunnelsView.SortDescriptions[i];
                    Model.TunnelsView.SortDescriptions[i] = new SortDescription(sdSec.PropertyName, sd.Direction);
                }
            }
            else if (mi.IsChecked)
            {
                sd.PropertyName = (string)mi.Tag;
                foreach (Control child in sortMenu.Items)
                {
                    if (child is MenuItem cmi && (string)cmi.Tag != "_" && cmi != mi)
                    {
                        cmi.IsChecked = false;
                    }
                }
            }
            else
            {
                mi.IsChecked = true;
                return;
            }
            Model.TunnelsView.SortDescriptions[0] = sd;
            Model.Save();
        }

        private void ScrollViewer_PreviewMouseWheel(object sender, System.Windows.Input.MouseWheelEventArgs e)
        {
            var view = (ScrollViewer)sender;
            view.ScrollToVerticalOffset(view.VerticalOffset - e.Delta);
            e.Handled = true;
        }
    }
}
