using System;
using System.Drawing;
using System.IO;
using System.Management;
using System.Threading;
using System.Windows.Forms;



namespace ClevoFanControl {
    public partial class frmMain : Form {

        private const int EC_POLL_INTERVAL = 10000; // interval to poll EC (increased to 10s to prevent EC timeouts)

        // Semaphore to serialize EC operations and prevent timeouts
        private static readonly SemaphoreSlim ecSemaphore = new SemaphoreSlim(1, 1);

        private IFanControl fan;

        int prevFanCPUPercentage = -1;
        int prevFanGPUPercentage = -1;

        int lastWLeft;
        int lastWTop;

        int currentCpuTemp;
        int currentGpuTemp;
        int prevCpuTemp;
        int prevGpuTemp;

        FanTable maxFanTable;
        FanTable halfFanTable;
        FanTable thirtyFanTable;
        FanTable fortyFanTable;
        FanTable sixtyFanTable;
        FanTable seventyFanTable;
        FanTable eightyFanTable;
        FanTable ninetyFanTable;

        FanTable userCpuFanTable = new FanTable();
        FanTable userGpuFanTable = new FanTable();

        FanTable cpuFanTable;
        FanTable gpuFanTable;

        string lastOnlineProfile = "";

        // --- Constants for hold duration ---
        private const int MIN_PEAK_HOLD_TICKS = 3;   // 3 ticks * 10 sec = 30 seconds hold before allowing a drop

        // --- Cached Font to avoid allocations ---
        private readonly Font openSans24 = new Font("Open Sans", 24);

        // --- Cached WMI searcher to avoid recreating every temperature read ---
        private ManagementObjectSearcher wmiTempSearcher;

        // --- Hold counters for CPU and GPU (for descending fan speeds) ---
        private int cpuHoldCounter = 0;
        private int gpuHoldCounter = 0;

        // --- Flag for immediate profile override ---
        private bool profileSwitchOverride = false;

        public frmMain() {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e) {

            fan = new ClevoEcInfo();

            // Initialize WMI searcher once for reuse
            try {
                wmiTempSearcher = new ManagementObjectSearcher(@"root\WMI", "SELECT * FROM MSAcpi_ThermalZoneTemperature");
            } catch {
                wmiTempSearcher = null;
            }

            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(UnhandledExceptionHandler);

            maxFanTable = FanTable.CreateConstant(100);
            halfFanTable = FanTable.CreateConstant(50);
            thirtyFanTable = FanTable.CreateConstant(30);
            fortyFanTable = FanTable.CreateConstant(40);
            sixtyFanTable = FanTable.CreateConstant(60);
            seventyFanTable = FanTable.CreateConstant(70);
            eightyFanTable = FanTable.CreateConstant(80);
            ninetyFanTable = FanTable.CreateConstant(90);

            cpuFanTable = userCpuFanTable;
            gpuFanTable = userGpuFanTable;

            LoadFanTableAndConfig();

            SetSliderValuesFromTable();

            prgCPUFan.Width = 0;
            prgGPUFan.Width = 0;

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
            // --- Read CPU temperature from WMI (no EC access needed!) ---
            currentCpuTemp = GetCurrentTemperature("CPU");

            // --- Read GPU temperature from EC (only 1 EC read per cycle now) ---
            ecSemaphore.Wait();
            try {
                currentGpuTemp = GetCurrentTemperature("GPU");
                Thread.Sleep(500); // Delay after EC read
            } finally {
                ecSemaphore.Release();
            }

            // --- Compute continuous target fan speeds ---
            int computedCpuFan = GetInterpolatedFanSpeed("CPU", currentCpuTemp);
            int computedGpuFan = GetInterpolatedFanSpeed("GPU", currentGpuTemp);

            // --- GPU override: if GPU > 75°C and its target is higher than CPU's, match CPU fan speed ---
            if (currentGpuTemp > 75 && computedCpuFan < computedGpuFan) {
                computedCpuFan = computedGpuFan;
            }

            // --- Profile switch override: apply changes immediately if user switched profiles ---
            if (profileSwitchOverride) {
                SetFanSpeedWithDelay(1, computedCpuFan);
                SetFanSpeedWithDelay(2, computedGpuFan);

                prevFanCPUPercentage = computedCpuFan;
                prevFanGPUPercentage = computedGpuFan;
                cpuHoldCounter = 0;
                gpuHoldCounter = 0;
                profileSwitchOverride = false;
            } else {
                // --- Apply 28-second hold when reducing fan speeds ---
                if (computedCpuFan < prevFanCPUPercentage) {
                    cpuHoldCounter++;
                    if (cpuHoldCounter < MIN_PEAK_HOLD_TICKS) {
                        // Maintain current speed until hold period expires.
                        computedCpuFan = prevFanCPUPercentage;
                    } else {
                        cpuHoldCounter = 0; // Hold period complete; allow reduction.
                    }
                } else {
                    cpuHoldCounter = 0;
                }

                if (computedGpuFan < prevFanGPUPercentage) {
                    gpuHoldCounter++;
                    if (gpuHoldCounter < MIN_PEAK_HOLD_TICKS) {
                        computedGpuFan = prevFanGPUPercentage;
                    } else {
                        gpuHoldCounter = 0;
                    }
                } else {
                    gpuHoldCounter = 0;
                }

                // --- Direct fan speed changes (no ramping) ---
                if (computedCpuFan != prevFanCPUPercentage) {
                    SetFanSpeedWithDelay(1, computedCpuFan);
                    prevFanCPUPercentage = computedCpuFan;
                }

                if (computedGpuFan != prevFanGPUPercentage) {
                    SetFanSpeedWithDelay(2, computedGpuFan);
                    prevFanGPUPercentage = computedGpuFan;
                }
            }
        }

