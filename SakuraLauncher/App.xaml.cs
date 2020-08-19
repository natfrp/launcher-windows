using System;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;
using System.Windows;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Security.Principal;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Runtime.InteropServices;

using fastJSON;

using SakuraLauncher.Model;
using SakuraLibrary;

namespace SakuraLauncher
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int SetProcessShutdownParameters(int dwLevel, int dwFlags);

        [DllImport("kernel32.dll")]
        public static extern bool QueryFullProcessImageName([In] IntPtr hProcess, [In] uint dwFlags, [Out] StringBuilder lpExeName, [In, Out] ref uint lpdwSize);
        
        public static Version FrpcVersion = null;
        public static float FrpcVersionSakura = 0;

        public static string AutoRunFile { get; private set; }

        public static App Instance = null;

        public static MessageBoxResult ShowMessage(string text, string title, MessageBoxImage icon, MessageBoxButton buttons = MessageBoxButton.OK)
        {
            return Instance.Dispatcher.Invoke(() => MessageBox.Show(text, title, buttons, icon));
        }

        public Mutex AppMutex = null;

        public App() : base()
        {
            Instance = this;
        }

        private void Application_Exit(object sender, ExitEventArgs e) => AppMutex?.ReleaseMutex();

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            /*
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
                var split = a.Split('=');
                if (split[0] == "--minimize")
                {
                    minimize = true;
                }
            }
            AppMutex = new Mutex(true, "SakuraLauncher_" + Utils.ExecutablePath.GetHashCode(), out bool created);
            AutoRunFile = Environment.GetFolderPath(Environment.SpecialFolder.Startup) + @"\SakuraLauncher_" + Utils.Md5(Utils.ExecutablePath) + ".lnk";
            if (created)
            {
                var test = Path.GetFullPath("");// TODO: TunnelModel.ClientPath);
                var processes = Process.GetProcessesByName("frpc").Where(p =>
                {
                    try
                    {
                        uint bufferSize = 256;
                        var sb = new StringBuilder((int)bufferSize - 1);
                        if (QueryFullProcessImageName(p.Handle, 0, sb, ref bufferSize))
                        {
                            return Path.GetFullPath(sb.ToString()) == test;
                        }
                    }
                    catch { }
                    return false;
                }).ToArray();

                if (processes.Length != 0)
                {
                    switch (MessageBox.Show("发现 " + processes.Length + " 个的残留的 frpc 进程, 是否尝试将其关闭?\n这些进程可能是启动器不正常退出造成的残留.\n如果您不知道如何选择, 请点 \"是\".\n\n是 = 关闭所有进程\n否 = 忽略并继续\n取消 = 退出程序", "注意", MessageBoxButton.YesNoCancel, MessageBoxImage.Information))
                    {
                    case MessageBoxResult.Yes:
                        foreach (var p in processes)
                        {
                            try
                            {
                                p.Kill();
                                p.WaitForExit(200);
                            }
                            catch { }
                        }
                        break;
                    case MessageBoxResult.No:
                        break;
                    default:
                        Environment.Exit(0);
                        break;
                    }
                }

                if (SetProcessShutdownParameters(0x300, 0) == 0)
                {
                    ShowMessage("无法设置关机优先级, 这可能导致隧道开机自启无法正常工作, 请检查杀毒软件是否拦截了此操作\n错误代码: " + Marshal.GetLastWin32Error(), "Oops", MessageBoxImage.Warning);
                }

                MainWindow = new MainWindow(File.Exists(AutoRunFile));
                if (!minimize)
                {
                    MainWindow.Show();
                }
            }
            else
            {
                ShowMessage("请不要重复开启 SakuraFrp 客户端. 如果想运行多个实例请将软件复制到其他目录.", "Oops", MessageBoxImage.Warning);
                Environment.Exit(0);
            }
        }

        private void TrayMenu_Show(object sender, RoutedEventArgs e) => MainWindow.Show();

        private void TrayMenu_Exit(object sender, RoutedEventArgs e) => MainWindow.Close();
    }
}
