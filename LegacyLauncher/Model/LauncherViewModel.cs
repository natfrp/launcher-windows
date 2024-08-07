using SakuraLibrary;
using SakuraLibrary.Helper;
using SakuraLibrary.Model;
using SakuraLibrary.Proto;
using System;
using static SakuraLibrary.Proto.Log.Types;

namespace LegacyLauncher.Model
{
    public class LauncherViewModel : LauncherModel
    {
        public readonly MainForm View;

        public LauncherViewModel(MainForm view) : base()
        {
            View = view;
            Dispatcher = new DispatcherWrapper(a => View.Invoke(a), a => View.BeginInvoke(a), () => !View.InvokeRequired);
            Run();
        }

        public override void ClearLog() => Dispatcher.Invoke(() => View.textBox_log.Clear());

        public override void Log(Log l, bool init)
        {
            if (l.Category == Category.Alert)
            {
                return;
            }
            if (l.Category != Category.Frpc)
            {
                l.Source = Utils.ParseTimestamp(l.Time).ToString("yyyy/MM/dd HH:mm:ss") + " " + l.Source + " " + l.Level switch
                {
                    Level.Debug => "D",
                    Level.Warn => "W",
                    Level.Error => "E",
                    Level.Fatal => "F",
                    _ => "I",
                };
            }
            View.textBox_log.AppendText(l.Source + " " + l.Data + Environment.NewLine);
        }

        public override IntPtr GetHwnd() => (IntPtr)View.Invoke(() => View.Handle);
    }
}
