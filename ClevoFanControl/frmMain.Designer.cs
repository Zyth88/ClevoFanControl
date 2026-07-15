
namespace ClevoFanControl {
    partial class frmMain {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(frmMain));
            this.tmrMain = new System.Windows.Forms.Timer(this.components);
            this.icoTray = new System.Windows.Forms.NotifyIcon(this.components);
            this.mnuMain = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuShowWindow = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuProfile30 = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuProfile40 = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuProfile50 = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuProfile60 = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuProfile70 = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuProfile80 = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuProfile90 = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuProfileMax = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuExit = new System.Windows.Forms.ToolStripMenuItem();
            this.pnlFanDuties = new System.Windows.Forms.Panel();
            this.imgStatFan = new System.Windows.Forms.PictureBox();
            this.lblFan = new System.Windows.Forms.Label();
            this.lblFanDutyHeader = new System.Windows.Forms.Label();
            this.pnlProfiles = new System.Windows.Forms.Panel();
            this.btnProfile80 = new System.Windows.Forms.RadioButton();
            this.btnProfile90 = new System.Windows.Forms.RadioButton();
            this.btnProfile70 = new System.Windows.Forms.RadioButton();
            this.btnProfile60 = new System.Windows.Forms.RadioButton();
            this.btnProfile40 = new System.Windows.Forms.RadioButton();
            this.btnProfile30 = new System.Windows.Forms.RadioButton();
            this.btnProfile50 = new System.Windows.Forms.RadioButton();
            this.btnProfileMax = new System.Windows.Forms.RadioButton();
            this.btnExit = new System.Windows.Forms.Button();
            this.btnAlwaysOnTop = new System.Windows.Forms.CheckBox();
            this.tipTooltip = new System.Windows.Forms.ToolTip(this.components);
            this.tmrGui = new System.Windows.Forms.Timer(this.components);
            this.mnuMain.SuspendLayout();
            this.pnlFanDuties.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgStatFan)).BeginInit();
            this.pnlProfiles.SuspendLayout();
            this.SuspendLayout();
            // 
            // tmrMain
            // 
            this.tmrMain.Interval = 1000;
            this.tmrMain.Tick += new System.EventHandler(this.tmrMain_Tick);
            // 
            // icoTray
            // 
            this.icoTray.BalloonTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            this.icoTray.BalloonTipText = "Clevo Fan Control";
            this.icoTray.BalloonTipTitle = "Clevo Fan Control";
            this.icoTray.ContextMenuStrip = this.mnuMain;
            this.icoTray.Icon = ((System.Drawing.Icon)(resources.GetObject("icoTray.Icon")));
            this.icoTray.Text = "Clevo Fan Control";
            this.icoTray.Visible = true;
            this.icoTray.DoubleClick += new System.EventHandler(this.icoTray_DoubleClick);
            // 
            // mnuMain
            // 
            this.mnuMain.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuShowWindow,
            this.mnuSeparator1,
            this.mnuProfile30,
            this.mnuProfile40,
            this.mnuProfile50,
            this.mnuProfile60,
            this.mnuProfile70,
            this.mnuProfile80,
            this.mnuProfile90,
            this.mnuProfileMax,
            this.mnuSeparator2,
            this.mnuExit});
            this.mnuMain.Name = "mnuMain";
            this.mnuMain.Size = new System.Drawing.Size(175, 236);
            // 
            // mnuShowWindow
            // 
            this.mnuShowWindow.Name = "mnuShowWindow";
            this.mnuShowWindow.Size = new System.Drawing.Size(174, 22);
            this.mnuShowWindow.Text = "&Show Window";
            this.mnuShowWindow.Click += new System.EventHandler(this.mnuShowWindow_Click);
            // 
            // mnuSeparator1
            // 
            this.mnuSeparator1.Name = "mnuSeparator1";
            this.mnuSeparator1.Size = new System.Drawing.Size(171, 6);
            // 
            // mnuProfile30
            // 
            this.mnuProfile30.Name = "mnuProfile30";
            this.mnuProfile30.Size = new System.Drawing.Size(174, 22);
            this.mnuProfile30.Text = "30% Speed Profile";
            this.mnuProfile30.Click += new System.EventHandler(this.mnuProfile30_Click);
            // 
            // mnuProfile40
            // 
            this.mnuProfile40.Name = "mnuProfile40";
            this.mnuProfile40.Size = new System.Drawing.Size(174, 22);
            this.mnuProfile40.Text = "40% Speed Profile";
            this.mnuProfile40.Click += new System.EventHandler(this.mnuProfile40_Click);
            // 
            // mnuProfile50
            // 
            this.mnuProfile50.Name = "mnuProfile50";
            this.mnuProfile50.Size = new System.Drawing.Size(174, 22);
            this.mnuProfile50.Text = "50% Speed Profile";
            this.mnuProfile50.Click += new System.EventHandler(this.mnuProfile50_Click);
            // 
            // mnuProfile60
            // 
            this.mnuProfile60.Name = "mnuProfile60";
            this.mnuProfile60.Size = new System.Drawing.Size(174, 22);
            this.mnuProfile60.Text = "60% Speed Profile";
            this.mnuProfile60.Click += new System.EventHandler(this.mnuProfile60_Click);
            // 
            // mnuProfile70
            // 
            this.mnuProfile70.Name = "mnuProfile70";
            this.mnuProfile70.Size = new System.Drawing.Size(174, 22);
            this.mnuProfile70.Text = "70% Speed Profile";
            this.mnuProfile70.Click += new System.EventHandler(this.mnuProfile70_Click);
            // 
            // mnuProfile80
            // 
            this.mnuProfile80.Name = "mnuProfile80";
            this.mnuProfile80.Size = new System.Drawing.Size(174, 22);
            this.mnuProfile80.Text = "80% Speed Profile";
            this.mnuProfile80.Click += new System.EventHandler(this.mnuProfile80_Click);
            // 
            // mnuProfile90
            // 
            this.mnuProfile90.Name = "mnuProfile90";
            this.mnuProfile90.Size = new System.Drawing.Size(174, 22);
            this.mnuProfile90.Text = "90% Speed Profile";
            this.mnuProfile90.Click += new System.EventHandler(this.mnuProfile90_Click);
            // 
            // mnuProfileMax
            // 
            this.mnuProfileMax.Name = "mnuProfileMax";
            this.mnuProfileMax.Size = new System.Drawing.Size(174, 22);
            this.mnuProfileMax.Text = "100% Speed Profile";
            this.mnuProfileMax.Click += new System.EventHandler(this.mnuProfileMax_Click);
            // 
            // mnuSeparator2
            // 
            this.mnuSeparator2.Name = "mnuSeparator2";
            this.mnuSeparator2.Size = new System.Drawing.Size(171, 6);
            // 
            // mnuExit
            // 
            this.mnuExit.Name = "mnuExit";
            this.mnuExit.Size = new System.Drawing.Size(174, 22);
            this.mnuExit.Text = "E&xit";
            this.mnuExit.Click += new System.EventHandler(this.mnuExit_Click);
            // 
            // pnlFanDuties
            // 
            this.pnlFanDuties.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlFanDuties.Controls.Add(this.imgStatFan);
            this.pnlFanDuties.Controls.Add(this.lblFan);
            this.pnlFanDuties.Controls.Add(this.lblFanDutyHeader);
            this.pnlFanDuties.Location = new System.Drawing.Point(12, 12);
            this.pnlFanDuties.Name = "pnlFanDuties";
            this.pnlFanDuties.Size = new System.Drawing.Size(297, 130);
            this.pnlFanDuties.TabIndex = 12;
            // 
            // imgStatFan
            // 
            this.imgStatFan.Image = global::ClevoFanControl.Properties.Resources.clevofancontrol;
            this.imgStatFan.Location = new System.Drawing.Point(254, 5);
            this.imgStatFan.Name = "imgStatFan";
            this.imgStatFan.Size = new System.Drawing.Size(36, 36);
            this.imgStatFan.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.imgStatFan.TabIndex = 10;
            this.imgStatFan.TabStop = false;
            // 
            // lblFan
            // 
            this.lblFan.Font = new System.Drawing.Font("Microsoft Sans Serif", 48F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFan.ForeColor = System.Drawing.Color.Black;
            this.lblFan.Location = new System.Drawing.Point(8, 38);
            this.lblFan.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblFan.Name = "lblFan";
            this.lblFan.Size = new System.Drawing.Size(280, 80);
            this.lblFan.TabIndex = 7;
            this.lblFan.Text = "0%";
            this.lblFan.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // lblFanDutyHeader
            // 
            this.lblFanDutyHeader.AutoSize = true;
            this.lblFanDutyHeader.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblFanDutyHeader.ForeColor = System.Drawing.Color.Black;
            this.lblFanDutyHeader.Location = new System.Drawing.Point(4, 9);
            this.lblFanDutyHeader.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lblFanDutyHeader.Name = "lblFanDutyHeader";
            this.lblFanDutyHeader.Size = new System.Drawing.Size(97, 20);
            this.lblFanDutyHeader.TabIndex = 0;
            this.lblFanDutyHeader.Text = "Fan Speed";
            // 
            // pnlProfiles
            // 
            this.pnlProfiles.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.pnlProfiles.Controls.Add(this.btnProfile80);
            this.pnlProfiles.Controls.Add(this.btnProfile90);
            this.pnlProfiles.Controls.Add(this.btnProfile70);
            this.pnlProfiles.Controls.Add(this.btnProfile60);
            this.pnlProfiles.Controls.Add(this.btnProfile40);
            this.pnlProfiles.Controls.Add(this.btnProfile30);
            this.pnlProfiles.Controls.Add(this.btnProfile50);
            this.pnlProfiles.Controls.Add(this.btnProfileMax);
            this.pnlProfiles.Location = new System.Drawing.Point(12, 148);
            this.pnlProfiles.Name = "pnlProfiles";
            this.pnlProfiles.Size = new System.Drawing.Size(297, 71);
            this.pnlProfiles.TabIndex = 13;
            // 
            // btnProfile80
            // 
            this.btnProfile80.Appearance = System.Windows.Forms.Appearance.Button;
            this.btnProfile80.Location = new System.Drawing.Point(85, 38);
            this.btnProfile80.Name = "btnProfile80";
            this.btnProfile80.Size = new System.Drawing.Size(60, 28);
            this.btnProfile80.TabIndex = 9;
            this.btnProfile80.Text = "80%";
            this.btnProfile80.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnProfile80.UseVisualStyleBackColor = true;
            this.btnProfile80.CheckedChanged += new System.EventHandler(this.btnProfile80_CheckedChanged);
            // 
            // btnProfile90
            // 
            this.btnProfile90.Appearance = System.Windows.Forms.Appearance.Button;
            this.btnProfile90.Location = new System.Drawing.Point(151, 38);
            this.btnProfile90.Name = "btnProfile90";
            this.btnProfile90.Size = new System.Drawing.Size(60, 28);
            this.btnProfile90.TabIndex = 10;
            this.btnProfile90.Text = "90%";
            this.btnProfile90.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnProfile90.UseVisualStyleBackColor = true;
            this.btnProfile90.CheckedChanged += new System.EventHandler(this.btnProfile90_CheckedChanged);
            // 
            // btnProfile70
            // 
            this.btnProfile70.Appearance = System.Windows.Forms.Appearance.Button;
            this.btnProfile70.Location = new System.Drawing.Point(19, 38);
            this.btnProfile70.Name = "btnProfile70";
            this.btnProfile70.Size = new System.Drawing.Size(60, 28);
            this.btnProfile70.TabIndex = 8;
            this.btnProfile70.Text = "70%";
            this.btnProfile70.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnProfile70.UseVisualStyleBackColor = true;
            this.btnProfile70.CheckedChanged += new System.EventHandler(this.btnProfile70_CheckedChanged);
            // 
            // btnProfile60
            // 
            this.btnProfile60.Appearance = System.Windows.Forms.Appearance.Button;
            this.btnProfile60.Location = new System.Drawing.Point(217, 5);
            this.btnProfile60.Name = "btnProfile60";
            this.btnProfile60.Size = new System.Drawing.Size(60, 28);
            this.btnProfile60.TabIndex = 8;
            this.btnProfile60.Text = "60%";
            this.btnProfile60.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnProfile60.UseVisualStyleBackColor = true;
            this.btnProfile60.CheckedChanged += new System.EventHandler(this.btnProfile60_CheckedChanged);
            // 
            // btnProfile40
            // 
            this.btnProfile40.Appearance = System.Windows.Forms.Appearance.Button;
            this.btnProfile40.Location = new System.Drawing.Point(85, 5);
            this.btnProfile40.Name = "btnProfile40";
            this.btnProfile40.Size = new System.Drawing.Size(60, 28);
            this.btnProfile40.TabIndex = 7;
            this.btnProfile40.Text = "40%";
            this.btnProfile40.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnProfile40.UseVisualStyleBackColor = true;
            this.btnProfile40.CheckedChanged += new System.EventHandler(this.btnProfile40_CheckedChanged);
            // 
            // btnProfile30
            // 
            this.btnProfile30.Appearance = System.Windows.Forms.Appearance.Button;
            this.btnProfile30.Location = new System.Drawing.Point(19, 5);
            this.btnProfile30.Name = "btnProfile30";
            this.btnProfile30.Size = new System.Drawing.Size(60, 28);
            this.btnProfile30.TabIndex = 6;
            this.btnProfile30.Text = "30%";
            this.btnProfile30.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnProfile30.UseVisualStyleBackColor = true;
            this.btnProfile30.CheckedChanged += new System.EventHandler(this.btnProfile30_CheckedChanged);
            // 
            // btnProfile50
            // 
            this.btnProfile50.Appearance = System.Windows.Forms.Appearance.Button;
            this.btnProfile50.Location = new System.Drawing.Point(151, 5);
            this.btnProfile50.Name = "btnProfile50";
            this.btnProfile50.Size = new System.Drawing.Size(60, 28);
            this.btnProfile50.TabIndex = 5;
            this.btnProfile50.Text = "50%";
            this.btnProfile50.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnProfile50.UseVisualStyleBackColor = true;
            this.btnProfile50.CheckedChanged += new System.EventHandler(this.btnProfile50_CheckedChanged);
            // 
            // btnProfileMax
            // 
            this.btnProfileMax.Appearance = System.Windows.Forms.Appearance.Button;
            this.btnProfileMax.Checked = true;
            this.btnProfileMax.Location = new System.Drawing.Point(217, 38);
            this.btnProfileMax.Name = "btnProfileMax";
            this.btnProfileMax.Size = new System.Drawing.Size(60, 28);
            this.btnProfileMax.TabIndex = 4;
            this.btnProfileMax.TabStop = true;
            this.btnProfileMax.Text = "100%";
            this.btnProfileMax.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnProfileMax.UseVisualStyleBackColor = true;
            this.btnProfileMax.CheckedChanged += new System.EventHandler(this.btnProfileMax_CheckedChanged);
            // 
            // btnExit
            // 
            this.btnExit.Location = new System.Drawing.Point(174, 225);
            this.btnExit.Name = "btnExit";
            this.btnExit.Size = new System.Drawing.Size(135, 30);
            this.btnExit.TabIndex = 14;
            this.btnExit.TabStop = false;
            this.btnExit.Text = "Exit";
            this.btnExit.UseVisualStyleBackColor = true;
            this.btnExit.Click += new System.EventHandler(this.btnExit_Click);
            // 
            // btnAlwaysOnTop
            // 
            this.btnAlwaysOnTop.Appearance = System.Windows.Forms.Appearance.Button;
            this.btnAlwaysOnTop.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnAlwaysOnTop.Location = new System.Drawing.Point(9, 225);
            this.btnAlwaysOnTop.Margin = new System.Windows.Forms.Padding(0, 3, 0, 3);
            this.btnAlwaysOnTop.Name = "btnAlwaysOnTop";
            this.btnAlwaysOnTop.Size = new System.Drawing.Size(130, 30);
            this.btnAlwaysOnTop.TabIndex = 15;
            this.btnAlwaysOnTop.Text = "Always on Top";
            this.btnAlwaysOnTop.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnAlwaysOnTop.UseVisualStyleBackColor = true;
            this.btnAlwaysOnTop.CheckedChanged += new System.EventHandler(this.btnAlwaysOnTop_CheckedChanged);
            // 
            // tmrGui
            // 
            this.tmrGui.Enabled = true;
            this.tmrGui.Interval = 2000;
            this.tmrGui.Tick += new System.EventHandler(this.tmrGui_Tick);
            // 
            // frmMain
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.BackColor = System.Drawing.Color.WhiteSmoke;
            this.ClientSize = new System.Drawing.Size(327, 260);
            this.Controls.Add(this.btnAlwaysOnTop);
            this.Controls.Add(this.btnExit);
            this.Controls.Add(this.pnlProfiles);
            this.Controls.Add(this.pnlFanDuties);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.MaximizeBox = false;
            this.Name = "frmMain";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Clevo Fan Control";
            this.WindowState = System.Windows.Forms.FormWindowState.Minimized;
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Load += new System.EventHandler(this.Form1_Load);
            this.LocationChanged += new System.EventHandler(this.frmMain_LocationChanged);
            this.mnuMain.ResumeLayout(false);
            this.pnlFanDuties.ResumeLayout(false);
            this.pnlFanDuties.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.imgStatFan)).EndInit();
            this.pnlProfiles.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Timer tmrMain;
        private System.Windows.Forms.NotifyIcon icoTray;
        private System.Windows.Forms.ContextMenuStrip mnuMain;
        private System.Windows.Forms.ToolStripMenuItem mnuShowWindow;
        private System.Windows.Forms.ToolStripMenuItem mnuExit;

        private System.Windows.Forms.Panel pnlFanDuties;
        private System.Windows.Forms.Label lblFanDutyHeader;
        private System.Windows.Forms.Label lblFan;

        private System.Windows.Forms.PictureBox imgStatFan;
        private System.Windows.Forms.Panel pnlProfiles;
        private System.Windows.Forms.RadioButton btnProfileMax;
        private System.Windows.Forms.ToolStripSeparator mnuSeparator1;
        private System.Windows.Forms.ToolStripMenuItem mnuProfileMax;
        private System.Windows.Forms.ToolStripSeparator mnuSeparator2;
        private System.Windows.Forms.Button btnExit;
        private System.Windows.Forms.CheckBox btnAlwaysOnTop;
        private System.Windows.Forms.ToolTip tipTooltip;
        private System.Windows.Forms.RadioButton btnProfile50;
        private System.Windows.Forms.RadioButton btnProfile30;
        private System.Windows.Forms.RadioButton btnProfile40;
        private System.Windows.Forms.RadioButton btnProfile60;
        private System.Windows.Forms.RadioButton btnProfile70;
        private System.Windows.Forms.RadioButton btnProfile80;
        private System.Windows.Forms.RadioButton btnProfile90;
        private System.Windows.Forms.ToolStripMenuItem mnuProfile50;
        private System.Windows.Forms.ToolStripMenuItem mnuProfile30;
        private System.Windows.Forms.ToolStripMenuItem mnuProfile40;
        private System.Windows.Forms.ToolStripMenuItem mnuProfile60;
        private System.Windows.Forms.ToolStripMenuItem mnuProfile70;
        private System.Windows.Forms.ToolStripMenuItem mnuProfile80;
        private System.Windows.Forms.ToolStripMenuItem mnuProfile90;
        private System.Windows.Forms.Timer tmrGui;
    }
}

