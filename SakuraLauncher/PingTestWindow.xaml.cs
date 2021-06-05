using System.Windows;

using SakuraLibrary.Model;

namespace SakuraLauncher
{
    /// <summary>
    /// PingTestWindow.xaml 的交互逻辑
    /// </summary>
    public partial class PingTestWindow : Window
    {
        public readonly PingTestModel Model;

        public PingTestWindow(LauncherModel launcher)
        {
            InitializeComponent();
            DataContext = Model = new PingTestModel(launcher);

            Model.DoTest();
        }

        private void Button_Click(object sender, RoutedEventArgs e) => Model.DoTest();
    }
}
