using Microsoft.Web.WebView2.Core;
using SakuraLauncher.Helper;
using SakuraLauncher.Model;
using SakuraLibrary.Model;
using SakuraLibrary.Proto;
using System;
using System.Diagnostics;
using System.Windows;

namespace SakuraLauncher
{
    /// <summary>
    /// CreateTunnelWindow.xaml 的交互逻辑
    /// </summary>
    public partial class CreateTunnelWindow2 : Window
    {
        public readonly LauncherViewModel Launcher;
        public readonly CreateTunnelModel Model;
        public readonly CreateTunnelBridge Bridge;

        public CreateTunnelWindow2(LauncherViewModel launcher, Tunnel edit = null)
        {
            InitializeComponent();
            Title = edit == null ? "创建隧道" : $"编辑隧道 #{edit.Id} {edit.Name}";

            Launcher = launcher;
            DataContext = Model = new(launcher);
            Bridge = new(this, edit);

            _ = Launcher.RequestReloadNodesAsync(false);

            Model.ReloadListening();

            webView.EnsureCoreWebView2Async().ContinueWith((r) =>
            {
                if (r.IsFaulted)
                {
                    if (MessageBox.Show("加载 WebView2 失败，请检查是否已安装 WebView2 运行时。\n按 \"确定\" 打开下载 WebView2 安装程序的下载页面。\n\n" + r.Exception.ToString(), "错误", MessageBoxButton.OKCancel, MessageBoxImage.Error) == MessageBoxResult.OK)
                    {
                        Process.Start("https://go.microsoft.com/fwlink/p/?LinkId=2124703");
                    }
                    Close();
                }
            });
        }

        private void Window_Closed(object sender, EventArgs e) => webView.Dispose();

        private void WebView_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            var core = webView.CoreWebView2;
#if !DEBUG
            core.Settings.AreDevToolsEnabled = false;
#endif
            core.Settings.IsGeneralAutofillEnabled = core.Settings.IsPasswordAutosaveEnabled = core.Settings.IsReputationCheckingRequired = false;

            webView.CoreWebView2.NewWindowRequested += CoreWebView2_NewWindowRequested;
            core.NavigationStarting += CoreWebView2_NavigationStarting;

            core.AddHostObjectToScript("SakuraLauncher", Launcher.Bridge);
            core.AddHostObjectToScript("CreateTunnel", Bridge);
#if DEBUG
            core.Navigate("http://localhost:3000/_launcher/create-tunnel");
#else
            core.Navigate("https://www.natfrp.com/_launcher/create-tunnel");
#endif
        }

        private void CoreWebView2_NewWindowRequested(object sender, CoreWebView2NewWindowRequestedEventArgs e)
        {
            var uri = new Uri(e.Uri);
            if (!uri.Host.EndsWith(".natfrp.com"))
            {
                e.Handled = true;
            }
        }

        private void CoreWebView2_NavigationStarting(object sender, CoreWebView2NavigationStartingEventArgs e)
        {
            var uri = new Uri(e.Uri);
            if ((!uri.Host.EndsWith(".natfrp.com") && uri.Host != "localhost") || !uri.PathAndQuery.StartsWith("/_launcher/"))
            {
                e.Cancel = true;
            }
        }
    }
}
