using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using fastJSON;
using SakuraLibrary.Proto;

using SakuraLauncher.Helper;

namespace SakuraLauncher.Model
{
    public class LauncherModel : ModelBase
    {
        public readonly MainWindow View;

        #region Main Window

        public bool Connected { get => _connected; set => Set(ref _connected, value); }
        private bool _connected;

        public User UserInfo { get => _userInfo; set => Set(ref _userInfo, value); }
        private User _userInfo;

        public int CurrentTab { get => _currentTab; set => Set(ref _currentTab, value); }
        private int _currentTab;

        [SourceBinding(nameof(CurrentTab))]
        public TabIndexTester CurrentTabTester { get; set; }

        public void SwitchTab(int id)
        {
            if (CurrentTab != id)
            {
                CurrentTab = id;
                View.BeginTabStoryboard("TabHideAnimation");
            }
        }

        #endregion

        #region Tunnel Tab Binding

        public ObservableCollection<ITunnelModel> Tunnels { get; set; } = new ObservableCollection<ITunnelModel>();

        #endregion

        #region Settings Tab Binding

        public string UserToken { get; set; }

        public Prop<bool> AutoRun { get; set; } = new Prop<bool>();
        public Prop<bool> SuppressInfo { get; set; } = new Prop<bool>();
        public Prop<bool> LogTextWrapping { get; set; } = new Prop<bool>(true);
        public Prop<bool> BypassProxy { get; set; } = new Prop<bool>(true);

        #endregion

        // WTF ↓
        public ObservableCollection<NodeData> Nodes { get; set; } = new ObservableCollection<NodeData>();

        public LauncherModel(MainWindow view)
        {
            View = view;
            CurrentTabTester = new TabIndexTester(this);

            PropertyChanged += (s, e) => Save();
        }

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

        public void Load()
        {
            var settings = Properties.Settings.Default;

            View.Width = settings.Width;
            View.Height = settings.Height;
            View.SetLogo(settings.LogoIndex);

            if (File.Exists("config.json"))
            {
                var json = JSON.ToObject<Dictionary<string, dynamic>>(File.ReadAllText("config.json"));
                if (json.ContainsKey("logo"))
                {
                    View.SetLogo((int)json["logo"]);
                }
                if (json.ContainsKey("width"))
                {
                    View.Width = json["width"];
                }
                if (json.ContainsKey("height"))
                {
                    View.Height = json["height"];
                }
                if (json.ContainsKey("suppressinfo") && json["suppressinfo"])
                {
                    SuppressInfo.Value = true;
                }
                if (!json.ContainsKey("log_text_wrapping") || json["log_text_wrapping"])
                {
                    LogTextWrapping.Value = true;
                }
                if (!json.ContainsKey("bypass_proxy") || json["bypass_proxy"])
                {
                    BypassProxy.Value = true;
                }
                if (!json.ContainsKey("check_update") || json["check_update"])
                {
                    // CheckUpdate.Value = true;
                }
            }
        }

        public void Save()
        {
            if (!View.CheckAccess())
            {
                View.Dispatcher.Invoke(() => Save());
                return;
            }

            var settings = Properties.Settings.Default;

            settings.Width = (int)View.Width;
            settings.Height = (int)View.Height;

            settings.Save();

            // TODO: Replace with app settings
            /*
            { "suppressinfo", SuppressInfo.Value },
            { "log_text_wrapping", LogTextWrapping.Value },
            { "bypass_proxy", BypassProxy.Value },
            { "check_update", CheckUpdate.Value }
            */
        }
    }
}