        private int GetCurrentTemperature(string device) {
            try {
                if (device == "CPU") {
                    // Use cached WMI searcher (much faster than creating new one each time)
                    if (wmiTempSearcher != null) {
                        foreach (ManagementObject obj in wmiTempSearcher.Get()) {
                            var temp = Convert.ToDouble(obj["CurrentTemperature"]);
                            // WMI returns temperature in tenths of Kelvin, convert to Celsius
                            var celsius = (int)((temp - 2732) / 10.0);
                            if (celsius > 0 && celsius < 120) {
                                prevCpuTemp = celsius;
                                return celsius;
                            }
                        }
                    }
                    // Fallback to previous value if WMI fails
                    return prevCpuTemp > 0 ? prevCpuTemp : 50;
                } else {
                    // For GPU, still use EC for now (could be replaced with NVML/AMD libs later)
                    var temp = fan?.GetECData(2).Remote ?? 0;
                    if (temp > 0 && temp <= 100) {
                        prevGpuTemp = (int)temp;
                        return (int)temp;
                    }
                    return prevGpuTemp > 0 ? prevGpuTemp : 40;
                }
            } catch {
                // If anything fails, return previous safe value
                return device == "CPU" ? (prevCpuTemp > 0 ? prevCpuTemp : 50) : (prevGpuTemp > 0 ? prevGpuTemp : 40);
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

            // CPU display
            lblCPUTemp.Text = currentCpuTemp + "°";
            lblCPUFan.Text = prevFanCPUPercentage + "%";
            prgCPUFan.Width = (int)((prevFanCPUPercentage / 100.0) * (prgCPUFanContainer.Width - 4));

            // GPU display
            if (SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Online) {
                if (currentGpuTemp > 20) {
                    lblGPUTemp.Text = currentGpuTemp + "°";
                    lblGPUTemp.Font = openSans24;
                    lblGPUHeader.ForeColor = Color.Black;
                    lblGPUTemp.ForeColor = Color.Black;
                    lblGPUFanHeader.ForeColor = Color.Black;
                    lblGPUFan.ForeColor = Color.Black;
                } else {
                    lblGPUTemp.Text = "Asleep";
                    lblGPUTemp.Font = openSans24;
                    lblGPUHeader.ForeColor = Color.DimGray;
                    lblGPUTemp.ForeColor = Color.DimGray;
                    lblGPUFanHeader.ForeColor = Color.DimGray;
                    lblGPUFan.ForeColor = Color.DimGray;
                }
                lblGPUFan.Text = prevFanGPUPercentage + "%";
                prgGPUFan.Width = (int)((prevFanGPUPercentage / 100.0) * (prgGPUFanContainer.Width - 4));
            } else {
                lblGPUTemp.Text = "Batt.";
                lblGPUTemp.Font = openSans24;
            }

            // Update the tray tooltip with current values.
            icoTray.Text = $"CPU\n  Temp: {currentCpuTemp}°\n  Fan: {prevFanCPUPercentage}%\n\nGPU\n" +
                           (currentGpuTemp > 20 ? $"  Temp: {currentGpuTemp}°\n  Fan: {prevFanGPUPercentage}%" : "  Asleep");
        }

        private void SetSliderValuesFromTable() {

            cpuPlot.Value01 = userCpuFanTable.Fan40;
            cpuPlot.Value02 = userCpuFanTable.Fan45;
            cpuPlot.Value03 = userCpuFanTable.Fan50;
            cpuPlot.Value04 = userCpuFanTable.Fan55;
            cpuPlot.Value05 = userCpuFanTable.Fan60;
            cpuPlot.Value06 = userCpuFanTable.Fan65;
            cpuPlot.Value07 = userCpuFanTable.Fan70;
            cpuPlot.Value08 = userCpuFanTable.Fan75;
            cpuPlot.Value09 = userCpuFanTable.Fan80;
            cpuPlot.Value10 = userCpuFanTable.Fan85;

            gpuPlot.Value01 = userGpuFanTable.Fan40;
            gpuPlot.Value02 = userGpuFanTable.Fan45;
            gpuPlot.Value03 = userGpuFanTable.Fan50;
            gpuPlot.Value04 = userGpuFanTable.Fan55;
            gpuPlot.Value05 = userGpuFanTable.Fan60;
            gpuPlot.Value06 = userGpuFanTable.Fan65;
            gpuPlot.Value07 = userGpuFanTable.Fan70;
            gpuPlot.Value08 = userGpuFanTable.Fan75;
            gpuPlot.Value09 = userGpuFanTable.Fan80;
            gpuPlot.Value10 = userGpuFanTable.Fan85;
        }

        private void LoadFanTableAndConfig() {

            var fanCurveFile = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\userfancurve.cfg";

            if (!File.Exists(fanCurveFile)) {
                SaveFanTableAndConfig();
            }

            using (var sw = new StreamReader(fanCurveFile)) {

                userCpuFanTable.Fan40 = Convert.ToInt32(sw.ReadLine());
                userCpuFanTable.Fan45 = Convert.ToInt32(sw.ReadLine());
                userCpuFanTable.Fan50 = Convert.ToInt32(sw.ReadLine());
                userCpuFanTable.Fan55 = Convert.ToInt32(sw.ReadLine());
                userCpuFanTable.Fan60 = Convert.ToInt32(sw.ReadLine());
                userCpuFanTable.Fan65 = Convert.ToInt32(sw.ReadLine());
                userCpuFanTable.Fan70 = Convert.ToInt32(sw.ReadLine());
                userCpuFanTable.Fan75 = Convert.ToInt32(sw.ReadLine());
                userCpuFanTable.Fan80 = Convert.ToInt32(sw.ReadLine());
                userCpuFanTable.Fan85 = Convert.ToInt32(sw.ReadLine());
                cpuFanTable = userCpuFanTable;

                userGpuFanTable.Fan40 = Convert.ToInt32(sw.ReadLine());
                userGpuFanTable.Fan45 = Convert.ToInt32(sw.ReadLine());
                userGpuFanTable.Fan50 = Convert.ToInt32(sw.ReadLine());
                userGpuFanTable.Fan55 = Convert.ToInt32(sw.ReadLine());
                userGpuFanTable.Fan60 = Convert.ToInt32(sw.ReadLine());
                userGpuFanTable.Fan65 = Convert.ToInt32(sw.ReadLine());
                userGpuFanTable.Fan70 = Convert.ToInt32(sw.ReadLine());
                userGpuFanTable.Fan75 = Convert.ToInt32(sw.ReadLine());
                userGpuFanTable.Fan80 = Convert.ToInt32(sw.ReadLine());
                userGpuFanTable.Fan85 = Convert.ToInt32(sw.ReadLine());
                gpuFanTable = userGpuFanTable;

            }

            var configFile = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + "\\ClevoFanControl.cfg";

            if (!File.Exists(configFile)) {
                SaveFanTableAndConfig();
            }

            int wLeft = 0, wTop = 0;

            try {
                using (var sw = new StreamReader(configFile)) {

                    var profile = sw.ReadLine();
                    lastOnlineProfile = profile;
                    if (profile == "1") {
                        btnProfileManual.Checked = true;
                        mnuProfileManual.Checked = true;
                    } else if (profile == "3") {
                        btnProfileMax.Checked = true;
                        mnuProfileMax.Checked = true;
                    } else if (profile == "4") {
                        btnProfile50.Checked = true;
                        mnuProfile50.Checked = true;
                    } else if (profile == "5") {
                        btnProfile30.Checked = true;
                        mnuProfile30.Checked = true;
                    } else if (profile == "6") {
                        btnProfile60.Checked = true;
                        mnuProfile60.Checked = true;
                    } else if (profile == "7") {
                        btnProfile70.Checked = true;
                        mnuProfile70.Checked = true;
                    } else if (profile == "8") {
                        btnProfile40.Checked = true;
                        mnuProfile40.Checked = true;
                    } else if (profile == "9") {
                        btnProfile80.Checked = true;
                        mnuProfile80.Checked = true;
                    } else if (profile == "10") {
                        btnProfile90.Checked = true;
                        mnuProfile90.Checked = true;
                    }

                    wLeft = Convert.ToInt32(sw.ReadLine());
                    wTop = Convert.ToInt32(sw.ReadLine());

                    lastWLeft = wLeft;
                    lastWTop = wTop;

                    btnAlwaysOnTop.Checked = Convert.ToBoolean(sw.ReadLine());
                    // Skip deprecated options: btnACFans, btnGpuBattMonitor, btnManualOnBatt
                    sw.ReadLine(); // btnACFans
                    sw.ReadLine(); // btnGpuBattMonitor
                    sw.ReadLine(); // trkGpuPower (GPU power limit - deprecated)
                    sw.ReadLine(); // btnManualOnBatt

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

            using (var sw = new StreamWriter(path + "userfancurve.cfg")) {

                sw.WriteLine(userCpuFanTable.Fan40);
                sw.WriteLine(userCpuFanTable.Fan45);
                sw.WriteLine(userCpuFanTable.Fan50);
                sw.WriteLine(userCpuFanTable.Fan55);
                sw.WriteLine(userCpuFanTable.Fan60);
                sw.WriteLine(userCpuFanTable.Fan65);
                sw.WriteLine(userCpuFanTable.Fan70);
                sw.WriteLine(userCpuFanTable.Fan75);
                sw.WriteLine(userCpuFanTable.Fan80);
                sw.WriteLine(userCpuFanTable.Fan85);

                sw.WriteLine(userGpuFanTable.Fan40);
                sw.WriteLine(userGpuFanTable.Fan45);
                sw.WriteLine(userGpuFanTable.Fan50);
                sw.WriteLine(userGpuFanTable.Fan55);
                sw.WriteLine(userGpuFanTable.Fan60);
                sw.WriteLine(userGpuFanTable.Fan65);
                sw.WriteLine(userGpuFanTable.Fan70);
                sw.WriteLine(userGpuFanTable.Fan75);
                sw.WriteLine(userGpuFanTable.Fan80);
                sw.WriteLine(userGpuFanTable.Fan85);

            }

            using (var sw = new StreamWriter(path + "ClevoFanControl.cfg")) {

                if (btnProfileManual.Checked) {
                    sw.WriteLine("1");
                } else if (btnProfileMax.Checked) {
                    sw.WriteLine("3");
                } else if (btnProfile50.Checked) {
                    sw.WriteLine("4");
                } else if (btnProfile30.Checked) {
                    sw.WriteLine("5");
                } else if (btnProfile60.Checked) {
                    sw.WriteLine("6");
                } else if (btnProfile70.Checked) {
                    sw.WriteLine("7");
                } else if (btnProfile40.Checked) {
                    sw.WriteLine("8");
                } else if (btnProfile80.Checked) {
                    sw.WriteLine("9");
                } else if (btnProfile90.Checked) {
                    sw.WriteLine("10");
                }

                if (Left > 10000 && Top > 10000) {
                    sw.WriteLine(Left);
                    sw.WriteLine(Top);
                } else {
                    sw.WriteLine(lastWLeft);
                    sw.WriteLine(lastWTop);
                }

                sw.WriteLine(btnAlwaysOnTop.Checked);
                sw.WriteLine(false); // Deprecated: btnACFans
                sw.WriteLine(false); // Deprecated: btnGpuBattMonitor
                sw.WriteLine(80); // Deprecated: trkGpuPower (GPU power limit - write default)
                sw.WriteLine(false); // Deprecated: btnManualOnBatt
            }

        }
        private void ShowWindow() {
            Show();
            WindowState = FormWindowState.Normal;
            ShowInTaskbar = true;
            // Lock form size to prevent auto-resize
            ClientSize = new Size(601, 592);
        }
        private void ExitApp() {
            tmrMain.Enabled = false;
            fan?.SetFansAuto(0);
            fan?.SetFansAuto(1);
            fan?.SetFansAuto(2);
            fan?.Dispose();
            openSans24?.Dispose();
            wmiTempSearcher?.Dispose();
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

        private void cpuBarChanged(object sender, EventArgs e) {
            TrackBar t = (TrackBar)sender;
            if (t.Name == "barCPU40") {
                cpuFanTable.Fan40 = t.Value;
                userCpuFanTable.Fan40 = t.Value;
            } else if (t.Name == "barCPU45") {
                cpuFanTable.Fan45 = t.Value;
                userCpuFanTable.Fan45 = t.Value;
            } else if (t.Name == "barCPU50") {
                cpuFanTable.Fan50 = t.Value;
                userCpuFanTable.Fan50 = t.Value;
            } else if (t.Name == "barCPU55") {
                cpuFanTable.Fan55 = t.Value;
                userCpuFanTable.Fan55 = t.Value;
            } else if (t.Name == "barCPU60") {
                cpuFanTable.Fan60 = t.Value;
                userCpuFanTable.Fan60 = t.Value;
            } else if (t.Name == "barCPU65") {
                cpuFanTable.Fan65 = t.Value;
                userCpuFanTable.Fan65 = t.Value;
            } else if (t.Name == "barCPU70") {
                cpuFanTable.Fan70 = t.Value;
                userCpuFanTable.Fan70 = t.Value;
            } else if (t.Name == "barCPU75") {
                cpuFanTable.Fan75 = t.Value;
                userCpuFanTable.Fan75 = t.Value;
            } else if (t.Name == "barCPU80") {
                cpuFanTable.Fan80 = t.Value;
                userCpuFanTable.Fan80 = t.Value;
            } else if (t.Name == "barCPU85") {
                cpuFanTable.Fan85 = t.Value;
                userCpuFanTable.Fan85 = t.Value;
            }
            SaveFanTableAndConfig();
        }

        private void gpuBarChanged(object sender, EventArgs e) {
            TrackBar t = (TrackBar)sender;
            if (t.Name == "barGPU40") {
                gpuFanTable.Fan40 = t.Value;
                userGpuFanTable.Fan40 = t.Value;
            } else if (t.Name == "barGPU45") {
                gpuFanTable.Fan45 = t.Value;
                userGpuFanTable.Fan45 = t.Value;
            } else if (t.Name == "barGPU50") {
                gpuFanTable.Fan50 = t.Value;
                userGpuFanTable.Fan50 = t.Value;
            } else if (t.Name == "barGPU55") {
                gpuFanTable.Fan55 = t.Value;
                userGpuFanTable.Fan55 = t.Value;
            } else if (t.Name == "barGPU60") {
                gpuFanTable.Fan60 = t.Value;
                userGpuFanTable.Fan60 = t.Value;
            } else if (t.Name == "barGPU65") {
                gpuFanTable.Fan65 = t.Value;
                userGpuFanTable.Fan65 = t.Value;
            } else if (t.Name == "barGPU70") {
                gpuFanTable.Fan70 = t.Value;
                userGpuFanTable.Fan70 = t.Value;
            } else if (t.Name == "barGPU75") {
                gpuFanTable.Fan75 = t.Value;
                userGpuFanTable.Fan75 = t.Value;
            } else if (t.Name == "barGPU80") {
                gpuFanTable.Fan80 = t.Value;
                userGpuFanTable.Fan80 = t.Value;
            } else if (t.Name == "barGPU85") {
                gpuFanTable.Fan85 = t.Value;
                userGpuFanTable.Fan85 = t.Value;
            }
            SaveFanTableAndConfig();
        }

        private void btnProfileManual_CheckedChanged(object sender, EventArgs e) {
            if (btnProfileManual.Checked) {
                cpuFanTable = userCpuFanTable;
                gpuFanTable = userGpuFanTable;
                mnuProfileManual.Checked = true;
                mnuProfileMax.Checked = false;
                mnuProfile50.Checked = false;
                mnuProfile30.Checked = false;
                mnuProfile40.Checked = false;
                mnuProfile60.Checked = false;
                mnuProfile70.Checked = false;
                mnuProfile80.Checked = false;
                mnuProfile90.Checked = false;
                if (SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Online) {
                    lastOnlineProfile = "1";
                }
                profileSwitchOverride = true;
            }
        }

        private void btnProfileMax_CheckedChanged(object sender, EventArgs e) {
            if (btnProfileMax.Checked) {
                cpuFanTable = maxFanTable;
                gpuFanTable = maxFanTable;
                mnuProfileManual.Checked = false;
                mnuProfileMax.Checked = true;
                mnuProfile50.Checked = false;
                mnuProfile30.Checked = false;
                mnuProfile40.Checked = false;
                mnuProfile60.Checked = false;
                mnuProfile70.Checked = false;
                mnuProfile80.Checked = false;
                mnuProfile90.Checked = false;
                if (SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Online) {
                    lastOnlineProfile = "3";
                }
                profileSwitchOverride = true;
            }
        }

        private void btnProfile50_CheckedChanged(object sender, EventArgs e) {
            if (btnProfile50.Checked) {
                cpuFanTable = halfFanTable;
                gpuFanTable = halfFanTable;
                mnuProfileManual.Checked = false;
                mnuProfileMax.Checked = false;
                mnuProfile50.Checked = true;
                mnuProfile30.Checked = false;
                mnuProfile40.Checked = false;
                mnuProfile60.Checked = false;
                mnuProfile70.Checked = false;
                mnuProfile80.Checked = false;
                mnuProfile90.Checked = false;
                if (SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Online) {
                    lastOnlineProfile = "4";
                }
                profileSwitchOverride = true;
            }
        }

        private void btnProfile30_CheckedChanged(object sender, EventArgs e) {
            if (btnProfile30.Checked) {
                cpuFanTable = thirtyFanTable;
                gpuFanTable = thirtyFanTable;
                mnuProfileManual.Checked = false;
                mnuProfileMax.Checked = false;
                mnuProfile50.Checked = false;
                mnuProfile30.Checked = true;
                mnuProfile40.Checked = false;
                mnuProfile60.Checked = false;
                mnuProfile70.Checked = false;
                mnuProfile80.Checked = false;
                mnuProfile90.Checked = false;
                if (SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Online) {
                    lastOnlineProfile = "5";
                }
                profileSwitchOverride = true;
            }
        }

        private void btnProfile60_CheckedChanged(object sender, EventArgs e) {
            if (btnProfile60.Checked) {
                cpuFanTable = sixtyFanTable;
                gpuFanTable = sixtyFanTable;
                mnuProfileManual.Checked = false;
                mnuProfileMax.Checked = false;
                mnuProfile50.Checked = false;
                mnuProfile30.Checked = false;
                mnuProfile40.Checked = false;
                mnuProfile60.Checked = true;
                mnuProfile70.Checked = false;
                mnuProfile80.Checked = false;
                mnuProfile90.Checked = false;
                if (SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Online) {
                    lastOnlineProfile = "6";
                }
                profileSwitchOverride = true;
            }
        }

        private void btnProfile70_CheckedChanged(object sender, EventArgs e) {
            if (btnProfile70.Checked) {
                cpuFanTable = seventyFanTable;
                gpuFanTable = seventyFanTable;
                mnuProfileManual.Checked = false;
                mnuProfileMax.Checked = false;
                mnuProfile50.Checked = false;
                mnuProfile30.Checked = false;
                mnuProfile60.Checked = false;
                mnuProfile70.Checked = true;
                mnuProfile40.Checked = false;
                mnuProfile80.Checked = false;
                mnuProfile90.Checked = false;
                if (SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Online) {
                    lastOnlineProfile = "7";
                }
                profileSwitchOverride = true;
            }
        }

        private void btnProfile40_CheckedChanged(object sender, EventArgs e) {
            if (btnProfile40.Checked) {
                cpuFanTable = fortyFanTable;
                gpuFanTable = fortyFanTable;
                mnuProfileManual.Checked = false;
                mnuProfileMax.Checked = false;
                mnuProfile50.Checked = false;
                mnuProfile30.Checked = false;
                mnuProfile60.Checked = false;
                mnuProfile70.Checked = false;
                mnuProfile40.Checked = true;
                mnuProfile80.Checked = false;
                mnuProfile90.Checked = false;
                if (SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Online) {
                    lastOnlineProfile = "8";
                }
                profileSwitchOverride = true;
            }
        }

        private void btnProfile80_CheckedChanged(object sender, EventArgs e) {
            if (btnProfile80.Checked) {
                cpuFanTable = eightyFanTable;
                gpuFanTable = eightyFanTable;
                mnuProfileManual.Checked = false;
                mnuProfileMax.Checked = false;
                mnuProfile50.Checked = false;
                mnuProfile30.Checked = false;
                mnuProfile60.Checked = false;
                mnuProfile70.Checked = false;
                mnuProfile40.Checked = false;
                mnuProfile80.Checked = true;
                mnuProfile90.Checked = false;
                if (SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Online) {
                    lastOnlineProfile = "9";
                }
                profileSwitchOverride = true;
            }
        }

        private void btnProfile90_CheckedChanged(object sender, EventArgs e) {
            if (btnProfile90.Checked) {
                cpuFanTable = ninetyFanTable;
                gpuFanTable = ninetyFanTable;
                mnuProfileManual.Checked = false;
                mnuProfileMax.Checked = false;
                mnuProfile50.Checked = false;
                mnuProfile30.Checked = false;
                mnuProfile60.Checked = false;
                mnuProfile70.Checked = false;
                mnuProfile40.Checked = false;
                mnuProfile80.Checked = false;
                mnuProfile90.Checked = true;
                if (SystemInformation.PowerStatus.PowerLineStatus == PowerLineStatus.Online) {
                    lastOnlineProfile = "10";
                }
                profileSwitchOverride = true;
            }
        }


        private void mnuProfileManual_Click(object sender, EventArgs e) {
            btnProfileManual.Checked = true;
        }

        private void mnuProfileMax_Click(object sender, EventArgs e) {
            btnProfileMax.Checked = true;
        }
        private void mnuProfile50_Click(object sender, EventArgs e) {
            btnProfile50.Checked = true;
        }

        private void mnuProfile30_Click(object sender, EventArgs e) {
            btnProfile30.Checked = true;
        }

        private void mnuProfile60_Click(object sender, EventArgs e) {
            btnProfile60.Checked = true;
        }

        private void mnuProfile70_Click(object sender, EventArgs e) {
            btnProfile70.Checked = true;
        }

        private void mnuProfile40_Click(object sender, EventArgs e) {
            btnProfile40.Checked = true;
        }

        private void mnuProfile80_Click(object sender, EventArgs e) {
            btnProfile80.Checked = true;
        }

        private void mnuProfile90_Click(object sender, EventArgs e) {
            btnProfile90.Checked = true;
        }


        private void frmMain_LocationChanged(object sender, EventArgs e) {
            if (WindowState != FormWindowState.Minimized) {
                lastWLeft = Left;
                lastWTop = Top;
                SetSliderValuesFromTable();
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

        private void cpuPlot_PlotChanged(object sender, CurveEditorControl.PlotChangedEventArgs e) {
            userCpuFanTable.Fan40 = e.PlotValues[0];
            userCpuFanTable.Fan45 = e.PlotValues[1];
            userCpuFanTable.Fan50 = e.PlotValues[2];
            userCpuFanTable.Fan55 = e.PlotValues[3];
            userCpuFanTable.Fan60 = e.PlotValues[4];
            userCpuFanTable.Fan65 = e.PlotValues[5];
            userCpuFanTable.Fan70 = e.PlotValues[6];
            userCpuFanTable.Fan75 = e.PlotValues[7];
            userCpuFanTable.Fan80 = e.PlotValues[8];
            userCpuFanTable.Fan85 = e.PlotValues[9];

            cpuFanTable.Fan40 = e.PlotValues[0];
            cpuFanTable.Fan45 = e.PlotValues[1];
            cpuFanTable.Fan50 = e.PlotValues[2];
            cpuFanTable.Fan55 = e.PlotValues[3];
            cpuFanTable.Fan60 = e.PlotValues[4];
            cpuFanTable.Fan65 = e.PlotValues[5];
            cpuFanTable.Fan70 = e.PlotValues[6];
            cpuFanTable.Fan75 = e.PlotValues[7];
            cpuFanTable.Fan80 = e.PlotValues[8];
            cpuFanTable.Fan85 = e.PlotValues[9];

            SaveFanTableAndConfig();
        }

        private void gpuPlot_PlotChanged(object sender, CurveEditorControl.PlotChangedEventArgs e) {
            userGpuFanTable.Fan40 = e.PlotValues[0];
            userGpuFanTable.Fan45 = e.PlotValues[1];
            userGpuFanTable.Fan50 = e.PlotValues[2];
            userGpuFanTable.Fan55 = e.PlotValues[3];
            userGpuFanTable.Fan60 = e.PlotValues[4];
            userGpuFanTable.Fan65 = e.PlotValues[5];
            userGpuFanTable.Fan70 = e.PlotValues[6];
            userGpuFanTable.Fan75 = e.PlotValues[7];
            userGpuFanTable.Fan80 = e.PlotValues[8];
            userGpuFanTable.Fan85 = e.PlotValues[9];

            gpuFanTable.Fan40 = e.PlotValues[0];
            gpuFanTable.Fan45 = e.PlotValues[1];
            gpuFanTable.Fan50 = e.PlotValues[2];
            gpuFanTable.Fan55 = e.PlotValues[3];
            gpuFanTable.Fan60 = e.PlotValues[4];
            gpuFanTable.Fan65 = e.PlotValues[5];
            gpuFanTable.Fan70 = e.PlotValues[6];
            gpuFanTable.Fan75 = e.PlotValues[7];
            gpuFanTable.Fan80 = e.PlotValues[8];
            gpuFanTable.Fan85 = e.PlotValues[9];

            SaveFanTableAndConfig();
        }

        private void tmrGui_Tick(object sender, EventArgs e) {
            UpdateGui();
        }

        private int GetInterpolatedFanSpeed(string device, int currentTemp) {
            int[] thresholds;
            int[] fanSpeeds;

            if (device == "CPU") {
                // Define thresholds and corresponding fan speeds for CPU.
                // (For example, if your CPU curve is 0% until 60°C then 30% at 60°C and rising further.)
                thresholds = new int[] { 40, 45, 50, 55, 60, 65, 70, 75, 80, 85 };
                fanSpeeds = new int[]
                {
            cpuFanTable.Fan40,
            cpuFanTable.Fan45,
            cpuFanTable.Fan50,
            cpuFanTable.Fan55,
            cpuFanTable.Fan60,
            cpuFanTable.Fan65,
            cpuFanTable.Fan70,
            cpuFanTable.Fan75,
            cpuFanTable.Fan80,
            cpuFanTable.Fan85
                };
            } else // GPU
              {
                thresholds = new int[] { 40, 45, 50, 55, 60, 65, 70, 75, 80, 85 };
                fanSpeeds = new int[]
                {
            gpuFanTable.Fan40,
            gpuFanTable.Fan45,
            gpuFanTable.Fan50,
            gpuFanTable.Fan55,
            gpuFanTable.Fan60,
            gpuFanTable.Fan65,
            gpuFanTable.Fan70,
            gpuFanTable.Fan75,
            gpuFanTable.Fan80,
            gpuFanTable.Fan85
                };
            }

            if (currentTemp <= thresholds[0])
                return fanSpeeds[0];
            if (currentTemp >= thresholds[thresholds.Length - 1])
                return fanSpeeds[fanSpeeds.Length - 1];

            // Find the two thresholds that bracket the current temperature.
            for (int i = 0; i < thresholds.Length - 1; i++) {
                if (currentTemp >= thresholds[i] && currentTemp < thresholds[i + 1]) {
                    double fraction = (currentTemp - thresholds[i]) / (double)(thresholds[i + 1] - thresholds[i]);
                    double interpolatedFan = fanSpeeds[i] + fraction * (fanSpeeds[i + 1] - fanSpeeds[i]);
                    return (int)Math.Round(interpolatedFan);
                }
            }
            return fanSpeeds[fanSpeeds.Length - 1];
        }

    }

    class FanTable {
        public int Fan40;
        public int Fan45;
        public int Fan50;
        public int Fan55;
        public int Fan60;
        public int Fan65;
        public int Fan70;
        public int Fan75;
        public int Fan80;
        public int Fan85;

        public static FanTable CreateConstant(int speed) {
            return new FanTable {
                Fan40 = speed, Fan45 = speed, Fan50 = speed, Fan55 = speed, Fan60 = speed,
                Fan65 = speed, Fan70 = speed, Fan75 = speed, Fan80 = speed, Fan85 = speed
            };
        }
    }

    public class PlotChangedEventArgs : EventArgs {
        public int[] PlotValues { get; set; }
    }

}
