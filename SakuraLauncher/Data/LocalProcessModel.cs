using SakuraLauncher.Helper;

namespace SakuraLauncher.Data
{
    public class LocalProcessModel : ModelBase
    {
        public string PID { get; set; }
        public string ProcessName { get; set; }

        public string Protocol { get; set; }
        public string Address { get; set; }
        public string Port { get; set; }
    }
}
