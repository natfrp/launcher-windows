using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
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
            try
            {
                if (!agreement.IsChecked.HasValue || !agreement.IsChecked.Value) throw new Exception("您需要仔细阅读说明内容并同意承担该功能可能带来的安全风险才能启用远程控制");
                if (password.Text == "") throw new Exception("密码不能为空");
                if (password.Text.Length < 8) throw new Exception("密码至少需要 8 位");

                Model.Config.RemoteManagementKey = password.Text;
                Model.PushServiceConfig(true);
            }
            catch (Exception ex)
            {
                Model.ShowMessage(ex.Message, "错误", SakuraLibrary.Model.LauncherModel.MessageMode.Error);
            }
            Close();
        }
    }
}
