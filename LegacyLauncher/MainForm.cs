using LegacyLauncher.Model;
using System;
using System.ComponentModel;
using System.Windows.Forms;
using MessageMode = SakuraLibrary.Model.LauncherModel.MessageMode;
using UpdateStatus = SakuraLibrary.Proto.SoftwareUpdate.Types.Status;

namespace LegacyLauncher
{
    public partial class MainForm : Form
    {
        public readonly LauncherViewModel Model;

        public MainForm()
        {
            InitializeComponent();
            launcherModelBindingSource.DataSource = Model = new LauncherViewModel(this);

            Model.PropertyChanged += Model_PropertyChanged;
        }

        private void Model_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
            case nameof(Model.LoggedIn):
                button_login.Text = Model.LoggedIn ? "注销" : "登录";
                break;
            case nameof(Model.UserInfo):
                if (Model.UserInfo.Status == SakuraLibrary.Proto.User.Types.Status.LoggedIn)
                {
                    Text = "SakuraFrp 远程管理配置 - #" + Model.UserInfo.Id + " " + Model.UserInfo.Name;
                }
                else
                {
                    Text = "SakuraFrp 远程管理配置";
                }
                break;
            case nameof(Model.Connected):
                label_unconnected.Visible = !Model.Connected;

                if (Model.Connected)
                {
                    Model.Daemon.DetectMode();

                    if (Model.IsDaemon && MessageBox.Show("当前运行模式不是系统服务，开机自启可能无法正常工作，是否自动修复？", "服务运行模式异常", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning) == DialogResult.OK)
                    {
                        Model.Daemon.SwitchMode();
                    }
                }
                break;
            case nameof(Model.HaveUpdate):
                if (Model.HaveUpdate)
                {
                    label_update.Text = Model.UpdateText;
                    label_update.Visible = true;
                    textBox_log.Height = ClientSize.Height - textBox_log.Top - label_update.Height - 6;
                }
                else
                {
                    label_update.Visible = false;
                    textBox_log.Height = ClientSize.Height - textBox_log.Top - 12;
                }
                break;
            }
        }

        private void label_update_Click(object sender, EventArgs e) => Model.ConfirmUpdate();

        private void button_checkUpdate_Click(object sender, EventArgs e)
        {
            Model.RequestCheckUpdateAsync().ContinueWith(r => Invoke(() =>
            {
                if (r.Exception != null)
                {
                    Model.ShowError(r.Exception);
                }
                else if (r.Result != null)
                {
                    if (r.Result.Status == UpdateStatus.NoUpdate)
                    {
                        Model.ShowMessage("当前没有可用更新", "提示", MessageMode.Info);
                    }
                    else if (r.Result.Status == UpdateStatus.Failed)
                    {
                        Model.ShowMessage("更新检查失败, 请查看日志输出", "错误", MessageMode.Error);
                    }
                }
            }));
        }

        private void button_login_Click(object sender, EventArgs e)
        {
            if (Model.LoggingIn)
            {
                return;
            }
            _ = Model.LoginOrLogout();
        }

        private void button_remoteMgmtPassword_Click(object sender, EventArgs e) => new RemoteConfigForm(Model).ShowDialog();

        private void button_clear_Click(object sender, EventArgs e) => Model.RequestClearLog();
    }
}
