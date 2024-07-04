using H.NotifyIcon.Core;
using MaterialDesignThemes.Wpf;
using Microsoft.Web.WebView2.Core;
using SakuraLauncher.Helper;
using SakuraLibrary;
using SakuraLibrary.Helper;
using SakuraLibrary.Model;
using SakuraLibrary.Proto;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using static SakuraLibrary.Proto.Log.Types;

namespace SakuraLauncher.Model
{
    public class LauncherViewModel : LauncherModel
    {
        public readonly MainWindow View;
        public readonly SakuraLauncherBridge Bridge;

        public CoreWebView2Environment WebView2Environment;

        public LauncherViewModel(MainWindow view) : base(false)
        {
            View = view;
            Bridge = new SakuraLauncherBridge(this);
            Dispatcher = new DispatcherWrapper(View.Dispatcher.Invoke, a => View.Dispatcher.BeginInvoke(a), View.Dispatcher.CheckAccess);

            CurrentTabTester = new TabIndexTester(this);

            var settings = Properties.Settings.Default;
            if (settings.UpgradeRequired)
            {
                settings.Upgrade();
                settings.UpgradeRequired = false;
                settings.Save();
            }

            View.Width = settings.Width;
            View.Height = settings.Height;

            Theme = settings.Theme;
            LogTextWrapping = settings.LogTextWrapping;
            NotificationMode = settings.NotificationMode;
            AdvancedMode = settings.AdvancedMode;
            LegacyCreateTunnel = settings.LegacyCreateTunnel;
            AlignWidth = settings.AlignWidth;

            var sortDirection = settings.SortDesc ? ListSortDirection.Descending : ListSortDirection.Ascending;
            TunnelsView.SortDescriptions.Add(new SortDescription(settings.SortField, sortDirection));
            TunnelsView.SortDescriptions.Add(new SortDescription(nameof(TunnelModel.Name), sortDirection));
            TunnelsView.SortDescriptions.Add(new SortDescription(nameof(TunnelModel.Id), sortDirection));

            Tunnels.CollectionChanged += (sender, e) => TunnelsView.Refresh();

            LogsView.Filter += e =>
            {
                var item = e as LogModel;
                return LogSourceFilter == "" || item.Source == LogSourceFilter;
            };

            var prevAvatar = "";
            PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == "_Login_State")
                {
                    SwitchTab(LoggedIn ? 0 : 2);
                }
                else if (e.PropertyName == "UserInfo")
                {
                    if (string.IsNullOrEmpty(UserInfo.Avatar))
                    {
                        Avatar = null;
                        prevAvatar = "";
                        return;
                    }
                    if (UserInfo.Avatar == prevAvatar)
                    {
                        return;
                    }

                    var uri = new Uri(UserInfo.Avatar);
                    var cache = Path.Combine(Consts.WorkingDirectory, "Temp", "avatar.jpg");
                    try
                    {
                        using (var client = new WebClient())
                        {
                            client.DownloadFile(uri, cache);
                        }
                        Avatar = new BitmapImage(new Uri(cache));
                        prevAvatar = UserInfo.Avatar;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex);
                    }
                }
            };

            Run();
        }

        #region ViewModel Abstraction

        private Window windowOverride = null;

        public void ShowDialog(Window w, UserControl owner)
        {
            windowOverride = w;
            View.Dispatcher.Invoke(() =>
            {
                w.Owner = Window.GetWindow(owner);
                w.ShowDialog();
            });
            windowOverride = null;
        }

        public override IntPtr GetHwnd() => View.Dispatcher.Invoke(() => new System.Windows.Interop.WindowInteropHelper(windowOverride ?? View).Handle);

        public bool SetupWebView2Environment()
        {
            try
            {
                var fixedVersionDir = Path.Combine(Consts.WorkingDirectory, "WebView2");
                WebView2Environment = CoreWebView2Environment.CreateAsync(Directory.Exists(fixedVersionDir) ? fixedVersionDir : null, Path.Combine(Consts.WorkingDirectory, "Temp"), new()
                {
                    EnableTrackingPrevention = false,
                    AreBrowserExtensionsEnabled = false,
                    AllowSingleSignOnUsingOSPrimaryAccount = false,
                }).WaitResult();

                // On 103.0.1264.77 async call fails
                const string versionRequired = "104.0.1293.70";
                if (CoreWebView2Environment.CompareBrowserVersions(WebView2Environment.BrowserVersionString, versionRequired) < 0)
                {
                    if (!LegacyCreateTunnel && ShowMessage($"当前 WebView2 版本 {WebView2Environment.BrowserVersionString} 过旧，创建、编辑隧道功能将无法正常工作。\n请升级 WebView2 到 {versionRequired} 或更新版本。\n\n按 \"确定\" 打开 WebView2 安装程序下载页面。", "错误", MessageMode.OkCancel | MessageMode.Error) == MessageResult.Ok)
                    {
                        Process.Start("https://go.microsoft.com/fwlink/p/?LinkId=2124703");
                    }
                    WebView2Environment = null;
                }
            }
            catch (Exception ex)
            {
                WebView2Environment = null;

                if (!LegacyCreateTunnel)
                {
                    if (!(ex is AggregateException ae && ae.InnerExceptions[0] is WebView2RuntimeNotFoundException))
                    {
                        ShowError(ex, "无法初始化 WebView2 运行环境");
                    }

                    if (ShowMessage("无法初始化 WebView2 运行环境，将无法使用创建、编辑隧道功能。\n\n请检查是否已安装 WebView2 运行时。\n\n按 \"确定\" 打开 WebView2 安装程序下载页面，安装后请重启启动器。", "错误", MessageMode.OkCancel | MessageMode.Error) == MessageResult.Ok)
                    {
                        Process.Start("https://go.microsoft.com/fwlink/p/?LinkId=2124703");
                    }
                }
            }
            return WebView2Environment != null;
        }

        #endregion

        #region Generic Properties

        public void Save()
        {
            var settings = Properties.Settings.Default;

            settings.Width = (int)View.Width;
            settings.Height = (int)View.Height;

            settings.Theme = Theme;
            settings.LogTextWrapping = LogTextWrapping;
            settings.NotificationMode = NotificationMode;
            settings.AdvancedMode = AdvancedMode;
            settings.LegacyCreateTunnel = LegacyCreateTunnel;
            settings.AlignWidth = AlignWidth;

            var sd = TunnelsView.SortDescriptions[0];
            settings.SortField = sd.PropertyName;
            settings.SortDesc = sd.Direction == ListSortDirection.Descending;

            settings.Save();
        }

        public SnackbarMessageQueue SnackMessageQueue { get; } = new SnackbarMessageQueue();

        public bool AutoRun
        {
            get => File.Exists(Utils.GetAutoRunFile(Consts.SakuraLauncherPrefix));
            set
            {
                var result = Utils.SetAutoRun(!AutoRun, Consts.SakuraLauncherPrefix);
                if (result != null)
                {
                    ShowMessage(result, "设置失败", MessageMode.Error);
                }
                RaisePropertyChanged();
            }
        }

        [SourceBinding(nameof(Connected), nameof(HaveUpdate))]
        public bool ShowNotification => HaveUpdate || !Connected;

        public BitmapImage Avatar { get => _avatar; set => Set(out _avatar, value); }
        private BitmapImage _avatar;

        public bool CheckingUpdate { get => _checkingUpdate; set => SafeSet(out _checkingUpdate, value); }
        private bool _checkingUpdate;

        public int Theme { get => _theme; set => Set(out _theme, value); }
        private int _theme;

        public bool AlignWidth { get => _alignWidth; set => Set(out _alignWidth, value); }
        private bool _alignWidth;

        public bool AdvancedMode { get => _advancedMode; set => Set(out _advancedMode, value); }
        private bool _advancedMode;

        public bool LegacyCreateTunnel { get => _legacyCreateTunnel; set => Set(out _legacyCreateTunnel, value); }
        private bool _legacyCreateTunnel;

        public ICollectionView TunnelsView => CollectionViewSource.GetDefaultView(Tunnels);

        #endregion

        #region Logging

        public string LogSourceFilter { get => _logSourceFilter; set => Set(out _logSourceFilter, value); }
        private string _logSourceFilter = "";

        public ObservableCollection<string> LogSourceList { get; set; } = new ObservableCollection<string>()
        {
            { "" }
        };

        public ICollectionView LogsView => CollectionViewSource.GetDefaultView(Logs);
        public ObservableCollection<LogModel> Logs { get; } = new ObservableCollection<LogModel>();

        public override void Log(Log l, bool init)
        {
            var entry = new LogModel()
            {
                Source = l.Source,
                Data = l.Data
            };
            switch (l.Category)
            {
            case Category.Alert:
                if (NotificationMode == 0 || (NotificationMode == 2 && l.Level > Level.Info))
                {
                    var icon = NotificationIcon.Info;
                    switch (l.Level)
                    {
                    case Level.Warn:
                        icon = NotificationIcon.Warning;
                        break;
                    case Level.Fatal:
                    case Level.Error:
                        icon = NotificationIcon.Error;
                        break;
                    }
                    View.trayIcon.ShowNotification(entry.Source, entry.Data, icon);
                }
                return;
            case Category.Frpc:
                entry.Source = "Tunnel/" + entry.Source;
                var match = LogModel.Pattern.Match(l.Data);
                if (match.Success)
                {
                    entry.Time = match.Groups["Time"].Value;
                    entry.Data = match.Groups["Content"].Value;
                    entry.Level = match.Groups["Level"].Value;
                    switch (match.Groups["Level"].Value)
                    {
                    case "W":
                        entry.LevelColor = LogModel.BrushWarning;
                        break;
                    case "E":
                        entry.LevelColor = LogModel.BrushError;
                        break;
                    }
                }
                break;
            case Category.Service:
                entry.Time = Utils.ParseTimestamp(l.Time).ToString("yyyy/MM/dd HH:mm:ss");
                switch (l.Level)
                {
                case Level.Debug:
                    entry.Level = "D";
                    break;
                case Level.Info:
                default:
                    entry.Level = "I";
                    break;
                case Level.Warn:
                    entry.Level = "W";
                    entry.LevelColor = LogModel.BrushWarning;
                    break;
                case Level.Error:
                    entry.Level = "E";
                    entry.LevelColor = LogModel.BrushError;
                    break;
                case Level.Fatal:
                    entry.Level = "F";
                    entry.LevelColor = LogModel.BrushError;
                    break;
                }
                break;
            default:
                return;
            }
            if (!LogSourceList.Contains(entry.Source))
            {
                LogSourceList.Add(entry.Source);
            }
            Logs.Add(entry);
            while (Logs.Count > 4096) Logs.RemoveAt(0);
        }

        public override void ClearLog() => Dispatcher.Invoke(() =>
        {
            Logs.Clear();
            LogSourceList.Clear();
            LogSourceList.Add("");
            LogSourceFilter = "";
        });

        #endregion

        #region Tab Switching

        public int CurrentTab { get => _currentTab; set => Set(out _currentTab, value); }
        private int _currentTab = -1;

        [SourceBinding(nameof(CurrentTab))]
        public TabIndexTester CurrentTabTester { get; set; }

        public void SwitchTab(int id) => Dispatcher.BeginInvoke(() =>
        {
            if (CurrentTab != id)
            {
                CurrentTab = id;
                View.BeginTabStoryboard("TabHideAnimation");
            }
        });

        #endregion
    }
}
