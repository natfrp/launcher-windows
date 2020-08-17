using System.IO.Pipes;

namespace SakuraFrpService.Pipe
{
    public class PipeConnection
    {
        public delegate void PipeConnectionEventHandler(PipeConnection connection);
        public delegate void PipeDataEventHandler(PipeConnection connection, int length);

        public byte[] Buffer = null;
        public PipeStream Pipe = null;
    }
}
