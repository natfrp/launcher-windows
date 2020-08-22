using System;
using System.Windows.Forms;
using System.Collections.Specialized;

using SakuraLibrary.Model;

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
            // listView_tunnels

            notifyIcon_tray.Icon = Icon;

            // TODO: checkBox_autorun.Checked = File.Exists(Program.AutoRunFile);
        }

        public void RefresnTunnels(object s = null, NotifyCollectionChangedEventArgs e = null)
        {
            listView_tunnels.BeginUpdate();
            listView_tunnels.Items.Clear();
            foreach (var t in Model.Tunnels)
            {
                listView_tunnels.Items.Add(new ListViewItem(new string[]
                {
                    t.Id.ToString(), t.Name, "#" + t.Node + " " + t.NodeName, t.Description
                })
                {
                    Tag = t,
                    Checked = t.Enabled
                });
            }
            listView_tunnels.EndUpdate();
        }

        #region General Events

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                WindowState = FormWindowState.Normal;
                Visible = false;
            }
        }

        private void toolStripMenuItem_show_Click(object sender, EventArgs e) => Show();

        private void toolStripMenuItem_exit_Click(object sender, EventArgs e) => Close();

        private void notifyIcon_tray_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                Visible = !Visible;
            }
        }

        private void checkBox_update_CheckedChanged(object sender, EventArgs e)
        {
            // TODO
        }

        private void checkBox_autorun_CheckedChanged(object sender, EventArgs e) => throw new NotImplementedException(); // TODO: Program.SetAutoRun(checkBox_autorun.Checked);

        private void checkBox_textwrap_CheckedChanged(object sender = null, EventArgs e = null)
        {
            textBox_log.WordWrap = checkBox_textwrap.Checked;
            textBox_log.ScrollBars = checkBox_textwrap.Checked ? ScrollBars.Vertical : ScrollBars.Both;
            // TODO: Save();
        }

        private void listView_tunnels_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (e.Item.Tag is TunnelModel t)
            {
                /*
                tunnel.DisplayObject = null;
                tunnel.Enabled = e.Item.Checked;
                // TDOO: Save();
                tunnel.DisplayObject = e.Item;
                */
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
                    // TODO
                }
            }
        }

        #endregion

        #region Button Events

        private void button_login_Click(object sender, EventArgs e)
        {
            textBox_token.Text = "yay";

            // TODO
        }

        private void button_create_Click(object sender, EventArgs e) => new CreateTunnelForm(Model).ShowDialog();

        private void button_clear_Click(object sender, EventArgs e)
        {
            // TODO: IPC
            textBox_log.Text = "";
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

        private void textBox_token_TextChanged(object sender, EventArgs e)
        {
        }
    }
}
