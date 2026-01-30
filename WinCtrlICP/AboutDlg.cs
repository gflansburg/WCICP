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
    public partial class AboutDlg : Form
    {
        public AboutDlg() : base()
        {
            InitializeComponent();
        }

        private void acceptBtn_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
