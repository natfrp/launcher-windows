using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using LegacyLauncher.Data;

namespace LegacyLauncher
{
    public partial class CreateTunnelForm : Form
    {
        public CreateTunnelForm(MainForm main)
        {
            InitializeComponent();

            foreach (var node in main.Nodes)
            {
                if (node.AcceptNew)
                {
                    comboBox_node.Items.Add(node);
                }
            }

            LoadListeningList();
        }

        public void LoadListeningList()
        {
            button_reload.Enabled = false;
            listView_listening.Items.Clear();
            var process = Process.Start(new ProcessStartInfo("netstat.exe", "-ano")
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true
            });
            process.OutputDataReceived += (s, e) =>
            {
                if (e.Data != null)
                {
                    var tokens = new List<string>(Regex.Split(e.Data.Trim(), "\\s+"));
                    if (tokens[0] == "UDP" && tokens.Count > 3 && tokens[2] == "*:*")
                    {
                        tokens.Insert(3, "LISTENING");
                    }
                    if ((tokens[0] == "TCP" || tokens[0] == "UDP") && tokens.Count > 4 && tokens[3] == "LISTENING")
                    {
                        var pname = "[拒绝访问]";
                        try
                        {
                            pname = Process.GetProcessById(int.Parse(tokens[4])).ProcessName;
                        }
                        catch { }
                        var spliter = tokens[1].LastIndexOf(':');
                        Invoke(new Action(() => listView_listening.Items.Add(new ListViewItem(new string[]
                        {
                            tokens[0],
                            tokens[1].Substring(0, spliter),
                            tokens[1].Substring(spliter + 1),
                            tokens[4],
                            pname
                        }))));
                    }
                }
            };
            process.BeginOutputReadLine();
            ThreadPool.QueueUserWorkItem(s =>
            {
                try
                {
                    process.WaitForExit(3000);
                    process.Kill();
                }
                catch { }
                finally
                {
                    Invoke(new Action(() => button_reload.Enabled = true));
                }
            });
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
            if (!(comboBox_node.SelectedItem is NodeData n))
            {
                MessageBox.Show("请选择穿透服务器", "Oops", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!ushort.TryParse(textBox_local_port.Text, out ushort port))
            {
                MessageBox.Show("本地端口不合法", "Oops", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!ushort.TryParse(textBox_remote.Text, out ushort remote))
            {
                MessageBox.Show("远程端口不合法, 如需随机请填写 0", "Oops", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            button_create.Enabled = false;
            button_create.Text = "创建中";
            var query = new StringBuilder("type=").Append(comboBox_type.Text.ToLower())
                    .Append("&name=").Append(textBox_name.Text)
                    .Append("&node=").Append(n.ID)
                    .Append("&local_ip=").Append(textBox_local_ip.Text)
                    .Append("&local_port=").Append(port)
                    .Append("&encryption=").Append(checkBox_encrypt.Checked ? "true" : "false")
                    .Append("&compression=").Append(checkBox_compress.Checked ? "true" : "false")
                    .Append("&remote_port=").Append(remote).ToString();
            ThreadPool.QueueUserWorkItem(s =>
            {
                try
                {
                    var json = Program.ApiRequest("create_tunnel", query);
                    if (json == null)
                    {
                        return;
                    }
                    Invoke(new Action(() =>
                    {
                        MainForm.Instance.AddTunnel(json["data"]);
                        if (MessageBox.Show("是否继续创建?", "创建成功", MessageBoxButtons.YesNo, MessageBoxIcon.Information) == DialogResult.Yes)
                        {
                            textBox_local_port.Text = "";
                            textBox_remote.Text = "0";
                            textBox_name.Text = "";
                            listView_listening.SelectedIndices.Clear();
                        }
                        else
                        {
                            Close();
                        }
                    }));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    Invoke(new Action(() =>
                    {
                        button_create.Enabled = true;
                        button_create.Text = "创建";
                    }));
                }
            });
        }

        private void button_reload_Click(object sender, EventArgs e) => LoadListeningList();
    }
}
