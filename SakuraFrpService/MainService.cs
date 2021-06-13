using System.ServiceProcess;

using SakuraFrpService.Provider;

namespace SakuraFrpService
{
    public partial class MainService : ServiceBase
    {
        public SakuraService Main = null;

        public MainService()
        {
            InitializeComponent();
            Main = new SakuraService(new ConfigProvider(), new UtilsProvider(), new SodiumProvider());
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
