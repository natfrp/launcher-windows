using System.Diagnostics;
using System.IO;
using System.Linq;

namespace SakuraFrpService.Provider
{
    public class UtilsProvider : IUtilsProvider
    {
        public Process[] SearchProcess(string name, string path) => Process.GetProcessesByName(name).Where(p =>
        {
            try
            {
                return Path.GetFullPath(p.MainModule.FileName) == path;
            }
            catch { }
            return false;
        }).ToArray();

        public bool IsMacOS => true;
    }
}
