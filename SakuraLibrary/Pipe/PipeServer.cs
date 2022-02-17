using Google.Protobuf;
using SakuraLibrary.Helper;
using SakuraLibrary.Proto;
using System;
using System.Collections.Generic;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;

namespace SakuraLibrary.Pipe
{
    public class PipeServer : IDisposable
    {
        public readonly string Name;
        public readonly int BufferSize;

        public Action<ServiceConnection, RequestBase> DataReceived { get; set; }

        protected NamedPipeServerStream ListeningPipe = null, ListeningPushPipe = null;

        protected List<PipeStream> PushPipes = new List<PipeStream>();
        protected Dictionary<PipeStream, PipeConnection> Pipes = new Dictionary<PipeStream, PipeConnection>();

        public bool Running => !StopEvent.WaitOne(0);

        protected ManualResetEvent StopEvent = new ManualResetEvent(true);

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
            StopEvent.Reset();

            BeginPipeListen();
            BeginPushPipeListen();
        }

        public void Stop()
        {
            if (!Running)
            {
                return;
            }
            StopEvent.Set();

            Dispose();
        }

        public void Dispose()
        {
            lock (Pipes)
            {
                ListeningPipe?.Dispose();
                ListeningPipe = null;

                foreach (var p in Pipes.Values)
                {
                    p.Dispose();
                }
                Pipes.Clear();
            }
            lock (PushPipes)
            {
                ListeningPushPipe?.Dispose();
                ListeningPushPipe = null;

                foreach (var p in PushPipes)
                {
                    p.Dispose();
                }
                PushPipes.Clear();
            }
        }

        protected NamedPipeServerStream CreateListenerPipe(bool push, AsyncCallback callback)
        {
            if (!Running)
            {
                return null;
            }
            try
            {
                var security = new PipeSecurity();
                security.AddAccessRule(new PipeAccessRule(WindowsIdentity.GetCurrent().User, PipeAccessRights.FullControl, AccessControlType.Allow));
                security.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.AuthenticatedUserSid, null), PipeAccessRights.ReadWrite, AccessControlType.Allow));
                security.AddAccessRule(new PipeAccessRule(new SecurityIdentifier(WellKnownSidType.NetworkSid, null), PipeAccessRights.FullControl, AccessControlType.Deny));

                // PipeDirection.InOut enables the client to read message, DON'T TOUCH IT
                var pipe = new NamedPipeServerStream(push ? Name + PipeConnection.PUSH_SUFFIX : Name, PipeDirection.InOut, NamedPipeServerStream.MaxAllowedServerInstances, PipeTransmissionMode.Message, PipeOptions.Asynchronous, push ? 0 : BufferSize, BufferSize, security);

                if (!push)
                {
                    pipe.ReadMode = PipeTransmissionMode.Message;
                }
                pipe.BeginWaitForConnection(callback, pipe);
                return pipe;
            }
            catch
            {
                Stop();
                return null;
            }
        }

        #region RESTful API Pipe

        protected void OnPipeConnect(IAsyncResult ar)
        {
            var pipe = ar.AsyncState as NamedPipeServerStream;
            try
            {
                pipe.EndWaitForConnection(ar);
            }
            catch
            {
                Stop();
                return;
            }

            if (!Running)
            {
                pipe.Dispose();
                return;
            }
            lock (Pipes)
            {
                var conn = new PipeConnection(new byte[BufferSize], pipe);
                ListeningPipe = null;

                try
                {
                    BeginPipeRead(conn);
                    Pipes.Add(conn.Pipe, conn);
                }
                catch
                {
                    conn.Dispose();
                }
            }

            BeginPipeListen();
        }

        protected void OnPipeData(IAsyncResult ar)
        {
            var conn = ar.AsyncState as PipeConnection;
            int count;
            try
            {
                count = conn.Pipe.EndRead(ar);
            }
            catch
            {
                Stop();
                return;
            }

            if (!Running)
            {
                return;
            }
            try
            {
                DataReceived?.Invoke(conn, RequestBase.Parser.ParseFrom(conn.Buffer, 0, conn.EnsureMessageComplete(count)));
                BeginPipeRead(conn);
            }
            catch
            {
                conn.Dispose();
                lock (Pipes)
                {
                    Pipes.Remove(conn.Pipe);
                }
                return;
            }
        }

        protected void BeginPipeListen()
        {
            lock (Pipes)
            {
                ListeningPipe = CreateListenerPipe(false, OnPipeConnect);
            }
        }

        protected void BeginPipeRead(PipeConnection conn)
        {
            if (!Running)
            {
                return;
            }
            conn.Pipe.BeginRead(conn.Buffer, 0, conn.Buffer.Length, OnPipeData, conn);
        }

        #endregion

        #region Message Push Pipe

        public void PushMessage(PushMessageBase msg)
        {
            if (!Running)
            {
                return;
            }
            var buffer = msg.ToByteArray();
            var remove = new List<PipeStream>();
            lock (PushPipes)
            {
                foreach (var p in PushPipes)
                {
                    try
                    {
                        p.Write(buffer, 0, buffer.Length);
                    }
                    catch
                    {
                        if (!p.IsConnected)
                        {
                            remove.Add(p);
                        }
                    }
                }
                foreach (var r in remove)
                {
                    PushPipes.Remove(r);
                }
            }
        }

        protected void OnPushPipeConnect(IAsyncResult ar)
        {
            var pipe = ar.AsyncState as NamedPipeServerStream;
            try
            {
                pipe.EndWaitForConnection(ar);
            }
            catch
            {
                Stop();
                return;
            }

            if (!Running)
            {
                pipe.Dispose();
                return;
            }
            lock (PushPipes)
            {
                PushPipes.Add(pipe);
                ListeningPushPipe = null;
            }

            BeginPushPipeListen();
        }

        protected void BeginPushPipeListen()
        {
            lock (PushPipes)
            {
                ListeningPushPipe = CreateListenerPipe(true, OnPushPipeConnect);
            }
        }

        #endregion
    }
}
