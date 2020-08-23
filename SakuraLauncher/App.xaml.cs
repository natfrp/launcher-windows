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
            /*
        public static Version FrpcVersion = null;
        public static float FrpcVersionSakura = 0;

            if (!File.Exists("SKIP_LAUNCHER_SIGN_VERIFY") && !WinTrust.VerifyEmbeddedSignature(Assembly.GetExecutingAssembly().Location))
            {
                ShowMessage("@@@@@@@@@@@@@@@@@@\n       !!!  警告: 启动器签名验证失败  !!!\n@@@@@@@@@@@@@@@@@@\n\n" +
                    "您使用的启动器文件未能通过签名验证, 该文件可能已损坏或被纂改\n您的电脑可能已经被病毒感染, 请立即进行杀毒然后重新下载完整的启动器压缩包\n\n" +
                    "如果您从源代码编译了启动器, 请在工作目录下创建 SKIP_LAUNCHER_SIGN_VERIFY 文件来禁用签名验证", "警告", MessageBoxImage.Warning);
            }

            if (!File.Exists(TunnelModel.ClientPath))
            {
                // Try to correct the working dir
                Environment.CurrentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (!File.Exists(TunnelModel.ClientPath))
                {
                    ShowMessage("未找到 frpc.exe, 请尝试重新下载客户端", "Oops", MessageBoxImage.Error);
                    Environment.Exit(0);
                }
            }

            if (!File.Exists("SKIP_FRPC_SIGN_VERIFY") && !WinTrust.VerifyEmbeddedSignature(TunnelModel.ClientPath))
            {
                ShowMessage("@@@@@@@@@@@@@@@@@@\n       !!!  警告: FRPC 签名验证失败  !!!\n@@@@@@@@@@@@@@@@@@\n\n" +
                    "您使用的 frpc.exe 未能通过签名验证, 该文件可能已损坏或被纂改\n您的电脑可能已经被病毒感染, 请立即进行杀毒然后重新下载完整的启动器压缩包\n\n" +
                    "如果您准备使用非 Sakura Frp 提供的 frpc.exe, 请在工作目录下创建 SKIP_FRPC_SIGN_VERIFY 文件来禁用签名验证", "警告", MessageBoxImage.Warning);
            }
            
            string[] temp = null;
            try
            {
                var start = new ProcessStartInfo(TunnelModel.ClientPath, "-v")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    StandardOutputEncoding = Encoding.UTF8
                };
                using (var p = Process.Start(start))
                {
                    p.Start();
                    p.WaitForExit(100);
                    temp = p.StandardOutput.ReadLine().Trim().Split(new string[] { "-sakura-" }, StringSplitOptions.RemoveEmptyEntries);
                }
            }
            catch { }
            
            if (temp[0].Length > 0 && temp[0][0] == 'v')
            {
                temp[0] = temp[0].Substring(1);
            }
            if (!Version.TryParse(temp[0], out FrpcVersion))
            {
                ShowMessage("无法获取 frpc.exe 的版本[1], 请尝试重新下载客户端", "Oops", MessageBoxImage.Error);
                Environment.Exit(0);
            }
            if (temp.Length == 2 && !float.TryParse(temp[1], out FrpcVersionSakura))
            {
                ShowMessage("无法获取 frpc.exe 的版本[2], 请尝试重新下载客户端", "Oops", MessageBoxImage.Error);
                Environment.Exit(0);
            }
            */

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

        private void TrayMenu_Exit(object sender, RoutedEventArgs e) => Environment.Exit(0);

        private void TrayMenu_ExitAll(object sender, RoutedEventArgs e)
        {
            (MainWindow as MainWindow).Model.Daemon.Stop();
            Environment.Exit(0);
        }
    }
}
