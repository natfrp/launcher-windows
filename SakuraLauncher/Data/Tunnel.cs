using System.Text;
using System.Diagnostics;

using SakuraLauncher.Helper;

namespace SakuraLauncher.Data
{
    public class Tunnel : ModelBase, ITunnel
    {
        public static string ClientPath = "frpc.exe";

        public static void LogOutput(object sender, DataReceivedEventArgs e)
        {
            if(e.Data != null)
            {
                MainWindow.Instance.Dispatcher.Invoke(() => MainWindow.Instance.Log(e.Data.Replace("\r", "").Replace("\n", "")));
            }
        }

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

        public string Name { get; set; }
        public string Type { get; set; }
        public string ServerID { get; set; }
        public string ServerName { get; set; }
        public string RemotePort { get; set; }
        public string LocalAddress { get; set; }

        public Process BaseProcess = null;

        public void Start()
        {
            Stop();
            var start = new ProcessStartInfo(ClientPath, new StringBuilder()
                .Append("-t ").Append(MainWindow.Instance.UserToken.Value)
                .Append(" -s ").Append(ServerID)
                .Append(" -p ").Append(Name).ToString())
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
