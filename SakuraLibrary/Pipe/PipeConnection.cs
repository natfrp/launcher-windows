using System;
using System.IO.Pipes;

using SakuraLibrary.Helper;

namespace SakuraLibrary.Pipe
{
    public class PipeConnection : ServiceConnection
    {
        public const string PUSH_SUFFIX = "_PUSH";

        public PipeStream Pipe = null;

        public PipeConnection(byte[] buffer, PipeStream pipe)
        {
            Buffer = buffer;
            Pipe = pipe;
        }

        public override void Dispose() => Pipe?.Dispose();

        public override void Send(byte[] data) => Pipe.Write(data, 0, data.Length);

        public int EnsureMessageComplete(int read)
        {
            int index = read;
            while (!Pipe.IsMessageComplete)
            {
                index += Pipe.Read(Buffer, index, Buffer.Length - index);
                if (index == Buffer.Length)
                {
                    throw new Exception("Data too long");
                }
            }
            return index;
        }
    }
}
