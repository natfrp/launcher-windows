using SakuraLibrary.Model;
using System;
using System.Windows.Forms;
using static SakuraLibrary.Model.LauncherModel;

namespace LegacyLauncher
{
    public partial class RemoteConfigForm : Form
    {
        public LauncherModel Model = null;

        public RemoteConfigForm(LauncherModel model)
        {
            InitializeComponent();
            Model = model;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (textBox_password.Text.Length < 8)
            {
                Model.ShowMessage("密码至少需要 8 位", "操作失败", MessageMode.Error);
                return;
            }
            Model.Config.RemoteManagementKey = textBox_password.Text;
            Model.PushServiceConfig(true);
            Close();
        }
    }
}
