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

        private void ButtonUpdate_Click(object sender, RoutedEventArgs e)
        {
            // TODO: IPC
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

        private void Save(object sender, RoutedEventArgs e) => Model.Save();
    }
}
