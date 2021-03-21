using System;
using System.IO;
using System.Windows;
using System.Threading;

using SakuraLibrary;

namespace SakuraLauncher
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public static new App Current => (App)Application.Current;

        public static MessageBoxResult ShowMessage(string text, string title, MessageBoxImage icon, MessageBoxButton buttons = MessageBoxButton.OK) => Current.Dispatcher.Invoke(() => MessageBox.Show(text, title, buttons, icon));

        public Mutex AppMutex = null;

        private void Application_Exit(object sender, ExitEventArgs e) => AppMutex?.ReleaseMutex();

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Environment.CurrentDirectory = Path.GetDirectoryName(Utils.ExecutablePath);

            Utils.VerifySignature(Utils.LibraryPath, Utils.ExecutablePath, Path.GetFullPath(Consts.ServiceExecutable));
            Utils.ValidateSettings();

            var minimize = false;
            foreach (var a in e.Args)
            {
                if (a == "--minimize")
                {
                    minimize = true;
                }
            }

            AppMutex = new Mutex(true, "SakuraLauncher_" + Utils.InstallationHash, out bool created);
            if (!created)
            {
                ShowMessage("请不要重复开启 SakuraFrp 客户端. 如果想运行多个实例请将软件复制到其他目录.", "Oops", MessageBoxImage.Warning);
                Environment.Exit(0);
            }

            var settings = SakuraLauncher.Properties.Settings.Default;
            if (settings.UpgradeRequired)
            {
                settings.Upgrade();
                settings.UpgradeRequired = false;
                settings.Save();
            }

            string color = "Teal", materialColor = "Teal";
            switch (settings.Theme)
            {
            case 1:
                color = "Gold";
                materialColor = "Amber";
                break;
            case 2:
                color = "5Yuan";
                materialColor = "Purple";
                break;
            case 3:
                color = "10Yuan";
                materialColor = "Blue";
                break;
            case 4:
                color = "20Yuan";
                materialColor = "Amber";
                break;
            case 5:
                color = "50Yuan";
                materialColor = "Teal";
                break;
            }
            Resources.MergedDictionaries[0].Source = new Uri("/Theme/" + color + ".xaml", UriKind.Relative);
            Resources.MergedDictionaries[1].Source = new Uri("pack://application:,,,/MaterialDesignColors;component/Themes/Recommended/Accent/MaterialDesignColor." + materialColor + ".xaml", UriKind.Absolute);
            Resources.MergedDictionaries[2].Source = new Uri("pack://application:,,,/MaterialDesignColors;component/Themes/Recommended/Primary/MaterialDesignColor." + materialColor + ".xaml", UriKind.Absolute);

            MainWindow = new MainWindow();
            if (!minimize)
            {
                MainWindow.Show();
            }
        }

        private void TrayMenu_Show(object sender, RoutedEventArgs e) => MainWindow.Show();

        private void TrayMenu_Exit(object sender, RoutedEventArgs e)
        {
            (MainWindow as MainWindow).trayIcon.Visibility = Visibility.Collapsed;
            Environment.Exit(0);
        }

        private void TrayMenu_ExitAll(object sender, RoutedEventArgs e)
        {
            var main = MainWindow as MainWindow;
            main.trayIcon.Visibility = Visibility.Collapsed;
            main.Model.Daemon.Stop();
            Environment.Exit(0);
        }
    }
}
