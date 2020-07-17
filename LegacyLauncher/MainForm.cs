using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Collections.Generic;

using fastJSON;

using LegacyLauncher.Data;

namespace LegacyLauncher
{
    public partial class MainForm : Form
    {
        public const int CONFIG_VERSION = 1;

        public static MainForm Instance = null;

        public string ConfigPath = null;

        public bool LoggedIn = false, LoggingIn = false;
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

                if (json.ContainsKey("loggedin") && (bool)json["loggedin"])
                {
                    TryLogin();
                }
            }

            ConfigPath = "config.json";

            #endregion
        }

        public void Log(string tunnel, string raw)
        {
            var lines = new string[Math.Min(textBox_log.Lines.Length + 1, 400)];
            lines[0] = raw;
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
                { "enable_tunnels", Tunnels.Where(t =>t.Enabled).Select(t => t.Name).ToList() }
            }));
        }

        public void TryLogin()
        {
            LoggingIn = true;
            textBox_token.Enabled = button_login.Enabled = false;
            ThreadPool.QueueUserWorkItem(s =>
            {
                try
                {
                    var tunnels = Program.ApiRequest("get_tunnels");
                    if (tunnels == null)
                    {
                        LoggingIn = false;
                        return;
                    }

                    var nodes = Program.ApiRequest("get_nodes");

                    if (nodes == null)
                    {
                        return;
                    }

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
                    MessageBox.Show(e.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    LoggingIn = false;
                    Invoke(new Action(() =>
                    {
                        textBox_token.Enabled = button_login.Enabled = true;
                    }));
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
            if (MessageBox.Show("确定要退出程序吗?\n退出后隧道会被关闭.", "Confirm", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk) != DialogResult.OK)
            {
                e.Cancel = true;
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

        private void checkBox_autorun_CheckedChanged(object sender, EventArgs e) => Program.SetAutoRun(checkBox_autorun.Checked);

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
                            MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
