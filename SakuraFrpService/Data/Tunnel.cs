using System;
using System.Text;
using System.Diagnostics;
using System.Text.RegularExpressions;

using Newtonsoft.Json.Linq;

using TunnelProto = SakuraLibrary.Proto.Tunnel;
using TunnelStatus = SakuraLibrary.Proto.Tunnel.Types.Status;

using SakuraFrpService.Manager;

namespace SakuraFrpService.Data
{
    public class Tunnel
    {
        public static readonly Regex ReportPattern = new Regex(@"\b(?<Type>\d{2})(?<JSON>.+)$", RegexOptions.Compiled | RegexOptions.Singleline);

        public readonly TunnelManager Manager = null;

        public int Id, Node;
        public string Name, Type, Note, Description;

        public string Tag => "Tunnel/" + Name;

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
                    FailCount = 0;
                    StartState = 0;
                }
                WaitTick = 0; // Update of Enabled property should trigger state check immediately
            }
        }
        private bool _enabled = false;

        public bool Running => BaseProcess != null && !BaseProcess.HasExited;

        /// <summary>
        /// Tunnel start state. Thread Safe.
        /// 0 = Idle, 1 = Pending, 2 = Success
        /// </summary>
        public byte StartState
        {
            get { lock (this) { return _startState; } }
            set { lock (this) { _startState = value; } }
        }
        private byte _startState = 0;

        /// <summary>
        /// Tick delay, hang up ticking when positive. Thread Safe.
        /// </summary>
        public int WaitTick
        {
            get { lock (this) { return _waitTick; } }
            set { lock (this) { _waitTick = value; } }
        }
        private int _waitTick;

        protected int FailCount
        {
            get { lock (this) { return _failCount; } }
            set { lock (this) { _failCount = value; } }
        }
        private int _failCount = 0;

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
                    WorkingDirectory = Manager.FrpcWorkingDirectory,
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardError = true,
                    RedirectStandardOutput = true,
                    StandardErrorEncoding = Encoding.UTF8,
                    StandardOutputEncoding = Encoding.UTF8
                });

                BaseProcess.Exited += OnProcessExit;
                BaseProcess.ErrorDataReceived += OnStderr;
                BaseProcess.OutputDataReceived += OnStdout;
                BaseProcess.EnableRaisingEvents = true;

                BaseProcess.BeginErrorReadLine();
                BaseProcess.BeginOutputReadLine();

                StartState = 1;

                return !BaseProcess.HasExited;
            }
            catch (Exception e)
            {
                Manager.Main.LogManager.Log(LogManager.CATEGORY_SERVICE_WARNING, Tag, "隧道启动失败: " + e.Message);
                Stop();
            }
            return false;
        }

        public void Stop()
        {
            try
            {
                if (!Running)
                {
                    return;
                }
                try
                {
                    // Ignore write errors, to kill the process properly later
                    BaseProcess.StandardInput.Write("stop\n");
                }
                catch { }
                if (!BaseProcess.WaitForExit(3500))
                {
                    Manager.Main.LogManager.Log(LogManager.CATEGORY_SERVICE_WARNING, Tag, "frpc 未响应, 正在强制结束进程");
                    BaseProcess.Kill();
                    if (!BaseProcess.WaitForExit(2000))
                    {
                        Manager.Main.LogManager.Log(LogManager.CATEGORY_SERVICE_ERROR, Tag, "无法结束 frpc 进程");
                    }
                }
            }
            catch (Exception e)
            {
                Manager.Main.LogManager.Log(LogManager.CATEGORY_SERVICE_ERROR, Tag, "隧道关闭出错: " + e.Message);
            }
            finally
            {
                Cleanup();
            }
        }

        public void FailAndCleanup()
        {
            if (++FailCount >= 4)
            {
                Enabled = false;
                FailCount = 0;
                Manager.Main.LogManager.Log(LogManager.CATEGORY_SERVICE_ERROR, Tag, "隧道持续启动失败, 已关闭该隧道");
                Manager.Main.LogManager.Log(LogManager.CATEGORY_NOTICE_ERROR, "隧道 " + Name + " 被关闭", "隧道持续启动失败, 已关闭该隧道");
            }
            else
            {
                WaitTick = 20 * 5 * (int)Math.Pow(2, FailCount - 1);
                Manager.Main.LogManager.Log(LogManager.CATEGORY_SERVICE_ERROR, Tag, "隧道启动失败, 将在 " + (5 * FailCount) + " 秒后重试");
            }
            Cleanup();
        }

        protected void OnProcessExit(object sender, EventArgs e)
        {
            WaitTick = 0;
            Manager.Main.LogManager.Log(LogManager.CATEGORY_SERVICE_INFO, Tag, "frpc 已退出");
            try
            {
                if (BaseProcess != null)
                {
                    BaseProcess.Exited -= OnProcessExit;
                }
            }
            catch { }
        }

        protected void OnStdout(object sender, DataReceivedEventArgs e)
        {
            if (e.Data != null)
            {
                Manager.Main.LogManager.Log(LogManager.CATEGORY_FRPC, Name, e.Data);
            }
        }

        protected void OnStderr(object sender, DataReceivedEventArgs e)
        {
            if (e.Data == null || e.Data.Length == 0)
            {
                return;
            }
            if (e.Data[0] == '\b')
            {
                try
                {
                    var match = ReportPattern.Match(e.Data);
                    if (match != null && match.Success)
                    {
                        HandleReport(int.Parse(match.Groups["Type"].Value), match.Groups["JSON"].Value);
                        return;
                    }
                    throw new Exception("格式不匹配");
                }
                catch (Exception ex)
                {
                    Manager.Main.LogManager.Log(LogManager.CATEGORY_SERVICE_WARNING, Tag, "上报数据解析失败: " + ex.Message);
                }
            }
            Manager.Main.LogManager.Log(LogManager.CATEGORY_FRPC, Name, e.Data);
        }

        protected void HandleReport(int type, string j)
        {
            dynamic json = JToken.Parse(j);
            switch (type)
            {
            case 1: // Launch success
                FailCount = 0;
                StartState = 2;
                WaitTick = 0; // Do state check immediately
                Manager.Main.LogManager.Log(LogManager.CATEGORY_NOTICE_INFO, "隧道 " + Name + " 启动成功", (string)json.message);
                break;
            case 2: // Fatal launch failure
                Enabled = false;
                Manager.Main.LogManager.Log(LogManager.CATEGORY_NOTICE_WARNING, "隧道 " + Name + " 启动失败", (string)json.message);
                break;
            default:
                Manager.Main.LogManager.Log(LogManager.CATEGORY_SERVICE_WARNING, Tag, "未知上报类型: " + type);
                return;
            }
            Manager.PushOne(this);
        }

        protected void Cleanup()
        {
            StartState = 0;
            if (BaseProcess != null)
            {
                BaseProcess.Dispose();
                BaseProcess = null;
            }
        }
    }
}
