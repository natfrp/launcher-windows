using System.ServiceProcess;

using SakuraLibrary;

using SakuraFrpService.Provider;

namespace SakuraFrpService
{
    public partial class MainService : ServiceBase
    {
        public SakuraService Main = null;

        public MainService()
        {
            InitializeComponent();
            Main = new SakuraService(new ConfigProvider(), new UtilsProvider(), new CommunicationProvider(UtilsWindows.InstallationPipeName), new SodiumProvider());
        }

        protected override void OnStart(string[] args)
        {
            Main.Start(false);
        }

        protected override void OnStop()
        {
            RequestAdditionalTime(60000);
            Main.Stop();
        }
    }
}
