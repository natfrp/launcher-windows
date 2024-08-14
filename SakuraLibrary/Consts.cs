using System;
using System.IO;

namespace SakuraLibrary
{
    public static class Consts
    {
        public const string Version = "3.1.2.0";

        public const string PipeName = "SakuraFrpService3";
        public const string ServiceName = "SakuraFrpService";
        public const string ServiceExecutable = "SakuraFrpService.exe";

        public const string SakuraLauncherPrefix = "SakuraLauncher_";
        public const string LegacyLauncherPrefix = "LegacySakuraLauncher_";

        public static readonly string WorkingDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), ServiceName) + "\\";
    }
}
