using SakuraLibrary.Helper;

namespace SakuraFrpService.Data
{
    public class RemotePipeConnection : ServiceConnection
    {
        public override void Send(byte[] data)
        {
            Buffer = data;
        }

        public override void Dispose() { }
    }
}
