using System.Windows;

using SakuraLauncher.Model;

namespace SakuraLauncher
{
    /// <summary>
    /// RemoteConfigWindow.xaml 的交互逻辑
    /// </summary>
    public partial class RemoteConfigWindow : Window
    {
        public readonly LauncherViewModel Model;

        public RemoteConfigWindow(LauncherViewModel launcher)
        {
            InitializeComponent();
            DataContext = Model = launcher;
        }

        private void ButtonUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (!agreement.IsChecked.HasValue || !agreement.IsChecked.Value)
            {
                App.ShowMessage("您需要仔细阅读说明内容并同意承担该功能可能带来的安全风险才能启用远程控制", "错误", MessageBoxImage.Error);
                return;
            }
            if (password.Text == "")
            {
                App.ShowMessage("密码不能为空", "错误", MessageBoxImage.Error);
                return;
            }
            Model.Config.RemoteKeyNew = password.Text;
            Model.PushServiceConfig();
            Model.Config.RemoteKeyNew = "";
            Model.Config.RemoteKeySet = true;
            Model.Config = Model.Config; // Hacky
            Close();
        }
    }
}
