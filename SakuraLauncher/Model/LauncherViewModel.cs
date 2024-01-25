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
using System.IO;
using System.Net;
using System.Windows;
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

            LogsView.Filter += e =>
            {
                var item = e as LogModel;
                return LogSourceFilter == "" || item.Source == LogSourceFilter;
            };
            TunnelsView.SortDescriptions.Add(new SortDescription(nameof(TunnelModel.Name), ListSortDirection.Ascending));

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

        public override IntPtr GetHwnd() => View.Dispatcher.Invoke(() => new System.Windows.Interop.WindowInteropHelper(View).Handle);

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
                    View.trayIcon.ShowBalloonTip(entry.Source, entry.Data, (Hardcodet.Wpf.TaskbarNotification.BalloonIcon)Math.Max(Math.Min((int)l.Level, 2), 0));
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
