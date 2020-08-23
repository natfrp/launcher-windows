using System;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Interop;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;

using SakuraLibrary;

using SakuraLauncher.View;
using SakuraLauncher.Model;

namespace SakuraLauncher
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public readonly LauncherViewModel Model;

        public UserControl[] Tabs = null;

        public MainWindow()
        {
            InitializeComponent();
            SetLogo(Properties.Settings.Default.LogoIndex);

            DataContext = Model = new LauncherViewModel(this);

            Tabs = new UserControl[] {
                new TunnelTab(Model),
                new LogTab(Model),
                new SettingsTab(Model),
                new AboutTab()
            };
            Model.SwitchTab(2);

            // TODO: Check daemon status
        }

        public void SetLogo(int index)
        {
            switch (index)
            {
            case 1:
                logo.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Resources/debian.png")));
                break;
            case 2:
                logo.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Resources/fishcake.png")));
                break;
            default:
                logo.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Resources/logo.png")));
                break;
            }
        }

        private void TrayIcon_TrayLeftMouseUp(object sender, RoutedEventArgs e)
        {
            Show();
            Topmost = true; // Still using this in 2020?
            Topmost = false;
        }

        #region Window Events

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NTAPI.ReleaseCapture();
            NTAPI.SendMessage(new WindowInteropHelper(this).Handle, 0xA1, new IntPtr(0x2), IntPtr.Zero);
        }

        private void Window_SizeChanged(object sender, SizeChangedEventArgs e) => Model.Save();

        private void ButtonHide_Click(object sender, RoutedEventArgs e) => Hide();

        private void ButtonMinimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void Logo_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var index = -1;
            switch (e.ClickCount)
            {
            case 3:
                index = 1;
                break;
            case 5:
                index = 2;
                break;
            }
            if (index != -1)
            {
                SetLogo(index);
                Properties.Settings.Default.LogoIndex = index;
                Properties.Settings.Default.Save();
            }
        }

        #endregion

        #region Tab Switching

        public void BeginTabStoryboard(string name) => tabContents.BeginStoryboard(Resources[name] as Storyboard);

        private void ButtonTab_Click(object sender, RoutedEventArgs e) => Model.SwitchTab(int.Parse((sender as Button).Tag as string));

        private void StoryboardTabHideAnimation_Completed(object sender, EventArgs e)
        {
            tabContents.Child = Tabs[Model.CurrentTab];
            BeginTabStoryboard("TabShowAnimation");
        }

        #endregion
    }
}
