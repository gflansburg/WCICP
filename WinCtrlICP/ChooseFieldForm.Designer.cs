namespace WinCtrlICP
{
    partial class ChooseFieldForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            listFields = new ListBox();
            btnOK = new Button();
            btnCancel = new Button();
            SuspendLayout();
            // 
            // listFields
            // 
            listFields.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            listFields.DisplayMember = "FriendlyName";
            listFields.FormattingEnabled = true;
            listFields.Location = new Point(12, 12);
            listFields.Name = "listFields";
            listFields.Size = new Size(345, 364);
            listFields.TabIndex = 0;
            listFields.ValueMember = "PropertyName";
            listFields.SelectedIndexChanged += listFields_SelectedIndexChanged;
            listFields.DoubleClick += listFields_DoubleClick;
            // 
            // btnOK
            // 
            btnOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnOK.Enabled = false;
            btnOK.Location = new Point(282, 391);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(75, 23);
            btnOK.TabIndex = 1;
            btnOK.Text = "&OK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += btnOK_Click;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.Location = new Point(201, 391);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 23);
            btnCancel.TabIndex = 2;
            btnCancel.Text = "&Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // ChooseFieldForm
            // 
            AcceptButton = btnOK;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(369, 426);
            Controls.Add(btnCancel);
            Controls.Add(btnOK);
            Controls.Add(listFields);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "ChooseFieldForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Choose Field";
            Load += ChooseFieldForm_Load;
            ResumeLayout(false);
        }

        #endregion

        private ListBox listFields;
        private Button btnOK;
        private Button btnCancel;
    }
}