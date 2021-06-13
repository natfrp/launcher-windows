using System.Diagnostics;

using SakuraLibrary;

namespace SakuraFrpService.Provider
{
    public class UtilsProvider : IUtilsProvider
    {
        public Process[] SearchProcess(string name, string testPath = null) => UtilsWindows.SearchProcess(name, testPath);
    }
}
