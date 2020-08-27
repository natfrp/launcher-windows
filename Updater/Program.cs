using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace SakuraUpdater
{
    static class Program
    {
        public static bool UpdateFrpc = false;
        public static LauncherType UpdateLauncher = LauncherType.None, LaunchLauncher = LauncherType.None;

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] argv)
        {
            Environment.CurrentDirectory = Path.GetDirectoryName(Process.GetCurrentProcess().MainModule.FileName);

            foreach (var arg in argv)
            {
                var split = arg.Split(new char[] { '=' }, 2);
                switch (split[0])
                {
                case "-wpf":
                    UpdateLauncher = LauncherType.WPF;
                    break;
                case "-legacy":
                    UpdateLauncher = LauncherType.Legacy;
                    break;
                case "-frpc":
                    UpdateFrpc = true;
                    break;
                case "-launch":
                    LaunchLauncher = split.Length > 1 && split[1] == "legacy" ? LauncherType.Legacy : LauncherType.WPF;
                    break;
                }
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            if (UpdateFrpc == false && UpdateLauncher == 0)
            {
                MessageBox.Show("SakuraFrp Updater v" + Assembly.GetExecutingAssembly().GetName().Version +
                    "\nUsage: SakuraUpdater <Args>" +
                    "\n-frpc\tUpdate frpc" +
                    "\n-wpf\tUpdate SakuraLauncher" +
                    "\n-legacy\tUpdate LegacyLauncher" +
                    "\n-launch=<wpf|legacy>\tLaunch launcher after update", "Usage", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Application.Run(new MainForm());
        }

        public static async Task<HttpWebResponse> HttpGet(string url, int timeoutMs = -1)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            var request = WebRequest.CreateHttp(url);
            request.Method = "GET";
            request.UserAgent = "SakuraUpdater/" + Assembly.GetExecutingAssembly().GetName().Version;
            request.AllowAutoRedirect = true;

            if (timeoutMs > 0)
            {
                request.Timeout = timeoutMs;
            }
            return await request.GetResponseAsync() as HttpWebResponse;
        }
    }
}
