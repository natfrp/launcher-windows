using SakuraLauncher.Model;

namespace SakuraLauncher.Helper
{
    public class TabIndexTester
    {
        public LauncherViewModel Model;

        public bool this[int offset] => Model.CurrentTab == offset;

        public TabIndexTester(LauncherViewModel main)
        {
            Model = main;
        }
    }
}
