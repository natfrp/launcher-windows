namespace SakuraLauncher.Data
{
    public class ServerData
    {
        public string ID { get; set; }
        public string Name { get; set; }

        public override string ToString() => "#" + ID + " " + Name;
    }
}
