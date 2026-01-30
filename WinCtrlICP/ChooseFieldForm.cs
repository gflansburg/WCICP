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
    public partial class ChooseFieldForm : Form
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        [Browsable(false)]
        public ProviderFieldListItem? SelectedField { get; private set; } = null;

        public ChooseFieldForm()
        {
            InitializeComponent();
        }

        private void ChooseFieldForm_Load(object sender, EventArgs e)
        {
            Dictionary<string, FlightSim.FlightSimFieldAttribute> fields = FlightSimFieldCatalog.GetFields();
            listFields.BeginUpdate();
            listFields.Items.Clear();

            // First: Custom Label entry
            listFields.Items.Add(new ProviderFieldListItem
            {
                PropertyName = string.Empty,   // empty = label
                FriendlyName = "Custom Label"
            });

            // Then: provider-bound fields
            foreach (var kvp in fields.OrderBy(x => x.Value.FriendlyName))
            {
                listFields.Items.Add(new ProviderFieldListItem
                {
                    PropertyName = kvp.Key,
                    FriendlyName = kvp.Value.FriendlyName
                });
            }

            listFields.EndUpdate();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            SelectedField = listFields.SelectedItem as ProviderFieldListItem;
            DialogResult = DialogResult.OK;
            Close();
        }

        private void listFields_SelectedIndexChanged(object sender, EventArgs e)
        {
            btnOK.Enabled = listFields.SelectedIndex != -1;
        }

        private void listFields_DoubleClick(object sender, EventArgs e)
        {
            if (listFields.SelectedIndex != -1)
            {
                btnOK_Click(sender, e);
            }
        }
    }
}
