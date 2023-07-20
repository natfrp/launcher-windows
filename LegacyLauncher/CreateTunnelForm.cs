using System;
using System.Windows.Forms;

using SakuraLibrary.Model;
using SakuraLibrary.Proto;

namespace LegacyLauncher
{
    public partial class CreateTunnelForm : Form
    {
        public readonly CreateTunnelModel Model;

        public CreateTunnelForm(LauncherModel main)
        {
            InitializeComponent();
            createTunnelModelBindingSource.DataSource = Model = new CreateTunnelModel(main);

            foreach (var node in Model.Nodes)
            {
                if (NodeFlags.AcceptNewTunnel(node))
                {
                    comboBox_node.Items.Add(node);
                }
            }

            Model.PropertyChanged += Model_PropertyChanged;
            Model.ReloadListening();
        }

        private void Model_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
            case nameof(Model.Loading):
                listView_listening.Enabled = button_reload.Enabled = !Model.Loading;
                if (!Model.Loading)
                {
                    listView_listening.BeginUpdate();
                    listView_listening.Items.Clear();
                    foreach (var l in Model.Listening)
                    {
                        listView_listening.Items.Add(new ListViewItem(new string[]
                        {
                            l.Protocol, l.Address, l.Port, l.PID, l.ProcessName
                        }));
                    }
                    listView_listening.EndUpdate();
                }
                break;
            }
        }

        private void listView_listening_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (e.IsSelected)
            {
                var item = e.Item.SubItems;
                Model.Type = item[0].Text;
                Model.LocalPort = int.Parse(item[2].Text);
                Model.LocalAddress = item[1].Text == "0.0.0.0" ? "127.0.0.1" : item[1].Text;
            }
        }

        private void button_create_Click(object sender, EventArgs e)
        {
            if (comboBox_node.SelectedItem is not Node n)
            {
                MessageBox.Show("请选择穿透节点", "Oops", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            button_create.Enabled = false;
            button_create.Text = "创建中";
            Model.RequestCreate(n.Id, (success, message) =>
            {
                if (!success)
                {
                    MessageBox.Show(message, "操作失败", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (MessageBox.Show(message + "\n是否继续创建?", "创建成功", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    Model.Dispatcher.Invoke(() => Close());
                    return;
                }
                Model.Dispatcher.Invoke(() =>
                {
                    button_create.Enabled = true;
                    button_create.Text = "创建";
                });
            });
        }

        private void comboBox_type_SelectedIndexChanged(object sender, EventArgs e)
        {
            Model.Type = comboBox_type.Text;
        }

        private void button_reload_Click(object sender, EventArgs e) => Model.ReloadListening();
    }
}
