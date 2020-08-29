namespace LegacyLauncher
{
    partial class MainForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.listView_tunnels = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.launcherModelBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.textBox_token = new System.Windows.Forms.TextBox();
            this.checkBox_autorun = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button_login = new System.Windows.Forms.Button();
            this.button_create = new System.Windows.Forms.Button();
            this.textBox_log = new System.Windows.Forms.TextBox();
            this.notifyIcon_tray = new System.Windows.Forms.NotifyIcon(this.components);
            this.contextMenuStrip_tray = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem_show = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem_exit = new System.Windows.Forms.ToolStripMenuItem();
            this.ToolStripMenuItem_exitAll = new System.Windows.Forms.ToolStripMenuItem();
            this.button_clear = new System.Windows.Forms.Button();
            this.contextMenuStrip_tunnel = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem_delete = new System.Windows.Forms.ToolStripMenuItem();
            this.checkBox_textwrap = new System.Windows.Forms.CheckBox();
            this.checkBox_update = new System.Windows.Forms.CheckBox();
            this.label_update = new System.Windows.Forms.Label();
            ((System.ComponentModel.ISupportInitialize)(this.launcherModelBindingSource)).BeginInit();
            this.contextMenuStrip_tray.SuspendLayout();
            this.contextMenuStrip_tunnel.SuspendLayout();
            this.SuspendLayout();
            // 
            // listView_tunnels
            // 
            this.listView_tunnels.CheckBoxes = true;
            this.listView_tunnels.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4});
            this.listView_tunnels.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.launcherModelBindingSource, "LoggedIn", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.listView_tunnels.Enabled = false;
            this.listView_tunnels.FullRowSelect = true;
            this.listView_tunnels.GridLines = true;
            this.listView_tunnels.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listView_tunnels.HideSelection = false;
            this.listView_tunnels.Location = new System.Drawing.Point(12, 39);
            this.listView_tunnels.MultiSelect = false;
            this.listView_tunnels.Name = "listView_tunnels";
            this.listView_tunnels.Size = new System.Drawing.Size(641, 213);
            this.listView_tunnels.TabIndex = 0;
            this.listView_tunnels.UseCompatibleStateImageBehavior = false;
            this.listView_tunnels.View = System.Windows.Forms.View.Details;
            this.listView_tunnels.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.listView_tunnels_ItemChecked);
            this.listView_tunnels.MouseClick += new System.Windows.Forms.MouseEventHandler(this.listView_tunnels_MouseClick);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "ID";
            this.columnHeader1.Width = 100;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "名称";
            this.columnHeader2.Width = 110;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "节点";
            this.columnHeader3.Width = 190;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "详情";
            this.columnHeader4.Width = 220;
            // 
            // launcherModelBindingSource
            // 
            this.launcherModelBindingSource.DataSource = typeof(SakuraLibrary.Model.LauncherModel);
            // 
            // textBox_token
            // 
            this.textBox_token.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.launcherModelBindingSource, "UserToken", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.textBox_token.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.launcherModelBindingSource, "TokenEditable", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.textBox_token.Location = new System.Drawing.Point(311, 11);
            this.textBox_token.Name = "textBox_token";
            this.textBox_token.Size = new System.Drawing.Size(129, 21);
            this.textBox_token.TabIndex = 1;
            // 
            // checkBox_autorun
            // 
            this.checkBox_autorun.AutoSize = true;
            this.checkBox_autorun.Location = new System.Drawing.Point(12, 14);
            this.checkBox_autorun.Name = "checkBox_autorun";
            this.checkBox_autorun.Size = new System.Drawing.Size(72, 16);
            this.checkBox_autorun.TabIndex = 4;
            this.checkBox_autorun.Text = "开机启动";
            this.checkBox_autorun.UseVisualStyleBackColor = true;
            this.checkBox_autorun.CheckedChanged += new System.EventHandler(this.checkBox_autorun_CheckedChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(246, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 12);
            this.label1.TabIndex = 6;
            this.label1.Text = "访问密钥:";
            // 
            // button_login
            // 
            this.button_login.Location = new System.Drawing.Point(446, 10);
            this.button_login.Name = "button_login";
            this.button_login.Size = new System.Drawing.Size(65, 23);
            this.button_login.TabIndex = 7;
            this.button_login.Text = "登录";
            this.button_login.UseVisualStyleBackColor = true;
            this.button_login.Click += new System.EventHandler(this.button_login_Click);
            // 
            // button_create
            // 
            this.button_create.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.launcherModelBindingSource, "LoggedIn", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.button_create.Enabled = false;
            this.button_create.Location = new System.Drawing.Point(588, 10);
            this.button_create.Name = "button_create";
            this.button_create.Size = new System.Drawing.Size(65, 23);
            this.button_create.TabIndex = 8;
            this.button_create.Text = "新建隧道";
            this.button_create.UseVisualStyleBackColor = true;
            this.button_create.Click += new System.EventHandler(this.button_create_Click);
            // 
            // textBox_log
            // 
            this.textBox_log.BackColor = System.Drawing.Color.Black;
            this.textBox_log.DataBindings.Add(new System.Windows.Forms.Binding("WordWrap", this.launcherModelBindingSource, "LogTextWrapping", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.textBox_log.Font = new System.Drawing.Font("Consolas", 10F);
            this.textBox_log.ForeColor = System.Drawing.Color.Silver;
            this.textBox_log.Location = new System.Drawing.Point(12, 258);
            this.textBox_log.Multiline = true;
            this.textBox_log.Name = "textBox_log";
            this.textBox_log.ReadOnly = true;
            this.textBox_log.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_log.Size = new System.Drawing.Size(641, 253);
            this.textBox_log.TabIndex = 9;
            // 
            // notifyIcon_tray
            // 
            this.notifyIcon_tray.ContextMenuStrip = this.contextMenuStrip_tray;
            this.notifyIcon_tray.Text = "SakuraFrp Launcher";
            this.notifyIcon_tray.Visible = true;
            this.notifyIcon_tray.MouseClick += new System.Windows.Forms.MouseEventHandler(this.notifyIcon_tray_MouseClick);
            // 
            // contextMenuStrip_tray
            // 
            this.contextMenuStrip_tray.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_show,
            this.toolStripMenuItem1,
            this.toolStripMenuItem_exit,
            this.ToolStripMenuItem_exitAll});
            this.contextMenuStrip_tray.Name = "contextMenuStrip1";
            this.contextMenuStrip_tray.Size = new System.Drawing.Size(125, 76);
            // 
            // toolStripMenuItem_show
            // 
            this.toolStripMenuItem_show.Name = "toolStripMenuItem_show";
            this.toolStripMenuItem_show.Size = new System.Drawing.Size(124, 22);
            this.toolStripMenuItem_show.Text = "显示";
            this.toolStripMenuItem_show.Click += new System.EventHandler(this.toolStripMenuItem_show_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(121, 6);
            // 
            // toolStripMenuItem_exit
            // 
            this.toolStripMenuItem_exit.Name = "toolStripMenuItem_exit";
            this.toolStripMenuItem_exit.Size = new System.Drawing.Size(124, 22);
            this.toolStripMenuItem_exit.Text = "退出";
            this.toolStripMenuItem_exit.Click += new System.EventHandler(this.toolStripMenuItem_exit_Click);
            // 
            // ToolStripMenuItem_exitAll
            // 
            this.ToolStripMenuItem_exitAll.Name = "ToolStripMenuItem_exitAll";
            this.ToolStripMenuItem_exitAll.Size = new System.Drawing.Size(124, 22);
            this.ToolStripMenuItem_exitAll.Text = "彻底退出";
            this.ToolStripMenuItem_exitAll.Click += new System.EventHandler(this.ToolStripMenuItem_exitAll_Click);
            // 
            // button_clear
            // 
            this.button_clear.Location = new System.Drawing.Point(517, 10);
            this.button_clear.Name = "button_clear";
            this.button_clear.Size = new System.Drawing.Size(65, 23);
            this.button_clear.TabIndex = 10;
            this.button_clear.Text = "清空日志";
            this.button_clear.UseVisualStyleBackColor = true;
            this.button_clear.Click += new System.EventHandler(this.button_clear_Click);
            // 
            // contextMenuStrip_tunnel
            // 
            this.contextMenuStrip_tunnel.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_delete});
            this.contextMenuStrip_tunnel.Name = "contextMenuStrip_tunnel";
            this.contextMenuStrip_tunnel.Size = new System.Drawing.Size(101, 26);
            // 
            // toolStripMenuItem_delete
            // 
            this.toolStripMenuItem_delete.Name = "toolStripMenuItem_delete";
            this.toolStripMenuItem_delete.Size = new System.Drawing.Size(100, 22);
            this.toolStripMenuItem_delete.Text = "删除";
            this.toolStripMenuItem_delete.Click += new System.EventHandler(this.toolStripMenuItem_delete_Click);
            // 
            // checkBox_textwrap
            // 
            this.checkBox_textwrap.AutoSize = true;
            this.checkBox_textwrap.Checked = true;
            this.checkBox_textwrap.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_textwrap.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.launcherModelBindingSource, "LogTextWrapping", true));
            this.checkBox_textwrap.Location = new System.Drawing.Point(90, 14);
            this.checkBox_textwrap.Name = "checkBox_textwrap";
            this.checkBox_textwrap.Size = new System.Drawing.Size(72, 16);
            this.checkBox_textwrap.TabIndex = 11;
            this.checkBox_textwrap.Text = "日志换行";
            this.checkBox_textwrap.UseVisualStyleBackColor = true;
            this.checkBox_textwrap.CheckedChanged += new System.EventHandler(this.checkBox_textwrap_CheckedChanged);
            // 
            // checkBox_update
            // 
            this.checkBox_update.AutoSize = true;
            this.checkBox_update.Checked = true;
            this.checkBox_update.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_update.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.launcherModelBindingSource, "CheckUpdate", true));
            this.checkBox_update.Location = new System.Drawing.Point(168, 14);
            this.checkBox_update.Name = "checkBox_update";
            this.checkBox_update.Size = new System.Drawing.Size(72, 16);
            this.checkBox_update.TabIndex = 12;
            this.checkBox_update.Text = "检查更新";
            this.checkBox_update.UseVisualStyleBackColor = true;
            this.checkBox_update.CheckedChanged += new System.EventHandler(this.checkBox_update_CheckedChanged);
            // 
            // label_update
            // 
            this.label_update.BackColor = System.Drawing.Color.Teal;
            this.label_update.Cursor = System.Windows.Forms.Cursors.Hand;
            this.label_update.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_update.ForeColor = System.Drawing.Color.White;
            this.label_update.Location = new System.Drawing.Point(0, 492);
            this.label_update.Name = "label_update";
            this.label_update.Size = new System.Drawing.Size(665, 31);
            this.label_update.TabIndex = 13;
            this.label_update.Text = "发现新版本, 点击此处更新";
            this.label_update.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label_update.Visible = false;
            this.label_update.Click += new System.EventHandler(this.label_update_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(665, 523);
            this.Controls.Add(this.label_update);
            this.Controls.Add(this.checkBox_update);
            this.Controls.Add(this.checkBox_textwrap);
            this.Controls.Add(this.button_clear);
            this.Controls.Add(this.textBox_log);
            this.Controls.Add(this.button_create);
            this.Controls.Add(this.button_login);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.checkBox_autorun);
            this.Controls.Add(this.textBox_token);
            this.Controls.Add(this.listView_tunnels);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.Text = "SakuraFrp Launcher";
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.launcherModelBindingSource)).EndInit();
            this.contextMenuStrip_tray.ResumeLayout(false);
            this.contextMenuStrip_tunnel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listView_tunnels;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.TextBox textBox_token;
        private System.Windows.Forms.CheckBox checkBox_autorun;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button_login;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.Button button_create;
        private System.Windows.Forms.NotifyIcon notifyIcon_tray;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip_tray;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_show;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_exit;
        private System.Windows.Forms.Button button_clear;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip_tunnel;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_delete;
        private System.Windows.Forms.CheckBox checkBox_textwrap;
        private System.Windows.Forms.CheckBox checkBox_update;
        private System.Windows.Forms.BindingSource launcherModelBindingSource;
        public System.Windows.Forms.TextBox textBox_log;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_exitAll;
        private System.Windows.Forms.Label label_update;
    }
}

