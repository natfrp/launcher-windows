using SakuraLibrary.Proto;

namespace SakuraLauncher.Model
{
    public class NodeModel
    {
        public Node Proto = null;

        public int Id => Proto.Id;
        public string Name => Proto.Name;
        public bool AcceptNew => Proto.AcceptNew;
        public int AcceptHttp => Proto.AcceptHttp;

        public NodeModel(Node n)
        {
            Proto = n;
        }

        public override string ToString() => "#" + Id + " " + Name;
    }
}
