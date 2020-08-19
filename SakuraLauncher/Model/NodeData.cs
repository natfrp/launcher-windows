namespace SakuraLauncher.Model
{
    public class NodeData
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public bool AcceptNew { get; set; }

        public override string ToString() => "#" + ID + " " + Name;
    }
}
