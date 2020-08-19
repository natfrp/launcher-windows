using System;
using System.Windows.Controls;

using SakuraLauncher.Model;

namespace SakuraLauncher.View
{
    /// <summary>
    /// SettingsTab.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsTab : UserControl
    {
        private LauncherModel Model => (LauncherModel)DataContext;

        public SettingsTab(LauncherModel main)
        {
            InitializeComponent();
            DataContext = main;
        }

        private void ToggleButtonAutoRun_Checked(object sender, System.Windows.RoutedEventArgs e) => throw new NotImplementedException(); // App.SetAutoRun(true);

        private void ToggleButtonAutoRun_Unchecked(object sender, System.Windows.RoutedEventArgs e) => throw new NotImplementedException(); // App.SetAutoRun(false);

        private void ButtonUpdate_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // TODO: IPC
        }

        private void ButtonLogin_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // TODO: IPC
        }
    }
}
