namespace WinCtrlICP
{
    partial class AboutDlg
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
            pictureBox1 = new PictureBox();
            label1 = new Label();
            label2 = new Label();
            label3 = new Label();
            acceptBtn = new Button();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            SuspendLayout();
            // 
            // pictureBox1
            // 
            pictureBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pictureBox1.Image = Properties.Resources.fighter_jet_icon_speed;
            pictureBox1.Location = new Point(12, 12);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(104, 104);
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label1.AutoSize = true;
            label1.Location = new Point(122, 12);
            label1.Name = "label1";
            label1.Size = new Size(68, 15);
            label1.TabIndex = 1;
            label1.Text = "WinCtrl ICP";
            // 
            // label2
            // 
            label2.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label2.AutoSize = true;
            label2.Location = new Point(122, 27);
            label2.Name = "label2";
            label2.Size = new Size(63, 15);
            label2.TabIndex = 2;
            label2.Text = "Version 1.0";
            // 
            // label3
            // 
            label3.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            label3.AutoSize = true;
            label3.Location = new Point(122, 42);
            label3.Name = "label3";
            label3.Size = new Size(151, 15);
            label3.TabIndex = 3;
            label3.Text = "Written by Glenn Flansburg";
            // 
            // acceptBtn
            // 
            acceptBtn.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            acceptBtn.Location = new Point(198, 93);
            acceptBtn.Name = "acceptBtn";
            acceptBtn.Size = new Size(75, 23);
            acceptBtn.TabIndex = 4;
            acceptBtn.Text = "OK";
            acceptBtn.UseVisualStyleBackColor = true;
            acceptBtn.Click += acceptBtn_Click;
            // 
            // AboutDlg
            // 
            AcceptButton = acceptBtn;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(282, 125);
            Controls.Add(acceptBtn);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(pictureBox1);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "AboutDlg";
            ShowIcon = false;
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "About";
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private PictureBox pictureBox1;
        private Label label1;
        private Label label2;
        private Label label3;
        private Button acceptBtn;
    }
}