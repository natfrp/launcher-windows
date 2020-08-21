using System.IO;
using System.Text;
using System.Linq;
using System.Diagnostics;
using System.Security.Principal;
using System.Security.Cryptography;

namespace SakuraLibrary
{
    public static class Utils
    {
        public static readonly string ExecutablePath = Process.GetCurrentProcess().MainModule.FileName;
        public static readonly bool IsAdministrator = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

        public static string Md5(byte[] data)
        {
            try
            {
                StringBuilder Result = new StringBuilder();
                foreach (byte Temp in new MD5CryptoServiceProvider().ComputeHash(data))
                {
                    if (Temp < 16)
                    {
                        Result.Append("0");
                        Result.Append(Temp.ToString("x"));
                    }
                    else
                    {
                        Result.Append(Temp.ToString("x"));
                    }
                }
                return Result.ToString();
            }
            catch
            {
                return "0000000000000000";
            }
        }

        public static string Md5(string Data) => Md5(Encoding.UTF8.GetBytes(Data));

        public static int CastInt(dynamic item)
        {
            switch (item)
            {
            case int i:
                return i;
            case long l:
                return (int)l;
            case string s:
                return int.Parse(s);
            default:
                return (int)item;
            }
        }

        public static bool SetAutoRun(bool start)
        {
            /* TODO
            try
            {
                if (start)
                {
                    if (File.Exists(AutoRunFile))
                    {
                        return true;
                    }
                    // Don't include IWshRuntimeLibrary here, IWshRuntimeLibrary.File will cause name conflict.
                    var shortcut = (IWshRuntimeLibrary.IWshShortcut)new IWshRuntimeLibrary.WshShell().CreateShortcut(AutoRunFile);
                    shortcut.TargetPath = ExecutablePath;
                    shortcut.Arguments = "--minimize";
                    shortcut.Description = "SakuraFrp Launcher Auto Start";
                    shortcut.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
                    shortcut.Save();
                }
                else if (File.Exists(AutoRunFile))
                {
                    File.Delete(AutoRunFile);
                }
                return true;
            }
            catch (Exception e)
            {
                ShowMessage("无法设置开机启动, 请检查杀毒软件是否拦截了此操作.\n\n" + e.ToString(), "Oops", MessageBoxImage.Error);
            }
            */
            return false;
        }

        public static Process[] SearchProcess(string name, string testPath = null)
        {
            return Process.GetProcessesByName(name).Where(p =>
            {
                try
                {
                    uint size = 256;
                    var sb = new StringBuilder((int)size - 1);
                    if (NTAPI.QueryFullProcessImageName(p.Handle, 0, sb, ref size))
                    {
                        return testPath == null || Path.GetFullPath(sb.ToString()) == testPath;
                    }
                }
                catch { }
                return false;
            }).ToArray();
        }
    }
}
