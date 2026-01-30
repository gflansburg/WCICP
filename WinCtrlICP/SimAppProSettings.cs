using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WinCtrlICP
{
    public partial class SimAppProSettings : Form
    {
        public SimAppProSettings()
        {
            InitializeComponent();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            Properties.Settings.Default.SapHost = tbIPAddress.Text;
            Properties.Settings.Default.SapPort = Convert.ToUInt16(numPort.Value);
            Properties.Settings.Default.Save();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void XPlaneSettings_Load(object sender, EventArgs e)
        {
            tbIPAddress.Text = Properties.Settings.Default.SapHost;
            numPort.Value = Properties.Settings.Default.SapPort;
            btnOK.Enabled = !string.IsNullOrEmpty(tbIPAddress.Text) && numPort.Value > 0 && tbIPAddress.Text.IsIPAddress();
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            btnOK.Enabled = !string.IsNullOrEmpty(tbIPAddress.Text) && numPort.Value > 0 && tbIPAddress.Text.IsIPAddress();
        }

        private void numericUpDown1_ValueChanged(object sender, EventArgs e)
        {
            btnOK.Enabled = !string.IsNullOrEmpty(tbIPAddress.Text) && numPort.Value > 0 && tbIPAddress.Text.IsIPAddress();
        }
    }
}
