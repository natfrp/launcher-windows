using System.Text;
using System.Threading;
using System.Diagnostics;

using SakuraLauncher.View;
using SakuraLauncher.Helper;

namespace SakuraLauncher.Data
{
    public class Tunnel : ModelBase, ITunnel
    {
        public static string ClientPath = "frpc.exe";

        public bool IsReal => true;
        public Tunnel Real => this;

        public bool Selected => MainWindow.Instance.CurrentListener == this;

        public bool Enabled
        {
            get => _enabled;
            set
            {
                if(_enabled == value || Exiting)
                {
                    return;
                }
                _enabled = value;
                if(value)
                {
                    Start();
                }
                else
                {
                    Stop();
                }
                RaisePropertyChanged();
            }
        }
        private bool _enabled;

        public int Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Description { get; set; }

        public int NodeID { get; set; }
        public string NodeName { get; set; }
        
        public Process BaseProcess = null;

        public void LogOutput(object sender, DataReceivedEventArgs e)
        {
            if(e.Data != null)
            {
                LogTab.Instance.Log(Name, e.Data.Replace("\r", "").Replace("\n", ""), -1);
            }
        }

        public bool Exiting = false;

        public void Start()
        {
            Stop();
            var start = new ProcessStartInfo(ClientPath, "-f " + MainWindow.Instance.UserToken.Value + ":" + Id)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardInput = true,
                RedirectStandardError = true,
                StandardErrorEncoding = Encoding.UTF8,
                RedirectStandardOutput = true,
                StandardOutputEncoding = Encoding.UTF8
            };
            try
            {
                BaseProcess = Process.Start(start);
                BaseProcess.EnableRaisingEvents = true;
                BaseProcess.Exited += (s, e) =>
                {
                    Enabled = false;
                };
                BaseProcess.OutputDataReceived += LogOutput;
                BaseProcess.BeginOutputReadLine();
                BaseProcess.ErrorDataReceived += LogOutput;
                BaseProcess.BeginErrorReadLine();
            }
            catch { }
        }

        public void Stop()
        {
            if(BaseProcess == null)
            {
                return;
            }
            Exiting = true;
            ThreadPool.QueueUserWorkItem(s =>
            {
                try
                {
                    if (!BaseProcess.HasExited)
                    {
                        BaseProcess.StandardInput.Write("stop\n");
                        if (!BaseProcess.WaitForExit(3500))
                        {
                            LogTab.Instance.Log("Launcher", "frpc 未响应, 正在强制结束进程", 1);
                            BaseProcess.Kill();
                        }
                    }
                    LogTab.Instance.Log("Launcher", "frpc 已结束", 0);
                    BaseProcess.Dispose();
                }
                catch { }
                finally
                {
                    Exiting = false;
                    BaseProcess = null;
                }
            });
        }
    }
}
