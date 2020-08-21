using System;
using System.Windows;

using SakuraLibrary.Model;
using SakuraLibrary.Proto;
using SakuraLibrary.Helper;

using SakuraLauncher.View;
using SakuraLauncher.Helper;

namespace SakuraLauncher.Model
{
    public class LauncherViewModel : LauncherModel
    {
        public readonly Action<bool, string> SimpleFailureHandler = (success, message) =>
        {
            if (!success)
            {
                App.ShowMessage(message, "操作失败", MessageBoxImage.Error, MessageBoxButton.OK);
            }
        };

        public readonly LogTab LogView;
        public readonly MainWindow View;

        public LauncherViewModel(MainWindow view)
        {
            View = view;
            Dispatcher = new DispatcherWrapper(a => View.Dispatcher.Invoke(a), a => View.Dispatcher.BeginInvoke(a), () => View.Dispatcher.CheckAccess());

            LogView = new LogTab(this);
        }

        public override void Log(Log l, bool init) => LogView.Log(l, init);

        public override void Load()
        {
            var settings = Properties.Settings.Default;

            View.Width = settings.Width;
            View.Height = settings.Height;

            LogTextWrapping = settings.LogTextWrapping;
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

            settings.Save();
        }

        public override bool Refresh()
        {
            if (base.Refresh())
            {
                SwitchTab(LoggedIn ? 0 : 2);
                return true;
            }
            return false;
        }

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
