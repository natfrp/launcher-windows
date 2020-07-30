using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Reflection;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections.Generic;

using fastJSON;
using Microsoft.Win32;

using LegacyLauncher.Data;

namespace LegacyLauncher
{
    public partial class MainForm : Form
    {
        public const int CONFIG_VERSION = 1;

        public static MainForm Instance = null;

        public string ConfigPath = null;

        public bool LoggedIn = false, LoggingIn = false, CheckUpdate = true;
        public string UserToken = "";

        public List<string> AutoStart = null;
        public List<Tunnel> Tunnels = new List<Tunnel>();
        public List<NodeData> Nodes = new List<NodeData>();

        public MainForm(bool minimize)
        {
            Instance = this;
            minimized = minimize;

            InitializeComponent();
            notifyIcon_tray.Icon = Icon;
            checkBox_autorun.Checked = File.Exists(Program.AutoRunFile);

            #region Load Config

            if (File.Exists("config.json"))
            {
                var json = JSON.ToObject<Dictionary<string, object>>(File.ReadAllText("config.json"));
                if (json.ContainsKey("token"))
                {
                    textBox_token.Text = UserToken = (string)json["token"];
                }

                if (json.ContainsKey("enable_tunnels") && json["enable_tunnels"] is List<object> enable_tunnels)
                {
                    AutoStart = ((List<object>)json["enable_tunnels"]).Select(t => t.ToString()).ToList();
                }

                checkBox_textwrap.Checked = !json.ContainsKey("log_text_wrapping") || (bool)json["log_text_wrapping"];
                Program.BypassProxy = !json.ContainsKey("bypass_proxy") || (bool)json["bypass_proxy"];
                CheckUpdate = !json.ContainsKey("check_update") || (bool)json["check_update"];

                if (json.ContainsKey("loggedin") && (bool)json["loggedin"])
                {
                    TryLogin();
                }
            }
            checkBox_textwrap_CheckedChanged();

            ConfigPath = "config.json";

            #endregion

            SystemEvents.SessionEnding += (s, e) =>
            {
                ConfigPath = null;
            };
        }

        public void Log(string tunnel, string raw)
        {
            if(InvokeRequired)
            {
                Invoke(new Action(() => Log(tunnel, raw)));
                return;
            }
            var lines = new string[Math.Min(textBox_log.Lines.Length + 1, 400)];
            lines[0] = tunnel + " " + raw;
            Array.Copy(textBox_log.Lines, 0, lines, 1, lines.Length - 1);
            textBox_log.Lines = lines;
        }

        public void Save()
        {
            if (ConfigPath == null)
            {
                return;
            }

            File.WriteAllText(ConfigPath, JSON.ToNiceJSON(new Dictionary<string, object>()
            {
                { "version", CONFIG_VERSION },
                { "token", UserToken.Trim() },
                { "loggedin", LoggedIn },
                { "bypass_proxy", Program.BypassProxy },
                { "check_update", CheckUpdate },
                { "log_text_wrapping", checkBox_textwrap.Checked },
                { "enable_tunnels", Tunnels.Where(t =>t.Enabled).Select(t => t.Name).ToList() }
            }));
        }

