using System.Windows;
using System.Windows.Controls;

namespace SakuraLauncher.View
{
    public partial class TunnelTab : UserControl
    {
        private readonly MainWindow Main = null;

        public TunnelTab(MainWindow main)
        {
            InitializeComponent();
            DataContext = Main = main;
        }

        private void ButtonAddListener_Click(object sender, RoutedEventArgs e) => new CreateTunnelWindow().ShowDialog();
    }
}
