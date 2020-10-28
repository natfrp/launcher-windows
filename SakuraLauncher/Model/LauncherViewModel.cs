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
        public Dictionary<string, string> failedData = new Dictionary<string, string>();

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

            var settings = Properties.Settings.Default;
            if (settings.UpgradeRequired)
            {
                settings.Upgrade();
                settings.UpgradeRequired = false;
                settings.Save();
            }

            View.Width = settings.Width;
            View.Height = settings.Height;

            SuppressInfo = settings.SuppressInfo;
            LogTextWrapping = settings.LogTextWrapping;
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
                    if (failedData.ContainsKey(l.Source))
                    {
                        if (View.IsVisible && !SuppressInfo)
                        {
                            string failedData_ = failedData[l.Source];
                            ThreadPool.QueueUserWorkItem(s => App.ShowMessage(failedData_, "隧道日志", MessageBoxImage.Information));
                        }
                        failedData.Remove(l.Source);
                    }
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
                else if (!init)
                {
                    if (!failedData.ContainsKey(l.Source))
                    {
                        failedData[l.Source] = "";
                    }
                    failedData[l.Source] += l.Data + "\n";
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

            settings.SuppressInfo = SuppressInfo;
            settings.LogTextWrapping = LogTextWrapping;

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
