using System;
using System.IO;
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
            if (Pipe.IsMessageComplete)
            {
                return read;
            }
            using (var ms = new MemoryStream())
            {
                ms.Write(Buffer, 0, read);
                while (!Pipe.IsMessageComplete)
                {
                    ms.Write(Buffer, 0, Pipe.Read(Buffer, 0, Buffer.Length));
                }

                var final = ms.ToArray();
                if (Buffer.Length < ms.Position)
                {
                    Buffer = final;
                }
                else
                {
                    Array.Copy(final, Buffer, final.Length);
                }
                return final.Length;
            }
        }
    }
}
