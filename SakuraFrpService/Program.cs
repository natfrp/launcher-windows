using System;
using System.Windows.Forms;
using System.ServiceProcess;

namespace SakuraFrpService
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        static int Main(string[] argv)
        {
            if (argv.Length != 0 && argv[0] == "--daemon")
            {
                var args = new string[argv.Length - 1];
                Array.Copy(argv, 1, args, 0, args.Length);

                var main = new MainService(true);
                main.DaemonRun(args);
                return main.ExitCode;
            }
            if (Environment.UserInteractive)
            {
                MessageBox.Show("You can't start the service directly.\nTo run as daemon, pass --daemon as the first parameter.\nKeep in mind that action above should be done by SakuraFrpLauncher automatically, not by user.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return -1;
            }
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new MainService(false)
            };
            ServiceBase.Run(ServicesToRun);
            return 0;
        }
    }
}
