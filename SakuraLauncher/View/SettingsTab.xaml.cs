using System.Windows.Controls;

namespace SakuraLauncher.View
{
    /// <summary>
    /// SettingsTab.xaml 的交互逻辑
    /// </summary>
    public partial class SettingsTab : UserControl
    {
        private readonly MainWindow Main = null;

        public SettingsTab(MainWindow main)
        {
            InitializeComponent();
            DataContext = Main = main;
        }

        private void ToggleButtonAutoRun_Checked(object sender, System.Windows.RoutedEventArgs e) => App.SetAutoRun(true);

        private void ToggleButtonAutoRun_Unchecked(object sender, System.Windows.RoutedEventArgs e) => App.SetAutoRun(false);

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            if(Main.LoggingIn)
            {
                return;
            }
            if(Main.LoggedIn)
            {
                foreach(var t in Main.Tunnels)
                {
                    if(t.IsReal)
                    {
                        t.Real.Stop();
                    }
                }
                Main.Tunnels.Clear();
                Main.LoggedIn.Value = false;
                Main.UserToken.Value = "";
                Main.Save();
            }
            else
            {
                Main.TryLogin();
            }
        }
    }
}
