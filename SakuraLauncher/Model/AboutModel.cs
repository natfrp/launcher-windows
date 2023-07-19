using SakuraLibrary.Model;
using System.Reflection;

namespace SakuraLauncher.Model
{
    public class AboutModel : ModelBase
    {
        private readonly LauncherViewModel Model;

        public string License => Properties.Resources.LICENSE;

        public string Version => Assembly.GetExecutingAssembly().GetName().Version.ToString();

        public string ServiceVersion => ""; // Model.Update?.CurrentVersionService ?? "-";

        public string FrpcVersion => ""; // Model.Update?.CurrentVersionFrpc == null || Model.Update.CurrentVersionFrpc == "" ? "-" : Model.Update.CurrentVersionFrpc;

        public AboutModel(LauncherViewModel model)
        {
            Model = model;
            model.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(Model.Update))
                {
                    RaisePropertyChanged(nameof(ServiceVersion));
                    RaisePropertyChanged(nameof(FrpcVersion));
                }
            };
        }
    }
}
