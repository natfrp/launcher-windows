using System;
using System.Windows.Forms;

using SakuraLibrary.Model;
using SakuraLibrary.Proto;
using SakuraLibrary.Helper;

namespace LegacyLauncher.Model
{
    public class LauncherViewModel : LauncherModel
    {
        public readonly Action<bool, string> SimpleFailureHandler = (success, message) =>
        {
            if (!success)
            {
                MessageBox.Show(Program.TopMostForm, message, "操作失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        };

        public readonly MainForm View;

        public LauncherViewModel(MainForm view)
        {
            View = view;
            Dispatcher = new DispatcherWrapper(a => View.Invoke(a), a => View.BeginInvoke(a), () => !View.InvokeRequired);
        }

        public override void Log(Log l, bool init)
        {

        }

        public override void Load()
        {
            var settings = Properties.Settings.Default;

            // View.Width = settings.Width;
            // View.Height = settings.Height;

            // LogTextWrapping = settings.LogTextWrapping;
        }

        public override void Save()
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => Save());
                return;
            }

            var settings = Properties.Settings.Default;

            // settings.Width = (int)View.Width;
            // settings.Height = (int)View.Height;
            // settings.LogTextWrapping = LogTextWrapping;

            settings.Save();
        }
    }
}
