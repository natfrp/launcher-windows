using System;
using System.IO;
using System.Windows.Forms;
using System.ComponentModel;
using System.Collections.Specialized;

using SakuraLibrary;
using SakuraLibrary.Model;
using MessageMode = SakuraLibrary.Model.LauncherModel.MessageMode;
using UpdateStatus = SakuraLibrary.Proto.SoftwareUpdate.Types.Status;

using LegacyLauncher.Model;

namespace LegacyLauncher
{
    public partial class MainForm : Form
    {
        public readonly LauncherViewModel Model;

        private string[] frpcLogLevels = ["trace", "debug", "info", "warn", "error"];
        private ToolStripMenuItem[] frpcLogLevelControls;

        public MainForm(bool minimize)
        {
            minimized = minimize;

            InitializeComponent();

            frpcLogLevelControls = [
                toolStripMenuItem_frpcLog_trace,
                toolStripMenuItem_frpcLog_debug,
                toolStripMenuItem_frpcLog_info,
                toolStripMenuItem_frpcLog_wann,
                toolStripMenuItem_frpcLog_error,
            ];
            launcherModelBindingSource.DataSource = Model = new LauncherViewModel(this);

            Model.Tunnels.CollectionChanged += RefresnTunnels;
            Model.PropertyChanged += Model_PropertyChanged;

            notifyIcon_tray.Icon = Icon;

            toolStripMenuItem_notificationMode.Checked = Model.NotificationMode == 0;
            toolStripMenuItem_autoStart.Checked = File.Exists(Utils.GetAutoRunFile(Consts.LegacyLauncherPrefix));
            toolStripMenuItem_runMode.Text = "运行模式: " + Model.WorkingMode;
        }

