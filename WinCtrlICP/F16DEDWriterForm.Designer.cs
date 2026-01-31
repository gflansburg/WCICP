namespace WinCtrlICP
{
    partial class F16DEDWriterForm
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(F16DEDWriterForm));
            pictureBox1 = new PictureBox();
            icpDisplayControl1 = new IcpDisplayControl();
            menuStrip1 = new MenuStrip();
            fileToolStripMenuItem = new ToolStripMenuItem();
            exitToolStripMenuItem1 = new ToolStripMenuItem();
            optionsToolStripMenuItem = new ToolStripMenuItem();
            startMinimizedToolStripMenuItem = new ToolStripMenuItem();
            minimizeToSystemTrayToolStripMenuItem = new ToolStripMenuItem();
            startWithMSFSToolStripMenuItem = new ToolStripMenuItem();
            shutdownWithFlightSimToolStripMenuItem = new ToolStripMenuItem();
            xPlaneSettingsToolStripMenuItem = new ToolStripMenuItem();
            joystickBindingsToolStripMenuItem = new ToolStripMenuItem();
            unitSystemToolStripMenuItem = new ToolStripMenuItem();
            aviationUSICAOToolStripMenuItem = new ToolStripMenuItem();
            metricSIToolStripMenuItem = new ToolStripMenuItem();
            viewToolStripMenuItem = new ToolStripMenuItem();
            cNIToolStripMenuItem = new ToolStripMenuItem();
            cOM1ToolStripMenuItem = new ToolStripMenuItem();
            cOM2ToolStripMenuItem = new ToolStripMenuItem();
            iFFToolStripMenuItem = new ToolStripMenuItem();
            nAV1ToolStripMenuItem = new ToolStripMenuItem();
            nAV2ToolStripMenuItem = new ToolStripMenuItem();
            gPSToolStripMenuItem = new ToolStripMenuItem();
            fLPNToolStripMenuItem = new ToolStripMenuItem();
            aPToolStripMenuItem = new ToolStripMenuItem();
            sWCHToolStripMenuItem = new ToolStripMenuItem();
            lGHTToolStripMenuItem = new ToolStripMenuItem();
            wXToolStripMenuItem = new ToolStripMenuItem();
            tIMEToolStripMenuItem = new ToolStripMenuItem();
            iNFOToolStripMenuItem = new ToolStripMenuItem();
            HROTToolStripMenuItem = new ToolStripMenuItem();
            HDISToolStripMenuItem = new ToolStripMenuItem();
            HCTLToolStripMenuItem = new ToolStripMenuItem();
            customToolStripMenuItem = new ToolStripMenuItem();
            customDisplayManagerToolStripMenuItem = new ToolStripMenuItem();
            helpToolStripMenuItem = new ToolStripMenuItem();
            licenseToolStripMenuItem = new ToolStripMenuItem();
            aboutToolStripMenuItem = new ToolStripMenuItem();
            contextMenuStrip1 = new ContextMenuStrip(components);
            openToolStripMenuItem = new ToolStripMenuItem();
            exitToolStripMenuItem = new ToolStripMenuItem();
            notifyIcon1 = new NotifyIcon(components);
            timer1 = new System.Windows.Forms.Timer(components);
            bALNToolStripMenuItem = new ToolStripMenuItem();
            aIRSToolStripMenuItem = new ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
            menuStrip1.SuspendLayout();
            contextMenuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // pictureBox1
            // 
            pictureBox1.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            pictureBox1.Image = (Image)resources.GetObject("pictureBox1.Image");
            pictureBox1.Location = new Point(-4, -1);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new Size(610, 757);
            pictureBox1.SizeMode = PictureBoxSizeMode.CenterImage;
            pictureBox1.TabIndex = 0;
            pictureBox1.TabStop = false;
            // 
            // icpDisplayControl1
            // 
            icpDisplayControl1.BackColor = Color.FromArgb(71, 74, 72);
            icpDisplayControl1.LeftMargin = 13;
            icpDisplayControl1.LineCount = 5;
            icpDisplayControl1.Location = new Point(88, 96);
            icpDisplayControl1.Name = "icpDisplayControl1";
            icpDisplayControl1.ShowSelectionBorder = false;
            icpDisplayControl1.Size = new Size(427, 128);
            icpDisplayControl1.TabIndex = 2;
            icpDisplayControl1.Text = "icpDisplayControl1";
            icpDisplayControl1.TopMargin = 2;
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new ToolStripItem[] { fileToolStripMenuItem, optionsToolStripMenuItem, viewToolStripMenuItem, helpToolStripMenuItem });
            menuStrip1.Location = new Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new Size(606, 24);
            menuStrip1.TabIndex = 4;
            menuStrip1.Text = "menuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            fileToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { exitToolStripMenuItem1 });
            fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            fileToolStripMenuItem.Size = new Size(37, 20);
            fileToolStripMenuItem.Text = "&File";
            // 
            // exitToolStripMenuItem1
            // 
            exitToolStripMenuItem1.Name = "exitToolStripMenuItem1";
            exitToolStripMenuItem1.Size = new Size(92, 22);
            exitToolStripMenuItem1.Text = "&Exit";
            exitToolStripMenuItem1.Click += exitToolStripMenuItem_Click;
            // 
            // optionsToolStripMenuItem
            // 
            optionsToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { startMinimizedToolStripMenuItem, minimizeToSystemTrayToolStripMenuItem, startWithMSFSToolStripMenuItem, shutdownWithFlightSimToolStripMenuItem, xPlaneSettingsToolStripMenuItem, joystickBindingsToolStripMenuItem, unitSystemToolStripMenuItem });
            optionsToolStripMenuItem.Name = "optionsToolStripMenuItem";
            optionsToolStripMenuItem.Size = new Size(61, 20);
            optionsToolStripMenuItem.Text = "&Options";
            // 
            // startMinimizedToolStripMenuItem
            // 
            startMinimizedToolStripMenuItem.Name = "startMinimizedToolStripMenuItem";
            startMinimizedToolStripMenuItem.Size = new Size(212, 22);
            startMinimizedToolStripMenuItem.Text = "&Start Minimized";
            startMinimizedToolStripMenuItem.Click += startMinimizedToolStripMenuItem_Click;
            // 
            // minimizeToSystemTrayToolStripMenuItem
            // 
            minimizeToSystemTrayToolStripMenuItem.Name = "minimizeToSystemTrayToolStripMenuItem";
            minimizeToSystemTrayToolStripMenuItem.Size = new Size(212, 22);
            minimizeToSystemTrayToolStripMenuItem.Text = "&Minimize To System Tray";
            minimizeToSystemTrayToolStripMenuItem.Click += minimizeToSystemTrayToolStripMenuItem_Click;
            // 
            // startWithMSFSToolStripMenuItem
            // 
            startWithMSFSToolStripMenuItem.Name = "startWithMSFSToolStripMenuItem";
            startWithMSFSToolStripMenuItem.Size = new Size(212, 22);
            startWithMSFSToolStripMenuItem.Text = "S&tart With MSFS";
            startWithMSFSToolStripMenuItem.Click += startWithMSFSToolStripMenuItem_Click;
            // 
            // shutdownWithFlightSimToolStripMenuItem
            // 
            shutdownWithFlightSimToolStripMenuItem.Name = "shutdownWithFlightSimToolStripMenuItem";
            shutdownWithFlightSimToolStripMenuItem.Size = new Size(212, 22);
            shutdownWithFlightSimToolStripMenuItem.Text = "Shutdown &With Flight Sim";
            shutdownWithFlightSimToolStripMenuItem.Click += shutdownWithFlightSimToolStripMenuItem_Click;
            // 
            // xPlaneSettingsToolStripMenuItem
            // 
            xPlaneSettingsToolStripMenuItem.Name = "xPlaneSettingsToolStripMenuItem";
            xPlaneSettingsToolStripMenuItem.Size = new Size(212, 22);
            xPlaneSettingsToolStripMenuItem.Text = "&X-Plane Settings...";
            xPlaneSettingsToolStripMenuItem.Click += xPlaneSettingsToolStripMenuItem_Click;
            // 
            // joystickBindingsToolStripMenuItem
            // 
            joystickBindingsToolStripMenuItem.Name = "joystickBindingsToolStripMenuItem";
            joystickBindingsToolStripMenuItem.Size = new Size(212, 22);
            joystickBindingsToolStripMenuItem.Text = "&Joystick Bindings...";
            joystickBindingsToolStripMenuItem.Click += joystickBindingsToolStripMenuItem_Click;
            // 
            // unitSystemToolStripMenuItem
            // 
            unitSystemToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { aviationUSICAOToolStripMenuItem, metricSIToolStripMenuItem });
            unitSystemToolStripMenuItem.Name = "unitSystemToolStripMenuItem";
            unitSystemToolStripMenuItem.Size = new Size(212, 22);
            unitSystemToolStripMenuItem.Text = "&Unit System";
            // 
            // aviationUSICAOToolStripMenuItem
            // 
            aviationUSICAOToolStripMenuItem.Name = "aviationUSICAOToolStripMenuItem";
            aviationUSICAOToolStripMenuItem.Size = new Size(176, 22);
            aviationUSICAOToolStripMenuItem.Text = "Aviation (US/ICAO)";
            aviationUSICAOToolStripMenuItem.Click += aviationUSICAOToolStripMenuItem_Click;
            // 
            // metricSIToolStripMenuItem
            // 
            metricSIToolStripMenuItem.Name = "metricSIToolStripMenuItem";
            metricSIToolStripMenuItem.Size = new Size(176, 22);
            metricSIToolStripMenuItem.Text = "Metric (SI)";
            metricSIToolStripMenuItem.Click += metricSIToolStripMenuItem_Click;
            // 
            // viewToolStripMenuItem
            // 
            viewToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { cNIToolStripMenuItem, cOM1ToolStripMenuItem, cOM2ToolStripMenuItem, iFFToolStripMenuItem, nAV1ToolStripMenuItem, nAV2ToolStripMenuItem, gPSToolStripMenuItem, fLPNToolStripMenuItem, aPToolStripMenuItem, sWCHToolStripMenuItem, lGHTToolStripMenuItem, wXToolStripMenuItem, tIMEToolStripMenuItem, iNFOToolStripMenuItem, HROTToolStripMenuItem, HDISToolStripMenuItem, HCTLToolStripMenuItem, bALNToolStripMenuItem, aIRSToolStripMenuItem, customToolStripMenuItem });
            viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            viewToolStripMenuItem.Size = new Size(44, 20);
            viewToolStripMenuItem.Text = "&View";
            // 
            // cNIToolStripMenuItem
            // 
            cNIToolStripMenuItem.Name = "cNIToolStripMenuItem";
            cNIToolStripMenuItem.Size = new Size(180, 22);
            cNIToolStripMenuItem.Text = "&CNI";
            cNIToolStripMenuItem.Click += cNIToolStripMenuItem_Click;
            // 
            // cOM1ToolStripMenuItem
            // 
            cOM1ToolStripMenuItem.Name = "cOM1ToolStripMenuItem";
            cOM1ToolStripMenuItem.Size = new Size(180, 22);
            cOM1ToolStripMenuItem.Text = "COM&1";
            cOM1ToolStripMenuItem.Click += cOM1ToolStripMenuItem_Click;
            // 
            // cOM2ToolStripMenuItem
            // 
            cOM2ToolStripMenuItem.Name = "cOM2ToolStripMenuItem";
            cOM2ToolStripMenuItem.Size = new Size(180, 22);
            cOM2ToolStripMenuItem.Text = "COM&2";
            cOM2ToolStripMenuItem.Click += cOM2ToolStripMenuItem_Click;
            // 
            // iFFToolStripMenuItem
            // 
            iFFToolStripMenuItem.Name = "iFFToolStripMenuItem";
            iFFToolStripMenuItem.Size = new Size(180, 22);
            iFFToolStripMenuItem.Text = "&IFF";
            iFFToolStripMenuItem.Click += iFFToolStripMenuItem_Click;
            // 
            // nAV1ToolStripMenuItem
            // 
            nAV1ToolStripMenuItem.Name = "nAV1ToolStripMenuItem";
            nAV1ToolStripMenuItem.Size = new Size(180, 22);
            nAV1ToolStripMenuItem.Text = "&NAV1";
            nAV1ToolStripMenuItem.Click += nAV1ToolStripMenuItem_Click;
            // 
            // nAV2ToolStripMenuItem
            // 
            nAV2ToolStripMenuItem.Name = "nAV2ToolStripMenuItem";
            nAV2ToolStripMenuItem.Size = new Size(180, 22);
            nAV2ToolStripMenuItem.Text = "NA&V2";
            nAV2ToolStripMenuItem.Click += nAV2ToolStripMenuItem_Click;
            // 
            // gPSToolStripMenuItem
            // 
            gPSToolStripMenuItem.Name = "gPSToolStripMenuItem";
            gPSToolStripMenuItem.Size = new Size(180, 22);
            gPSToolStripMenuItem.Text = "&GPS";
            gPSToolStripMenuItem.Click += gPSToolStripMenuItem_Click;
            // 
            // fLPNToolStripMenuItem
            // 
            fLPNToolStripMenuItem.Name = "fLPNToolStripMenuItem";
            fLPNToolStripMenuItem.Size = new Size(180, 22);
            fLPNToolStripMenuItem.Text = "FL&PN";
            fLPNToolStripMenuItem.Click += fLPNToolStripMenuItem_Click;
            // 
            // aPToolStripMenuItem
            // 
            aPToolStripMenuItem.Name = "aPToolStripMenuItem";
            aPToolStripMenuItem.Size = new Size(180, 22);
            aPToolStripMenuItem.Text = "&AP";
            aPToolStripMenuItem.Click += aPToolStripMenuItem_Click;
            // 
            // sWCHToolStripMenuItem
            // 
            sWCHToolStripMenuItem.Name = "sWCHToolStripMenuItem";
            sWCHToolStripMenuItem.Size = new Size(180, 22);
            sWCHToolStripMenuItem.Text = "S&WCH";
            sWCHToolStripMenuItem.Click += sWCHToolStripMenuItem_Click;
            // 
            // lGHTToolStripMenuItem
            // 
            lGHTToolStripMenuItem.Name = "lGHTToolStripMenuItem";
            lGHTToolStripMenuItem.Size = new Size(180, 22);
            lGHTToolStripMenuItem.Text = "&LGHT";
            lGHTToolStripMenuItem.Click += lGHTToolStripMenuItem_Click;
            // 
            // wXToolStripMenuItem
            // 
            wXToolStripMenuItem.Name = "wXToolStripMenuItem";
            wXToolStripMenuItem.Size = new Size(180, 22);
            wXToolStripMenuItem.Text = "W&X";
            wXToolStripMenuItem.Click += wXToolStripMenuItem_Click;
            // 
            // tIMEToolStripMenuItem
            // 
            tIMEToolStripMenuItem.Name = "tIMEToolStripMenuItem";
            tIMEToolStripMenuItem.Size = new Size(180, 22);
            tIMEToolStripMenuItem.Text = "&TIME";
            tIMEToolStripMenuItem.Click += tIMEToolStripMenuItem_Click;
            // 
            // iNFOToolStripMenuItem
            // 
            iNFOToolStripMenuItem.Name = "iNFOToolStripMenuItem";
            iNFOToolStripMenuItem.Size = new Size(180, 22);
            iNFOToolStripMenuItem.Text = "IN&FO";
            iNFOToolStripMenuItem.Click += iNFOToolStripMenuItem_Click;
            // 
            // HROTToolStripMenuItem
            // 
            HROTToolStripMenuItem.Name = "HROTToolStripMenuItem";
            HROTToolStripMenuItem.Size = new Size(180, 22);
            HROTToolStripMenuItem.Text = "H&ROT";
            HROTToolStripMenuItem.Click += HROTToolStripMenuItem_Click;
            // 
            // HDISToolStripMenuItem
            // 
            HDISToolStripMenuItem.Name = "HDISToolStripMenuItem";
            HDISToolStripMenuItem.Size = new Size(180, 22);
            HDISToolStripMenuItem.Text = "H&DIS";
            HDISToolStripMenuItem.Click += HDISToolStripMenuItem_Click;
            // 
            // HCTLToolStripMenuItem
            // 
            HCTLToolStripMenuItem.Name = "HCTLToolStripMenuItem";
            HCTLToolStripMenuItem.Size = new Size(180, 22);
            HCTLToolStripMenuItem.Text = "&HCTL";
            HCTLToolStripMenuItem.Click += HCTLToolStripMenuItem_Click;
            // 
            // customToolStripMenuItem
            // 
            customToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { customDisplayManagerToolStripMenuItem });
            customToolStripMenuItem.Name = "customToolStripMenuItem";
            customToolStripMenuItem.Size = new Size(180, 22);
            customToolStripMenuItem.Text = "C&ustom...";
            // 
            // customDisplayManagerToolStripMenuItem
            // 
            customDisplayManagerToolStripMenuItem.Name = "customDisplayManagerToolStripMenuItem";
            customDisplayManagerToolStripMenuItem.Size = new Size(216, 22);
            customDisplayManagerToolStripMenuItem.Text = "&Custom Display Manager...";
            customDisplayManagerToolStripMenuItem.Click += customDisplayManagerToolStripMenuItem_Click;
            // 
            // helpToolStripMenuItem
            // 
            helpToolStripMenuItem.DropDownItems.AddRange(new ToolStripItem[] { licenseToolStripMenuItem, aboutToolStripMenuItem });
            helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            helpToolStripMenuItem.Size = new Size(44, 20);
            helpToolStripMenuItem.Text = "&Help";
            // 
            // licenseToolStripMenuItem
            // 
            licenseToolStripMenuItem.Name = "licenseToolStripMenuItem";
            licenseToolStripMenuItem.Size = new Size(113, 22);
            licenseToolStripMenuItem.Text = "&License";
            licenseToolStripMenuItem.Click += licenseToolStripMenuItem_Click;
            // 
            // aboutToolStripMenuItem
            // 
            aboutToolStripMenuItem.Name = "aboutToolStripMenuItem";
            aboutToolStripMenuItem.Size = new Size(113, 22);
            aboutToolStripMenuItem.Text = "&About";
            aboutToolStripMenuItem.Click += aboutToolStripMenuItem_Click;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new ToolStripItem[] { openToolStripMenuItem, exitToolStripMenuItem });
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new Size(104, 48);
            // 
            // openToolStripMenuItem
            // 
            openToolStripMenuItem.Name = "openToolStripMenuItem";
            openToolStripMenuItem.Size = new Size(103, 22);
            openToolStripMenuItem.Text = "&Open";
            openToolStripMenuItem.Click += openToolStripMenuItem_Click;
            // 
            // exitToolStripMenuItem
            // 
            exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            exitToolStripMenuItem.Size = new Size(103, 22);
            exitToolStripMenuItem.Text = "&Exit";
            exitToolStripMenuItem.Click += exitToolStripMenuItem_Click;
            // 
            // notifyIcon1
            // 
            notifyIcon1.ContextMenuStrip = contextMenuStrip1;
            notifyIcon1.Icon = (Icon)resources.GetObject("notifyIcon1.Icon");
            notifyIcon1.Text = "WinCtrl ICP";
            notifyIcon1.Visible = true;
            notifyIcon1.MouseDoubleClick += notifyIcon1_MouseDoubleClick;
            // 
            // timer1
            // 
            timer1.Enabled = true;
            timer1.Interval = 1000;
            timer1.Tick += timer1_Tick;
            // 
            // bALNToolStripMenuItem
            // 
            bALNToolStripMenuItem.Name = "bALNToolStripMenuItem";
            bALNToolStripMenuItem.Size = new Size(180, 22);
            bALNToolStripMenuItem.Text = "&BALN";
            bALNToolStripMenuItem.Click += bALNToolStripMenuItem_Click;
            // 
            // aIRSToolStripMenuItem
            // 
            aIRSToolStripMenuItem.Name = "aIRSToolStripMenuItem";
            aIRSToolStripMenuItem.Size = new Size(180, 22);
            aIRSToolStripMenuItem.Text = "AIR&S";
            aIRSToolStripMenuItem.Click += aIRSToolStripMenuItem_Click;
            // 
            // F16DEDWriterForm
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(606, 729);
            Controls.Add(menuStrip1);
            Controls.Add(icpDisplayControl1);
            Controls.Add(pictureBox1);
            FormBorderStyle = FormBorderStyle.FixedSingle;
            Icon = (Icon)resources.GetObject("$this.Icon");
            Margin = new Padding(4, 3, 4, 3);
            MaximizeBox = false;
            Name = "F16DEDWriterForm";
            StartPosition = FormStartPosition.CenterScreen;
            Text = "WinCtrl ICP";
            FormClosing += F16DEDWriterForm_FormClosing;
            Shown += F16DEDWriterForm_Shown;
            ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            contextMenuStrip1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();

        }

        #endregion

        private PictureBox pictureBox1;
        private IcpDisplayControl icpDisplayControl1;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem fileToolStripMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem1;
        private ToolStripMenuItem optionsToolStripMenuItem;
        private ToolStripMenuItem startMinimizedToolStripMenuItem;
        private ToolStripMenuItem minimizeToSystemTrayToolStripMenuItem;
        private ToolStripMenuItem helpToolStripMenuItem;
        private ToolStripMenuItem licenseToolStripMenuItem;
        private ToolStripMenuItem aboutToolStripMenuItem;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem openToolStripMenuItem;
        private ToolStripMenuItem exitToolStripMenuItem;
        private NotifyIcon notifyIcon1;
        private ToolStripMenuItem xPlaneSettingsToolStripMenuItem;
        private ToolStripMenuItem shutdownWithFlightSimToolStripMenuItem;
        private System.Windows.Forms.Timer timer1;
        private ToolStripMenuItem viewToolStripMenuItem;
        private ToolStripMenuItem cNIToolStripMenuItem;
        private ToolStripMenuItem cOM1ToolStripMenuItem;
        private ToolStripMenuItem cOM2ToolStripMenuItem;
        private ToolStripMenuItem iFFToolStripMenuItem;
        private ToolStripMenuItem nAV1ToolStripMenuItem;
        private ToolStripMenuItem customToolStripMenuItem;
        private ToolStripMenuItem customDisplayManagerToolStripMenuItem;
        private ToolStripMenuItem joystickBindingsToolStripMenuItem;
        private ToolStripMenuItem startWithMSFSToolStripMenuItem;
        private ToolStripMenuItem iNFOToolStripMenuItem;
        private ToolStripMenuItem unitSystemToolStripMenuItem;
        private ToolStripMenuItem aviationUSICAOToolStripMenuItem;
        private ToolStripMenuItem metricSIToolStripMenuItem;
        private ToolStripMenuItem wXToolStripMenuItem;
        private ToolStripMenuItem aPToolStripMenuItem;
        private ToolStripMenuItem sWCHToolStripMenuItem;
        private ToolStripMenuItem lGHTToolStripMenuItem;
        private ToolStripMenuItem tIMEToolStripMenuItem;
        private ToolStripMenuItem HROTToolStripMenuItem;
        private ToolStripMenuItem HDISToolStripMenuItem;
        private ToolStripMenuItem HCTLToolStripMenuItem;
        private ToolStripMenuItem nAV2ToolStripMenuItem;
        private ToolStripMenuItem gPSToolStripMenuItem;
        private ToolStripMenuItem fLPNToolStripMenuItem;
        private ToolStripMenuItem bALNToolStripMenuItem;
        private ToolStripMenuItem aIRSToolStripMenuItem;
    }
}

