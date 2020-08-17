using System;
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

                var main = new MainService();
                main.DaemonRun(args);
                return main.ExitCode;
            }
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[]
            {
                new MainService()
            };
            ServiceBase.Run(ServicesToRun);
            return 0;
        }
    }
}
