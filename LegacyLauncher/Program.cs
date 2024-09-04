using System;
using System.IO;
using System.Threading;
using System.Windows.Forms;

using SakuraLibrary;

namespace LegacyLauncher
{
    static class Program
    {
        public static Mutex AppMutex = null;
        public static Form TopMostForm => new() { TopMost = true };

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Environment.CurrentDirectory = Path.GetDirectoryName(Utils.ExecutablePath);

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Utils.VerifySignature(Utils.LibraryPath, Utils.ExecutablePath, Path.GetFullPath(Consts.ServiceExecutable));
            Utils.ValidateSettings();

            var minimize = false;
            foreach (var a in args)
            {
                var split = a.Split('=');
                if (split[0] == "--minimize")
                {
                    minimize = true;
                }
            }

            AppMutex = new Mutex(true, "SakuraFrpLauncher3_Legacy", out bool created);
            if (created)
            {
                Application.Run(new MainForm(minimize));
            }
            else
            {
                MessageBox.Show(TopMostForm, "请不要重复开启 SakuraFrp 客户端", "Oops", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                Environment.Exit(0);
            }

            AppMutex.ReleaseMutex();
        }
    }
}
