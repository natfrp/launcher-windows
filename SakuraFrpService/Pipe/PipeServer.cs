using System;
using System.IO.Pipes;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace SakuraFrpService.Pipe
{
    public class PipeServer
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool GetNamedPipeClientProcessId(IntPtr Pipe, out uint ClientProcessId);

        public static uint GetPipePID(PipeConnection conn) => GetPipePID(conn.Pipe);

        public static uint GetPipePID(PipeStream pipe)
        {
            if (GetNamedPipeClientProcessId(pipe.SafePipeHandle.DangerousGetHandle(), out uint pid))
            {
                return pid;
            }
            return 0;
        }

        public event PipeConnection.PipeConnectionEventHandler Connected;
        public event PipeConnection.PipeConnectionEventHandler Disconnected;

        public event PipeConnection.PipeDataEventHandler DataReceived;

        public NamedPipeServerStream ListeningPipe = null;
        public Dictionary<PipeStream, PipeConnection> Pipes = new Dictionary<PipeStream, PipeConnection>();

        public readonly string Name;
        public readonly int BufferSize;

        public bool Running { get; protected set; }

        public PipeServer(string name, int bufferSize = 1048576)
        {
            Name = name;
            BufferSize = bufferSize;
        }

        public void Start()
        {
            if (Running)
            {
                return;
            }
            Running = true;

            BeginPipeListen();
        }

        public void Stop()
        {
            if (!Running)
            {
                return;
            }
            Running = false;

            ListeningPipe.Dispose();
            ListeningPipe = null;

            lock (Pipes)
            {
                foreach (var p in Pipes)
                {
                    p.Key.Close();
                    p.Key.Dispose();
                }
                Pipes.Clear();
            }
        }

        public void OnPipeConnect(IAsyncResult ar)
        {
            try
            {
                ListeningPipe.EndWaitForConnection(ar);
                var conn = new PipeConnection()
                {
                    Pipe = ListeningPipe,
                    Buffer = new byte[BufferSize]
                };
                ListeningPipe = null;

                lock (Pipes)
                {
                    Pipes.Add(ListeningPipe, conn);
                }

                BeginPipeRead(conn);
                Connected?.Invoke(conn);
            }
            catch { }

            BeginPipeListen();
        }

        public void OnPipeData(IAsyncResult ar)
        {
            var conn = ar.AsyncState as PipeConnection;
            int count = 0;
            try
            {
                count = conn.Pipe.EndRead(ar);
                DataReceived?.Invoke(conn, count);
            }
            catch
            {
                if (!conn.Pipe.IsConnected)
                {
                    lock (Pipes)
                    {
                        Pipes.Remove(conn.Pipe);
                    }
                    Disconnected?.Invoke(conn);
                    conn.Pipe.Dispose();
                    return;
                }
            }
            BeginPipeRead(conn);
        }

        protected void BeginPipeListen()
        {
            if (!Running)
            {
                return;
            }
            lock (Pipes)
            {
                if (ListeningPipe != null)
                {
                    // Shouldn't happen
                    ListeningPipe.Dispose();
                }
                ListeningPipe = new NamedPipeServerStream(Name, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Message, PipeOptions.Asynchronous);
                ListeningPipe.BeginWaitForConnection(OnPipeConnect, ListeningPipe);
            }
        }

        protected void BeginPipeRead(PipeConnection pipe)
        {
            pipe.Pipe.BeginRead(pipe.Buffer, 0, BufferSize, OnPipeData, pipe.Pipe);
        }
    }
}