        public void TryLogin()
        {
            LoggingIn = true;
            textBox_token.Enabled = button_login.Enabled = false;
            Program.ApiRequest("get_tunnels").ContinueWith(t =>
            {
                var tunnels = t.Result;
                if (tunnels == null)
                {
                    LoggingIn = false;
                    return;
                }
                Program.ApiRequest("get_nodes").ContinueWith(t2 =>
                {
                    var nodes = t2.Result;
                    if (nodes == null)
                    {
                        return;
                    }
                    try
                    {
                        Nodes.Clear();
                        foreach (dynamic j in nodes["data"])
                        {
                            Nodes.Add(new NodeData()
                            {
                                ID = (int)j["id"],
                                Name = (string)j["name"],
                                AcceptNew = (bool)j["accept_new"]
                            });
                        }

                        if (AutoStart == null)
                        {
                            AutoStart = new List<string>();
                            foreach (var tunnel in Tunnels)
                            {
                                if (tunnel.Enabled)
                                {
                                    AutoStart.Add(tunnel.Name);
                                }
                                tunnel.Stop();
                            }
                        }

                        Tunnels.Clear();
                        Invoke(new Action(() =>
                        {
                            listView_tunnels.Items.Clear();
                            foreach (dynamic j in tunnels["data"])
                            {
                                AddTunnel(j);
                            }
                        }));

                        if (AutoStart != null)
                        {
                            foreach (var tunnel in Tunnels)
                            {
                                if (AutoStart.Contains(tunnel.Name))
                                {
                                    tunnel.Enabled = true;
                                }
                            }
                            AutoStart = null;
                        }

                        LoggedIn = true;
                        Invoke(new Action(() =>
                        {
                            button_login.Text = "注销";
                            textBox_token.Enabled = false;
                            listView_tunnels.Enabled = button_create.Enabled = true;
                        }));
                        Save();
                    }
                    catch (Exception e)
                    {
                        Invoke(new Action(() => MessageBox.Show(e.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)));
                    }
                    finally
                    {
                        LoggingIn = false;
                        Invoke(new Action(() =>
                        {
                            button_login.Enabled = true;
                            textBox_token.Enabled = !LoggedIn;
                        }));
                    }
                });
            });
        }

        public void TryCheckUpdate(bool silent = false)
        {
            if (!File.Exists("SakuraUpdater.exe"))
            {
                if (!silent)
                {
                    MessageBox.Show(Program.TopMostForm, "自动更新程序不存在, 无法进行更新检查", "Oops", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                return;
            }
            Program.ApiRequest("get_version", "legacy=true").ContinueWith(t =>
            {
                var version = t.Result;
                if (version == null)
                {
                    return;
                }
                try
                {
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
                    if (Program.FrpcVersion.CompareTo(Version.Parse(temp[0])) < 0 || Program.FrpcVersionSakura < float.Parse(temp[1]))
                    {
                        frpc_update = true;
                        sb.Append("frpc 最新版: ")
                            .AppendLine(version["frpc"]["version"] as string)
                            .AppendLine("更新日志:")
                            .AppendLine(version["frpc"]["note"] as string);
                    }

                    if (!launcher_update && !frpc_update)
                    {
                        MessageBox.Show(Program.TopMostForm, "您当前使用的启动器和 frpc 均为最新版本", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    else if (MessageBox.Show(Program.TopMostForm, sb.ToString(), "发现新版本, 是否更新", MessageBoxButtons.OK, MessageBoxIcon.Asterisk) == DialogResult.OK)
                    {
                        Process.Start("SakuraUpdater.exe", (launcher_update ? "-legacy" : "") + (frpc_update ? " -frpc" : ""));
                        Invoke(new Action(() => Close()));
                    }
                }
                catch (Exception e)
                {
                    MessageBox.Show(Program.TopMostForm, "检查更新出错:\n" + e.ToString(), "Oops", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            });
        }

        public void AddTunnel(dynamic json)
        {
            var name = "未知节点";
            foreach (NodeData node in Nodes)
            {
                if (node.ID == (int)json["node"])
                {
                    name = node.Name;
                    break;
                }
            }

            var t = new Tunnel(this)
            {
                Id = (int)json["id"],
                Name = (string)json["name"],
                Type = ((string)json["type"]).ToUpper(),
                Node = (int)json["node"],
                DisplayObject = new ListViewItem(new string[]
                {
                    json["id"].ToString(),
                    (string)json["name"],
                    "#" + json["node"] + " " + name,
                    (string)json["description"]
                })
            };
            t.DisplayObject.Tag = t;

            Tunnels.Add(t);
            listView_tunnels.Items.Add(t.DisplayObject);
        }

        #region General Events

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if(ConfigPath == null || e.CloseReason != CloseReason.UserClosing)
            {
                return;
            }
            foreach(var t in Tunnels)
            {
                if(t.Enabled)
                {
                    if (MessageBox.Show("确定要退出程序吗?\n退出后所有隧道都会被关闭.", "Confirm", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk) != DialogResult.OK)
                    {
                        e.Cancel = true;
                    }
                    return;
                }
            }
        }

        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            ConfigPath = null;
            foreach (var l in Tunnels)
            {
                l.Stop();
            }
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                WindowState = FormWindowState.Normal;
                Visible = false;
            }
        }

        private void toolStripMenuItem_show_Click(object sender, EventArgs e) => Show();

        private void toolStripMenuItem_exit_Click(object sender, EventArgs e)
        {
            FormClosing -= MainForm_FormClosing;
            Close();
        }

        private void notifyIcon_tray_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Visible = !Visible;
            }
        }

        private void checkBox_update_CheckedChanged(object sender, EventArgs e)
        {
            CheckUpdate = checkBox_update.Checked;
            Save();
        }

        private void checkBox_autorun_CheckedChanged(object sender, EventArgs e) => Program.SetAutoRun(checkBox_autorun.Checked);

        private void checkBox_textwrap_CheckedChanged(object sender = null, EventArgs e = null)
        {
            textBox_log.WordWrap = checkBox_textwrap.Checked;
            textBox_log.ScrollBars = checkBox_textwrap.Checked ? ScrollBars.Vertical : ScrollBars.Both;
            Save();
        }

        private void listView_tunnels_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (e.Item.Tag is Tunnel tunnel)
            {
                tunnel.DisplayObject = null;
                tunnel.Enabled = e.Item.Checked;
                Save();
                tunnel.DisplayObject = e.Item;
            }
        }

