using System;
using System.IO;
using System.Windows;
using System.Threading;

using MaterialDesignThemes.Wpf;

using SakuraLibrary;

namespace SakuraLauncher
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        public static new App Current => (App)Application.Current;

        public Mutex AppMutex = null;
        public EventWaitHandle ActivateEventHandle = null;

        private void Application_Exit(object sender, ExitEventArgs e) => AppMutex?.ReleaseMutex();

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            Environment.CurrentDirectory = Path.GetDirectoryName(Utils.ExecutablePath);

            Utils.VerifySignature(Utils.LibraryPath, Utils.ExecutablePath, Path.GetFullPath(Consts.ServiceExecutable));
            Utils.ValidateSettings();

            var tmp = Path.Combine(Consts.WorkingDirectory, "Temp");
            Environment.SetEnvironmentVariable("TEMP", tmp);
            Environment.SetEnvironmentVariable("TMP", tmp);
            Directory.CreateDirectory(tmp);

            var minimize = false;
            foreach (var a in e.Args)
            {
                if (a == "--minimize")
                {
                    minimize = true;
                }
            }

            AppMutex = new Mutex(true, "SakuraLauncher_" + Utils.InstallationHash, out bool created);
            ActivateEventHandle = new EventWaitHandle(false, EventResetMode.AutoReset, "SakuraLauncher_ActivateEvent_" + Utils.InstallationHash);
            if (!created)
            {
                if (!ActivateEventHandle.Set())
                {
                    Current.Dispatcher.Invoke(() => MessageBox.Show("请不要重复开启 SakuraFrp 客户端", "Oops", MessageBoxButton.OK, MessageBoxImage.Warning));
                }
                Environment.Exit(0);
            }
            new Thread(() =>
            {
                while (ActivateEventHandle.WaitOne())
                {
                    Dispatcher.InvokeAsync(() =>
                    {
                        MainWindow.Show();
                        MainWindow.WindowState = WindowState.Normal;
                        MainWindow.Activate();
                    });
                }
            })
            {
                IsBackground = true,
            }.Start();

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

            var theme = Resources.MergedDictionaries[0] as BundledTheme;
            theme.PrimaryColor = (MaterialDesignColors.PrimaryColor)Enum.Parse(typeof(MaterialDesignColors.PrimaryColor), materialColor);
            theme.SecondaryColor = (MaterialDesignColors.SecondaryColor)Enum.Parse(typeof(MaterialDesignColors.SecondaryColor), materialColor);

            Resources.MergedDictionaries[1].Source = new Uri("/Theme/" + color + ".xaml", UriKind.Relative);

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
