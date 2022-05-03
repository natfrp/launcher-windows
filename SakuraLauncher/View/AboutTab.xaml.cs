using SakuraLauncher.Model;
using System.Windows.Controls;

namespace SakuraLauncher.View
{
    /// <summary>
    /// AboutTab.xaml 的交互逻辑
    /// </summary>
    public partial class AboutTab : UserControl
    {
        public AboutTab(LauncherViewModel model)
        {
            InitializeComponent();
            DataContext = new AboutModel(model);
        }
    }
}
