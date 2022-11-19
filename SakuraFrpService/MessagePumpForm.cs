using System;
using System.Windows.Forms;

using Microsoft.Win32;

namespace SakuraFrpService
{
    public partial class MessagePumpForm : Form
    {
        public MainService Service;

        public MessagePumpForm(MainService service)
        {
            if (!service.Daemonize)
            {
                throw new InvalidOperationException();
            }
            Service = service;

            SuspendLayout();
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new System.Drawing.Size(0, 0);
            FormBorderStyle = FormBorderStyle.None;
            Name = "MessagePumpForm";
            Text = "SakuraFrp Service Message Pump";
            ShowInTaskbar = false;
            WindowState = FormWindowState.Minimized;
            Load += new EventHandler(MessagePumpForm_Load);
            FormClosing += new FormClosingEventHandler(MessagePumpForm_FormClosing);
            ResumeLayout(false);
        }

        public new void Close()
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => base.Close()));
            }
            else
            {
                base.Close();
            }
        }

        private void MessagePumpForm_Load(object sender, EventArgs e)
        {
            Hide();
            SystemEvents.SessionEnding += OnSessionEnding;
        }

        private void MessagePumpForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            SystemEvents.SessionEnding -= OnSessionEnding;
        }

        #region Event Handlers

        private void OnSessionEnding(object sender, SessionEndingEventArgs e)
        {
            Service.Stop();
            SystemEvents.SessionEnding -= OnSessionEnding;
        }

        #endregion
    }
}
