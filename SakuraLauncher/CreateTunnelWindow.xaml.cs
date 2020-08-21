using System.Windows;
using System.Windows.Controls;

using SakuraLibrary.Model;

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
            DataContext = Model = new CreateTunnelModel(launcher);

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
            Model.RequestCreate(node.Id, (success, message) =>
            {
                if (!success)
                {
                    App.ShowMessage(message, "操作失败", MessageBoxImage.Error);
                    return;
                }
                if (App.ShowMessage(message + "\n是否继续创建?", "创建成功", MessageBoxImage.Question, MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                {
                    Dispatcher.Invoke(() => Close());
                    return;
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
