using System;
using System.IO;
using System.IO.Pipes;

using Google.Protobuf;

using SakuraLibrary.Proto;

namespace SakuraLibrary.Pipe
{
    public class PipeConnection : IDisposable
    {
        public const string PUSH_SUFFIX = "_PUSH";

        public delegate void PipeConnectionEventHandler(PipeConnection connection);
        public delegate void PipeDataEventHandler(PipeConnection connection, int length);

        public byte[] Buffer = null;
        public PipeStream Pipe = null;

        public PipeConnection(byte[] buffer, PipeStream pipe)
        {
            Buffer = buffer;
            Pipe = pipe;
        }

        public void Dispose() => Pipe?.Dispose();

        public void Send(byte[] data) => Pipe.Write(data, 0, data.Length);

        public void SendProto(IMessage message)
        {
            using (var ms = new MemoryStream())
            {
                message.WriteTo(ms);
                Send(ms.ToArray());
            }
        }

        public void RespondFailure(string message = "") => SendProto(new ResponseBase()
        {
            Success = false,
            Message = message ?? ""
        });

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
