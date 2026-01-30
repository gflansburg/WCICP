namespace WinCtrlICP
{
    partial class LicenseDlg
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
            lblLicense = new Label();
            btnOK = new Button();
            SuspendLayout();
            // 
            // lblLicense
            // 
            lblLicense.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            lblLicense.Location = new Point(12, 9);
            lblLicense.Name = "lblLicense";
            lblLicense.Size = new Size(538, 360);
            lblLicense.TabIndex = 0;
            lblLicense.Text = "License";
            // 
            // btnOK
            // 
            btnOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnOK.Location = new Point(475, 372);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(75, 23);
            btnOK.TabIndex = 1;
            btnOK.Text = "&OK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += btnOK_Click;
            // 
            // LicenseDlg
            // 
            AcceptButton = btnOK;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(562, 407);
            Controls.Add(btnOK);
            Controls.Add(lblLicense);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "LicenseDlg";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "License";
            ResumeLayout(false);
        }

        #endregion

        private Label lblLicense;
        private Button btnOK;
    }
}