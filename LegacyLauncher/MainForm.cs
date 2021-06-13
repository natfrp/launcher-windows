using System;
using System.IO;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Specialized;

using SakuraLibrary;
using SakuraLibrary.Model;
using SakuraLibrary.Proto;

using LegacyLauncher.Model;

namespace LegacyLauncher
{
    public partial class MainForm : Form
    {
        public readonly LauncherViewModel Model;

        public MainForm(bool minimize)
        {
            minimized = minimize;

            InitializeComponent();
            launcherModelBindingSource.DataSource = Model = new LauncherViewModel(this);

            Model.Tunnels.CollectionChanged += RefresnTunnels;
            Model.PropertyChanged += Model_PropertyChanged;

            notifyIcon_tray.Icon = Icon;

            checkBox_autorun.Checked = File.Exists(UtilsWindows.GetAutoRunFile(Consts.LegacyLauncherPrefix));
        }

        public void RefresnTunnels(object s = null, NotifyCollectionChangedEventArgs e = null)
        {
            listView_tunnels.BeginUpdate();
            listView_tunnels.Items.Clear();
            foreach (var t in Model.Tunnels)
            {
                var item = new ListViewItem(new string[]
                {
                    t.Id.ToString(), t.Name, "#" + t.Node + " " + t.NodeName, t.Description
                })
                {
                    Checked = t.Enabled
                };
                listView_tunnels.Items.Add(item);
                item.Tag = t;
                t.PropertyChanged -= Tunnel_PropertyChanged;
                t.PropertyChanged += Tunnel_PropertyChanged;
            }
            listView_tunnels.EndUpdate();
        }

        #region Property Events

        private void Tunnel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var t = sender as TunnelModel;
            foreach (ListViewItem item in listView_tunnels.Items)
            {
                if (item.Tag == t)
                {
                    switch (e.PropertyName)
                    {
                    case nameof(TunnelModel.Enabled):
                        item.Tag = null;
                        item.Checked = t.Enabled;
                        item.Tag = t;
                        break;
                    case nameof(TunnelModel.Id):
                        item.SubItems[0].Text = t.Id.ToString();
                        break;
                    case nameof(TunnelModel.Name):
                        item.SubItems[1].Text = t.Name;
                        break;
                    case nameof(TunnelModel.Node):
                    case nameof(TunnelModel.NodeName):
                        item.SubItems[2].Text = "#" + t.Node + " " + t.NodeName;
                        break;
                    case nameof(TunnelModel.Description):
                        item.SubItems[3].Text = t.Description;
                        break;
                    }
                    return;
                }
            }
        }

        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
            case nameof(Model.LoggedIn):
                button_login.Text = Model.LoggedIn ? "注销" : "登录";
                break;
            case nameof(Model.LogTextWrapping):
                textBox_log.ScrollBars = Model.LogTextWrapping ? ScrollBars.Vertical : ScrollBars.Both;
                break;
            case nameof(Model.HaveUpdate):
                if (Model.HaveUpdate)
                {
                    label_update.Visible = true;
                    textBox_log.Height = 231;
                }
                else
                {
                    label_update.Visible = false;
                    textBox_log.Height = 253;
                }
                break;
            }
        }

        #endregion

        #region General Events

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            e.Cancel = true;
            Visible = false;
        }

        private void toolStripMenuItem_show_Click(object sender, EventArgs e) => Show();

        private void toolStripMenuItem_exit_Click(object sender, EventArgs e)
        {
            notifyIcon_tray.Visible = false;
            Environment.Exit(0);
        }

        private void ToolStripMenuItem_exitAll_Click(object sender, EventArgs e)
        {
            notifyIcon_tray.Visible = false;
            Model.Daemon.Stop();
            Environment.Exit(0);
        }

        private void notifyIcon_tray_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Visible = !Visible;
            }
        }

        private void label_update_Click(object sender, EventArgs e) => Model.ConfirmUpdate(true, Model.SimpleFailureHandler, Model.SimpleConfirmHandler, Model.SimpleWarningHandler);

        private void checkBox_update_CheckedChanged(object sender, EventArgs e)
        {
            if (Model.CheckUpdate == checkBox_update.Checked)
            {
                return;
            }
            Model.CheckUpdate = checkBox_update.Checked;
            Model.Save();
        }

        private void checkBox_textwrap_CheckedChanged(object sender, EventArgs e)
        {
            Model.LogTextWrapping = checkBox_textwrap.Checked;
            Model.Save();
        }

        private void checkBox_autorun_CheckedChanged(object sender, EventArgs e) => UtilsWindows.SetAutoRun(checkBox_autorun.Checked, Consts.LegacyLauncherPrefix);

        private void listView_tunnels_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (e.Item.Tag is TunnelModel t)
            {
                t.Enabled = e.Item.Checked;
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
                if (item.Tag is TunnelModel tunnel)
                {
                    if (MessageBox.Show("是否确定删除隧道 " + tunnel.Name + "?\n该操作不可撤销.", "Confirm", MessageBoxButtons.OKCancel, MessageBoxIcon.Asterisk) != DialogResult.OK)
                    {
                        return;
                    }
                    listView_tunnels.Enabled = false;
                    Model.RequestDeleteTunnel(tunnel.Id, (a, b) =>
                    {
                        Model.Dispatcher.Invoke(() => listView_tunnels.Enabled = true);
                        Model.SimpleFailureHandler(a, b);
                    });
                }
            }
        }

        #endregion

        #region Button Events

        private void button_login_Click(object sender, EventArgs e)
        {
            if (Model.LoggingIn)
            {
                return;
            }
            Model.RequestLogin(Model.SimpleFailureHandler);
        }

        private void button_create_Click(object sender, EventArgs e) => new CreateTunnelForm(Model).ShowDialog();

        private void button_clear_Click(object sender, EventArgs e)
        {
            textBox_log.Text = "";
            Model.Pipe.Request(MessageID.LogClear);
        }

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
