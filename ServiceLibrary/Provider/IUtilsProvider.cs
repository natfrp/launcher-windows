using System.Diagnostics;

namespace SakuraFrpService.Provider
{
    public interface IUtilsProvider
    {
        Process[] SearchProcess(string name, string path);

        bool IsMacOS { get; }
    }
}
