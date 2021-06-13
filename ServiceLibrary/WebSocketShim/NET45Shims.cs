using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SakuraFrpService.WebSocketShim
{
    static class SocketExtensions
    {
        public static Task ConnectAsync(this Socket socket, IPAddress address, int port)
        {
            return Task.Factory.FromAsync(
                (targetAddress, targetPort, callback, state) => ((Socket)state).BeginConnect(targetAddress, targetPort, callback, state),
                asyncResult => ((Socket)asyncResult.AsyncState).EndConnect(asyncResult),
                address,
                port,
                state: socket);
        }
    }

    static class WebSocketUtil
    {
        public static ManagedWebSocket CreateClientWebSocket(Stream innerStream,
            string subProtocol, int receiveBufferSize, int sendBufferSize,
            TimeSpan keepAliveInterval, bool useZeroMaskingKey, ArraySegment<byte> internalBuffer)
        {
            if (innerStream == null)
            {
                throw new ArgumentNullException(nameof(innerStream));
            }

            if (!innerStream.CanRead || !innerStream.CanWrite)
            {
                throw new ArgumentException(!innerStream.CanRead ? SR.NotReadableStream : SR.NotWriteableStream, nameof(innerStream));
            }

            if (subProtocol != null)
            {
                WebSocketValidate.ValidateSubprotocol(subProtocol);
            }

            if (keepAliveInterval != Timeout.InfiniteTimeSpan && keepAliveInterval < TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(nameof(keepAliveInterval), keepAliveInterval,
                    SR.Format(SR.net_WebSockets_ArgumentOutOfRange_TooSmall,
                    0));
            }

            if (receiveBufferSize <= 0 || sendBufferSize <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    receiveBufferSize <= 0 ? nameof(receiveBufferSize) : nameof(sendBufferSize),
                    receiveBufferSize <= 0 ? receiveBufferSize : sendBufferSize,
                    SR.Format(SR.net_WebSockets_ArgumentOutOfRange_TooSmall, 0));
            }

            return ManagedWebSocket.CreateFromConnectedStream(
                innerStream, false, subProtocol, keepAliveInterval,
                receiveBufferSize, internalBuffer);
        }
    }

    static class UriExtensions
    {
        public static string GetIdnHost(this Uri uri)
        {
            return new System.Globalization.IdnMapping().GetAscii(uri.Host);
        }
    }

}