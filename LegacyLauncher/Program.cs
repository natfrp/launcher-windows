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

namespace LegacyLauncher
{
    static class Program
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern int SetProcessShutdownParameters(int dwLevel, int dwFlags);

        [DllImport("kernel32.dll")]
        public static extern bool QueryFullProcessImageName([In] IntPtr hProcess, [In] uint dwFlags, [Out] StringBuilder lpExeName, [In, Out] ref uint lpdwSize);

        public static readonly string ExecutablePath = Process.GetCurrentProcess().MainModule.FileName;
        public static readonly bool IsAdministrator = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

        public static string AutoRunFile = Environment.GetFolderPath(Environment.SpecialFolder.Startup) + @"\LegacySakuraLauncher_" + Md5(ExecutablePath) + ".lnk";
        public static string DefaultUserAgent = "SakuraLauncher/" + Assembly.GetExecutingAssembly().GetName().Version;

        public static Mutex AppMutex = null;
        public static Form TopMostForm => new Form() { TopMost = true };

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

        public static Dictionary<string, dynamic> ApiRequest(string action, string query = null)
        {
            try
            {
                var json = JSON.ToObject<Dictionary<string, object>>(HttpGetString("https://api.natfrp.com/launcher/" + action + "?token=" + MainForm.Instance.UserToken.Trim() + (query == null ? "" : "&" + query)));
                if ((bool)json["success"])
                {
                    return json;
                }
                MessageBox.Show(TopMostForm, json["message"] as string ?? "出现未知错误", "Oops", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (NotSupportedException)
            {
                if (MessageBox.Show(TopMostForm, "您的系统不支持 TLS 1.2, 无法完成请求\n请先安装 .NET Framework 4.5 及以上版本\n\n是否自动打开下载页面?", "Oops", MessageBoxButtons.YesNo, MessageBoxIcon.Error) == DialogResult.Yes)
                {
                    try
                    {
                        Process.Start("https://dotnet.microsoft.com/download/dotnet-framework/net45");
                    }
                    catch { }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(TopMostForm, "无法完成请求, 请检查网络连接并重试\n\n" + e.ToString(), "Oops", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            return null;
        }

        public static string HttpGetString(string url, Encoding encoding = null, int timeoutMs = 5000, IWebProxy proxy = null)
        {
            if (encoding == null)
            {
                encoding = Encoding.UTF8;
            }
            return encoding.GetString(HttpGetBytes(url, timeoutMs, proxy));
        }

        public static byte[] HttpGetBytes(string url, int timeoutMs = -1, IWebProxy proxy = null)
        {
            ServicePointManager.SecurityProtocol = (SecurityProtocolType)(3072 | 768); // Tls12 & Tls11
            if (url.StartsWith("//"))
            {
                url = "https:" + url;
            }
            var request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            request.UserAgent = DefaultUserAgent;
            request.Credentials = CredentialCache.DefaultCredentials;
            if (proxy != null)
            {
                request.Proxy = proxy;
            }
            if (timeoutMs > 0)
            {
                request.Timeout = timeoutMs;
            }
            using (var response = request.GetResponse() as HttpWebResponse)
            {
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception("Bad HTTP Status(" + url + "):" + response.StatusCode + " " + response.StatusDescription);
                }
                using (var ms = new MemoryStream())
                {
                    int count;
                    byte[] buffer = new byte[4096];
                    var stream = response.GetResponseStream();
                    while ((count = stream.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        ms.Write(buffer, 0, count);
                    }
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
                MessageBox.Show(TopMostForm, "未找到 frpc.exe, 请尝试重新下载客户端.", "Oops", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
