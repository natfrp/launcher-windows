using System;
using System.IO;
using System.Windows.Forms;
using System.ComponentModel;
using System.ServiceProcess;
using System.Configuration.Install;

using SakuraLibrary;

namespace SakuraFrpService
{
    static class Program
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        static int Main(string[] argv)
        {
            if (Path.GetFileName(Utils.ExecutablePath) != Consts.ServiceExecutable)
            {
                if (Environment.UserInteractive)
                {
                    MessageBox.Show("Do NOT rename SakuraFrpService.exe.\n请不要重命名 SakuraFrpService.exe", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return 1;
            }
            if (argv.Length != 0)
            {
                switch (argv[0])
                {
                case "--install":
                    try
                    {
                        ManagedInstallerClass.InstallHelper(new string[] { Utils.ExecutablePath });
                        return 0;
                    }
                    catch (Exception e) when (e.InnerException is Win32Exception w32 && w32.NativeErrorCode == 1073) // ERROR_SERVICE_EXISTS
                    {
                        MessageBox.Show("服务已存在", "操作失败", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.ToString(), "操作失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    return 2;
                case "--uninstall":
                    try
                    {
                        ManagedInstallerClass.InstallHelper(new string[] { "/u", Utils.ExecutablePath });
                        return 0;
                    }
                    catch (Exception e) when (e.InnerException is Win32Exception w32 && w32.NativeErrorCode == 1060) // ERROR_SERVICE_DOES_NOT_EXIST
                    {
                        MessageBox.Show("服务未安装", "操作失败", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception e)
                    {
                        MessageBox.Show(e.ToString(), "操作失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    return 2;
                case "--daemon":
                    var args = new string[argv.Length - 1];
                    Array.Copy(argv, 1, args, 0, args.Length);

                    var main = new MainService(true);
                    main.DaemonRun(args);
                    return main.ExitCode;
                }
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
