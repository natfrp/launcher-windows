using System;
using System.Windows.Forms;

using SakuraLibrary.Model;

namespace LegacyLauncher
{
    public partial class CreateTunnelForm : Form
    {
        public readonly CreateTunnelModel Model;

        public CreateTunnelForm(LauncherModel main)
        {
            InitializeComponent();
            Model = new CreateTunnelModel(main);
            /*
            foreach (var node in main.Nodes)
            {
                if (node.AcceptNew)
                {
                    comboBox_node.Items.Add(node);
                }
            }
            */
            Model.ReloadListening();
        }

        private void listView_listening_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            if (e.IsSelected)
            {
                var item = e.Item.SubItems;
                comboBox_type.Text = item[0].Text;
                textBox_local_ip.Text = item[1].Text == "0.0.0.0" ? "127.0.0.1" : item[1].Text;
                textBox_local_port.Text = item[2].Text;
            }
        }

        private void button_create_Click(object sender, EventArgs e)
        {
            if (!(comboBox_node.SelectedItem is NodeModel n))
            {
                MessageBox.Show("请选择穿透服务器", "Oops", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            button_create.Enabled = false;
            button_create.Text = "创建中";
            // TODO
        }

        private void button_reload_Click(object sender, EventArgs e) => Model.ReloadListening();
    }
}
