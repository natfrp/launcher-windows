using System;
using System.Windows;
using System.Threading;
using System.Windows.Controls;

using SakuraLibrary.Proto;

using SakuraLauncher.Model;

namespace SakuraLauncher.View
{
    /// <summary>
    /// SettingsTab.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsTab : UserControl
    {
        private readonly LauncherModel Model;

        public SettingsTab(LauncherModel main)
        {
            InitializeComponent();
            DataContext = Model = main;
        }

        private void ToggleButtonAutoRun_Checked(object sender, RoutedEventArgs e) => throw new NotImplementedException(); // App.SetAutoRun(true);

        private void ToggleButtonAutoRun_Unchecked(object sender, RoutedEventArgs e) => throw new NotImplementedException(); // App.SetAutoRun(false);

        private void ButtonUpdate_Click(object sender, RoutedEventArgs e)
        {
            // TODO: IPC
        }

        private void ButtonLogin_Click(object sender, RoutedEventArgs e)
        {
            Model.LoggingIn = true;
            ThreadPool.QueueUserWorkItem(s =>
            {
                try
                {
                    var resp = Model.Pipe.Request(new RequestBase()
                    {
                        Type = MessageID.UserLogin,
                        DataUserLogin = new UserLogin()
                        {
                            Token = Model.UserToken
                        }
                    });
                    if (!resp.Success)
                    {
                        App.ShowMessage(resp.Message, "登录失败", MessageBoxImage.Error, MessageBoxButton.OK);
                        return;
                    }
                    Model.UserInfo = resp.DataUser;
                }
                finally
                {
                    Model.LoggingIn = false;
                }
            });
        }
    }
}
