using FlightSim;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace WinCtrlICP
{
    public partial class CustomDisplayManagerForm : Form
    {
        public List<UserIcpDisplay> UserIcpDisplays { get; private set; }

        public bool IsDirty { get; private set; }

        public CustomDisplayManagerForm(List<UserIcpDisplay> userIcpDisplays)
        {
            UserIcpDisplays = userIcpDisplays;
            InitializeComponent();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            UserIcpDisplays = lstDisplays.Items.OfType<UserIcpDisplay>().ToList();
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (lstDisplays.SelectedItem is UserIcpDisplay item)
            {
                if (MessageBox.Show(this, $"Are you sure you waich to delete '{item.DisplayName}'?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                {
                    lstDisplays.Items.Remove(item);
                    UpdateDisplay();
                    IsDirty = true;
                }
            }
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (lstDisplays.SelectedItem is not UserIcpDisplay display)
            {
                return;
            }
            using var form = new UserIcpDisplayForm(UserIcpDisplays)
            {
                UserIcpDisplay = display
            };
            string displayName = display.DisplayName;
            if (form.ShowDialog(this) == DialogResult.OK)
            {
                if (!displayName.Equals(display.DisplayName))
                {
                    int index = lstDisplays.SelectedIndex;
                    var item = lstDisplays.Items[index];
                    lstDisplays.Items.RemoveAt(index);
                    lstDisplays.Items.Insert(index, item);
                    lstDisplays.SelectedIndex = index;
                }
                UpdateDisplay();
                IsDirty = true;
            }
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            UserIcpDisplayForm form = new UserIcpDisplayForm(UserIcpDisplays);
            if (form.ShowDialog() == DialogResult.OK)
            {
                if (form.UserIcpDisplay != null)
                {
                    lstDisplays.SelectedIndex = lstDisplays.Items.Add(form.UserIcpDisplay);
                    UpdateDisplay();
                    IsDirty = true;
                }
            }
        }

        private void CustomDisplayManagerForm_Load(object sender, EventArgs e)
        {
            LoadDisplays();
        }

        private void UpdateDisplay()
        {
            if (lstDisplays.SelectedItem is not UserIcpDisplay display)
            {
                icpDisplayControl.SetLines(IcpDisplayControl.BlankIcpLines);
            }
            else
            {
                icpDisplayControl.SetLines(display.BuildIcpLines(FlightSimProviders.Preview));
            }
        }

        private void LoadDisplays()
        {
            if (UserIcpDisplays != null)
            {
                lstDisplays.BeginUpdate();
                lstDisplays.Items.Clear();
                foreach (var item in UserIcpDisplays)
                {
                    lstDisplays.Items.Add(item);
                }
                lstDisplays.EndUpdate();
            }
        }

        private void lstDisplays_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnEdit.Enabled = lstDisplays.SelectedIndex != -1;
            btnDelete.Enabled = lstDisplays.SelectedIndex != -1;
            UpdateDisplay();
        }

        private void lstDisplays_DoubleClick(object sender, EventArgs e)
        {
            btnEdit_Click(sender, e);
        }
    }
}
