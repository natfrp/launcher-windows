using System.Text;
using System.Diagnostics;
using System.Windows.Forms;
using System.Threading;

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
                Main.Log(Name, e.Data.Replace("\r", "").Replace("\n", ""));
            }
        }
        
        public void Start()
        {
            Stop();
            var start = new ProcessStartInfo(ClientPath, "-n -f " + Main.UserToken + ":" + Id)
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
            if (BaseProcess == null)
            {
                return;
            }
            try
            {
                if (!BaseProcess.HasExited)
                {
                    BaseProcess.StandardInput.Write("stop\n");
                    if (!BaseProcess.WaitForExit(3500))
                    {
                        Main.Log("Launcher", "frpc 未响应, 正在强制结束进程");
                        BaseProcess.Kill();
                    }
                }
                Main.Log("Launcher", "frpc 已结束");
                BaseProcess.Dispose();
            }
            catch { }
            finally
            {
                BaseProcess = null;
            }
        }
    }
}
