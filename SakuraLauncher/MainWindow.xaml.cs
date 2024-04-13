using SakuraLauncher.Model;
using SakuraLauncher.View;
using SakuraLibrary;
using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;

namespace SakuraLauncher
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public readonly LauncherViewModel Model;

        public double ScaleFactor = 1;
        public readonly int SideWidth = 180 + (int)Math.Ceiling(SystemParameters.VerticalScrollBarWidth) + 8 * 2, CardWidth = 256 + 16 * 2;

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
                new AboutTab(Model)
            };
            Model.SwitchTab(2);
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

        private int GetAlignedWidth(double width)
        {
            double side = SideWidth * ScaleFactor, card = CardWidth * ScaleFactor;
            return (int)(Math.Max(Math.Round((width - side) / card), 2) * card + side);
        }

        private void TrayIcon_TrayLeftMouseUp(object sender, RoutedEventArgs e)
        {
            Show();
            Topmost = true; // Still using this in 2020?
            Topmost = false;
        }

        #region Window Events

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            var source = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);

            if (Model.AlignWidth)
            {
                ScaleFactor = 1;
                Width = GetAlignedWidth(Width); // DPI-irrelevant
            }
            ScaleFactor = source.CompositionTarget.TransformToDevice.M11;

            source.AddHook((IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled) =>
            {
                if (Model.AlignWidth && msg == (int)WindowsMessages.WM_SIZING)
                {
                    var rect = Marshal.PtrToStructure<RECT>(lParam);
                    rect.Width = GetAlignedWidth(rect.Width);
                    Marshal.StructureToPtr(rect, lParam, false);

                    handled = true;
                }
                return IntPtr.Zero;
            });

            Model.SetupWebView2Environment();
        }

        private void Window_DpiChanged(object sender, DpiChangedEventArgs e) => ScaleFactor = e.NewDpi.DpiScaleX;

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            NTAPI.ReleaseCapture();
            NTAPI.SendMessage(new WindowInteropHelper(this).Handle, 0xA1, new IntPtr(0x2), IntPtr.Zero);
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Hide();
            e.Cancel = true;
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

        private void Update_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) => Model.ConfirmUpdate();

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
