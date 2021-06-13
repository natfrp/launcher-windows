using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using System.Text;

namespace SakuraLibrary
{
    public static class UtilsWindows
    {
        public static readonly string LibraryPath = Assembly.GetExecutingAssembly().Location;
        public static readonly string ExecutablePath = Process.GetCurrentProcess().MainModule.FileName;

        public static readonly bool IsAdministrator = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

        public static readonly string InstallationHash = Utils.Md5(Assembly.GetExecutingAssembly().Location);
        public static readonly string InstallationPipeName = InstallationHash + "_" + Consts.PipeName;

        public static string GetAutoRunFile(string prefix) => Environment.GetFolderPath(Environment.SpecialFolder.Startup) + "\\" + prefix + Utils.Md5(ExecutablePath) + ".lnk";

        public static string SetAutoRun(bool start, string prefix)
        {
            try
            {
                var file = GetAutoRunFile(prefix);
                if (start)
                {
                    if (File.Exists(file))
                    {
                        return null;
                    }
                    var shortcut = (IWshRuntimeLibrary.IWshShortcut)new IWshRuntimeLibrary.WshShell().CreateShortcut(file);
                    shortcut.TargetPath = ExecutablePath;
                    shortcut.Arguments = "--minimize";
                    shortcut.Description = "SakuraFrp Launcher 开机自启";
                    shortcut.Save();
                }
                else if (File.Exists(file))
                {
                    File.Delete(file);
                }
                return null;
            }
            catch (Exception e)
            {
                return "无法设置开机启动, 请检查杀毒软件是否拦截了此操作\n\n" + e.ToString();
            }
        }

        public static Process[] SearchProcess(string name, string testPath = null) => Process.GetProcessesByName(name).Where(p =>
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

        public static void VerifySignature(params string[] files)
        {
            // 如果您准备自己编译启动器或使用其他版本的 frpc
            // 请自行修改此部分代码或使用 Debug 构建来禁用签名验证

#if !DEBUG // && false
            try
            {
                var failure = files.Where(f => !File.Exists(f)).ToList();
                if (failure.Count != 0)
                {
                    NTAPI.MessageBox(0, "@@@@@@@@@@@@@@@@@@\n" +
                        "         !!!  错误: 启动器文件损坏  !!!\n" +
                        "@@@@@@@@@@@@@@@@@@\n\n" +
                        "下列文件不存在:\n" + string.Join("\n", failure) + "\n\n" +
                        "请重新安装启动器\n如果重装后还看到此提示，请检查杀毒软件是否删除了启动器文件", "错误", 0x10);
                    Environment.Exit(1);
                }

                var key = new PublicKey(new Oid("1.2.840.113549.1.1.1"), new AsnEncodedData(new byte[] { 05, 00 }), new AsnEncodedData(Properties.Resources.sakura_sign));
                using (var rsa = (RSACryptoServiceProvider)key.Key)
                {
                    failure = files.Where(f => !File.Exists(f + ".sig") || !rsa.VerifyData(File.ReadAllBytes(f), "SHA256", File.ReadAllBytes(f + ".sig"))).ToList();
                    if (failure.Count == 0)
                    {
                        return;
                    }
                    NTAPI.MessageBox(0, "@@@@@@@@@@@@@@@@@@\n" +
                        "         !!!  警告: 文件签名验证失败  !!!\n" +
                        "@@@@@@@@@@@@@@@@@@\n\n" +
                        "下列文件未通过数字签名校验:\n" + string.Join("\n", failure) + "\n\n" +
                        "这些文件可能已损坏或被纂改, 这意味着您的电脑可能已经被病毒感染\n\n" +
                        "请立即进行全盘杀毒并重新安装启动器", "错误", 0x10);
                }
            }
            catch (Exception e)
            {
                NTAPI.MessageBox(0, "@@@@@@@@@@@@@@@@@@\n" +
                    "         !!!  警告: 文件签名验证失败  !!!\n" +
                    "@@@@@@@@@@@@@@@@@@\n" +
                    "出现内部错误, 请截图此报错并联系管理员\n\n" + e, "错误", 0x10);
            }
            Environment.Exit(1);
#endif
        }

        public static bool ValidateSettings()
        {
            try
            {
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.PerUserRoamingAndLocal);
                config.SaveAs(config.FilePath + ".bak", ConfigurationSaveMode.Full, true);
                return true;
            }
            catch (ConfigurationErrorsException e)
            {
                if (!File.Exists(e.Filename))
                {
                    return true;
                }
                File.Delete(e.Filename);

                var backup = e.Filename + ".bak";
                if (File.Exists(backup))
                {
                    File.Move(backup, e.Filename);
                }
            }
            return false;
        }
    }
}
