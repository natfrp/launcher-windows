using System.Windows;
using System.Windows.Controls;

using SakuraLauncher.Model;

namespace SakuraLauncher.View
{
    /// <summary>
    /// SettingsTab.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsTab : UserControl
    {
        private readonly LauncherViewModel Model;

        public SettingsTab(LauncherViewModel main)
        {
            InitializeComponent();
            DataContext = Model = main;
        }

        private void ButtonUpdate_Click(object sender, RoutedEventArgs e) => Model.RequestUpdateCheck(update =>
        {
            if (!update.UpdateFrpc && !update.UpdateLauncher)
            {
                App.ShowMessage("您当前使用的启动器与 frpc 均为最新版本", "提示", MessageBoxImage.Information);
                return;
            }
            Model.ConfirmUpdate(false, Model.SimpleFailureHandler, Model.SimpleConfirmHandler, Model.SimpleWarningHandler);
        });

        private void ButtonLogin_Click(object sender, RoutedEventArgs e)
        {
            if (Model.LoggingIn)
            {
                return;
            }
            Model.RequestLogin(Model.SimpleFailureHandler);
        }

        private void ButtonSwitchMode_Click(object sender, RoutedEventArgs e) => Model.SwitchWorkingMode(Model.SimpleHandler, Model.SimpleConfirmHandler);

        private void ButtonRemotePassword_Click(object sender, RoutedEventArgs e) => new RemoteConfigWindow(Model).ShowDialog();

        private void Save(object sender, RoutedEventArgs e) => Model.Save();
    }
}
