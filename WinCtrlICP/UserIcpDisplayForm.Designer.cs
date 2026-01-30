namespace WinCtrlICP
{
    partial class UserIcpDisplayForm
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
            overlayHost = new Panel();
            lstFields = new ListBox();
            btnOk = new Button();
            btnCancel = new Button();
            btnDelete = new Button();
            btnAdd = new Button();
            label1 = new Label();
            textName = new TextBox();
            chkInverted = new CheckBox();
            btnBind = new Button();
            lblCaptureStatus = new Label();
            label2 = new Label();
            SuspendLayout();
            // 
            // overlayHost
            // 
            overlayHost.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            overlayHost.BackColor = Color.FromArgb(71, 74, 72);
            overlayHost.Location = new Point(12, 12);
            overlayHost.Name = "overlayHost";
            overlayHost.Size = new Size(427, 128);
            overlayHost.TabIndex = 4;
            overlayHost.Text = "overlayHost";
            overlayHost.LocationChanged += overlayHost_LocationChanged;
            overlayHost.SizeChanged += overlayHost_SizeChanged;
            // 
            // lstFields
            // 
            lstFields.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lstFields.DisplayMember = "ItemFriendlyName";
            lstFields.FormattingEnabled = true;
            lstFields.Location = new Point(12, 209);
            lstFields.Name = "lstFields";
            lstFields.Size = new Size(427, 289);
            lstFields.TabIndex = 5;
            lstFields.SelectedIndexChanged += lstFields_SelectedIndexChanged;
            lstFields.DoubleClick += lstFields_DoubleClick;
            // 
            // btnOk
            // 
            btnOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnOk.Enabled = false;
            btnOk.Location = new Point(364, 538);
            btnOk.Name = "btnOk";
            btnOk.Size = new Size(75, 23);
            btnOk.TabIndex = 6;
            btnOk.Text = "&Save";
            btnOk.UseVisualStyleBackColor = true;
            btnOk.Click += btnOk_Click;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.Location = new Point(283, 538);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 23);
            btnCancel.TabIndex = 7;
            btnCancel.Text = "&Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // btnDelete
            // 
            btnDelete.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnDelete.Enabled = false;
            btnDelete.Location = new Point(202, 538);
            btnDelete.Name = "btnDelete";
            btnDelete.Size = new Size(75, 23);
            btnDelete.TabIndex = 8;
            btnDelete.Text = "&Delete";
            btnDelete.UseVisualStyleBackColor = true;
            btnDelete.Click += btnDelete_Click;
            // 
            // btnAdd
            // 
            btnAdd.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnAdd.Location = new Point(121, 538);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new Size(75, 23);
            btnAdd.TabIndex = 9;
            btnAdd.Text = "&Add";
            btnAdd.UseVisualStyleBackColor = true;
            btnAdd.Click += btnAdd_Click;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(12, 152);
            label1.Name = "label1";
            label1.Size = new Size(42, 15);
            label1.TabIndex = 10;
            label1.Text = "Name:";
            // 
            // textName
            // 
            textName.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            textName.Location = new Point(60, 149);
            textName.Name = "textName";
            textName.Size = new Size(379, 23);
            textName.TabIndex = 11;
            textName.TextChanged += textBox1_TextChanged;
            // 
            // chkInverted
            // 
            chkInverted.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            chkInverted.AutoSize = true;
            chkInverted.Enabled = false;
            chkInverted.Location = new Point(370, 507);
            chkInverted.Name = "chkInverted";
            chkInverted.Size = new Size(69, 19);
            chkInverted.TabIndex = 12;
            chkInverted.Text = "&Inverted";
            chkInverted.UseVisualStyleBackColor = true;
            chkInverted.CheckedChanged += chkInverted_CheckedChanged;
            // 
            // btnBind
            // 
            btnBind.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnBind.Location = new Point(283, 504);
            btnBind.Name = "btnBind";
            btnBind.Size = new Size(75, 23);
            btnBind.TabIndex = 13;
            btnBind.Text = "&Bind";
            btnBind.UseVisualStyleBackColor = true;
            btnBind.Click += btnBind_Click;
            // 
            // lblCaptureStatus
            // 
            lblCaptureStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            lblCaptureStatus.AutoEllipsis = true;
            lblCaptureStatus.Location = new Point(12, 504);
            lblCaptureStatus.Name = "lblCaptureStatus";
            lblCaptureStatus.Size = new Size(265, 23);
            lblCaptureStatus.TabIndex = 14;
            lblCaptureStatus.TextAlign = ContentAlignment.MiddleLeft;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(12, 191);
            label2.Name = "label2";
            label2.Size = new Size(37, 15);
            label2.TabIndex = 15;
            label2.Text = "Fields";
            // 
            // UserIcpDisplayForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(451, 570);
            Controls.Add(label2);
            Controls.Add(lblCaptureStatus);
            Controls.Add(btnBind);
            Controls.Add(chkInverted);
            Controls.Add(textName);
            Controls.Add(label1);
            Controls.Add(btnAdd);
            Controls.Add(btnDelete);
            Controls.Add(btnCancel);
            Controls.Add(btnOk);
            Controls.Add(lstFields);
            Controls.Add(overlayHost);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "UserIcpDisplayForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Custom Display";
            FormClosing += UserIcpDisplayForm_FormClosing;
            Load += UserIcpDisplayForm_Load;
            Shown += UserIcpDisplayForm_Shown;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Panel overlayHost;
        private ListBox lstFields;
        private Button btnOk;
        private Button btnCancel;
        private Button btnDelete;
        private Button btnAdd;
        private Label label1;
        private TextBox textName;
        private CheckBox chkInverted;
        private Button btnBind;
        private Label lblCaptureStatus;
        private Label label2;
    }
}