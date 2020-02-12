using System.Reflection;
using System.Windows.Controls;

namespace SakuraLauncher.View
{
    /// <summary>
    /// AboutTab.xaml 的交互逻辑
    /// </summary>
    public partial class AboutTab : UserControl
    {
        public string License => Properties.Resources.LICENSE;
        public string Version => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public AboutTab()
        {
            InitializeComponent();
            DataContext = this;
        }
    }
}
