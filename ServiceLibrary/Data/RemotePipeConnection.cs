using SakuraLibrary.Pipe;

namespace SakuraFrpService.Data
{
    class RemotePipeConnection : PipeConnection
    {
        public RemotePipeConnection() : base(null, null) { }

        public override void Send(byte[] data)
        {
            Buffer = data;
        }
    }
}
