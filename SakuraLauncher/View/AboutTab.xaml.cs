using SakuraLauncher.Helper;
using SakuraLauncher.Model;
using System.Windows.Controls;

namespace SakuraLauncher.View
{
    /// <summary>
    /// AboutTab.xaml 的交互逻辑
    /// </summary>
    public partial class AboutTab : UserControl
    {
        private readonly TouchScrollHelper scrollHelper;

        public AboutTab(LauncherViewModel model)
        {
            InitializeComponent();
            scrollHelper = new TouchScrollHelper(scrollViewer);

            DataContext = model;
        }
    }
}
