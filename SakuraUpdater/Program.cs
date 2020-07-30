using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Windows.Forms;
using System.Threading.Tasks;

namespace SakuraUpdater
{
    static class Program
    {
        public static bool UpdateFrpc = false;
        public static int UpdateLauncher = 0;

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] argv)
        {
            foreach (var arg in argv)
            {
                switch (arg)
                {
                case "-launcher":
                    UpdateLauncher = 1;
                    break;
                case "-legacy":
                    UpdateLauncher = 2;
                    break;
                case "-frpc":
                    UpdateFrpc = true;
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
                    "\n-legacy\tUpdate LegacyLauncher" +
                    "\n-launcher\tUpdate SakuraLauncher", "Usage", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
