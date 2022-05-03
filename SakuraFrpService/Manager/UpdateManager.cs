using System;
using System.IO;
using System.Net;
using System.Text;
using System.Xml.Linq;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Runtime.InteropServices;

using SakuraLibrary;
using SakuraLibrary.Proto;
using SakuraLibrary.Helper;

namespace SakuraFrpService.Manager
{
    public class UpdateManager : IAsyncManager
    {
        public const string Tag = "Service/UpdateManager";

        public readonly string TempDir = Path.Combine(Path.GetTempPath(), "Sakura-" + Guid.NewGuid());

        public readonly MainService Main;
        public readonly AsyncManager AsyncManager;

        public UpdateStatus Status = new UpdateStatus();

        public int UpdateInterval
        {
            get => _updateInterval;
            set
            {
                _updateInterval = value <= 0 ? -1 : Math.Max(value, 3600);
                if (_updateInterval == -1)
                {
                    lock (this)
                    {
                        LastCheck = DateTime.MinValue;
                        AbortDownload();

                        Status.UpdateAvailable = false;
                        Status.UpdateReadyDir = "";
                        PushStatus();
                    }
                }
            }
        }
        private int _updateInterval;

        private double FrpcSakura = 0;
        private Version FrpcVersion;

        private DateTime LastCheck = DateTime.MinValue;

        private bool UpdateFrpc = false, UpdateLauncher = false;
        private Thread DownloadThread = null;

        public UpdateManager(MainService main)
        {
            Main = main;
            AsyncManager = new AsyncManager(Run);

            UpdateInterval = Properties.Settings.Default.UpdateInterval;
        }

        public void IssueUpdateCheck()
        {
            if (!AsyncManager.Running)
            {
                Main.LogManager.Log(LogManager.CATEGORY_SERVICE_ERROR, Tag, "自动更新功能未启用, 无法检查更新");
                PushStatus();
                return;
            }
            lock (this)
            {
                AbortDownload();
                LastCheck = DateTime.MinValue;
            }
        }

        public bool LoadFrpcVersion()
        {
            try
            {
                using (var p = Process.Start(new ProcessStartInfo(Main.TunnelManager.FrpcPath, "-v")
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    StandardOutputEncoding = Encoding.UTF8
                }))
                {
                    if (!p.WaitForExit(1000))
                    {
                        p.Kill();
                        p.WaitForExit(500);
                    }
                    return TryParseFrpcVersion(p.StandardOutput.ReadLine(), out FrpcVersion, out FrpcSakura);
                }
            }
            catch (Exception e)
            {
                Main.LogManager.Log(LogManager.CATEGORY_SERVICE_ERROR, Tag, e.ToString());
                return false;
            }
        }

        // Actual version number for 0.34.2-sakura-[mainVersion]-[tag(alpha/beta/rc)]-[stateCode] will be
        // [mainVersion] - [tagging(0.03/0.02/0.01)] + [stateCode]
        public bool TryParseFrpcVersion(string data, out Version version, out double sakura)
        {
            sakura = 0;
            var temp = data.Trim().Split('-');
            if (temp.Length == 0 || temp[0].Length == 0)
            {
                version = null;
                return false;
            }
            if (temp[0][0] == 'v')
            {
                temp[0] = temp[0].Substring(1);
            }
            if (!Version.TryParse(temp[0], out version))
            {
                return false;
            }
            if (temp.Length >= 3 && double.TryParse(temp[2], out sakura))
            {
                // For testing versions
                if (temp.Length == 5 && int.TryParse(temp[4], out int status))
                {
                    switch (temp[3])
                    {
                    case "alpha":
                        sakura -= 0.03;
                        break;
                    case "beta":
                        sakura -= 0.02;
                        break;
                    case "rc":
                        sakura -= 0.01;
                        break;
                    }
                    sakura += 0.001 * status;
                }
            }
            return true;
        }

        public string GetTempPath(string file) => Path.Combine(TempDir, file);

        public void AbortDownload()
        {
            lock (this)
            {
                try
                {
                    DownloadThread?.Abort();
                }
                catch { }
                DownloadThread = null;
            }
        }

        public void EmptyTempPath()
        {
            try
            {
                var dir = new DirectoryInfo(TempDir);
                foreach (var f in dir.GetFiles())
                {
                    f.Delete();
                }
                foreach (var d in dir.GetDirectories())
                {
                    d.Delete(true);
                }
            }
            catch { }
        }

