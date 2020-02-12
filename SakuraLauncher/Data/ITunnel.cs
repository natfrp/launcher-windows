namespace SakuraLauncher.Data
{
    public interface ITunnel
    {
        bool IsReal { get; }
        Tunnel Real { get; }
    }
}
