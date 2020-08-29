using System.Windows;
using System.Windows.Controls;

using SakuraLibrary.Proto;

using SakuraLauncher.Model;

namespace SakuraLauncher.View
{
    /// <summary>
    /// LogTab.xaml 的交互逻辑
    /// </summary>
    public partial class LogTab : UserControl
    {
        private readonly LauncherViewModel Model;

        private bool AutoScroll = true;

        public LogTab(LauncherViewModel main)
        {
            InitializeComponent();
            DataContext = Model = main;
        }

        private void ButtonClear_Click(object sender, RoutedEventArgs e)
        {
            Model.Logs.Clear();
            Model.Pipe.Request(MessageID.LogClear);
        }

        private void ScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (e.Source is ScrollViewer sv)
            {
                if (e.ExtentHeightChange == 0)
                {
                    AutoScroll = sv.VerticalOffset == sv.ScrollableHeight;
                }
                else if (AutoScroll)
                {
                    sv.ScrollToVerticalOffset(sv.ExtentHeight);
                }
            }
        }
    }
}
