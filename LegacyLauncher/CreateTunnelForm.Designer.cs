namespace LegacyLauncher
{
    partial class CreateTunnelForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CreateTunnelForm));
            this.listView_listening = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.textBox_local_ip = new System.Windows.Forms.TextBox();
            this.createTunnelModelBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.textBox_local_port = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.button_create = new System.Windows.Forms.Button();
            this.comboBox_node = new System.Windows.Forms.ComboBox();
            this.label6 = new System.Windows.Forms.Label();
            this.textBox_remote = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.comboBox_type = new System.Windows.Forms.ComboBox();
            this.label4 = new System.Windows.Forms.Label();
            this.textBox_name = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.button_reload = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.createTunnelModelBindingSource)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // listView_listening
            // 
            this.listView_listening.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5});
            this.listView_listening.FullRowSelect = true;
            this.listView_listening.GridLines = true;
            this.listView_listening.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.Nonclickable;
            this.listView_listening.HideSelection = false;
            this.listView_listening.Location = new System.Drawing.Point(12, 12);
            this.listView_listening.Name = "listView_listening";
            this.listView_listening.Size = new System.Drawing.Size(441, 270);
            this.listView_listening.TabIndex = 0;
            this.listView_listening.UseCompatibleStateImageBehavior = false;
            this.listView_listening.View = System.Windows.Forms.View.Details;
            this.listView_listening.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.listView_listening_ItemSelectionChanged);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "类型";
            this.columnHeader1.Width = 50;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "监听地址";
            this.columnHeader2.Width = 120;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "监听端口";
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "PID";
            this.columnHeader4.Width = 50;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "进程名";
            this.columnHeader5.Width = 140;
            // 
            // textBox_local_ip
            // 
            this.textBox_local_ip.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.createTunnelModelBindingSource, "LocalAddress", true, System.Windows.Forms.DataSourceUpdateMode.OnPropertyChanged));
            this.textBox_local_ip.Location = new System.Drawing.Point(71, 20);
            this.textBox_local_ip.Name = "textBox_local_ip";
            this.textBox_local_ip.Size = new System.Drawing.Size(136, 21);
            this.textBox_local_ip.TabIndex = 1;
            // 
            // createTunnelModelBindingSource
            // 
            this.createTunnelModelBindingSource.DataSource = typeof(SakuraLibrary.Model.CreateTunnelModel);
            // 
            // textBox_local_port
            // 
            this.textBox_local_port.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.createTunnelModelBindingSource, "LocalPort", true));
            this.textBox_local_port.Location = new System.Drawing.Point(230, 20);
            this.textBox_local_port.Name = "textBox_local_port";
            this.textBox_local_port.Size = new System.Drawing.Size(60, 21);
            this.textBox_local_port.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 23);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 12);
            this.label1.TabIndex = 3;
            this.label1.Text = "本地地址:";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.button_create);
            this.groupBox1.Controls.Add(this.comboBox_node);
            this.groupBox1.Controls.Add(this.label6);
            this.groupBox1.Controls.Add(this.textBox_remote);
            this.groupBox1.Controls.Add(this.label5);
            this.groupBox1.Controls.Add(this.comboBox_type);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Controls.Add(this.textBox_name);
            this.groupBox1.Controls.Add(this.label3);
            this.groupBox1.Controls.Add(this.label2);
            this.groupBox1.Controls.Add(this.textBox_local_ip);
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.textBox_local_port);
            this.groupBox1.Location = new System.Drawing.Point(459, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(296, 160);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "隧道配置";
            // 
            // button_create
            // 
            this.button_create.Location = new System.Drawing.Point(215, 131);
            this.button_create.Name = "button_create";
            this.button_create.Size = new System.Drawing.Size(75, 23);
            this.button_create.TabIndex = 5;
            this.button_create.Text = "创建";
            this.button_create.UseVisualStyleBackColor = true;
            this.button_create.Click += new System.EventHandler(this.button_create_Click);
            // 
            // comboBox_node
            // 
            this.comboBox_node.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_node.FormattingEnabled = true;
            this.comboBox_node.Location = new System.Drawing.Point(59, 105);
            this.comboBox_node.Name = "comboBox_node";
            this.comboBox_node.Size = new System.Drawing.Size(231, 20);
            this.comboBox_node.TabIndex = 12;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(6, 108);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(47, 12);
            this.label6.TabIndex = 11;
            this.label6.Text = "服务器:";
            // 
            // textBox_remote
            // 
            this.textBox_remote.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.createTunnelModelBindingSource, "RemotePort", true));
            this.textBox_remote.Location = new System.Drawing.Point(238, 74);
            this.textBox_remote.Name = "textBox_remote";
            this.textBox_remote.Size = new System.Drawing.Size(52, 21);
            this.textBox_remote.TabIndex = 10;
            this.textBox_remote.Text = "0";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(131, 77);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(101, 12);
            this.label5.TabIndex = 9;
            this.label5.Text = "远程端口(0随机):";
            // 
            // comboBox_type
            // 
            this.comboBox_type.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.createTunnelModelBindingSource, "Type", true));
            this.comboBox_type.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBox_type.FormattingEnabled = true;
            this.comboBox_type.Items.AddRange(new object[] {
            "TCP",
            "UDP"});
            this.comboBox_type.Location = new System.Drawing.Point(71, 74);
            this.comboBox_type.Name = "comboBox_type";
            this.comboBox_type.Size = new System.Drawing.Size(54, 20);
            this.comboBox_type.TabIndex = 8;
            this.comboBox_type.SelectedIndexChanged += new System.EventHandler(this.comboBox_type_SelectedIndexChanged);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(6, 77);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(59, 12);
            this.label4.TabIndex = 7;
            this.label4.Text = "隧道类型:";
            // 
            // textBox_name
            // 
            this.textBox_name.DataBindings.Add(new System.Windows.Forms.Binding("Text", this.createTunnelModelBindingSource, "TunnelName", true));
            this.textBox_name.Location = new System.Drawing.Point(131, 47);
            this.textBox_name.MaxLength = 15;
            this.textBox_name.Name = "textBox_name";
            this.textBox_name.Size = new System.Drawing.Size(159, 21);
            this.textBox_name.TabIndex = 6;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(6, 50);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(119, 12);
            this.label3.TabIndex = 5;
            this.label3.Text = "隧道名称(留空随机):";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(213, 23);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(11, 12);
            this.label2.TabIndex = 4;
            this.label2.Text = ":";
            // 
            // button_reload
            // 
            this.button_reload.Location = new System.Drawing.Point(382, 288);
            this.button_reload.Name = "button_reload";
            this.button_reload.Size = new System.Drawing.Size(71, 23);
            this.button_reload.TabIndex = 14;
            this.button_reload.Text = "刷新";
            this.button_reload.UseVisualStyleBackColor = true;
            this.button_reload.Click += new System.EventHandler(this.button_reload_Click);
            // 
            // CreateTunnelForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(767, 323);
            this.Controls.Add(this.button_reload);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.listView_listening);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "CreateTunnelForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "新建穿透隧道";
            ((System.ComponentModel.ISupportInitialize)(this.createTunnelModelBindingSource)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ListView listView_listening;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.TextBox textBox_local_ip;
        private System.Windows.Forms.TextBox textBox_local_port;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox textBox_name;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.TextBox textBox_remote;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ComboBox comboBox_type;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.ComboBox comboBox_node;
        private System.Windows.Forms.Button button_create;
        private System.Windows.Forms.Button button_reload;
        private System.Windows.Forms.BindingSource createTunnelModelBindingSource;
    }
}