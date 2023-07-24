using SakuraLauncher.Helper;
using SakuraLauncher.Model;
using SakuraLibrary;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MessageMode = SakuraLibrary.Model.LauncherModel.MessageMode;
using UpdateStatus = SakuraLibrary.Proto.SoftwareUpdate.Types.Status;

namespace SakuraLauncher.View
{
    /// <summary>
    /// SettingsTab.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsTab : UserControl
    {
        private readonly LauncherViewModel Model;
        private readonly TouchScrollHelper scrollHelper;

        public SettingsTab(LauncherViewModel main)
        {
            InitializeComponent();
            scrollHelper = new TouchScrollHelper(scrollViewer);

            DataContext = Model = main;
        }

        private void ButtonUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (Model.CheckingUpdate)
            {
                return;
            }
            Model.CheckingUpdate = true;
            Model.RequestCheckUpdateAsync().ContinueWith(r => Dispatcher.Invoke(() =>
            {
                Model.CheckingUpdate = false;
                if (r.Exception != null)
                {
                    Model.ShowMessage(r.Exception.ToString(), "更新检查失败", MessageMode.Error);
                }
                else if (r.Result != null)
                {
                    if (r.Result.Status == UpdateStatus.NoUpdate)
                    {
                        Model.ShowMessage("当前没有可用更新", "提示", MessageMode.Info);
                    }
                    else if (r.Result.Status == UpdateStatus.Failed)
                    {
                        Model.ShowMessage("更新检查失败, 请查看日志输出", "错误", MessageMode.Error);
                    }
                }
            }));
        }

        private void ButtonLogin_Click(object sender, RoutedEventArgs e)
        {
            if (Model.LoggingIn)
            {
                return;
            }
            _ = Model.LoginOrLogout();
        }

        private void ButtonRefreshNodes_Click(object sender, RoutedEventArgs e)
        {
            var btn = sender as Button;
            btn.IsEnabled = false;
            Model.RequestReloadNodesAsync().ContinueWith(r => Dispatcher.Invoke(() =>
            {
                if (r.Exception != null)
                {
                    Model.ShowMessage(r.Exception.ToString(), "节点列表刷新失败", MessageMode.Error);
                }
                else
                {
                    Model.SnackMessageQueue.Enqueue("节点列表刷新成功");
                }
                btn.IsEnabled = true;
            }));
        }

        private void ButtonSwitchMode_Click(object sender, RoutedEventArgs e) => Model.SwitchWorkingMode();

        private void ButtonRemotePassword_Click(object sender, RoutedEventArgs e) => new RemoteConfigWindow(Model).ShowDialog();

        private void ButtonOpenCWD_Click(object sender, RoutedEventArgs e) => Process.Start(new ProcessStartInfo()
        {
            FileName = Consts.WorkingDirectory,
            UseShellExecute = true,
            Verb = "open"
        });

        private void Hint_TouchUp(object sender, TouchEventArgs e)
        {
            if ((sender as FrameworkElement)?.ToolTip is string tooltip)
            {
                Model.SnackMessageQueue.Enqueue(tooltip);
            }
        }

        private void Save(object sender, RoutedEventArgs e) => Model.Save();
    }
}
