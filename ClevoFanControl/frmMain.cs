using System;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;



namespace ClevoFanControl {
    public partial class frmMain : Form {

        private const int EC_POLL_INTERVAL = 10000; // interval to poll EC (increased to 10s to prevent EC timeouts)

        // Semaphore to serialize EC operations and prevent timeouts
        private static readonly SemaphoreSlim ecSemaphore = new SemaphoreSlim(1, 1);

        private IFanControl fan;

        int prevFanPercentage = -1;

        int lastWLeft;
        int lastWTop;

        FanTable maxFanTable;
        FanTable halfFanTable;
        FanTable thirtyFanTable;
        FanTable fortyFanTable;
        FanTable sixtyFanTable;
        FanTable seventyFanTable;
        FanTable eightyFanTable;
        FanTable ninetyFanTable;

        FanTable currentFanTable;

        private bool profileSwitchOverride = false;

        private void SetActiveProfile(FanTable table, ToolStripMenuItem activeMenu) {
            currentFanTable = table;

            // Uncheck all menu items
            mnuProfile30.Checked = false;
            mnuProfile40.Checked = false;
            mnuProfile50.Checked = false;
            mnuProfile60.Checked = false;
            mnuProfile70.Checked = false;
            mnuProfile80.Checked = false;
            mnuProfile90.Checked = false;
            mnuProfileMax.Checked = false;

            // Check the active menu item
            activeMenu.Checked = true;
            profileSwitchOverride = true;
        }

        private string GetCurrentProfileId() {
            if (btnProfile30.Checked) return "30";
            if (btnProfile40.Checked) return "40";
            if (btnProfile50.Checked) return "50";
            if (btnProfile60.Checked) return "60";
            if (btnProfile70.Checked) return "70";
            if (btnProfile80.Checked) return "80";
            if (btnProfile90.Checked) return "90";
            if (btnProfileMax.Checked) return "100";
            return "100"; // Default
        }

        private void LoadProfile(string profileId) {
            switch (profileId) {
                case "30":
                    btnProfile30.Checked = true;
                    break;
                case "40":
                    btnProfile40.Checked = true;
                    break;
                case "50":
                    btnProfile50.Checked = true;
                    break;
                case "60":
                    btnProfile60.Checked = true;
                    break;
                case "70":
                    btnProfile70.Checked = true;
                    break;
                case "80":
                    btnProfile80.Checked = true;
                    break;
                case "90":
                    btnProfile90.Checked = true;
                    break;
                default:
                    btnProfileMax.Checked = true;
                    break;
            }
        }

        public frmMain() {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) {

            fan = new ClevoEcInfo();

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledExceptionHandler);

            // Ensure button text is correct
            btnProfileMax.Text = "100%";

            maxFanTable = FanTable.CreateConstant(100);
            halfFanTable = FanTable.CreateConstant(50);
            thirtyFanTable = FanTable.CreateConstant(30);
            fortyFanTable = FanTable.CreateConstant(40);
            sixtyFanTable = FanTable.CreateConstant(60);
            seventyFanTable = FanTable.CreateConstant(70);
            eightyFanTable = FanTable.CreateConstant(80);
            ninetyFanTable = FanTable.CreateConstant(90);

            LoadFanTableAndConfig();

            // Set currentFanTable after loading config so the saved profile is used
            // If no CheckedChanged event fired, default to max for safety
            if (currentFanTable == null) {
                currentFanTable = maxFanTable;
            }

            tmrMain.Interval = EC_POLL_INTERVAL;
            tmrMain.Enabled = true;

            WindowState = FormWindowState.Minimized;
            ShowInTaskbar = false;
            Visible = false;
        }

