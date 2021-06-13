using System;

using SakuraLibrary.Proto;
using SakuraLibrary.Helper;

namespace SakuraFrpService.Provider
{
    public interface ICommunicationProvider : IDisposable
    {
        Action<ServiceConnection, int> DataReceived { get; set; }

        bool Running { get; }

        void Start();
        void Stop();

        void PushMessage(PushMessageBase msg);
    }
}
