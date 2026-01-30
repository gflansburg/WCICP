using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace WinCtrlICP
{
    public partial class EditLabelForm : Form
    {
        public string LabelText => txtLabel.Text;

        public const int MaxLength = 25;

        public const string AllowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890↕{}<>[]+-*/=°|↓↑.,!?:;&_'\"%#@ ";

        public EditLabelForm(string initialText)
        {
            InitializeComponent();
            txtLabel.MaxLength = MaxLength;
            txtLabel.Text = initialText ?? string.Empty;
            txtLabel.CharacterCasing = CharacterCasing.Upper;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void txtLabel_TextChanged(object sender, EventArgs e)
        {
            int caret = txtLabel.SelectionStart;
            string filtered = new string(
                txtLabel.Text
                    .ToUpperInvariant()
                    .Where(c => AllowedChars.Contains(c))
                    .Take(MaxLength)
                    .ToArray()
            );
            if (txtLabel.Text != filtered)
            {
                txtLabel.Text = filtered;
                txtLabel.SelectionStart = Math.Min(caret, txtLabel.Text.Length);
            }
        }

        private void txtLabel_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (char.IsControl(e.KeyChar))
            {
                return;
            }
            char c = char.ToUpperInvariant(e.KeyChar);
            if (!AllowedChars.Contains(c))
            {
                e.Handled = true;
                return;
            }
            e.KeyChar = c;
        }

        private void EditLabelForm_Shown(object sender, EventArgs e)
        {
            txtLabel.Focus();
            txtLabel.SelectAll();
        }
    }
}
