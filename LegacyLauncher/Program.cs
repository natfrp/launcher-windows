using System;
using System.IO;
using System.Net;
using System.Text;
using System.Linq;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Forms;
using System.Security.Principal;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Runtime.InteropServices;

using fastJSON;

using LegacyLauncher.Data;
using System.Threading.Tasks;

namespace LegacyLauncher
{
    static class Program
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int SetProcessShutdownParameters(int dwLevel, int dwFlags);

        [DllImport("kernel32.dll")]
        public static extern bool QueryFullProcessImageName([In] IntPtr hProcess, [In] uint dwFlags, [Out] StringBuilder lpExeName, [In, Out] ref uint lpdwSize);

        /* This approach sometimes kills launcher, unstable.
        public enum ConsoleCtrlEvent
        {
            CTRL_C = 0,
            CTRL_BREAK = 1,
            CTRL_CLOSE = 2,
            CTRL_LOGOFF = 5,
            CTRL_SHUTDOWN = 6
        }

        [DllImport("kernel32.dll")]
        public static extern bool GenerateConsoleCtrlEvent(ConsoleCtrlEvent sigevent, int dwProcessGroupId);
        
        [DllImport("kernel32.dll")]
        public static extern bool AttachConsole(uint dwProcessId);

        [DllImport("kernel32.dll")]
        public static extern bool FreeConsole();
        
        public delegate bool ConsoleCtrlDelegate(uint CtrlType);

        [DllImport("kernel32.dll")]
        public static extern bool SetConsoleCtrlHandler(ConsoleCtrlDelegate HandlerRoutine, bool Add);
        */

        public static readonly string ExecutablePath = Process.GetCurrentProcess().MainModule.FileName;
        public static readonly bool IsAdministrator = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

        public static string AutoRunFile = Environment.GetFolderPath(Environment.SpecialFolder.Startup) + @"\LegacySakuraLauncher_" + Md5(ExecutablePath) + ".lnk";
        public static string DefaultUserAgent = "LegacyLauncher/" + Assembly.GetExecutingAssembly().GetName().Version;

        public static Version FrpcVersion = null;
        public static float FrpcVersionSakura = 0;

        public static Mutex AppMutex = null;
        public static Form TopMostForm => new Form() { TopMost = true };

        // We don't provide UI for this option
        public static bool BypassProxy = true;

        #region Assistant Methods

        public static bool SetAutoRun(bool start)
        {
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
                    shortcut.Description = "SakuraFrp Legacy Launcher Auto Start";
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
                MessageBox.Show(TopMostForm, "无法设置开机启动, 请检查杀毒软件是否拦截了此操作.\n\n" + e.ToString(), "Oops", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return false;
        }

        public static async Task<dynamic> ApiRequest(string action, string query = null)
        {
            try
            {
                var json = JSON.ToObject<Dictionary<string, dynamic>>(await HttpGetString("https://api.natfrp.com/launcher/" + action + "?token=" + MainForm.Instance.UserToken.Trim() + (query == null ? "" : "&" + query)));
                if (json["success"])
                {
                    return json;
                }
                MessageBox.Show(TopMostForm, json["message"] ?? "出现未知错误", "Oops", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception e)
            {
                MessageBox.Show(TopMostForm, "无法完成请求, 请检查网络连接并重试\n\n" + e.ToString(), "Oops", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return null;
        }

        public static async Task<string> HttpGetString(string url, Encoding encoding = null, int timeoutMs = 5000, bool redirect = false)
        {
            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }
            return encoding.GetString(await HttpGetBytes(url, timeoutMs, redirect));
        }

        public static async Task<byte[]> HttpGetBytes(string url, int timeoutMs = -1, bool redirect = false)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            if (url.StartsWith("//"))
            {
                url = "https:" + url;
            }
            var request = WebRequest.CreateHttp(url);
            request.Method = "GET";
            request.UserAgent = DefaultUserAgent;
            request.Credentials = CredentialCache.DefaultCredentials;
            request.AllowAutoRedirect = redirect;

            if (BypassProxy)
            {
                request.Proxy = null;
            }
            if (timeoutMs > 0)
            {
                request.Timeout = timeoutMs;
            }
            using (var response = await request.GetResponseAsync() as HttpWebResponse)
            {
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception("Bad HTTP Status(" + url + "):" + response.StatusCode + " " + response.StatusDescription);
                }
                using (var ms = new MemoryStream())
                {
                    response.GetResponseStream().CopyTo(ms);
                    return ms.ToArray();
                }
            }
        }

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

        public static string Md5(string Data) => Md5(EncodeByteArray(Data));

        public static byte[] EncodeByteArray(string data) => data == null ? null : Encoding.UTF8.GetBytes(data);

        #endregion

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

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

            var minimize = false;
            foreach (var a in args)
            {
                var split = a.Split('=');
                if (split[0] == "--minimize")
                {
                    minimize = true;
                }
            }

            AppMutex = new Mutex(true, "LegacySakuraLauncher_" + Md5(Path.GetFullPath("config.json")), out bool created);

            if (created)
            {
                var test = Path.GetFullPath(Tunnel.ClientPath);
                var processes = Process.GetProcessesByName("frpc").Where(p =>
                {
                    try
                    {
                        uint bufferSize = 256;
                        var sb = new StringBuilder((int)bufferSize - 1);
                        if (QueryFullProcessImageName(p.Handle, 0, sb, ref bufferSize))
                        {
                            return Path.GetFullPath(sb.ToString()) == test;
                        }
                    }
                    catch { }
                    return false;
                }).ToArray();

                if (processes.Length != 0)
                {
                    switch (MessageBox.Show("发现 " + processes.Length + " 个的残留的 frpc 进程, 是否尝试将其关闭?\n这些进程可能是启动器不正常退出造成的残留.\n如果您不知道如何选择, 请点 \"是\".\n\n是 = 关闭所有进程\n否 = 忽略并继续\n取消 = 退出程序", "注意", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Information))
                    {
                    case DialogResult.Yes:
                        foreach (var p in processes)
                        {
                            try
                            {
                                p.Kill();
                                p.WaitForExit(200);
                            }
                            catch { }
                        }
                        break;
                    case DialogResult.No:
                        break;
                    default:
                        Environment.Exit(0);
                        break;
                    }
                }

                if (SetProcessShutdownParameters(0x300, 0) == 0)
                {
                    MessageBox.Show(TopMostForm, "无法设置关机优先级, 这可能导致隧道开机自启无法正常工作, 请检查杀毒软件是否拦截了此操作\n错误代码: " + Marshal.GetLastWin32Error(), "Oops", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

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
