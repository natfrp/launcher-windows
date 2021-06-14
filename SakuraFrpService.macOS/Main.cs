using AppKit;

using SakuraFrpService.Provider;

namespace SakuraFrpService
{
    static class MainClass
    {
        static void Main(string[] args)
        {
            NSApplication.Init();
            new SakuraService(new ConfigProvider(), new UtilsProvider(), new CommunicationProvider(), new SodiumProvider()).Start(false);
            NSApplication.Main(args);
        }
    }
}
