using System;
using System.Linq;
using System.Threading;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using SakuraLibrary.Pipe;
using SakuraLibrary.Proto;
using SakuraLibrary.Helper;

namespace SakuraLibrary.Model
{
    public abstract class LauncherModel : ModelBase
    {
        public readonly PipeClient Pipe = new PipeClient(Consts.PipeName);

        protected Thread PipeThread;

        // TODO: Remove view
        public LauncherModel()
        {
            PipeThread = new Thread(new ThreadStart(PipeWork))
            {
                IsBackground = true
            };
            PipeThread.Start();

            Pipe.ServerPush += Pipe_ServerPush;
            PropertyChanged += (s, e) => Save();
        }

        public abstract void Log(Log l, bool init = false);

        public abstract void Save();
        public abstract void Load();

        public virtual bool Refresh()
        {
            var logs = Pipe.Request(new RequestBase()
            {
                Type = MessageID.LogGet
            });
            Dispatcher.Invoke(() =>
            {
                foreach (var l in logs.DataLog.Data)
                {
                    Log(l, true);
                }
                Nodes.Clear();
                Tunnels.Clear();
            });
            var nodes = Pipe.Request(new RequestBase()
            {
                Type = MessageID.NodeList
            });
            var tunnels = Pipe.Request(new RequestBase()
            {
                Type = MessageID.TunnelList
            });
            if (!nodes.Success || !tunnels.Success)
            {
                return false;
            }
            Dispatcher.Invoke(() =>
            {
                var map = new Dictionary<int, string>();
                foreach (var n in nodes.DataNodes.Nodes)
                {
                    Nodes.Add(new NodeModel(n));
                    map.Add(n.Id, n.Name);
                }
                foreach (var t in tunnels.DataTunnels.Tunnels)
                {
                    Tunnels.Add(new TunnelModel(t, this, map));
                }
            });
            return true;
        }

        #region IPC Handling

        protected void PipeWork()
        {
            while (true)
            {
                lock (Pipe)
                {
                    if (!Pipe.Connected)
                    {
                        Connected = false;
                        if (Pipe.Connect())
                        {
                            Connected = true;
                            var resp = Pipe.Request(new RequestBase()
                            {
                                Type = MessageID.UserInfo
                            });
                            UserInfo = resp.DataUser;
                            Refresh();
                        }
                        continue;
                    }
                }
                Thread.Sleep(500);
            }
        }

        protected void Pipe_ServerPush(PipeConnection connection, int length)
        {
            try
            {
                var msg = PushMessageBase.Parser.ParseFrom(connection.Buffer, 0, length);
                switch (msg.Type)
                {
                case PushMessageID.UpdateUser:
                    UserInfo = msg.DataUser;
                    break;
                case PushMessageID.UpdateTunnel:
                    Dispatcher.Invoke(() =>
                    {
                        foreach (var t in Tunnels)
                        {
                            if (t.Id == msg.DataTunnel.Id)
                            {
                                t.Proto = msg.DataTunnel;
                                t.SetNodeName(Nodes.ToDictionary(k => k.Id, v => v.Name));
                                break;
                            }
                        }
                    });
                    break;
                case PushMessageID.UpdateTunnels:
                    Dispatcher.Invoke(() =>
                    {
                        Tunnels.Clear();
                        var map = Nodes.ToDictionary(k => k.Id, v => v.ToString());
                        foreach (var t in msg.DataTunnels.Tunnels)
                        {
                            Tunnels.Add(new TunnelModel(t, this, map));
                        }
                    });
                    break;
                case PushMessageID.UpdateNodes:
                    Dispatcher.Invoke(() =>
                    {
                        Nodes.Clear();
                        var map = new Dictionary<int, string>();
                        foreach (var n in msg.DataNodes.Nodes)
                        {
                            Nodes.Add(new NodeModel(n));
                            map.Add(n.Id, n.Name);
                        }
                        foreach (var t in Tunnels)
                        {
                            t.SetNodeName(map);
                        }
                    });
                    break;
                case PushMessageID.AppendLog:
                    Dispatcher.Invoke(() =>
                    {
                        foreach (var l in msg.DataLog.Data)
                        {
                            Log(l);
                        }
                    });
                    break;
                }
            }
            catch { }
        }

        #endregion

