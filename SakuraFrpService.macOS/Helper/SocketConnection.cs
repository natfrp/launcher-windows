using System;
using System.Net.Sockets;

using SakuraLibrary.Proto;
using SakuraLibrary.Helper;

namespace SakuraFrpService.Helper
{
    public class SocketConnection : ServiceConnection
    {
        public Socket Socket = null;

        public SocketConnection(byte[] buffer, Socket socket)
        {
            Buffer = buffer;
            Socket = socket;
        }

        public override void Dispose() => Socket?.Dispose();

        public override void Send(byte[] data)
        {
            if (data.Length > Buffer.Length)
            {
                throw new Exception("Data too long");
            }
            Socket.Send(BitConverter.GetBytes(data.Length));
            Socket.Send(data);
        }

        public RequestBase FinishReceive(int read)
        {
            if (read != 4)
            {
                throw new Exception("Protocol violation");
            }

            var count = BitConverter.ToInt32(Buffer, 0);
            if (count > Buffer.Length)
            {
                throw new Exception("Data too long");
            }

            int index = 0;
            do
            {
                index += Socket.Receive(Buffer, index, count - index, SocketFlags.None);
            }
            while (count > index);
            return RequestBase.Parser.ParseFrom(Buffer, 0, count);
        }
    }
}
