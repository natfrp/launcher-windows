using System.Text;
using System.Diagnostics;

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
                if(_enabled == value)
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
                MainWindow.Instance.Dispatcher.Invoke(() => MainWindow.Instance.Log(Name, e.Data.Replace("\r", "").Replace("\n", "")));
            }
        }

        public void Start()
        {
            Stop();
            var start = new ProcessStartInfo(ClientPath, new StringBuilder()
                .Append("-t ").Append(MainWindow.Instance.UserToken.Value)
                .Append(" -s ").Append(NodeID)
                .Append(" -p ").Append(Id).ToString())
            {
                CreateNoWindow = true,
                UseShellExecute = false,
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
            try
            {
                if(!BaseProcess.HasExited && !BaseProcess.CloseMainWindow())
                {
                    BaseProcess.Kill();
                }
                BaseProcess.Dispose();
            }
            catch { }
            BaseProcess = null;
        }
    }
}
