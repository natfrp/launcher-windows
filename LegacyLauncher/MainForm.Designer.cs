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
            this.textBox_token = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.button_login = new System.Windows.Forms.Button();
            this.textBox_log = new System.Windows.Forms.TextBox();
            this.button_clear = new System.Windows.Forms.Button();
            this.label_update = new System.Windows.Forms.Label();
            this.label_unconnected = new System.Windows.Forms.Label();
            this.checkBox_remoteMgmt = new System.Windows.Forms.CheckBox();
            this.button_remoteMgmtPassword = new System.Windows.Forms.Button();
            this.button_checkUpdate = new System.Windows.Forms.Button();
            this.launcherModelBindingSource = new System.Windows.Forms.BindingSource(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.launcherModelBindingSource)).BeginInit();
            this.SuspendLayout();
            // 
            // textBox_token
            // 
            this.textBox_token.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.launcherModelBindingSource, "UserToken", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.textBox_token.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.launcherModelBindingSource, "TokenEditable", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.textBox_token.Location = new System.Drawing.Point(75, 10);
            this.textBox_token.Name = "textBox_token";
            this.textBox_token.Size = new System.Drawing.Size(129, 21);
            this.textBox_token.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(10, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 12);
            this.label1.TabIndex = 6;
            this.label1.Text = "访问密钥:";
            // 
            // button_login
            // 
            this.button_login.Location = new System.Drawing.Point(210, 9);
            this.button_login.Name = "button_login";
            this.button_login.Size = new System.Drawing.Size(65, 23);
            this.button_login.TabIndex = 7;
            this.button_login.Text = "登录";
            this.button_login.UseVisualStyleBackColor = true;
            this.button_login.Click += new System.EventHandler(this.button_login_Click);
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
            this.textBox_log.Location = new System.Drawing.Point(12, 39);
            this.textBox_log.Margin = new System.Windows.Forms.Padding(3, 3, 3, 6);
            this.textBox_log.Multiline = true;
            this.textBox_log.Name = "textBox_log";
            this.textBox_log.ReadOnly = true;
            this.textBox_log.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBox_log.Size = new System.Drawing.Size(684, 269);
            this.textBox_log.TabIndex = 9;
            // 
            // button_clear
            // 
            this.button_clear.Location = new System.Drawing.Point(626, 9);
            this.button_clear.Name = "button_clear";
            this.button_clear.Size = new System.Drawing.Size(70, 23);
            this.button_clear.TabIndex = 10;
            this.button_clear.Text = "清空日志";
            this.button_clear.UseVisualStyleBackColor = true;
            this.button_clear.Click += new System.EventHandler(this.button_clear_Click);
            // 
            // label_update
            // 
            this.label_update.BackColor = System.Drawing.Color.Teal;
            this.label_update.Cursor = System.Windows.Forms.Cursors.Hand;
            this.label_update.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.launcherModelBindingSource, "UpdateText", true));
            this.label_update.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.label_update.Font = new System.Drawing.Font("Microsoft YaHei UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label_update.ForeColor = System.Drawing.Color.White;
            this.label_update.Location = new System.Drawing.Point(0, 292);
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
            this.label_unconnected.Location = new System.Drawing.Point(0, 261);
            this.label_unconnected.Name = "label_unconnected";
            this.label_unconnected.Size = new System.Drawing.Size(708, 31);
            this.label_unconnected.TabIndex = 13;
            this.label_unconnected.Text = "未连接到守护进程, 大部分功能将不可用, 请尝试重启启动器";
            this.label_unconnected.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.label_unconnected.Click += new System.EventHandler(this.label_update_Click);
            // 
            // checkBox_remoteMgmt
            // 
            this.checkBox_remoteMgmt.AutoSize = true;
            this.checkBox_remoteMgmt.Checked = true;
            this.checkBox_remoteMgmt.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBox_remoteMgmt.DataBindings.Add(new System.Windows.Forms.Binding("Checked", this.launcherModelBindingSource, "RemoteManagement", true));
            this.checkBox_remoteMgmt.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.launcherModelBindingSource, "CanEnableRemoteManagement", true));
            this.checkBox_remoteMgmt.Location = new System.Drawing.Point(306, 13);
            this.checkBox_remoteMgmt.Name = "checkBox_remoteMgmt";
            this.checkBox_remoteMgmt.Size = new System.Drawing.Size(72, 16);
            this.checkBox_remoteMgmt.TabIndex = 12;
            this.checkBox_remoteMgmt.Text = "远程管理";
            this.checkBox_remoteMgmt.UseVisualStyleBackColor = true;
            // 
            // button_remoteMgmtPassword
            // 
            this.button_remoteMgmtPassword.DataBindings.Add(new System.Windows.Forms.Binding("Enabled", this.launcherModelBindingSource, "LoggedIn", true));
            this.button_remoteMgmtPassword.Location = new System.Drawing.Point(384, 9);
            this.button_remoteMgmtPassword.Name = "button_remoteMgmtPassword";
            this.button_remoteMgmtPassword.Size = new System.Drawing.Size(131, 23);
            this.button_remoteMgmtPassword.TabIndex = 7;
            this.button_remoteMgmtPassword.Text = "设置远程管理密码";
            this.button_remoteMgmtPassword.UseVisualStyleBackColor = true;
            this.button_remoteMgmtPassword.Click += new System.EventHandler(this.button_remoteMgmtPassword_Click);
            // 
            // button_checkUpdate
            // 
            this.button_checkUpdate.DataBindings.Add(new System.Windows.Forms.Binding("Visible", this.launcherModelBindingSource, "CheckUpdate", true));
            this.button_checkUpdate.Location = new System.Drawing.Point(550, 9);
            this.button_checkUpdate.Name = "button_checkUpdate";
            this.button_checkUpdate.Size = new System.Drawing.Size(70, 23);
            this.button_checkUpdate.TabIndex = 14;
            this.button_checkUpdate.Text = "检查更新";
            this.button_checkUpdate.UseVisualStyleBackColor = true;
            this.button_checkUpdate.Click += new System.EventHandler(this.button_checkUpdate_Click);
            // 
            // launcherModelBindingSource
            // 
            this.launcherModelBindingSource.DataSource = typeof(SakuraLibrary.Model.LauncherModel);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(708, 323);
            this.Controls.Add(this.button_checkUpdate);
            this.Controls.Add(this.label_unconnected);
            this.Controls.Add(this.label_update);
            this.Controls.Add(this.checkBox_remoteMgmt);
            this.Controls.Add(this.button_clear);
            this.Controls.Add(this.textBox_log);
            this.Controls.Add(this.button_remoteMgmtPassword);
            this.Controls.Add(this.button_login);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.textBox_token);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(724, 362);
            this.Name = "MainForm";
            this.Text = "SakuraFrp 远程管理配置";
            ((System.ComponentModel.ISupportInitialize)(this.launcherModelBindingSource)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox textBox_token;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button_login;
        private System.Windows.Forms.Button button_clear;
        private System.Windows.Forms.BindingSource launcherModelBindingSource;
        public System.Windows.Forms.TextBox textBox_log;
        private System.Windows.Forms.Label label_update;
        private System.Windows.Forms.Label label_unconnected;
        private System.Windows.Forms.CheckBox checkBox_remoteMgmt;
        private System.Windows.Forms.Button button_remoteMgmtPassword;
        private System.Windows.Forms.Button button_checkUpdate;
    }
}

