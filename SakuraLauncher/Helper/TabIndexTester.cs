namespace SakuraLauncher.Helper
{
    public class TabIndexTester
    {
        public MainWindow Main;

        public bool this[int offset] => Main.CurrentTab == offset;

        public TabIndexTester(MainWindow main)
        {
            Main = main;
        }
    }
}
