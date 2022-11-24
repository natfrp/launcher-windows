using SakuraLauncher.Helper;
using SakuraLauncher.Model;
using SakuraLibrary;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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

            Model.PropertyChanged += (s, e) =>
            {
                if (Model.CheckingUpdate && e.PropertyName == nameof(Model.Update))
                {
                    Model.CheckingUpdate = false;
                    if (!Model.Update.UpdateAvailable)
                    {
                        App.ShowMessage("您当前使用的启动器与 frpc 均为最新版本", "提示", MessageBoxImage.Information);
                    }
                }
            };
        }

        private void ButtonUpdate_Click(object sender, RoutedEventArgs e)
        {
            if (Model.CheckingUpdate)
            {
                return;
            }
            Model.CheckingUpdate = true;
            Model.RequestUpdateCheck();
        }

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
