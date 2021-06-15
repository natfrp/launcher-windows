using System;
using System.IO;
using System.Net.Sockets;
using System.Collections.Generic;

using Google.Protobuf;

using SakuraLibrary.Proto;
using SakuraLibrary.Helper;

using SakuraFrpService.Helper;

namespace SakuraFrpService.Provider
{
    public class CommunicationProvider : ICommunicationProvider
    {
        public readonly string Path;
        public readonly int BufferSize;

        public Action<ServiceConnection, RequestBase> DataReceived { get; set; }

        protected Socket MainListener = null, PushListener = null;

        public bool Running
        {
            get
            {
                lock (_runningLock)
                {
                    return _running;
                }
            }
            protected set
            {
                lock (_runningLock)
                {
                    _running = value;
                }
            }
        }
        private bool _running;
        private object _runningLock = new object();

        protected List<Socket> PushSockets = new List<Socket>();
        protected Dictionary<Socket, SocketConnection> Sockets = new Dictionary<Socket, SocketConnection>();

        public CommunicationProvider(string path, int bufferSize = 1048576)
        {
            Path = path;
            BufferSize = bufferSize;
        }

        public void Start()
        {
            if (Running)
            {
                return;
            }
            Running = true;

            MainListener = CreateSocket(Path + "service.sock");
            PushListener = CreateSocket(Path + "service-push.sock");

            BeginAccept();
            BeginPushAccept();
        }

        public void Stop()
        {
            if (!Running)
            {
                return;
            }
            Running = false;
            Dispose();
        }

        public void Dispose()
        {
            lock (Sockets)
            {
                MainListener?.Close();
                MainListener?.Dispose();
                MainListener = null;

                foreach (var p in Sockets.Values)
                {
                    p.Socket.Shutdown(SocketShutdown.Both);
                    p.Socket.Close();
                    p.Dispose();
                }
                Sockets.Clear();
            }
            lock (PushSockets)
            {
                PushListener?.Close();
                PushListener?.Dispose();
                PushListener = null;

                foreach (var p in PushSockets)
                {
                    p.Shutdown(SocketShutdown.Both);
                    p.Close();
                    p.Dispose();
                }
                PushSockets.Clear();
            }
        }

        protected Socket CreateSocket(string file)
        {
            if (File.Exists(file))
            {
                File.Delete(file);
            }
            var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
            socket.Bind(new UnixDomainSocketEndPoint(file));
            socket.Listen(50);
            return socket;
        }

        #region RESTful API Socket

        protected void BeginAccept()
        {
            if (!Running)
            {
                return;
            }
            MainListener.BeginAccept(OnConnect, null);
        }

        protected void OnConnect(IAsyncResult ar)
        {
            try
            {
                var conn = new SocketConnection(new byte[BufferSize], MainListener.EndAccept(ar));
                lock (Sockets)
                {
                    Sockets.Add(conn.Socket, conn);
                }

                BeginReceive(conn);
            }
            catch { }
            BeginAccept();
        }

        protected void BeginReceive(SocketConnection conn)
        {
            try
            {
                conn.Socket.BeginReceive(conn.Buffer, 0, 4, SocketFlags.None, OnDataReceived, conn);
            }
            catch
            {
                lock (Sockets)
                {
                    Sockets.Remove(conn.Socket);
                }
                conn.Dispose();
            }
        }

        protected void OnDataReceived(IAsyncResult ar)
        {
            var conn = ar.AsyncState as SocketConnection;
            try
            {
                DataReceived?.Invoke(conn, conn.FinishReceive(conn.Socket.EndReceive(ar)));
            }
            catch
            {
                lock (Sockets)
                {
                    Sockets.Remove(conn.Socket);
                }
                conn.Dispose();
                return;
            }
            BeginReceive(conn);
        }

        #endregion

        #region Message Push Socket

        public void PushMessage(PushMessageBase msg)
        {
            var buffer = msg.ToByteArray();
            var remove = new List<Socket>();
            lock (PushSockets)
            {
                foreach (var p in PushSockets)
                {
                    try
                    {
                        p.Send(buffer);
                    }
                    catch
                    {
                        p.Dispose();
                        remove.Add(p);
                    }
                }
                foreach (var r in remove)
                {
                    PushSockets.Remove(r);
                }
            }
        }

        protected void BeginPushAccept()
        {
            if (!Running)
            {
                return;
            }
            PushListener.BeginAccept(OnPushConnect, null);
        }

        protected void OnPushConnect(IAsyncResult ar)
        {
            try
            {
                lock (PushSockets)
                {
                    PushSockets.Add(PushListener.EndAccept(ar));
                }
            }
            catch { }
            BeginPushAccept();
        }

        #endregion
    }
}
