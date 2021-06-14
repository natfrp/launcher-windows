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

        public Action<ServiceConnection, int> DataReceived { get; set; }

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

            BeginListen();
            BeginPushListen();
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

        #region RESTful API Socket

        protected void OnConnect(IAsyncResult ar)
        {
            try
            {
                var sock = MainListener.EndAccept(ar);
                var conn = new SocketConnection(new byte[BufferSize], sock);

                lock (Sockets)
                {
                    Sockets.Add(conn.Socket, conn);
                }

                BeginPipeRead(conn);
            }
            catch { }
            BeginListen();
        }

        protected void OnDataReceived(IAsyncResult ar)
        {
            var conn = ar.AsyncState as SocketConnection;
            try
            {
                DataReceived?.Invoke(conn, conn.EnsureMessageComplete(conn.Socket.EndReceive(ar)));
            }
            catch
            {
                if (!conn.Socket.Connected)
                {
                    lock (Sockets)
                    {
                        Sockets.Remove(conn.Socket);
                    }
                    conn.Dispose();
                    return;
                }
            }
            BeginPipeRead(conn);
        }

        protected void BeginListen()
        {
            if (!Running)
            {
                return;
            }
            if (MainListener == null)
            {
                var file = Path + "service.sock";
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
                MainListener = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
                MainListener.Bind(new UnixDomainSocketEndPoint(file));

                MainListener.Listen(50);
            }
            MainListener.BeginAccept(OnConnect, null);
        }

        protected void BeginPipeRead(SocketConnection conn) => conn.Socket.BeginReceive(conn.Buffer, 0, conn.Buffer.Length, SocketFlags.None, OnDataReceived, conn);

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
                        if (!p.Connected)
                        {
                            remove.Add(p);
                        }
                    }
                }
                foreach (var r in remove)
                {
                    PushSockets.Remove(r);
                }
            }
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
            BeginPushListen();
        }

        protected void BeginPushListen()
        {
            if (!Running)
            {
                return;
            }
            if (PushListener == null)
            {
                var file = Path + "service-push.sock";
                if (File.Exists(file))
                {
                    File.Delete(file);
                }
                PushListener = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
                PushListener.Bind(new UnixDomainSocketEndPoint(file));

                PushListener.Listen(50);
            }
            PushListener.BeginAccept(OnPushConnect, null);
        }

        #endregion
    }
}
