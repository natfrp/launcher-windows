using System;
using System.Text;
using System.Diagnostics;

using TunnelProto = SakuraLibrary.Proto.Tunnel;
using TunnelStatus = SakuraLibrary.Proto.Tunnel.Types.Status;

using SakuraFrpService.Manager;

namespace SakuraFrpService.Data
{
    public class Tunnel
    {
        public readonly TunnelManager Manager = null;

        public int Id, Node;
        public string Name, Type, Note, Description;

        public int WaitTick = 0, FailCount = 0;
        public bool Enabled = false;
        public bool Running => BaseProcess != null && !BaseProcess.HasExited;

        public Process BaseProcess = null;

        public Tunnel(TunnelManager manager)
        {
            Manager = manager;
        }

        public TunnelProto CreateProto() => new TunnelProto()
        {
            Id = Id,
            Node = Node,
            Name = Name,
            Type = Type,
            Note = Note,
            Description = Description,
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

                BaseProcess.ErrorDataReceived += OnProcessData;
                BaseProcess.OutputDataReceived += OnProcessData;

                BaseProcess.BeginErrorReadLine();
                BaseProcess.BeginOutputReadLine();
                return true;
            }
            catch (Exception e)
            {
                Manager.Main.LogManager.Log(LogManager.CATEGORY_SERVICE_WARNING, Name, "隧道启动失败: " + e.Message);
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
                if (BaseProcess.HasExited)
                {
                    return;
                }
                BaseProcess.StandardInput.Write("stop\n");
                if (!BaseProcess.WaitForExit(3500))
                {
                    Manager.Main.LogManager.Log(LogManager.CATEGORY_SERVICE_WARNING, Name, "frpc 未响应, 正在强制结束进程");
                    BaseProcess.Kill();
                }
                Manager.Main.LogManager.Log(LogManager.CATEGORY_SERVICE_INFO, Name, "frpc 已结束");
            }
            finally
            {
                Cleanup();
            }
        }

        protected void OnProcessData(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                Manager.Main.LogManager.Log(LogManager.CATEGORY_FRPC, Name, e.Data);
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