        public void RefresnTunnels(object s = null, NotifyCollectionChangedEventArgs e = null)
        {
            listView_tunnels.BeginUpdate();
            listView_tunnels.Items.Clear();
            foreach (var t in Model.Tunnels)
            {
                var item = new ListViewItem([
                    t.Id.ToString(), t.Name, "#" + t.Node + " " + t.NodeName, t.Description, t.Note
                ])
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
                    case nameof(TunnelModel.Note):
                        item.SubItems[4].Text = t.Note;
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
            case nameof(Model.UserInfo):
                Text = "SakuraFrp Launcher Classic";
                if (Model.LoggedIn && Model.UserInfo != null)
                {
                    Text += " - #" + Model.UserInfo.Id + " " + Model.UserInfo.Name + " [" + Model.UserInfo.Group?.Name + " / " + Model.UserInfo.Speed + "]";
                }
                break;
            case nameof(Model.NotificationMode):
                toolStripMenuItem_notificationMode.Checked = Model.NotificationMode == 0;
                break;
            case nameof(Model.LogTextWrapping):
                textBox_log.ScrollBars = Model.LogTextWrapping ? ScrollBars.Vertical : ScrollBars.Both;
                toolStripMenuItem_logWrap.Checked = Model.LogTextWrapping;
                break;
            case nameof(Model.Connected):
                label_unconnected.Visible = !Model.Connected;
                toolStripMenuItem_checkUpdate.Enabled = Model.Connected;

                // This won't be updated on model connection change
                toolStripMenuItem_logWrap.Checked = Model.LogTextWrapping;
                break;
            case nameof(Model.HaveUpdate):
                if (Model.HaveUpdate)
                {
                    label_update.Text = Model.UpdateText;
                    label_update.Visible = true;
                    textBox_log.Height = ClientSize.Height - textBox_log.Top - label_update.Height - 6;
                }
                else
                {
                    label_update.Visible = false;
                    textBox_log.Height = ClientSize.Height - textBox_log.Top - 12;
                }
                break;
            case nameof(Model.CheckUpdate):
                toolStripMenuItem_checkUpdate.Checked = Model.CheckUpdate;
                break;
            case nameof(Model.RemoteManagement):
                toolStripMenuItem_remoteMgmtEnable.Checked = Model.RemoteManagement;
                break;
            case nameof(Model.CanEnableRemoteManagement):
                toolStripMenuItem_remoteMgmtEnable.Enabled = Model.CanEnableRemoteManagement;
                break;
            case nameof(Model.SwitchingMode):
                toolStripMenuItem_runMode.Enabled = !Model.SwitchingMode;
                break;
            case nameof(Model.EnableTLS):
                toolStripMenuItem_frpcForceTls.Checked = Model.EnableTLS;
                break;
            case nameof(Model.FrpcLogLevel):
                foreach (var c in frpcLogLevelControls)
                {
                    c.Checked = false;
                }
                var idx = Array.IndexOf(frpcLogLevels, Model.FrpcLogLevel);
                if (idx >= 0)
                {
                    frpcLogLevelControls[idx].Checked = true;
                }
                toolStripMenuItem_frpcLogLevel.Text = "frpc 日志等级 [" + (Model.FrpcLogLevel.Length > 1 ? Model.FrpcLogLevel.Substring(0, 1).ToUpper() + Model.FrpcLogLevel.Substring(1) : Model.FrpcLogLevel) + "]";
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

        private void label_update_Click(object sender, EventArgs e) => Model.ConfirmUpdate();

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

        private void toolStripMenuItem_reload_Click(object sender, EventArgs e)
        {
            listView_tunnels.Enabled = false;
            Model.RequestReloadTunnelsAsync().ContinueWith(_ => Invoke(() => listView_tunnels.Enabled = true));
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
                    Model.RequestDeleteTunnelAsync(tunnel.Id).ContinueWith(_ => Invoke(() => listView_tunnels.Enabled = true));
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
            _ = Model.LoginOrLogout();
        }

        private void button_create_Click(object sender, EventArgs e) => new CreateTunnelForm(Model) { Owner = this }.ShowDialog();

        private void button_clear_Click(object sender, EventArgs e) => Model.RequestClearLog();

        #endregion

        #region Settings

        private DateTime lastClose = DateTime.MinValue;

        private void button_settings_Click(object sender, EventArgs e)
        {
            if (DateTime.Now - lastClose < TimeSpan.FromMilliseconds(100))
            {
                return;
            }
            if (contextMenuStrip_settings.Visible)
            {
                contextMenuStrip_settings.Close();
                return;
            }

            toolStripMenuItem_runMode.Text = "运行模式: " + Model.WorkingMode;

            contextMenuStrip_settings.Show(button_settings, 0, button_settings.Height);
        }

        private void toolStripMenuItem_autoStart_Click(object sender, EventArgs e)
        {
            Utils.SetAutoRun(!toolStripMenuItem_autoStart.Checked, Consts.LegacyLauncherPrefix);
            toolStripMenuItem_autoStart.Checked = File.Exists(Utils.GetAutoRunFile(Consts.LegacyLauncherPrefix));
        }

        private void contextMenuStrip_settings_Closing(object sender, ToolStripDropDownClosingEventArgs e)
        {
            if (e.CloseReason == ToolStripDropDownCloseReason.ItemClicked)
            {
                e.Cancel = true;
                return;
            }
            lastClose = DateTime.Now;
        }

        private void toolStripMenuItem_notificationMode_Click(object sender, EventArgs e)
        {
            Model.NotificationMode = Model.NotificationMode == 1 ? 0 : 1;
            Model.Save();
        }

        private void toolStripMenuItem_logWrap_Click(object sender, EventArgs e)
        {
            Model.LogTextWrapping = !Model.LogTextWrapping;
            Model.Save();
        }

        private void toolStripMenuItem_checkUpdate_Click(object sender, EventArgs e)
        {
            Model.CheckUpdate = !Model.CheckUpdate;
            if (Model.CheckUpdate)
            {
                Model.RequestCheckUpdateAsync().ContinueWith(r => Invoke(() =>
                {
                    if (r.Exception != null)
                    {
                        Model.ShowError(r.Exception);
                    }
                    else if (r.Result != null)
                    {
                        if (r.Result.Status == UpdateStatus.NoUpdate)
                        {
                            Model.ShowMessage("当前没有可用更新", "提示", MessageMode.Info);
                        }
                        else if (r.Result.Status == UpdateStatus.Failed)
                        {
                            Model.ShowMessage("更新检查失败, 请查看日志输出", "错误", MessageMode.Error);
                        }
                    }
                }));
            }
        }

        private void toolStripMenuItem_remoteMgmtEnable_Click(object sender, EventArgs e)
        {
            Model.RemoteManagement = !Model.RemoteManagement;
            toolStripMenuItem_remoteMgmtEnable.Checked = Model.RemoteManagement;
        }

        private void toolStripMenuItem_remoteMgmtPass_Click(object sender, EventArgs e) => new RemoteConfigForm(Model) { Owner = this }.ShowDialog();

        private void toolStripMenuItem_frpcLog_trace_Click(object sender, EventArgs e) => Model.FrpcLogLevel = "trace";

        private void toolStripMenuItem_frpcLog_debug_Click(object sender, EventArgs e) => Model.FrpcLogLevel = "debug";

        private void toolStripMenuItem_frpcLog_info_Click(object sender, EventArgs e) => Model.FrpcLogLevel = "info";

        private void toolStripMenuItem_frpcLog_wann_Click(object sender, EventArgs e) => Model.FrpcLogLevel = "warn";

        private void toolStripMenuItem_frpcLog_error_Click(object sender, EventArgs e) => Model.FrpcLogLevel = "error";

        private void toolStripMenuItem_frpcForceTls_Click(object sender, EventArgs e) => Model.EnableTLS = !Model.EnableTLS;

        private void toolStripMenuItem_runMode_Click(object sender, EventArgs e) => Model.SwitchWorkingMode();

        private void toolStripMenuItem_workDir_Click(object sender, EventArgs e) => Model.RequestOpenCWD();

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