        private void listView_tunnels_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right && listView_tunnels.SelectedItems.Count == 1)
            {
                contextMenuStrip_tunnel.Show(listView_tunnels.PointToScreen(e.Location));
            }
        }

        private void toolStripMenuItem_delete_Click(object sender, EventArgs e)
        {
            if (listView_tunnels.SelectedItems.Count == 1)
            {
                var item = listView_tunnels.SelectedItems[0];
                if (item.Tag is Tunnel tunnel)
                {
                    if (MessageBox.Show("是否确定删除隧道 " + tunnel.Name + "?\n该操作不可撤销.", "Confirm", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk) != DialogResult.OK)
                    {
                        return;
                    }
                    listView_tunnels.Enabled = false;
                    ThreadPool.QueueUserWorkItem(s =>
                    {
                        try
                        {
                            var json = Program.ApiRequest("delete_tunnel", "tunnel=" + tunnel.Id);
                            if (json == null)
                            {
                                return;
                            }
                            tunnel.Stop();
                            Tunnels.Remove(tunnel);
                            Invoke(new Action(() => listView_tunnels.Items.Remove(item)));
                        }
                        catch (Exception ex)
                        {
                            Invoke(new Action(() => MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)));
                        }
                        finally
                        {
                            Invoke(new Action(() => listView_tunnels.Enabled = true));
                        }
                    });
                }
            }
        }

        #endregion

        #region Button Events

        private void button_login_Click(object sender, EventArgs e)
        {
            if (LoggingIn)
            {
                return;
            }
            if (LoggedIn)
            {
                foreach (var t in Tunnels)
                {
                    t.Stop();
                }
                Tunnels.Clear();
                listView_tunnels.Items.Clear();
                LoggedIn = false;
                UserToken = "";
                Save();
                button_login.Text = "登录";
                textBox_token.Enabled = true;
                listView_tunnels.Enabled = button_create.Enabled = false;
            }
            else
            {
                UserToken = textBox_token.Text;
                TryLogin();
            }
        }

        private void button_create_Click(object sender, EventArgs e) => new CreateTunnelForm(this).ShowDialog();

        private void button_clear_Click(object sender, EventArgs e) => textBox_log.Text = "";

        #endregion

        #region Start Minimized

        private bool minimized = false;

        protected override void SetVisibleCore(bool value)
        {
            if (minimized)
            {
                minimized = false;
                value = false;
                if (!IsHandleCreated) CreateHandle();
            }
            base.SetVisibleCore(value);
        }

        #endregion
    }
}
