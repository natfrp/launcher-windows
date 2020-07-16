namespace SakuraLauncher.Data
{
    public class ServerData
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public bool AcceptNew { get; set; }

        public override string ToString() => "#" + ID + " " + Name;
    }
}
