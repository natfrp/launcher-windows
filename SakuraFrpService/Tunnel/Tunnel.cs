using System.Text;
using System.Diagnostics;

using TunnelProto = SakuraLibrary.Proto.Tunnel;
using TunnelStatus = SakuraLibrary.Proto.Tunnel.Types.Status;

namespace SakuraFrpService.Tunnel
{
    public class Tunnel
    {
        public readonly TunnelLogger Logger = null;
        public readonly TunnelManager Manager = null;

        public int Id, Node;
        public string Name, Type, Description;

        public bool Enabled = false;
        public bool Running => BaseProcess != null && !BaseProcess.HasExited;
        public Process BaseProcess = null;

        public Tunnel(TunnelManager manager, int logRotateSize = 1024)
        {
            Manager = manager;
            Logger = new TunnelLogger(logRotateSize);
        }

        public TunnelProto CreateProto() => new TunnelProto()
        {
            Id = Id,
            Name = Name,
            Status = Enabled ? (Running ? TunnelStatus.Running : TunnelStatus.Pending) : TunnelStatus.Disabled
        };

        public bool Start()
        {
            if (Running)
            {
                return false;
            }
            try
            {
                BaseProcess = Process.Start(new ProcessStartInfo(Manager.FrpcPath, Manager.GetArguments(Id))
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    StandardErrorEncoding = Encoding.UTF8,
                    StandardOutputEncoding = Encoding.UTF8
                });

                BaseProcess.ErrorDataReceived += Logger.DataReceived;
                BaseProcess.OutputDataReceived += Logger.DataReceived;

                BaseProcess.BeginErrorReadLine();
                BaseProcess.BeginOutputReadLine();
                return true;
            }
            catch
            {
                Stop();
            }
            return false;
        }

        public void Stop()
        {
            if (!Running)
            {
                Cleanup();
                return;
            }
            try
            {
                BaseProcess.StandardInput.Write("stop\n");
                if (!BaseProcess.WaitForExit(3500))
                {
                    // Main.Log("Launcher", "frpc 未响应, 正在强制结束进程");
                    BaseProcess.Kill();
                }
                // Main.Log("Launcher", "frpc 已结束");
            }
            finally
            {
                Cleanup();
            }
        }

        protected void Cleanup()
        {
            if (BaseProcess != null)
            {
                BaseProcess.Dispose();
                BaseProcess = null;
            }
        }
    }
}
