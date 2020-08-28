using System;
using System.IO;
using System.Windows;
using System.Diagnostics;
using System.Configuration;
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
            if (App.ShowMessage(string.Format("{0}{1}是否进行更新?", update.UpdateFrpc ? "frpc 有新版本\n" : "", update.UpdateLauncher ? "启动器有新版本\n" : ""), "提示", MessageBoxImage.Asterisk, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
            {
                Model.DoUpdate(false, Model.SimpleFailureHandler);
            }
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

        private void ButtonLOL_Click(object sender, RoutedEventArgs e) => Process.Start(Path.GetDirectoryName(ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal).FilePath));

        private void ButtonLOL2_Click(object sender, RoutedEventArgs e) => Process.Start("calc.exe");

        private void ButtonLOL3_Click(object sender, RoutedEventArgs e)
        {
            Properties.Settings.Default.Theme = Properties.Settings.Default.Theme == 1 ? 0 : 1;
            Model.Save();
            Environment.Exit(0);
        }
    }
}
