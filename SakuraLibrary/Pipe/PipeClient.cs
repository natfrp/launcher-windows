using System;
using System.IO.Pipes;

namespace SakuraLibrary.Pipe
{
    public class PipeClient : IDisposable
    {
        public event PipeConnection.PipeDataEventHandler ServerPush;

        public PipeConnection Pipe = null, PushPipe = null;

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
                Pipe = new PipeConnection(new byte[BufferSize], pipe);

                pipe = new NamedPipeClientStream(Host, Name + PipeConnection.PUSH_SUFFIX, PipeDirection.In, PipeOptions.Asynchronous);
                pipe.Connect();
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

        protected void OnPushPipeData(IAsyncResult ar)
        {
            try
            {
                ServerPush?.Invoke(PushPipe, PushPipe.EnsureMessageComplete(PushPipe.Pipe.EndRead(ar)));
            }
            catch
            {
                if (!PushPipe.Pipe.IsConnected)
                {
                    Dispose();
                    return;
                }
            }
            BeginPushPipeRead();
        }

        protected void BeginPushPipeRead() => PushPipe.Pipe.BeginRead(PushPipe.Buffer, 0, PushPipe.Buffer.Length, OnPushPipeData, null);
    }
}
