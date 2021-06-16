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
            new SakuraService(new ConfigProvider(), new UtilsProvider(), new CommunicationProvider(NSFileManager.DefaultManager.GetContainerUrl("moe.berd.SakuraL").Path + "/Library/Caches"), new SodiumProvider()).Start(false);
            NSApplication.Main(args);
        }
    }
}
