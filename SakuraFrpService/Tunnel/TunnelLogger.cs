using System.Diagnostics;
using System.Collections.Concurrent;

namespace SakuraFrpService.Tunnel
{
    public class TunnelLogger : ConcurrentQueue<string>
    {
        public int RotateSize;

        public TunnelLogger(int bufferSize)
        {
            RotateSize = bufferSize;
        }

        public void Log(string data)
        {
            if (data == null)
            {
                return;
            }
            while (Count > RotateSize)
            {
                TryDequeue(out string _);
            }
            Enqueue(data);
        }

        public void DataReceived(object sender, DataReceivedEventArgs e) => Log(e.Data);
    }
}