        public void PushStatus() => Main.Pipe.PushMessage(new PushMessageBase()
        {
            Type = PushMessageID.PushUpdate,
            DataUpdate = Status
        });

        private async Task<UpdateStatus> CheckUpdate()
        {
            AbortDownload();
            var result = await Natfrp.Request<Natfrp.GetVersion>("get_version");
            lock (this)
            {
                UpdateLauncher = Version.TryParse(result.Launcher.Version, out Version launcher) && launcher > Assembly.GetExecutingAssembly().GetName().Version;
                UpdateFrpc = TryParseFrpcVersion(result.Frpc.Version, out Version frpc, out double sakura) && (frpc > FrpcVersion || (frpc == FrpcVersion && FrpcSakura != 0 && sakura > FrpcSakura));

                var note = new List<string>();
                if (UpdateLauncher)
                {
                    note.Add(string.Format("启动器 v{0}:\n{1}", result.Launcher.Version, result.Launcher.Note));
                }
                if (UpdateFrpc)
                {
                    note.Add(string.Format("frpc v{0}:\n{1}", result.Frpc.Version, result.Frpc.Note));
                }

                Status = new UpdateStatus
                {
                    UpdateAvailable = UpdateFrpc || UpdateLauncher,
                    UpdateReadyDir = "",
                    DownloadCurrent = 0,
                    DownloadTotal = 0,
                    Note = string.Join("\n\n", note),
                    UpdateManagerRunning = true
                };
                if (Status.UpdateAvailable)
                {
                    try
                    {
                        DownloadThread = new Thread(new ThreadStart(DownloadUpdate))
                        {
                            IsBackground = true
                        };
                        DownloadThread.Start();
                    }
                    catch (Exception e)
                    {
                        Main.LogManager.Log(LogManager.CATEGORY_SERVICE_ERROR, "UpdateManager", e.ToString());
                    }
                }

                LastCheck = DateTime.Now;
                PushStatus();

                return Status;
            }
        }

