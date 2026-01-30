namespace WinCtrlICP
{
    partial class EditLabelForm
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
            txtLabel = new TextBox();
            btnOK = new Button();
            btnCancel = new Button();
            SuspendLayout();
            // 
            // txtLabel
            // 
            txtLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            txtLabel.Location = new Point(12, 12);
            txtLabel.Name = "txtLabel";
            txtLabel.Size = new Size(315, 23);
            txtLabel.TabIndex = 0;
            txtLabel.TextChanged += txtLabel_TextChanged;
            txtLabel.KeyPress += txtLabel_KeyPress;
            // 
            // btnOK
            // 
            btnOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            btnOK.Location = new Point(252, 42);
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
            btnCancel.Location = new Point(171, 42);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new Size(75, 23);
            btnCancel.TabIndex = 2;
            btnCancel.Text = "&Cancel";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += btnCancel_Click;
            // 
            // EditLabelForm
            // 
            AcceptButton = btnOK;
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = btnCancel;
            ClientSize = new Size(339, 77);
            Controls.Add(btnCancel);
            Controls.Add(btnOK);
            Controls.Add(txtLabel);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "EditLabelForm";
            ShowInTaskbar = false;
            StartPosition = FormStartPosition.CenterScreen;
            Text = "Edit Label";
            Shown += EditLabelForm_Shown;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox txtLabel;
        private Button btnOK;
        private Button btnCancel;
    }
}