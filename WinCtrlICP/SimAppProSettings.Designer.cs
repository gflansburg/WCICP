namespace WinCtrlICP
{
    partial class SimAppProSettings
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
            label1 = new Label();
            tbIPAddress = new TextBox();
            label2 = new Label();
            numPort = new NumericUpDown();
            btnOK = new Button();
            btnCancel = new Button();
            ((System.ComponentModel.ISupportInitialize)numPort).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Location = new Point(16, 18);
            label1.Margin = new Padding(4, 0, 4, 0);
            label1.Name = "label1";
            label1.Size = new Size(65, 15);
            label1.TabIndex = 0;
            label1.Text = "IP Address:";
            // 
            // tbIPAddress
            // 
            tbIPAddress.Location = new Point(94, 15);
            tbIPAddress.Margin = new Padding(4, 3, 4, 3);
            tbIPAddress.Name = "tbIPAddress";
            tbIPAddress.Size = new Size(245, 23);
            tbIPAddress.TabIndex = 1;
            tbIPAddress.TextChanged += textBox1_TextChanged;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Location = new Point(54, 48);
            label2.Margin = new Padding(4, 0, 4, 0);
            label2.Name = "label2";
            label2.Size = new Size(32, 15);
            label2.TabIndex = 2;
            label2.Text = "Port:";
            // 
            // numPort
            // 
            numPort.Location = new Point(94, 46);
            numPort.Margin = new Padding(4, 3, 4, 3);
            numPort.Maximum = new decimal(new int[] { 65535, 0, 0, 0 });
            numPort.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numPort.Name = "numPort";
            numPort.Size = new Size(140, 23);
            numPort.TabIndex = 3;
            numPort.Value = new decimal(new int[] { 1, 0, 0, 0 });
            numPort.ValueChanged += numericUpDown1_ValueChanged;
            // 
            // btnOK
            // 
            btnOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnOK.Location = new Point(253, 80);
            btnOK.Margin = new Padding(4, 3, 4, 3);
            btnOK.Name = "btnOK";
            btnOK.Size = new Size(88, 27);
            btnOK.TabIndex = 4;
            btnOK.Text = "&OK";
            btnOK.UseVisualStyleBackColor = true;
            btnOK.Click += btnOK_Click;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnCancel.DialogResult = DialogResult.Cancel;
            btnCancel.Location = new Point(159, 80);
            btnCancel.Margin = new Padding(4, 3, 4, 3);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(88, 27);
            btnCancel.TabIndex = 5;
            btnCancel.Text = "&Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            // 
            // XPlaneSettings
            // 
            AcceptButton = btnOK;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(355, 120);
            Controls.Add(btnCancel);
            Controls.Add(btnOK);
            Controls.Add(numPort);
            Controls.Add(label2);
            Controls.Add(tbIPAddress);
            Controls.Add(label1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            Margin = new Padding(4, 3, 4, 3);
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "XPlaneSettings";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "SimAppPro Settings";
            Load += XPlaneSettings_Load;
            ((System.ComponentModel.ISupportInitialize)numPort).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbIPAddress;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown numPort;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
    }
}