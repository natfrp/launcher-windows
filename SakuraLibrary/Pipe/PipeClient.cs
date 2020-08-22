using System;
using System.IO.Pipes;

using Google.Protobuf;

using SakuraLibrary.Proto;

namespace SakuraLibrary.Pipe
{
    public class PipeClient : IDisposable
    {
        public event PipeConnection.PipeDataEventHandler ServerPush;

        public PipeConnection Pipe = null, PushPipe = null;

        public bool Connected => Pipe != null && PushPipe != null && Pipe.Pipe.IsConnected && PushPipe.Pipe.IsConnected;

        public readonly int BufferSize;
        public readonly string Name, Host;

        public PipeClient(string name, string host = ".", int bufferSize = 1048576)
        {
            Name = name;
            Host = host;
            BufferSize = bufferSize;
        }

        public bool Connect()
        {
            try
            {
                var pipe = new NamedPipeClientStream(Host, Name, PipeDirection.InOut, PipeOptions.Asynchronous);
                pipe.Connect();
                pipe.ReadMode = PipeTransmissionMode.Message;
                Pipe = new PipeConnection(new byte[BufferSize], pipe);

                pipe = new NamedPipeClientStream(Host, Name + PipeConnection.PUSH_SUFFIX, PipeDirection.InOut, PipeOptions.Asynchronous);
                pipe.Connect();
                pipe.ReadMode = PipeTransmissionMode.Message;
                PushPipe = new PipeConnection(new byte[BufferSize], pipe);

                BeginPushPipeRead();
                return true;
            }
            catch
            {
                Dispose();
            }
            return false;
        }

        public void Dispose()
        {
            Pipe?.Dispose();
            Pipe = null;

            PushPipe?.Dispose();
            PushPipe = null;
        }

        public ResponseBase Request(RequestBase message)
        {
            lock (this)
            {
                try
                {
                    var data = message.ToByteArray();
                    Pipe.Pipe.Write(data, 0, data.Length);
                    return ResponseBase.Parser.ParseFrom(Pipe.Buffer, 0, Pipe.EnsureMessageComplete(Pipe.Pipe.Read(Pipe.Buffer, 0, Pipe.Buffer.Length)));
                }
                catch
                {
                    if (!Pipe.Pipe.IsConnected)
                    {
                        Dispose();
                    }
                }
                return null;
            }
        }

        protected void OnPushPipeData(IAsyncResult ar)
        {
            if (!PushPipe.Pipe.IsConnected)
            {
                Dispose();
                return;
            }
            try
            {
                ServerPush?.Invoke(PushPipe, PushPipe.EnsureMessageComplete(PushPipe.Pipe.EndRead(ar)));
            }
            catch { }
            BeginPushPipeRead();
        }

        protected void BeginPushPipeRead() => PushPipe.Pipe.BeginRead(PushPipe.Buffer, 0, PushPipe.Buffer.Length, OnPushPipeData, null);
    }
}
