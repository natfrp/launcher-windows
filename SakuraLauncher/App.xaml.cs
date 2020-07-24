using System;
using System.IO;
using System.Net;
using System.Text;
using System.Windows;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Security.Principal;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Runtime.InteropServices;

using fastJSON;
using SakuraLauncher.Data;

namespace SakuraLauncher
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        [DllImport("user32.dll")]
        public static extern bool ReleaseCapture();

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        public static readonly string ExecutablePath = Process.GetCurrentProcess().MainModule.FileName;
        public static readonly bool IsAdministrator = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);

        public static string AutoRunFile { get; private set; }
        public static string DefaultUserAgent = "SakuraLauncher/" + Assembly.GetExecutingAssembly().GetName().Version;

        public static App Instance = null;

        #region Assistant Methods

        public static MessageBoxResult ShowMessage(string text, string title, MessageBoxImage icon, MessageBoxButton buttons = MessageBoxButton.OK)
        {
            return Instance.Dispatcher.Invoke(() => MessageBox.Show(text, title, buttons, icon));
        }

        public static bool SetAutoRun(bool start)
        {
            try
            {
                if(start)
                {
                    if(File.Exists(AutoRunFile))
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
                else if(File.Exists(AutoRunFile))
                {
                    File.Delete(AutoRunFile);
                }
                return true;
            }
            catch(Exception e)
            {
                ShowMessage("无法设置开机启动, 请检查杀毒软件是否拦截了此操作.\n\n" + e.ToString(), "Oops", MessageBoxImage.Error);
            }
            return false;
        }

        public static async Task<dynamic> ApiRequest(string action, string query = null)
        {
            try
            {
                var json = JSON.ToObject<Dictionary<string, dynamic>>(await HttpGetString("https://api.natfrp.com/launcher/" + action + "?token=" + (Instance.MainWindow as MainWindow).UserToken.Value.Trim() + (query == null ? "" : "&" + query)));
                if (json["success"])
                {
                    return json;
                }
                ShowMessage(json["message"] ?? "出现未知错误", "Oops", MessageBoxImage.Error);
            }
            catch(Exception e)
            {
                ShowMessage("无法完成请求, 请检查网络连接并重试\n\n" + e.ToString(), "Oops", MessageBoxImage.Error);
            }
            return null;
        }

        public static async Task<string> HttpGetString(string url, Encoding encoding = null, int timeoutMs = 5000, bool redirect = false, IWebProxy proxy = null)
        {
            if(encoding == null)
            {
                encoding = Encoding.UTF8;
            }
            return encoding.GetString(await HttpGetBytes(url, timeoutMs, redirect, proxy));
        }

        public static async Task<byte[]> HttpGetBytes(string url, int timeoutMs = -1, bool redirect = false, IWebProxy proxy = null)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            if(url.StartsWith("//"))
            {
                url = "https:" + url;
            }
            var request = WebRequest.CreateHttp(url);
            request.Method = "GET";
            request.UserAgent = DefaultUserAgent;
            request.Credentials = CredentialCache.DefaultCredentials;
            request.AllowAutoRedirect = redirect;
            if(proxy != null)
            {
                request.Proxy = proxy;
            }
            if(timeoutMs > 0)
            {
                request.Timeout = timeoutMs;
            }
            using(var response = await request.GetResponseAsync() as HttpWebResponse)
            {
                if(response.StatusCode != HttpStatusCode.OK)
                {
                    throw new Exception("Bad HTTP Status(" + url + "):" + response.StatusCode + " " + response.StatusDescription);
                }
                using(var ms = new MemoryStream())
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
                foreach(byte Temp in new MD5CryptoServiceProvider().ComputeHash(data))
                {
                    if(Temp < 16)
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

        public Mutex AppMutex = null;

        public App() : base()
        {
            Instance = this;
        }

        private void Application_Exit(object sender, ExitEventArgs e) => AppMutex?.ReleaseMutex();

        private void Application_Startup(object sender, StartupEventArgs e)
        {
            if(!File.Exists(Tunnel.ClientPath))
            {
                ShowMessage("未找到 frpc.exe, 请尝试重新下载客户端.", "Oops", MessageBoxImage.Error);
                Environment.Exit(0);
            }
            var minimize = false;
            foreach(var a in e.Args)
            {
                var split = a.Split('=');
                if(split[0] == "--minimize")
                {
                    minimize = true;
                }
            }
            AppMutex = new Mutex(true, "SakuraLauncher_" + Md5(Path.GetFullPath("config.json")), out bool created);
            AutoRunFile = Environment.GetFolderPath(Environment.SpecialFolder.Startup) + @"\SakuraLauncher_" + Md5(ExecutablePath) + ".lnk";
            if(created)
            {
                MainWindow = new MainWindow(File.Exists(AutoRunFile));
                if(!minimize)
                {
                    MainWindow.Show();
                }
            }
            else
            {
                ShowMessage("请不要重复开启 SakuraFrp 客户端. 如果想运行多个实例请将软件复制到其他目录.", "Oops", MessageBoxImage.Warning);
                Environment.Exit(0);
            }
        }

        private void TrayMenu_Show(object sender, RoutedEventArgs e) => MainWindow.Show();

        private void TrayMenu_Exit(object sender, RoutedEventArgs e) => MainWindow.Close();
    }
}