        private void DownloadUpdate()
        {
            try
            {
                Directory.CreateDirectory(TempDir);
                EmptyTempPath();

                var query = new List<string>()
                {
                    "xml"
                };
                lock (this)
                {
                    if (UpdateLauncher)
                    {
                        if (File.Exists("SakuraLauncher.exe"))
                        {
                            query.Add("launcher");
                        }
                        if (File.Exists("LegacyLauncher.exe"))
                        {
                            query.Add("legacy");
                        }
                        if (query.Count == 1)
                        {
                            throw new Exception("启动器文件被损坏, 请卸载并重新安装启动器");
                        }
                    }
                    if (UpdateFrpc)
                    {
                        query.Add("frpc_" + RuntimeInformation.ProcessArchitecture.ToString().ToLower());
                    }
                }

                XDocument xml;
                using (var ms = Natfrp.Request("get_version", string.Join("&", query)).WaitResult())
                {
                    var tmp = ms.ToArray();
                    File.WriteAllBytes(GetTempPath("tasks.xml"), tmp);
                    xml = XDocument.Parse(Encoding.UTF8.GetString(tmp));
                }

                lock (this)
                {
                    Status.DownloadCurrent = Status.DownloadTotal = 0;
                    foreach (var task in xml.Element("tasks").Elements())
                    {
                        Status.DownloadTotal += uint.Parse(task.Attribute("size").Value);
                    }
                }

                Main.LogManager.Log(LogManager.CATEGORY_SERVICE_INFO, Tag, "开始下载更新, 临时目录: " + TempDir);
                foreach (var task in xml.Element("tasks").Elements())
                {
                    int tries = 3;
                    uint previous_progress = Status.DownloadCurrent;
                    while (true)
                    {
                        if (tries-- <= 0)
                        {
                            Main.LogManager.Log(LogManager.CATEGORY_SERVICE_ERROR, Tag, "下载重试次数超过上限, 更新失败");
                        }
                        lock (this)
                        {
                            Status.DownloadCurrent = previous_progress;
                        }

                        var url = task.Attribute("url").Value;
                        var hash = task.Attribute("hash").Value.ToLower();
                        var target = GetTempPath(Path.GetFileName(url));

                        if (File.Exists(target))
                        {
                            if (Utils.Md5(File.ReadAllBytes(target)) == hash)
                            {
                                // Already downloaded
                                lock (this)
                                {
                                    Status.DownloadCurrent += uint.Parse(task.Attribute("size").Value);
                                }
                                break;
                            }
                            File.Delete(target);
                        }

                        Main.LogManager.Log(LogManager.CATEGORY_SERVICE_INFO, Tag, "开始下载: " + url);

                        var request = Natfrp.CreateRequest(task.Attribute("url").Value);
                        using (var fs = File.OpenWrite(target))
                        using (var response = request.GetResponseAsync().WaitResult() as HttpWebResponse)
                        {
                            if (response.StatusCode != HttpStatusCode.OK)
                            {
                                Main.LogManager.Log(LogManager.CATEGORY_SERVICE_ERROR, Tag, "下载失败: HTTP 状态异常, " + response.StatusCode);
                                continue;
                            }
                            using (var ws = response.GetResponseStream())
                            using (var hasher = MD5.Create())
                            {
                                hasher.Initialize();

                                byte[] buffer = new byte[4096];
                                long complete = 0, total = response.ContentLength;

                                while (complete < total)
                                {
                                    int count = ws.Read(buffer, 0, buffer.Length);
                                    fs.Write(buffer, 0, count);
                                    hasher.TransformBlock(buffer, 0, count, null, 0);
                                    lock (this)
                                    {
                                        complete += count;
                                        Status.DownloadCurrent += (uint)count;
                                    }
                                }
                                hasher.TransformFinalBlock(new byte[0], 0, 0);

                                var test = BitConverter.ToString(hasher.Hash).Replace("-", "").ToLower();
                                if (test != hash)
                                {
                                    Main.LogManager.Log(LogManager.CATEGORY_SERVICE_ERROR, Tag, "下载失败: Hash 不匹配 [" + test + "!=" + hash + "]");
                                    continue;
                                }

                                Main.LogManager.Log(LogManager.CATEGORY_SERVICE_INFO, Tag, "下载完成: " + Path.GetFileName(fs.Name) + ", " + Math.Round(complete / 1048576f, 2) + " MiB");
                            }
                        }
                        break;
                    }
                }

                lock (this)
                {
                    Status.UpdateReadyDir = TempDir;
                    PushStatus();
                }
            }
            catch (Exception e)
            {
                EmptyTempPath();
                lock (this)
                {
                    Status.UpdateReadyDir = "";
                    if (e is ThreadAbortException || (e.InnerException != null && e.InnerException is ThreadAbortException))
                    {
                        Main.LogManager.Log(LogManager.CATEGORY_SERVICE_WARNING, Tag, "更新下载被终止");
                    }
                    else
                    {
                        Main.LogManager.Log(LogManager.CATEGORY_SERVICE_ERROR, Tag, "下载失败: " + e.ToString());
                        Status.UpdateAvailable = false;
                    }
                    PushStatus();
                }
            }
        }

        protected void Run()
        {
            do
            {
                try
                {
                    lock (this)
                    {
                        if (Status.UpdateAvailable && Status.UpdateReadyDir == "" && DownloadThread != null && DownloadThread.IsAlive)
                        {
                            PushStatus();
                            continue;
                        }
                        else if (UpdateInterval <= 0 || (DateTime.Now - LastCheck).TotalSeconds <= UpdateInterval)
                        {
                            continue;
                        }
                    }
                    CheckUpdate().WaitResult();
                }
                catch (AggregateException e) when (e.InnerExceptions.Count == 1)
                {
                    Main.LogManager.Log(LogManager.CATEGORY_SERVICE_ERROR, Tag, "更新检查失败, " + e.InnerExceptions[0].ToString());
                }
                catch (Exception e)
                {
                    Main.LogManager.Log(LogManager.CATEGORY_SERVICE_ERROR, Tag, e.ToString());
                }
            }
            while (!AsyncManager.StopEvent.WaitOne(1000));
        }

        #region IAsyncManager

        public bool Running => AsyncManager.Running;

        public void Start()
        {
            if (!LoadFrpcVersion())
            {
                Main.LogManager.Log(LogManager.CATEGORY_SERVICE_WARNING, Tag, ": 无法获取 frpc 版本, 自动更新将不会启用");
            }
            else
            {
                Status.UpdateManagerRunning = true;
                AsyncManager.Start();
            }
        }

        public void Stop(bool kill = false)
        {
            AsyncManager.Stop(kill);
            AbortDownload();
        }

        #endregion
    }
}
