using System;

using SakuraLibrary.Proto;
using SakuraLibrary.Helper;

namespace SakuraFrpService.Provider
{
    public class CommunicationProvider : ICommunicationProvider
    {
        public Action<ServiceConnection, int> DataReceived { get; set; }

        public bool Running => true;

        public void Start()
        {
            // TODO:
        }

        public void Stop()
        {

        }

        public void Dispose()
        {

        }

        public void PushMessage(PushMessageBase msg)
        {

        }
    }
}
