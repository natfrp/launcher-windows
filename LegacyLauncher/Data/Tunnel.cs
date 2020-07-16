using System.Text;
using System.Diagnostics;
using System.Windows.Forms;

namespace LegacyLauncher.Data
{
    public class Tunnel
    {
        public static string ClientPath = "frpc.exe";

        public bool Enabled
        {
            get => _enabled;
            set
            {
                if (_enabled == value)
                {
                    return;
                }
                _enabled = value;
                if (value)
                {
                    Start();
                }
                else
                {
                    Stop();
                }
                if (DisplayObject != null)
                {
                    Main.Invoke((MethodInvoker)delegate {
                        DisplayObject.Checked = _enabled;
                    });
                }
            }
        }
        private bool _enabled;

        public int Id;
        public int Node;
        public string Name;
        public string Type;

        public ListViewItem DisplayObject = null;

        public Process BaseProcess = null;

        public MainForm Main = null;

        public Tunnel(MainForm main) => Main = main;

        public void LogOutput(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                Main.Invoke((MethodInvoker)delegate {
                    Main.Log(Name, e.Data.Replace("\r", "").Replace("\n", ""));
                });
            }
        }

        public void Start()
        {
            Stop();
            var start = new ProcessStartInfo(ClientPath, new StringBuilder()
                .Append("-t ").Append(Main.UserToken)
                .Append(" -s ").Append(Node)
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
            if (BaseProcess == null)
            {
                return;
            }
            try
            {
                if (!BaseProcess.HasExited && !BaseProcess.CloseMainWindow())
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
