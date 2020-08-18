using System.IO;
using System.IO.Pipes;

using Google.Protobuf;

namespace SakuraFrpService.Pipe
{
    public class PipeConnection
    {
        public delegate void PipeConnectionEventHandler(PipeConnection connection);
        public delegate void PipeDataEventHandler(PipeConnection connection, int length);

        public byte[] Buffer = null;
        public PipeStream Pipe = null;

        public void Send(byte[] data) => Pipe.Write(data, 0, data.Length);

        public void SendProto(IMessage message)
        {
            using (var ms = new MemoryStream())
            {
                message.WriteTo(ms);
                Send(ms.ToArray());
            }
        }
    }
}