        /* TODO: lul
        public void TryCheckUpdate(bool silent = false)
        {
            if (!File.Exists("SakuraUpdater.exe"))
            {
                if (!silent)
                {
                    App.ShowMessage("自动更新程序不存在, 无法进行更新检查", "Oops", MessageBoxImage.Error);
                }
                return;
            }
            CheckingUpdate.Value = true;
            App.ApiRequest("get_version", "legacy=false").ContinueWith(t =>
            {
                try
                {
                    var version = t.Result;
                    if (version == null)
                    {
                        return;
                    }
                    var sb = new StringBuilder();
                    bool launcher_update = false, frpc_update = false;
                    if (Assembly.GetExecutingAssembly().GetName().Version.CompareTo(Version.Parse(version["launcher"]["version"] as string)) < 0)
                    {
                        launcher_update = true;
                        sb.Append("启动器最新版: ")
                            .AppendLine(version["launcher"]["version"] as string)
                            .AppendLine("更新日志:")
                            .AppendLine(version["launcher"]["note"] as string)
                            .AppendLine();
                    }

                    var temp = (version["frpc"]["version"] as string).Split(new string[] { "-sakura-" }, StringSplitOptions.None);
                    if (App.FrpcVersion.CompareTo(Version.Parse(temp[0])) < 0 || App.FrpcVersionSakura < float.Parse(temp[1]))
                    {
                        frpc_update = true;
                        sb.Append("frpc 最新版: ")
                            .AppendLine(version["frpc"]["version"] as string)
                            .AppendLine("更新日志:")
                            .AppendLine(version["frpc"]["note"] as string);
                    }

                    if (!launcher_update && !frpc_update)
                    {
                        if (!silent)
                        {
                            App.ShowMessage("您当前使用的启动器和 frpc 均为最新版本", "提示", MessageBoxImage.Information);
                        }
                    }
                    else if (App.ShowMessage(sb.ToString(), "发现新版本, 是否更新", MessageBoxImage.Asterisk, MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {
                        ConfigPath = null;
                        foreach (var l in Tunnels)
                        {
                            if (l.IsReal)
                            {
                                l.Real.Stop();
                            }
                        }
                        Process.Start("SakuraUpdater.exe", (launcher_update ? "-launcher" : "") + (frpc_update ? " -frpc" : ""));
                        Environment.Exit(0);
                    }
                }
                catch (Exception e)
                {
                    if (!silent)
                    {
                        App.ShowMessage("检查更新出错:\n" + e.ToString(), "Oops", MessageBoxImage.Error);
                    }
                }
                finally
                {
                    Dispatcher.Invoke(() => CheckingUpdate.Value = false);
                }
            });
        }
        */

        #region Main Window

        public bool Connected { get => _connected; set => Set(out _connected, value); }
        private bool _connected = false;

        public User UserInfo { get => _userInfo; set => SafeSet(out _userInfo, value); }
        private User _userInfo = new User();

        public int CurrentTab { get => _currentTab; set => Set(out _currentTab, value); }
        private int _currentTab = -1;

        public ObservableCollection<NodeModel> Nodes { get; set; } = new ObservableCollection<NodeModel>();

        #endregion

        #region Tunnel

        public ObservableCollection<TunnelModel> Tunnels { get; set; } = new ObservableCollection<TunnelModel>();

        #endregion

        #region Settings

        [SourceBinding(nameof(UserInfo))]
        public string UserToken { get => UserInfo.Status == User.Types.Status.LoggedIn ? "****************" : _userToken; set => SafeSet(out _userToken, value); }
        private string _userToken = "";

        [SourceBinding(nameof(UserInfo))]
        public bool LoggedIn => UserInfo.Status == User.Types.Status.LoggedIn;

        [SourceBinding(nameof(LoggingIn))]
        public bool TokenEditable => !LoggingIn;

        public bool LoggingIn { get => _loggingIn; set => SafeSet(out _loggingIn, value); }
        private bool _loggingIn;

        public bool SuppressInfo { get => _suppressInfo; set => Set(out _suppressInfo, value); }
        private bool _suppressInfo;

        public bool LogTextWrapping { get => _logTextWrapping; set => Set(out _logTextWrapping, value); }
        private bool _logTextWrapping;

        public void RequestDeleteTunnel(int id, Action<bool, string> callback)
        {
            ThreadPool.QueueUserWorkItem(s =>
            {
                try
                {
                    var resp = Pipe.Request(new RequestBase()
                    {
                        Type = MessageID.TunnelDelete,
                        DataId = id
                    });
                    callback(resp.Success, resp.Message);
                }
                catch (Exception e)
                {
                    callback(false, e.ToString());
                }
            });
        }

        public void RequestLogin(Action<bool, string> callback)
        {
            LoggingIn = true;
            ThreadPool.QueueUserWorkItem(s =>
            {
                try
                {
                    var resp = Pipe.Request(LoggedIn ? new RequestBase()
                    {
                        Type = MessageID.UserLogout
                    } : new RequestBase()
                    {
                        Type = MessageID.UserLogin,
                        DataUserLogin = new UserLogin()
                        {
                            Token = UserToken
                        }
                    });
                    callback(resp.Success, resp.Message);
                    Refresh();
                }
                catch (Exception e)
                {
                    callback(false, e.ToString());
                }
                finally
                {
                    LoggingIn = false;
                }
            });
        }

        #endregion
    }
}
