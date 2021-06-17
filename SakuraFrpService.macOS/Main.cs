using System.Linq;
using System.Diagnostics;

using AppKit;
using Foundation;

using SakuraFrpService.Provider;

namespace SakuraFrpService
{
    static class MainClass
    {
        static void Main(string[] args)
        {
            NSApplication.Init();

            var list = NSWorkspace.SharedWorkspace.RunningApplications
                .Where(app => app.BundleIdentifier == "moe.berd.SakuraLauncher.Service" && app.ProcessIdentifier != Process.GetCurrentProcess().Id)
                .Select(app => app.ProcessIdentifier)
                .ToArray();

            if (list.Length != 0)
            {
                var alert = new NSAlert()
                {
                    AlertStyle = NSAlertStyle.Warning,
                    InformativeText = "找到 " + list.Length + " 个重复进程，PID = [" + string.Join(',', list) + "]",
                    MessageText = "请勿重复开启 SakuraFrp Service"
                };
                return;
            }

            new SakuraService(new ConfigProvider(), new UtilsProvider(), new CommunicationProvider(NSFileManager.DefaultManager.GetContainerUrl("moe.berd.SakuraL").Path + "/Library/Caches"), new SodiumProvider()).Start(false);
            NSApplication.Main(args);
        }
    }
}
