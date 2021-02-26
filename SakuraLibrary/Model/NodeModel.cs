using SakuraLibrary.Proto;

namespace SakuraLibrary.Model
{
    public class NodeModel
    {
        public Node Proto = null;

        public int Id => Proto.Id;
        public string Name => Proto.Name;
        public bool AcceptNew => Proto.AcceptNew;

        public bool Enabled => Proto.Id >= 0;

        public NodeModel(Node n)
        {
            Proto = n;
        }

        public override string ToString() => Id < 0 ? Name : "#" + Id + " " + Name;
    }
}
