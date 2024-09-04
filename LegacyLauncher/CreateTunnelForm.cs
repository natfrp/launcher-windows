using System;
using System.Drawing;
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
                if (!node.Enabled)
                {
                    node.Name = "--- " + node.Name + " ---";
                    comboBox_node.Items.Add(node);
                }
                else if (NodeFlags.AcceptNewTunnel(node))
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
            if (comboBox_node.SelectedItem is not Node n || !n.Enabled)
            {
                MessageBox.Show("请选择穿透节点", "Oops", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            button_create.Enabled = false;
            button_create.Text = "创建中";
            Model.RequestCreate(n.Id, (close) =>
            {
                if (close)
                {
                    Close();
                    return;
                }
                button_create.Enabled = true;
                button_create.Text = "创建";
            });
        }

        private void comboBox_type_SelectedIndexChanged(object sender, EventArgs e)
        {
            Model.Type = comboBox_type.Text;
        }

        private void button_reload_Click(object sender, EventArgs e) => Model.ReloadListening();

        private void comboBox_node_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            if (e.Index < 0) return;
            var node = comboBox_node.Items[e.Index] as Node;

            var size = e.Graphics.MeasureString(node.DisplayName, comboBox_node.Font, comboBox_node.DropDownWidth - (node.Enabled ? 14 : 4));
            e.ItemHeight = (int)Math.Ceiling(size.Height) + 8; // Vertical padding 4
            e.ItemWidth = comboBox_node.DropDownWidth;

            if (node.Enabled)
            {
                var descSize = e.Graphics.MeasureString(string.IsNullOrWhiteSpace(node.Description) ? "通用穿透节点" : node.Description, comboBox_node.Font, comboBox_node.DropDownWidth);
                e.ItemHeight += (int)Math.Ceiling(descSize.Height) + 2;
            }
            else
            {
                e.ItemHeight += 4;
                if (e.Index != 0)
                {
                    e.ItemHeight += 1; // Separator
                }
            }
        }

        private void comboBox_node_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) return;
            var node = comboBox_node.Items[e.Index] as Node;

            var bounds = new Rectangle(e.Bounds.Location, e.Bounds.Size);
            var isEditItem = (e.State & DrawItemState.ComboBoxEdit) == DrawItemState.ComboBoxEdit;

            // Do not draw focus for disabled (group name) item
            using var background = new SolidBrush(!isEditItem && node.Enabled ? e.BackColor : comboBox_node.BackColor);
            using var foreground = new SolidBrush(!isEditItem && node.Enabled ? e.ForeColor : comboBox_node.ForeColor);
            using var format = new StringFormat { LineAlignment = StringAlignment.Center };

            // Background
            e.Graphics.FillRectangle(background, bounds);

            // For edit items, only draw name
            if (isEditItem)
            {
                bounds.Offset(2, 1);
                e.Graphics.DrawString(node.DisplayName, comboBox_node.Font, foreground, bounds, format);
                return;
            }

            // Group name
            if (!node.Enabled)
            {
                // Upper separator
                if (e.Index != 0)
                {
                    e.Graphics.DrawLine(Pens.Gainsboro, new Point(bounds.Left, bounds.Top), new Point(bounds.Right, bounds.Top));

                    bounds.Offset(0, 1);
                    bounds.Height -= 1;
                }

                // Vertical padding 6, Horizontal padding 2
                bounds.Inflate(-2, -6);
                e.Graphics.DrawString(node.DisplayName, comboBox_node.Font, foreground, bounds, format);
                return;
            }

            // Vertical padding 4, Horizontal padding 2
            bounds.Inflate(-2, -4);

            // Indent 10px (2 padding) for visual hierarchy
            bounds.Offset(12, 0);
            bounds.Width -= 12;

            // Draw title
            var titleHeight = (int)Math.Ceiling(e.Graphics.MeasureString(node.DisplayName, comboBox_node.Font, bounds.Width).Height);
            e.Graphics.DrawString(node.DisplayName, comboBox_node.Font, foreground, new Rectangle(bounds.Left, bounds.Top, bounds.Width, titleHeight), format);

            // Draw description with 2px offset
            titleHeight += 2;
            bounds.Offset(0, titleHeight);
            bounds.Height -= titleHeight;

            using var dim = new SolidBrush(Color.FromArgb(foreground.Color.A / 2, foreground.Color));
            e.Graphics.DrawString(string.IsNullOrWhiteSpace(node.Description) ? "通用穿透节点" : node.Description, comboBox_node.Font, dim, bounds, format);
        }

        private void comboBox_node_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (comboBox_node.SelectedItem is Node n && !n.Enabled)
            {
                // try to select the next enabled item
                var next = comboBox_node.SelectedIndex + 1;
                if (next < comboBox_node.Items.Count)
                {
                    comboBox_node.SelectedIndex = next;
                }
                else
                {
                    comboBox_node.SelectedIndex = -1;
                }
            }
        }
    }
}
