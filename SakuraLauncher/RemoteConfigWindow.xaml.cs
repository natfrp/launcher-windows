using SakuraLauncher.Model;
using System;
using System.Windows;
using static SakuraLibrary.Model.LauncherModel;

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
                if (!agreement.IsChecked.HasValue || !agreement.IsChecked.Value)
                {
                    Model.ShowMessage("您需要仔细阅读说明内容并同意承担该功能可能带来的安全风险才能启用远程管理", "操作失败", MessageMode.Error);
                    return;
                }
                if (password.Text.Length < 8)
                {
                    Model.ShowMessage("密码至少需要 8 位", "操作失败", MessageMode.Error);
                    return;
                }
                Model.Config.RemoteManagementKey = password.Text;
                Model.PushServiceConfig(true);
            }
            catch (Exception ex)
            {
                Model.ShowError(ex);
            }
            Close();
        }
    }
}
