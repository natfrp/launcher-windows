using System;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.ComponentModel;
using System.Collections.ObjectModel;

using SakuraLibrary;
using SakuraLibrary.Model;
using SakuraLibrary.Proto;
using SakuraLibrary.Helper;
using MaterialDesignThemes.Wpf;

using SakuraLauncher.Helper;
using static SakuraLibrary.Proto.Log.Types;

namespace SakuraLauncher.Model
{
    public class LauncherViewModel : LauncherModel
    {
        public readonly MainWindow View;

        public LauncherViewModel(MainWindow view) : base(false)
        {
            View = view;
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
            AlignWidth = settings.AlignWidth;

            LogsView.Filter += e =>
            {
                var item = e as LogModel;
                return LogSourceFilter == "" || item.Source == LogSourceFilter;
            };
            TunnelsView.SortDescriptions.Add(new SortDescription(nameof(TunnelModel.Name), ListSortDirection.Ascending));

            PropertyChanged += (sender, e) =>
            {
                if (e.PropertyName == nameof(LoggedIn))
                {
                    SwitchTab(LoggedIn ? 0 : 2);
                }
            };

            Run();
        }

        #region ViewModel Abstraction

        public override bool ShowMessage(string message, string title, MessageMode mode) => View.Dispatcher.Invoke(() => MessageBox.Show(
            View, message, title,
            mode == MessageMode.Confirm ? MessageBoxButton.OKCancel : MessageBoxButton.OK,
            mode switch
            {
                MessageMode.Confirm => MessageBoxImage.Question,
                MessageMode.Info => MessageBoxImage.Information,
                MessageMode.Warning => MessageBoxImage.Warning,
                MessageMode.Error => MessageBoxImage.Error,
                _ => MessageBoxImage.None
            }
        )) == MessageBoxResult.OK;

        public override void Save()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => Save());
                return;
            }

            var settings = Properties.Settings.Default;

            settings.Width = (int)View.Width;
            settings.Height = (int)View.Height;

            settings.Theme = Theme;
            settings.LogTextWrapping = LogTextWrapping;
            settings.NotificationMode = NotificationMode;
            settings.AlignWidth = AlignWidth;

            settings.Save();
        }

        #endregion

        #region Generic Properties

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

        public bool CheckingUpdate { get => _checkingUpdate; set => SafeSet(out _checkingUpdate, value); }
        private bool _checkingUpdate;

        [SourceBinding(nameof(UserInfo))]
        public string UserName => UserInfo.Name;

        [SourceBinding(nameof(UserInfo))]
        public string UserMeta => "";

        public int Theme { get => _theme; set => Set(out _theme, value); }
        private int _theme;

        public bool AlignWidth { get => _alignWidth; set => Set(out _alignWidth, value); }
        private bool _alignWidth;

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
                Dispatcher.Invoke(() => View.trayIcon.ShowBalloonTip(entry.Source, entry.Data, (Hardcodet.Wpf.TaskbarNotification.BalloonIcon)Math.Max(Math.Min((int)l.Level, 2), 1)));
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
            Dispatcher.Invoke(() =>
            {
                if (!LogSourceList.Contains(entry.Source))
                {
                    LogSourceList.Add(entry.Source);
                }
                Logs.Add(entry);
                while (Logs.Count > 4096) Logs.RemoveAt(0);
            });
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
