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

using SakuraLauncher.Helper;

namespace SakuraLauncher.Model
{
    public class LauncherViewModel : LauncherModel
    {
        public readonly Func<string, bool> SimpleConfirmHandler = message => App.ShowMessage(message, "操作确认", MessageBoxImage.Asterisk, MessageBoxButton.OKCancel) == MessageBoxResult.OK;
        public readonly Func<string, bool> SimpleWarningHandler = message => App.ShowMessage(message, "警告", MessageBoxImage.Warning, MessageBoxButton.YesNo) == MessageBoxResult.Yes;
        public readonly Action<bool, string> SimpleHandler = (success, message) => App.ShowMessage(message, success ? "操作成功" : "操作失败", success ? MessageBoxImage.Information : MessageBoxImage.Error);
        public readonly Action<bool, string> SimpleFailureHandler = (success, message) =>
        {
            if (!success)
            {
                App.ShowMessage(message, "操作失败", MessageBoxImage.Error);
            }
        };

        public readonly MainWindow View;

        public LauncherViewModel(MainWindow view)
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

            LogsViewSource.Filter += e =>
            {
                var item = e as LogModel;
                return LogSourceFilter == "" || item.Source == LogSourceFilter;
            };
        }

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

            settings.Save();
        }

        public override bool SyncAll()
        {
            if (base.SyncAll())
            {
                SwitchTab(LoggedIn ? 0 : 2);
                return true;
            }
            return false;
        }

        #region Generic Properties

        public bool AutoRun
        {
            get => File.Exists(Utils.GetAutoRunFile(Consts.SakuraLauncherPrefix));
            set
            {
                var result = Utils.SetAutoRun(!AutoRun, Consts.SakuraLauncherPrefix);
                if (result != null)
                {
                    App.ShowMessage(result, "设置失败", MessageBoxImage.Error);
                }
                RaisePropertyChanged();
            }
        }

        [SourceBinding(nameof(Connected), nameof(HaveUpdate))]
        public bool ShowNotification => HaveUpdate || !Connected;

        [SourceBinding(nameof(Update), nameof(Connected))]
        public bool UpdateEnabled => Connected && Update != null && Update.UpdateManagerRunning;

        public bool CheckingUpdate { get => _checkingUpdate; set => SafeSet(out _checkingUpdate, value); }
        private bool _checkingUpdate;

        [SourceBinding(nameof(UserInfo))]
        public string UserName => UserInfo.Name;

        [SourceBinding(nameof(UserInfo))]
        public string UserMeta => UserInfo.Meta;

        public int Theme { get => _theme; set => Set(out _theme, value); }
        private int _theme;

        #endregion

        #region Logging

        public string LogSourceFilter { get => _logSourceFilter; set => Set(out _logSourceFilter, value); }
        private string _logSourceFilter = "";

        public ObservableCollection<string> LogSourceList { get; set; } = new ObservableCollection<string>()
        {
            { "" }
        };

        public ICollectionView LogsViewSource => CollectionViewSource.GetDefaultView(Logs);
        public ObservableCollection<LogModel> Logs { get; } = new ObservableCollection<LogModel>();

        public override void Log(Log l, bool init)
        {
            var entry = new LogModel()
            {
                Source = l.Source,
                Data = l.Data
            };
            if (l.Category == 0) // CATEGORY_FRPC
            {
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
            }
            else
            {
                entry.Time = Utils.ParseSakuraTime(l.Time).ToString("yyyy/MM/dd HH:mm:ss");
                switch (l.Category)
                {
                case 1:
                default:
                    entry.Level = "I";
                    break;
                case 2:
                    entry.Level = "W";
                    entry.LevelColor = LogModel.BrushWarning;
                    break;
                case 3:
                    entry.Level = "E";
                    entry.LevelColor = LogModel.BrushError;
                    break;
                case 4: // Notice INFO
                case 5: // Notice WARNING
                case 6: // Notice ERROR
                    switch (NotificationMode)
                    {
                    case 1:
                        return;
                    case 2:
                        if (l.Category == 4)
                        {
                            return;
                        }
                        break;
                    }
                    Dispatcher.Invoke(() => View.trayIcon.ShowBalloonTip(entry.Source, entry.Data, (Hardcodet.Wpf.TaskbarNotification.BalloonIcon)l.Category - 3));
                    return;
                }
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
