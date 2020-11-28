using System;
using System.IO;
using System.Windows;
using System.Threading;
using System.Collections.Generic;
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

            LogTextWrapping = settings.LogTextWrapping;
            SuppressNotification = settings.SuppressNotification;
        }

        public override void ClearLog() => Dispatcher.Invoke(() => Logs.Clear());

        public override void Log(Log l, bool init)
        {
            var entry = new LogModel()
            {
                Source = l.Source,
                Data = l.Data
            };
            if (l.Category == 0) // CATEGORY_FRPC
            {
                var match = LogModel.Pattern.Match(l.Data);
                if (match.Success)
                {
                    entry.Time = match.Groups["Time"].Value;
                    entry.Data = match.Groups["Content"].Value;
                    entry.Level = match.Groups["Level"].Value + ":" + match.Groups["Source"].Value;
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
                    entry.Level = "INFO";
                    break;
                case 2:
                    entry.Level = "WARNING";
                    entry.LevelColor = LogModel.BrushWarning;
                    break;
                case 3:
                    entry.Level = "ERROR";
                    entry.LevelColor = LogModel.BrushError;
                    break;
                case 4: // Notice INFO
                case 5: // Notice WARNING
                case 6: // Notice ERROR
                    if (!SuppressNotification)
                    {
                        Dispatcher.Invoke(() => View.trayIcon.ShowBalloonTip(entry.Source, entry.Data, (Hardcodet.Wpf.TaskbarNotification.BalloonIcon)l.Category - 3));
                    }
                    return;
                }
            }
            Dispatcher.Invoke(() =>
            {
                Logs.Add(entry);
                while (Logs.Count > 4096) Logs.RemoveAt(0);
            });
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

            settings.LogTextWrapping = LogTextWrapping;
            settings.SuppressNotification = SuppressNotification;

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

        public ObservableCollection<LogModel> Logs { get; set; } = new ObservableCollection<LogModel>();

        [SourceBinding(nameof(Connected), nameof(HaveUpdate))]
        public bool ShowNotification => HaveUpdate || !Connected;

        public int CurrentTab { get => _currentTab; set => Set(out _currentTab, value); }
        private int _currentTab = -1;

        [SourceBinding(nameof(CurrentTab))]
        public TabIndexTester CurrentTabTester { get; set; }

        public void SwitchTab(int id)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => SwitchTab(id));
                return;
            }
            if (CurrentTab != id)
            {
                CurrentTab = id;
                View.BeginTabStoryboard("TabHideAnimation");
            }
        }
    }
}
