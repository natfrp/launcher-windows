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
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.launcherModelBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.textBox_token = new System.Windows.Forms.TextBox();
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
            this.toolStripMenuItem_reload = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_delete = new System.Windows.Forms.ToolStripMenuItem();
            this.label_update = new System.Windows.Forms.Label();
            this.label_unconnected = new System.Windows.Forms.Label();
            this.button_settings = new System.Windows.Forms.Button();
            this.contextMenuStrip_settings = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.toolStripMenuItem_autoStart = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_logWrap = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_checkUpdate = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem_remoteMgmtEnable = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_remoteMgmtPass = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem_frpcLogLevel = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_frpcLog_trace = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_frpcLog_debug = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_frpcLog_info = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_frpcLog_wann = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_frpcLog_error = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_frpcForceTls = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem_runMode = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_workDir = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem_notificationMode = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.launcherModelBindingSource)).BeginInit();
            this.contextMenuStrip_tray.SuspendLayout();
            this.contextMenuStrip_tunnel.SuspendLayout();
            this.contextMenuStrip_settings.SuspendLayout();
            this.SuspendLayout();
            // 
            // listView_tunnels
            // 
            this.listView_tunnels.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView_tunnels.CheckBoxes = true;
            this.listView_tunnels.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5});
            this.listView_tunnels.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.launcherModelBindingSource, "LoggedIn", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.listView_tunnels.Enabled = false;
            this.listView_tunnels.FullRowSelect = true;
            this.listView_tunnels.GridLines = true;
            this.listView_tunnels.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listView_tunnels.HideSelection = false;
            this.listView_tunnels.Location = new System.Drawing.Point(12, 39);
            this.listView_tunnels.MultiSelect = false;
            this.listView_tunnels.Name = "listView_tunnels";
            this.listView_tunnels.Size = new System.Drawing.Size(684, 213);
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
            this.columnHeader3.DisplayIndex = 3;
            this.columnHeader3.Text = "节点";
            this.columnHeader3.Width = 150;
            // 
            // columnHeader4
            // 
            this.columnHeader4.DisplayIndex = 4;
            this.columnHeader4.Text = "详情";
            this.columnHeader4.Width = 190;
            // 
            // columnHeader5
            // 
            this.columnHeader5.DisplayIndex = 2;
            this.columnHeader5.Text = "备注";
            this.columnHeader5.Width = 110;
            // 
            // textBox_token
            // 
            this.textBox_token.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.launcherModelBindingSource, "UserToken", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.textBox_token.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.launcherModelBindingSource, "TokenEditable", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.textBox_token.Location = new System.Drawing.Point(77, 11);
            this.textBox_token.Name = "textBox_token";
            this.textBox_token.Size = new System.Drawing.Size(129, 21);
            this.textBox_token.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 15);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 12);
            this.label1.TabIndex = 6;
            this.label1.Text = "访问密钥:";
            // 
            // button_login
            // 
            this.button_login.Location = new System.Drawing.Point(212, 10);
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
            this.button_create.Location = new System.Drawing.Point(610, 10);
            this.button_create.Name = "button_create";
            this.button_create.Size = new System.Drawing.Size(87, 23);
            this.button_create.TabIndex = 8;
            this.button_create.Text = "新建隧道";
            this.button_create.UseVisualStyleBackColor = true;
            this.button_create.Click += new System.EventHandler(this.button_create_Click);
            // 
            // textBox_log
            // 
            this.textBox_log.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox_log.BackColor = System.Drawing.Color.Black;
            this.textBox_log.DataBindings.Add(new System.Windows.Forms.Binding("WordWrap", this.launcherModelBindingSource, "LogTextWrapping", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.textBox_log.Font = new System.Drawing.Font("Consolas", 10F);
            this.textBox_log.ForeColor = System.Drawing.Color.Silver;
            this.textBox_log.Location = new System.Drawing.Point(12, 258);
            this.textBox_log.Margin = new System.Windows.Forms.Padding(3, 3, 3, 6);
            this.textBox_log.Multiline = true;
            this.textBox_log.Name = "textBox_log";
            this.textBox_log.ReadOnly = true;
            this.textBox_log.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_log.Size = new System.Drawing.Size(684, 250);
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
            this.contextMenuStrip_tray.Size = new System.Drawing.Size(137, 76);
            // 
            // toolStripMenuItem_show
            // 
            this.toolStripMenuItem_show.Name = "toolStripMenuItem_show";
            this.toolStripMenuItem_show.Size = new System.Drawing.Size(136, 22);
            this.toolStripMenuItem_show.Text = "显示";
            this.toolStripMenuItem_show.Click += new System.EventHandler(this.toolStripMenuItem_show_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(133, 6);
            // 
            // toolStripMenuItem_exit
            // 
            this.toolStripMenuItem_exit.Name = "toolStripMenuItem_exit";
            this.toolStripMenuItem_exit.Size = new System.Drawing.Size(136, 22);
            this.toolStripMenuItem_exit.Text = "退出启动器";
            this.toolStripMenuItem_exit.Click += new System.EventHandler(this.toolStripMenuItem_exit_Click);
            // 
            // ToolStripMenuItem_exitAll
            // 
            this.ToolStripMenuItem_exitAll.Name = "ToolStripMenuItem_exitAll";
            this.ToolStripMenuItem_exitAll.Size = new System.Drawing.Size(136, 22);
            this.ToolStripMenuItem_exitAll.Text = "彻底退出";
            this.ToolStripMenuItem_exitAll.Click += new System.EventHandler(this.ToolStripMenuItem_exitAll_Click);
            // 
            // button_clear
            // 
            this.button_clear.Location = new System.Drawing.Point(517, 10);
            this.button_clear.Name = "button_clear";
            this.button_clear.Size = new System.Drawing.Size(87, 23);
            this.button_clear.TabIndex = 10;
            this.button_clear.Text = "清空日志";
            this.button_clear.UseVisualStyleBackColor = true;
            this.button_clear.Click += new System.EventHandler(this.button_clear_Click);
            // 
            // contextMenuStrip_tunnel
            // 
            this.contextMenuStrip_tunnel.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_reload,
            this.toolStripMenuItem_delete});
            this.contextMenuStrip_tunnel.Name = "contextMenuStrip_tunnel";
            this.contextMenuStrip_tunnel.Size = new System.Drawing.Size(101, 48);
            // 
            // toolStripMenuItem_reload
            // 
            this.toolStripMenuItem_reload.Name = "toolStripMenuItem_reload";
            this.toolStripMenuItem_reload.Size = new System.Drawing.Size(100, 22);
            this.toolStripMenuItem_reload.Text = "刷新";
            this.toolStripMenuItem_reload.Click += new System.EventHandler(this.toolStripMenuItem_reload_Click);
            // 
            // toolStripMenuItem_delete
            // 
            this.toolStripMenuItem_delete.Name = "toolStripMenuItem_delete";
            this.toolStripMenuItem_delete.Size = new System.Drawing.Size(100, 22);
            this.toolStripMenuItem_delete.Text = "删除";
            this.toolStripMenuItem_delete.Click += new System.EventHandler(this.toolStripMenuItem_delete_Click);
            // 
            // label_update
            // 
            this.label_update.BackColor = System.Drawing.Color.Teal;
            this.label_update.Cursor = System.Windows.Forms.Cursors.Hand;
            this.label_update.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.launcherModelBindingSource, "UpdateText", true));
            this.label_update.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.label_update.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_update.ForeColor = System.Drawing.Color.White;
            this.label_update.Location = new System.Drawing.Point(0, 492);
            this.label_update.Name = "label_update";
            this.label_update.Size = new System.Drawing.Size(708, 31);
            this.label_update.TabIndex = 13;
            this.label_update.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label_update.Visible = false;
            this.label_update.Click += new System.EventHandler(this.label_update_Click);
            // 
            // label_unconnected
            // 
            this.label_unconnected.BackColor = System.Drawing.Color.OrangeRed;
            this.label_unconnected.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.label_unconnected.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_unconnected.ForeColor = System.Drawing.Color.White;
            this.label_unconnected.Location = new System.Drawing.Point(0, 461);
            this.label_unconnected.Name = "label_unconnected";
            this.label_unconnected.Size = new System.Drawing.Size(708, 31);
            this.label_unconnected.TabIndex = 13;
            this.label_unconnected.Text = "未连接到守护进程, 大部分功能将不可用, 请尝试重启启动器";
            this.label_unconnected.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label_unconnected.Click += new System.EventHandler(this.label_update_Click);
            // 
            // button_settings
            // 
            this.button_settings.ContextMenuStrip = this.contextMenuStrip_settings;
            this.button_settings.Location = new System.Drawing.Point(446, 10);
            this.button_settings.Name = "button_settings";
            this.button_settings.Size = new System.Drawing.Size(65, 23);
            this.button_settings.TabIndex = 14;
            this.button_settings.Text = "设置";
            this.button_settings.UseVisualStyleBackColor = true;
            this.button_settings.Click += new System.EventHandler(this.button_settings_Click);
            // 
            // contextMenuStrip_settings
            // 
            this.contextMenuStrip_settings.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_autoStart,
            this.toolStripMenuItem_logWrap,
            this.toolStripMenuItem_notificationMode,
            this.toolStripMenuItem_checkUpdate,
            this.toolStripSeparator1,
            this.toolStripMenuItem_remoteMgmtEnable,
            this.toolStripMenuItem_remoteMgmtPass,
            this.toolStripSeparator2,
            this.toolStripMenuItem_frpcLogLevel,
            this.toolStripMenuItem_frpcForceTls,
            this.toolStripSeparator3,
            this.toolStripMenuItem_runMode,
            this.toolStripMenuItem_workDir});
            this.contextMenuStrip_settings.Name = "contextMenuStrip_settings";
            this.contextMenuStrip_settings.Size = new System.Drawing.Size(181, 264);
            this.contextMenuStrip_settings.Closing += new System.Windows.Forms.ToolStripDropDownClosingEventHandler(this.contextMenuStrip_settings_Closing);
            // 
            // toolStripMenuItem_autoStart
            // 
            this.toolStripMenuItem_autoStart.Name = "toolStripMenuItem_autoStart";
            this.toolStripMenuItem_autoStart.Size = new System.Drawing.Size(180, 22);
            this.toolStripMenuItem_autoStart.Text = "开机启动";
            this.toolStripMenuItem_autoStart.Click += new System.EventHandler(this.toolStripMenuItem_autoStart_Click);
            // 
            // toolStripMenuItem_logWrap
            // 
            this.toolStripMenuItem_logWrap.Name = "toolStripMenuItem_logWrap";
            this.toolStripMenuItem_logWrap.Size = new System.Drawing.Size(180, 22);
            this.toolStripMenuItem_logWrap.Text = "日志换行";
            this.toolStripMenuItem_logWrap.Click += new System.EventHandler(this.toolStripMenuItem_logWrap_Click);
            // 
            // toolStripMenuItem_checkUpdate
            // 
            this.toolStripMenuItem_checkUpdate.Name = "toolStripMenuItem_checkUpdate";
            this.toolStripMenuItem_checkUpdate.Size = new System.Drawing.Size(180, 22);
            this.toolStripMenuItem_checkUpdate.Text = "检查更新";
            this.toolStripMenuItem_checkUpdate.Click += new System.EventHandler(this.toolStripMenuItem_checkUpdate_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(177, 6);
            // 
            // toolStripMenuItem_remoteMgmtEnable
            // 
            this.toolStripMenuItem_remoteMgmtEnable.Name = "toolStripMenuItem_remoteMgmtEnable";
            this.toolStripMenuItem_remoteMgmtEnable.Size = new System.Drawing.Size(180, 22);
            this.toolStripMenuItem_remoteMgmtEnable.Text = "远程管理";
            this.toolStripMenuItem_remoteMgmtEnable.Click += new System.EventHandler(this.toolStripMenuItem_remoteMgmtEnable_Click);
            // 
            // toolStripMenuItem_remoteMgmtPass
            // 
            this.toolStripMenuItem_remoteMgmtPass.Name = "toolStripMenuItem_remoteMgmtPass";
            this.toolStripMenuItem_remoteMgmtPass.Size = new System.Drawing.Size(180, 22);
            this.toolStripMenuItem_remoteMgmtPass.Text = "远程管理密码";
            this.toolStripMenuItem_remoteMgmtPass.Click += new System.EventHandler(this.toolStripMenuItem_remoteMgmtPass_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(177, 6);
            // 
            // toolStripMenuItem_frpcLogLevel
            // 
            this.toolStripMenuItem_frpcLogLevel.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem_frpcLog_trace,
            this.toolStripMenuItem_frpcLog_debug,
            this.toolStripMenuItem_frpcLog_info,
            this.toolStripMenuItem_frpcLog_wann,
            this.toolStripMenuItem_frpcLog_error});
            this.toolStripMenuItem_frpcLogLevel.Name = "toolStripMenuItem_frpcLogLevel";
            this.toolStripMenuItem_frpcLogLevel.Size = new System.Drawing.Size(180, 22);
            this.toolStripMenuItem_frpcLogLevel.Text = "frpc 日志等级";
            // 
            // toolStripMenuItem_frpcLog_trace
            // 
            this.toolStripMenuItem_frpcLog_trace.Name = "toolStripMenuItem_frpcLog_trace";
            this.toolStripMenuItem_frpcLog_trace.Size = new System.Drawing.Size(151, 22);
            this.toolStripMenuItem_frpcLog_trace.Text = "追踪 [Trace]";
            this.toolStripMenuItem_frpcLog_trace.Click += new System.EventHandler(this.toolStripMenuItem_frpcLog_trace_Click);
            // 
            // toolStripMenuItem_frpcLog_debug
            // 
            this.toolStripMenuItem_frpcLog_debug.Name = "toolStripMenuItem_frpcLog_debug";
            this.toolStripMenuItem_frpcLog_debug.Size = new System.Drawing.Size(151, 22);
            this.toolStripMenuItem_frpcLog_debug.Text = "调试 [Debug]";
            this.toolStripMenuItem_frpcLog_debug.Click += new System.EventHandler(this.toolStripMenuItem_frpcLog_debug_Click);
            // 
            // toolStripMenuItem_frpcLog_info
            // 
            this.toolStripMenuItem_frpcLog_info.Name = "toolStripMenuItem_frpcLog_info";
            this.toolStripMenuItem_frpcLog_info.Size = new System.Drawing.Size(151, 22);
            this.toolStripMenuItem_frpcLog_info.Text = "信息 [Info]";
            this.toolStripMenuItem_frpcLog_info.Click += new System.EventHandler(this.toolStripMenuItem_frpcLog_info_Click);
            // 
            // toolStripMenuItem_frpcLog_wann
            // 
            this.toolStripMenuItem_frpcLog_wann.Name = "toolStripMenuItem_frpcLog_wann";
            this.toolStripMenuItem_frpcLog_wann.Size = new System.Drawing.Size(151, 22);
            this.toolStripMenuItem_frpcLog_wann.Text = "警告 [Warn]";
            this.toolStripMenuItem_frpcLog_wann.Click += new System.EventHandler(this.toolStripMenuItem_frpcLog_wann_Click);
            // 
            // toolStripMenuItem_frpcLog_error
            // 
            this.toolStripMenuItem_frpcLog_error.Name = "toolStripMenuItem_frpcLog_error";
            this.toolStripMenuItem_frpcLog_error.Size = new System.Drawing.Size(151, 22);
            this.toolStripMenuItem_frpcLog_error.Text = "错误 [Error]";
            this.toolStripMenuItem_frpcLog_error.Click += new System.EventHandler(this.toolStripMenuItem_frpcLog_error_Click);
            // 
            // toolStripMenuItem_frpcForceTls
            // 
            this.toolStripMenuItem_frpcForceTls.Name = "toolStripMenuItem_frpcForceTls";
            this.toolStripMenuItem_frpcForceTls.Size = new System.Drawing.Size(180, 22);
            this.toolStripMenuItem_frpcForceTls.Text = "强制 frpc TLS";
            this.toolStripMenuItem_frpcForceTls.Click += new System.EventHandler(this.toolStripMenuItem_frpcForceTls_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(177, 6);
            // 
            // toolStripMenuItem_runMode
            // 
            this.toolStripMenuItem_runMode.Name = "toolStripMenuItem_runMode";
            this.toolStripMenuItem_runMode.Size = new System.Drawing.Size(180, 22);
            this.toolStripMenuItem_runMode.Text = "运行模式: -";
            this.toolStripMenuItem_runMode.Click += new System.EventHandler(this.toolStripMenuItem_runMode_Click);
            // 
            // toolStripMenuItem_workDir
            // 
            this.toolStripMenuItem_workDir.Name = "toolStripMenuItem_workDir";
            this.toolStripMenuItem_workDir.Size = new System.Drawing.Size(180, 22);
            this.toolStripMenuItem_workDir.Text = "工作目录";
            this.toolStripMenuItem_workDir.Click += new System.EventHandler(this.toolStripMenuItem_workDir_Click);
            // 
            // toolStripMenuItem_notificationMode
            // 
            this.toolStripMenuItem_notificationMode.Name = "toolStripMenuItem_notificationMode";
            this.toolStripMenuItem_notificationMode.Size = new System.Drawing.Size(180, 22);
            this.toolStripMenuItem_notificationMode.Text = "状态通知";
            this.toolStripMenuItem_notificationMode.Click += new System.EventHandler(this.toolStripMenuItem_notificationMode_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(708, 523);
            this.Controls.Add(this.button_settings);
            this.Controls.Add(this.label_unconnected);
            this.Controls.Add(this.label_update);
            this.Controls.Add(this.button_clear);
            this.Controls.Add(this.textBox_log);
            this.Controls.Add(this.button_create);
            this.Controls.Add(this.button_login);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox_token);
            this.Controls.Add(this.listView_tunnels);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(724, 562);
            this.Name = "MainForm";
            this.Text = "SakuraFrp Launcher Classic";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            ((System.ComponentModel.ISupportInitialize)(this.launcherModelBindingSource)).EndInit();
            this.contextMenuStrip_tray.ResumeLayout(false);
            this.contextMenuStrip_tunnel.ResumeLayout(false);
            this.contextMenuStrip_settings.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listView_tunnels;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.TextBox textBox_token;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button_login;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.Button button_create;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip_tray;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_show;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_exit;
        private System.Windows.Forms.Button button_clear;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip_tunnel;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_delete;
        private System.Windows.Forms.BindingSource launcherModelBindingSource;
        public System.Windows.Forms.TextBox textBox_log;
        private System.Windows.Forms.ToolStripSeparator toolStripMenuItem1;
        private System.Windows.Forms.ToolStripMenuItem ToolStripMenuItem_exitAll;
        private System.Windows.Forms.Label label_update;
        private System.Windows.Forms.Label label_unconnected;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_reload;
        internal System.Windows.Forms.NotifyIcon notifyIcon_tray;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.Button button_settings;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip_settings;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_autoStart;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_logWrap;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_checkUpdate;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_remoteMgmtEnable;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_remoteMgmtPass;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_runMode;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_frpcForceTls;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_workDir;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_frpcLogLevel;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_frpcLog_trace;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_frpcLog_debug;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_frpcLog_info;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_frpcLog_wann;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_frpcLog_error;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem_notificationMode;
    }
}

