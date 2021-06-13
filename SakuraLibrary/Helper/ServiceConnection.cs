using System;

using Google.Protobuf;

using SakuraLibrary.Proto;

namespace SakuraLibrary.Helper
{
    public abstract class ServiceConnection : IDisposable
    {
        public byte[] Buffer = null;

        public abstract void Dispose();

        public abstract void Send(byte[] data);

        public void SendProto(IMessage message) => Send(message.ToByteArray());

        public void RespondFailure(string message = "") => SendProto(new ResponseBase()
        {
            Success = false,
            Message = message ?? ""
        });
    }
}
