using System;
using System.Windows.Forms;

using SakuraLibrary;
using SakuraLibrary.Model;
using SakuraLibrary.Proto;
using SakuraLibrary.Helper;
using static SakuraLibrary.Proto.Log.Types;

namespace LegacyLauncher.Model
{
    public class LauncherViewModel : LauncherModel
    {
        public readonly MainForm View;

        public LauncherViewModel(MainForm view) : base(true)
        {
            View = view;
            Dispatcher = new DispatcherWrapper(a => View.Invoke(a), a => View.BeginInvoke(a), () => !View.InvokeRequired);

            var settings = Properties.Settings.Default;
            if (settings.UpgradeRequired)
            {
                settings.Upgrade();
                settings.UpgradeRequired = false;
                settings.Save();
            }

            LogTextWrapping = settings.LogTextWrapping;
            NotificationMode = settings.SuppressInfo ? 1 : 0;

            Run();
        }

        #region ViewModel Abstraction

        public override void ClearLog() => Dispatcher.Invoke(() => View.textBox_log.Clear());

        public override void Log(Log l, bool init)
        {
            if (l.Category == Category.Alert)
            {
                if (NotificationMode == 0 || (NotificationMode == 2 && l.Level > Level.Info))
                {
                    View.notifyIcon_tray.ShowBalloonTip(5000, l.Source, l.Data, l.Level switch
                    {
                        Level.Info => ToolTipIcon.Info,
                        Level.Warn => ToolTipIcon.Warning,
                        Level.Error => ToolTipIcon.Error,
                        Level.Fatal => ToolTipIcon.Error,
                        _ => ToolTipIcon.None,
                    });
                }
                return;
            }
            if (l.Category != Category.Frpc)
            {
                l.Source = Utils.ParseTimestamp(l.Time).ToString("yyyy/MM/dd HH:mm:ss") + " " + l.Source;
            }
            View.textBox_log.AppendText(l.Source + " " + l.Level switch
            {
                Level.Debug => "D ",
                Level.Warn => "W ",
                Level.Error => "E ",
                Level.Fatal => "F ",
                _ => "I ",
            } + l.Data + Environment.NewLine);
        }

        public override IntPtr GetHwnd() => (IntPtr)View.Invoke(() => View.Handle);

        #endregion

        public void Save()
        {
            var settings = Properties.Settings.Default;

            settings.SuppressInfo = NotificationMode != 0;
            settings.LogTextWrapping = LogTextWrapping;

            settings.Save();
        }
    }
}
