using System.ComponentModel;
using System.Configuration.Install;

using SakuraLibrary;

namespace SakuraFrpService
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
            serviceInstaller1.ServiceName = Consts.ServiceName;
        }
    }
}
