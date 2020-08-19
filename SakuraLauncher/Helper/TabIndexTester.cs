using SakuraLauncher.Model;

namespace SakuraLauncher.Helper
{
    public class TabIndexTester
    {
        public LauncherModel Model;

        public bool this[int offset] => Model.CurrentTab == offset;

        public TabIndexTester(LauncherModel main)
        {
            Model = main;
        }
    }
}
