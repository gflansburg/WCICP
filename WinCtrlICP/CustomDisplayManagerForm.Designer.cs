namespace WinCtrlICP
{
    partial class CustomDisplayManagerForm
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
            icpDisplayControl = new IcpDisplayControl();
            lstDisplays = new ListBox();
            btnClose = new Button();
            btnDelete = new Button();
            btnEdit = new Button();
            btnNew = new Button();
            label1 = new Label();
            SuspendLayout();
            // 
            // icpDisplayControl
            // 
            icpDisplayControl.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            icpDisplayControl.BackColor = Color.FromArgb(71, 74, 72);
            icpDisplayControl.LineCount = 5;
            icpDisplayControl.Location = new Point(12, 12);
            icpDisplayControl.Name = "icpDisplayControl";
            icpDisplayControl.ShowSelectionBorder = false;
            icpDisplayControl.Size = new Size(427, 128);
            icpDisplayControl.TabIndex = 3;
            icpDisplayControl.Text = "icpDisplayControl";
            // 
            // lstDisplays
            // 
            lstDisplays.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lstDisplays.DisplayMember = "DisplayName";
            lstDisplays.FormattingEnabled = true;
            lstDisplays.Location = new Point(12, 179);
            lstDisplays.Name = "lstDisplays";
            lstDisplays.Size = new Size(427, 349);
            lstDisplays.TabIndex = 4;
            lstDisplays.SelectedIndexChanged += lstDisplays_SelectedIndexChanged;
            lstDisplays.DoubleClick += lstDisplays_DoubleClick;
            // 
            // btnClose
            // 
            btnClose.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnClose.Location = new Point(364, 535);
            btnClose.Name = "btnClose";
            btnClose.Size = new Size(75, 23);
            btnClose.TabIndex = 5;
            btnClose.Text = "&Close";
            btnClose.UseVisualStyleBackColor = true;
            btnClose.Click += btnClose_Click;
            // 
            // btnDelete
            // 
            btnDelete.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnDelete.Enabled = false;
            btnDelete.Location = new Point(283, 535);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(75, 23);
            btnDelete.TabIndex = 6;
            btnDelete.Text = "&Delete";
            btnDelete.UseVisualStyleBackColor = true;
            btnDelete.Click += btnDelete_Click;
            // 
            // btnEdit
            // 
            btnEdit.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnEdit.Enabled = false;
            btnEdit.Location = new Point(202, 535);
            btnEdit.Name = "btnEdit";
            btnEdit.Size = new Size(75, 23);
            btnEdit.TabIndex = 7;
            btnEdit.Text = "&Edit";
            btnEdit.UseVisualStyleBackColor = true;
            btnEdit.Click += btnEdit_Click;
            // 
            // btnNew
            // 
            btnNew.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnNew.Location = new Point(121, 535);
            btnNew.Name = "btnNew";
            btnNew.Size = new Size(75, 23);
            btnNew.TabIndex = 8;
            btnNew.Text = "&New";
            btnNew.UseVisualStyleBackColor = true;
            btnNew.Click += btnNew_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 156);
            label1.Name = "label1";
            label1.Size = new Size(50, 15);
            label1.TabIndex = 9;
            label1.Text = "Displays";
            // 
            // CustomDisplayManagerForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnClose;
            ClientSize = new Size(451, 570);
            Controls.Add(label1);
            Controls.Add(btnNew);
            Controls.Add(btnEdit);
            Controls.Add(btnDelete);
            Controls.Add(btnClose);
            Controls.Add(lstDisplays);
            Controls.Add(icpDisplayControl);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "CustomDisplayManagerForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Custom Display Manager";
            Load += CustomDisplayManagerForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private IcpDisplayControl icpDisplayControl;
        private ListBox lstDisplays;
        private Button btnClose;
        private Button btnDelete;
        private Button btnEdit;
        private Button btnNew;
        private Label label1;
    }
}