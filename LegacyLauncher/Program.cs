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

            AppMutex = new Mutex(true, "SakuraFrpLauncher3_Legacy", out bool created);
            var ActivateEventHandle = new EventWaitHandle(false, EventResetMode.AutoReset, "SakuraFrpLauncher3_Legacy_ActivateEvent");
            if (!created)
            {
                if (!ActivateEventHandle.Set())
                {
                    MessageBox.Show(new Form() { TopMost = true }, "请不要重复开启 SakuraFrp 客户端", "Oops", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                Environment.Exit(0);
            }

            var deprecatedAutoRunFile = Utils.GetAutoRunFile(Consts.LegacyLauncherPrefix);
            try
            {
                if (File.Exists(deprecatedAutoRunFile))
                {
                    File.Delete(deprecatedAutoRunFile);
                }
            }
            catch { }

            var mainWindow = new MainForm();
            new Thread(() =>
            {
                while (ActivateEventHandle.WaitOne())
                {
                    mainWindow.Invoke(() =>
                    {
                        mainWindow.Show();
                        mainWindow.WindowState = FormWindowState.Normal;
                        mainWindow.Activate();
                    });
                }
            })
            {
                IsBackground = true,
            }.Start();
            Application.Run(mainWindow);

            AppMutex.ReleaseMutex();
        }
    }
}
