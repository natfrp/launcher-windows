using System.Net.Sockets;

using SakuraLibrary.Helper;

namespace SakuraFrpService.Helper
{
    public class SocketConnection : ServiceConnection
    {
        public Socket Socket = null;

        public SocketConnection(byte[] buffer, Socket pipe)
        {
            Buffer = buffer;
            Socket = pipe;
        }

        public override void Dispose() => Socket?.Dispose();

        public override void Send(byte[] data) => Socket.Send(data);

        public int EnsureMessageComplete(int read)
        {
            // TODO:
            return read;
        }
    }
}
