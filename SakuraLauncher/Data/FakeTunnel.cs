namespace SakuraLauncher.Data
{
    public class FakeTunnel : ITunnel
    {
        public bool IsReal => false;
        public Tunnel Real => null;
    }
}