        private void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs args) {
            SetFansToMaximum();
            MessageBox.Show("An unexpected error has occurred, fans have been set to 100% for safety.", "Clevo Fan Control Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void tmrMain_Tick(object sender, EventArgs e) {
            // Get target fan speed from current profile (both fans use same profile)
            int targetFanSpeed = currentFanTable.Speed;

            // Apply changes immediately if user switched profiles or speed changed
            if (profileSwitchOverride || targetFanSpeed != prevFanPercentage) {
                SetFanSpeedWithDelay(1, targetFanSpeed);
                SetFanSpeedWithDelay(2, targetFanSpeed);

                prevFanPercentage = targetFanSpeed;
                profileSwitchOverride = false;
            }
        }

        private void SetFanSpeedWithDelay(int fanNr, int speed) {
            Thread.Sleep(300);
            ecSemaphore.Wait();
            try {
                fan?.SetFanSpeed(fanNr, speed);
                Thread.Sleep(500);
            } finally {
                ecSemaphore.Release();
            }
        }

        private void SetFansToMaximum() {
            ecSemaphore.Wait();
            try {
                fan?.SetFanSpeed(1, 100);
                Thread.Sleep(500);
            } finally {
                ecSemaphore.Release();
            }

            Thread.Sleep(300); // Spacing between operations

            ecSemaphore.Wait();
            try {
                fan?.SetFanSpeed(2, 100);
                Thread.Sleep(500);
            } finally {
                ecSemaphore.Release();
            }
        }

        private void UpdateGui() {
            // Skip GUI updates entirely when minimized or hidden - saves CPU cycles
            if (WindowState == FormWindowState.Minimized || !Visible) {
                return;
            }

            // Update fan speed display (since both fans run at same speed, just show one value)
            lblFan.Text = prevFanPercentage + "%";

            // Update the tray tooltip with current fan speed
            icoTray.Text = $"Fan Speed: {prevFanPercentage}%";
        }

        private void LoadFanTableAndConfig() {

            var configFile = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\ClevoFanControl.cfg";

            if (!File.Exists(configFile)) {
                SaveFanTableAndConfig();
                return;
            }

            int wLeft = 0, wTop = 0;

            try {
                using (var sw = new StreamReader(configFile)) {

                    var profile = sw.ReadLine();
                    LoadProfile(profile);

                    wLeft = Convert.ToInt32(sw.ReadLine());
                    wTop = Convert.ToInt32(sw.ReadLine());

                    lastWLeft = wLeft;
                    lastWTop = wTop;

                    btnAlwaysOnTop.Checked = Convert.ToBoolean(sw.ReadLine());
                }
            } catch { }

            Left = wLeft;
            Top = wTop;

            if (!IsOnScreen(this)) {
                wLeft = (Screen.PrimaryScreen.Bounds.Width / 2) - (this.ClientSize.Width / 2);
                wTop = (Screen.PrimaryScreen.Bounds.Height / 2) - (this.ClientSize.Height / 2);
                lastWLeft = wLeft;
                lastWTop = wTop;
                Left = wLeft;
                Top = wTop;
            }

        }

        private void SaveFanTableAndConfig() {

            var path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\";

            using (var sw = new StreamWriter(path + "ClevoFanControl.cfg")) {

                sw.WriteLine(GetCurrentProfileId());

                if (Left > 10000 && Top > 10000) {
                    sw.WriteLine(Left);
                    sw.WriteLine(Top);
                } else {
                    sw.WriteLine(lastWLeft);
                    sw.WriteLine(lastWTop);
                }

                sw.WriteLine(btnAlwaysOnTop.Checked);
            }

        }
        private void ShowWindow() {
            Show();
            WindowState = FormWindowState.Normal;
            ShowInTaskbar = true;
            ClientSize = new Size(327, 260);
        }
        private void ExitApp() {
            tmrMain.Enabled = false;
            fan?.SetFansAuto(0);
            fan?.SetFansAuto(1);
            fan?.SetFansAuto(2);
            fan?.Dispose();
            SaveFanTableAndConfig();
            Close();
            Application.Exit();
            Environment.Exit(1);
        }

        public bool IsOnScreen(Form form) {
            Screen[] screens = Screen.AllScreens;
            foreach (Screen screen in screens) {
                Rectangle formRectangle = new Rectangle(form.Left, form.Top,
                                                         form.Width, form.Height);

                if (screen.WorkingArea.Contains(formRectangle)) {
                    return true;
                }
            }

            return false;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e) {
            if (e.CloseReason != CloseReason.WindowsShutDown) {
                e.Cancel = true;
                WindowState = FormWindowState.Minimized;
                ShowInTaskbar = false;
                FormBorderStyle = FormBorderStyle.None;
                Visible = false;
            }
        }

        private void mnuExit_Click(object sender, EventArgs e) {
            ExitApp();
        }

        private void mnuShowWindow_Click(object sender, EventArgs e) {
            ShowWindow();
        }

        private void icoTray_DoubleClick(object sender, EventArgs e) {
            ShowWindow();
        }

        private void btnProfileMax_CheckedChanged(object sender, EventArgs e) {
            if (btnProfileMax.Checked) {
                SetActiveProfile(maxFanTable, mnuProfileMax);
            }
        }

        private void btnProfile50_CheckedChanged(object sender, EventArgs e) {
            if (btnProfile50.Checked) {
                SetActiveProfile(halfFanTable, mnuProfile50);
            }
        }

        private void btnProfile30_CheckedChanged(object sender, EventArgs e) {
            if (btnProfile30.Checked) {
                SetActiveProfile(thirtyFanTable, mnuProfile30);
            }
        }

        private void btnProfile60_CheckedChanged(object sender, EventArgs e) {
            if (btnProfile60.Checked) {
                SetActiveProfile(sixtyFanTable, mnuProfile60);
            }
        }

        private void btnProfile70_CheckedChanged(object sender, EventArgs e) {
            if (btnProfile70.Checked) {
                SetActiveProfile(seventyFanTable, mnuProfile70);
            }
        }

        private void btnProfile40_CheckedChanged(object sender, EventArgs e) {
            if (btnProfile40.Checked) {
                SetActiveProfile(fortyFanTable, mnuProfile40);
            }
        }

        private void btnProfile80_CheckedChanged(object sender, EventArgs e) {
            if (btnProfile80.Checked) {
                SetActiveProfile(eightyFanTable, mnuProfile80);
            }
        }

        private void btnProfile90_CheckedChanged(object sender, EventArgs e) {
            if (btnProfile90.Checked) {
                SetActiveProfile(ninetyFanTable, mnuProfile90);
            }
        }


        private void mnuProfileMax_Click(object sender, EventArgs e) {
            btnProfileMax.Checked = true;
            SaveFanTableAndConfig();
        }
        private void mnuProfile50_Click(object sender, EventArgs e) {
            btnProfile50.Checked = true;
            SaveFanTableAndConfig();
        }

        private void mnuProfile30_Click(object sender, EventArgs e) {
            btnProfile30.Checked = true;
            SaveFanTableAndConfig();
        }

        private void mnuProfile60_Click(object sender, EventArgs e) {
            btnProfile60.Checked = true;
            SaveFanTableAndConfig();
        }

        private void mnuProfile70_Click(object sender, EventArgs e) {
            btnProfile70.Checked = true;
            SaveFanTableAndConfig();
        }

        private void mnuProfile40_Click(object sender, EventArgs e) {
            btnProfile40.Checked = true;
            SaveFanTableAndConfig();
        }

        private void mnuProfile80_Click(object sender, EventArgs e) {
            btnProfile80.Checked = true;
            SaveFanTableAndConfig();
        }

        private void mnuProfile90_Click(object sender, EventArgs e) {
            btnProfile90.Checked = true;
            SaveFanTableAndConfig();
        }


        private void frmMain_LocationChanged(object sender, EventArgs e) {
            if (WindowState != FormWindowState.Minimized) {
                lastWLeft = Left;
                lastWTop = Top;
                SaveFanTableAndConfig();
                ShowInTaskbar = true;
                FormBorderStyle = FormBorderStyle.FixedSingle;
                Visible = true;
            }
        }
        private void btnExit_Click(object sender, EventArgs e) {
            ExitApp();
        }

        private void btnAlwaysOnTop_CheckedChanged(object sender, EventArgs e) {
            TopMost = btnAlwaysOnTop.Checked;
        }

        private void tmrGui_Tick(object sender, EventArgs e) {
            UpdateGui();
        }

    }

    class FanTable {
        public int Speed;

        public static FanTable CreateConstant(int speed) {
            return new FanTable { Speed = speed };
        }
    }
}
