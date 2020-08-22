using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

using SakuraLibrary;

namespace LegacyLauncher
{
    static class Program
    {
        // public static string AutoRunFile = Environment.GetFolderPath(Environment.SpecialFolder.Startup) + @"\LegacySakuraLauncher_" + Md5(ExecutablePath) + ".lnk";

        // public static Version FrpcVersion = null;
        // public static float FrpcVersionSakura = 0;

        public static Mutex AppMutex = null;
        public static Form TopMostForm => new Form() { TopMost = true };

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Environment.CurrentDirectory = Path.GetDirectoryName(Utils.ExecutablePath);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            /*
            if (!File.Exists("SKIP_LAUNCHER_SIGN_VERIFY") && !WinTrust.VerifyEmbeddedSignature(Assembly.GetExecutingAssembly().Location))
            {
                MessageBox.Show(TopMostForm, "@@@@@@@@@@@@@@@@@@\n       !!!  警告: 启动器签名验证失败  !!!\n@@@@@@@@@@@@@@@@@@\n\n" +
                    "您使用的启动器文件未能通过签名验证, 该文件可能已损坏或被纂改\n您的电脑可能已经被病毒感染, 请立即进行杀毒然后重新下载完整的启动器压缩包\n\n" +
                    "如果您从源代码编译了启动器, 请在工作目录下创建 SKIP_LAUNCHER_SIGN_VERIFY 文件来禁用签名验证", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            if (!File.Exists(Tunnel.ClientPath))
            {
                // Try to correct the working dir
                Environment.CurrentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
                if (!File.Exists(Tunnel.ClientPath))
                {
                    MessageBox.Show(TopMostForm, "未找到 frpc.exe, 请尝试重新下载客户端", "Oops", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    Environment.Exit(0);
                }
            }

            if (!File.Exists("SKIP_FRPC_SIGN_VERIFY") && !WinTrust.VerifyEmbeddedSignature(Tunnel.ClientPath))
            {
                MessageBox.Show(TopMostForm, "@@@@@@@@@@@@@@@@@@\n       !!!  警告: FRPC 签名验证失败  !!!\n@@@@@@@@@@@@@@@@@@\n\n" +
                    "您使用的 frpc.exe 未能通过签名验证, 该文件可能已损坏或被纂改\n您的电脑可能已经被病毒感染, 请立即进行杀毒然后重新下载完整的启动器压缩包\n\n" +
                    "如果您准备使用非 Sakura Frp 提供的 frpc.exe, 请在工作目录下创建 SKIP_FRPC_SIGN_VERIFY 文件来禁用签名验证", "警告", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

            string[] temp = null;
            try
            {
                var start = new ProcessStartInfo(Tunnel.ClientPath, "-v")
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
                MessageBox.Show(TopMostForm, "无法获取 frpc.exe 的版本[1], 请尝试重新下载客户端", "Oops", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }
            if (temp.Length == 2 && !float.TryParse(temp[1], out FrpcVersionSakura))
            {
                MessageBox.Show(TopMostForm, "无法获取 frpc.exe 的版本[2], 请尝试重新下载客户端", "Oops", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(0);
            }
            */

            var minimize = false;
            foreach (var a in args)
            {
                var split = a.Split('=');
                if (split[0] == "--minimize")
                {
                    minimize = true;
                }
            }

            AppMutex = new Mutex(true, "LegacySakuraLauncher_" + Utils.Md5(Utils.ExecutablePath), out bool created);

            if (created)
            {
                Application.Run(new MainForm(minimize));
            }
            else
            {
                MessageBox.Show(TopMostForm, "请不要重复开启 SakuraFrp 客户端. 如果想运行多个实例请将软件复制到其他目录.", "Oops", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Environment.Exit(0);
            }

            AppMutex.ReleaseMutex();
        }
    }
}
