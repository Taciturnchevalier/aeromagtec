using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Concurrent;
using System.Threading;
using aeromagtec.Utilities;
using aeromagtec.Comms;
using log4net;
using aeromagtec.Controls;
using System.Net.Sockets;
using System.Net;
using leomon;
using DoWorkEventHandler = System.ComponentModel.DoWorkEventHandler;
using System.Globalization;
using System.Collections.Generic;

namespace aeromagtec
{
    public partial class MainV2 : Form
    {
        public static List<LinkInterface> Comports = new List<LinkInterface>();

        public static menuicons displayicons = new burntkermitmenuicons();

        //40->100
        public static int errorcount = 0;

        /// <summary>
        /// used to call anything as needed.
        /// </summary>
        public static MainV2 instance = null;

        //public static string IPadress = "192.168.0.7";
        public static string IPadress = "127.0.0.1";

        public static bool MONO = false;

        public static string MsgText;

        public static int TCPPort = 5050;
        private Controls.MainSwitcher MyView;

        //public static MainSwitcher View;
        public GCSViews.FlightData FlightData;

        private static readonly ILog log =
                                                                                                                                                                    LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static LinkInterface _comPort = new LinkInterface();

        /// <summary>
        /// This 'Control' is the toolstrip control that holds the comport combo, baudrate combo etc
        /// Otiginally seperate controls, each hosted in a toolstip sqaure, combined into this custom
        /// control for layout reasons.
        /// </summary>
        //public static ConnectionControl _connectionControl;
        private static DisplayView _displayConfiguration = new DisplayView().Advanced();

        private string baud = "115200";

        private DateTime connectButtonUpdate = DateTime.Now;

        private bool Connectfalg = true;

        /// <summary>
        /// store the time we first connect
        /// </summary>
        private DateTime connecttime = DateTime.Now;

        private Thread httpthread;

        private bool isExit = false;

        private DateTime lastscreenupdate = DateTime.Now;

        private DateTime OpenTime = DateTime.Now;

        private Thread serialreaderthread;

        /// <summary>
        /// controls the main serial reader thread
        /// </summary>
        private bool serialThread = false;

        private ManualResetEvent SerialThreadrunner = new ManualResetEvent(false);

        private DateTime TimeStart;

        private int totalDataReceivedBytes = 0;

        private int totalSendBytes = 0;

        /// <summary>
        /// Comport name
        /// </summary>
        public static string comPortName = "";

        //定义当前窗体的宽度
        private float x;

        private float y;

        //定义当前窗体的高度
        public MainV2()
        {
            log.Info("Mainv2 create");

            // load config
            // load last saved connection settings
            //LoadConfig(configfilename);

            //ShowAirports = true;

            instance = this;

            //disable dpi scaling
            if (Font.Name != "宋体")
            {
                //Chinese displayed normally when scaling. But would be too small or large using this line of code.
                using (var g = CreateGraphics())
                {
                    Font = new Font(Font.Name, 8.25f * 96f / g.DpiX, Font.Style, Font.Unit, Font.GdiCharSet,
                        Font.GdiVerticalFont);
                }
            }
            InitializeComponent();
            //MyView = new MainSwitcher(this);
            //View = MyView;
            x = this.Width;
            y = this.Height;
            setTag(this);

            //var t = Type.GetType("Mono.Runtime");
            //MONO = (t != null);

            //if (!MONO) // windows only
            //{
            //    if (Settings.Instance["showconsole"] != null && Settings.Instance["showconsole"].ToString() == "True")
            //    {
            //    }
            //    else
            //    {
            //        int win = NativeMethods.FindWindow("ConsoleWindowClass", null);
            //        NativeMethods.ShowWindow(win, NativeMethods.SW_HIDE); // hide window
            //    }

            //    // prevent system from sleeping while program open
            //    var previousExecutionState =
            //        NativeMethods.SetThreadExecutionState(NativeMethods.ES_CONTINUOUS | NativeMethods.ES_SYSTEM_REQUIRED);
            //}

            if (Settings.Instance["showairports"] != null)
            {
                MainV2.ShowAirports = bool.Parse(Settings.Instance["showairports"]);
            }
            try
            {
                log.Info("Create FD");
                FlightData = new GCSViews.FlightData();
                //FlightData.Parent = this;
                FlightData.TopLevel = false;
                this.splitContainer1.Panel1.Controls.Add(FlightData);

                log.Info("Create FP");
                //FlightPlanner = new GCSViews.FlightPlanner();
                //Configuration = new GCSViews.ConfigurationView.Setup();
                log.Info("Create SIM");
                //Simulation = new SITL();
                //Firmware = new GCSViews.Firmware();
                //Terminal = new GCSViews.Terminal();

                //FlightData.Width = MyView.Width;
                //FlightPlanner.Width = MyView.Width;
                //Simulation.Width = MyView.Width;
            }
            catch (ArgumentException e)
            {
                //http://www.microsoft.com/en-us/download/details.aspx?id=16083
                //System.ArgumentException: Font 'Arial' does not support style 'Regular'.

                log.Fatal(e);
                CustomMessageBox.Show(e.ToString() +
                                      "\n\n Font Issues? Please install this http://www.microsoft.com/en-us/download/details.aspx?id=16083");
                //splash.Close();
                //this.Close();
                Application.Exit();
            }
            catch (Exception e)
            {
                log.Fatal(e);
                CustomMessageBox.Show("A Major error has occured : " + e.ToString());
                Application.Exit();
            }
            //set first instance display configuration

            // load old config

            //UpdateTextHandler = new UpdateAcceptTextBoxTextHandler(UpdateText);
            try
            {
                if (!Directory.Exists(Settings.Instance.LogDir))
                    Directory.CreateDirectory(Settings.Instance.LogDir);
            }
            catch (Exception ex) { log.Error(ex); }

            //Microsoft.Win32.SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;

            if (!MONO)
            {
                Microsoft.Win32.RegistryKey installed_versions =
                    Microsoft.Win32.Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP");
                string[] version_names = installed_versions.GetSubKeyNames();
                //version names start with 'v', eg, 'v3.5' which needs to be trimmed off before conversion
                double Framework = Convert.ToDouble(version_names[version_names.Length - 1].Remove(0, 1),
                    CultureInfo.InvariantCulture);
                int SP =
                    Convert.ToInt32(installed_versions.OpenSubKey(version_names[version_names.Length - 1])
                        .GetValue("SP", 0));

                if (Framework < 4.0)
                {
                    CustomMessageBox.Show("This program requires .NET Framework 4.0. You currently have " + Framework);
                }
            }

            Application.DoEvents();
            Application.DoEvents();

            Comports.Add(comPort);

            MainV2.comPort.MavChanged += comPort_MavChanged;

            // save config to test we have write access
            SaveConfig();
            //SaveConfig();

            //DataconnectWork.DoWork += new DoWorkEventHandler(DataconnectWork_DoWork);
            //DataconnectWork.RunWorkerCompleted +=
            //     new RunWorkerCompletedEventHandler(DataconnectWork_RunWorkerCompleted);
        }

        /// <summary>
        /// Active Comport interface
        /// </summary>
        public static LinkInterface comPort
        {
            get
            {
                return _comPort;
            }
            set
            {
                if (_comPort == value)
                    return;
                _comPort = value;
                _comPort = value;
                _comPort.MavChanged -= instance.comPort_MavChanged;
                _comPort.MavChanged += instance.comPort_MavChanged;
                instance.comPort_MavChanged(null, null);
            }
        }

        private void comPort_MavChanged(object sender, EventArgs e)
        {
            log.Info("Mav Changed " + MainV2.comPort.MAV.sysid);

            HUD.Custom.src = MainV2.comPort.MAV.cs;

            //CustomWarning.defaultsrc = MainV2.comPort.MAV.cs;

            if (this.InvokeRequired)
            {
                this.Invoke((MethodInvoker)delegate
                {
                    instance.MyView.Reload();
                });
            }
            else
            {
                instance.MyView.Reload();
            }
        }

        private void LoadConfig()
        {
            try
            {
                log.Info("Loading config");

                Settings.Instance.Load();

                comPortName = Settings.Instance.ComPort;
            }
            catch (Exception ex)
            {
                log.Error("Bad Config File", ex);
            }
        }

        private void SaveConfig()
        {
            try
            {
                log.Info("Saving config");
                Settings.Instance.ComPort = comPortName;

                Settings.Instance.Save();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(ex.ToString());
            }
        }

        public static bool IsConnected { get; set; }

        //public static DisplayView DisplayConfiguration
        //{
        //    get { return _displayConfiguration; }
        //    set
        //    {
        //        _displayConfiguration = value;
        //        Settings.Instance["displayview"] = _displayConfiguration.ConvertToString();
        //        if (LayoutChanged != null)
        //            LayoutChanged(null, EventArgs.Empty);
        //    }
        //}
        public static bool ShowAirports { get; set; }

        //public static event EventHandler LayoutChanged;
        public static bool ShowTFR { get; set; }

        public void doConnect(LinkInterface comPort, string portname)
        {
            bool skipconnectcheck = false;

            switch (portname)
            {
                case "preset":
                    skipconnectcheck = true;

                    break;

                case "TCP":
                    comPort.BaseStream = new TcpSerial();

                    break;

                case "UDP":
                    comPort.BaseStream = new UdpSerial();

                    break;

                case "UDPCl":
                    comPort.BaseStream = new UdpSerialConnect();

                    break;

                case "AUTO":
                    // do autoscan
                    Comms.CommsSerialScan.Scan(true);
                    DateTime deadline = DateTime.Now.AddSeconds(50);
                    while (Comms.CommsSerialScan.foundport == false)
                    {
                        System.Threading.Thread.Sleep(100);

                        if (DateTime.Now > deadline || Comms.CommsSerialScan.run == 0)
                        {
                            CustomMessageBox.Show(Strings.Timeout);

                            return;
                        }
                    }
                    return;

                default:
                    comPort.BaseStream = new SerialPort();
                    break;
            }

            // Tell the connection UI that we are now connected.
            //_connectionControl.IsConnected(true);

            // Here we want to reset the connection stats counter etc.

            comPort.MAV.cs.ResetInternals();

            //cleanup any log being played
            comPort.logreadmode = false;
            if (comPort.logplaybackfile != null)
                comPort.logplaybackfile.Close();
            comPort.logplaybackfile = null;

            try
            {
                log.Info("Set Portname");
                // set port, then options
                if (portname.ToLower() != "preset")
                    comPort.BaseStream.PortName = portname;

                log.Info("Set Baudrate");
                try
                {
                    if (baud != "" && baud != "0")
                        comPort.BaseStream.BaudRate = int.Parse(baud);
                }
                catch (Exception exp)
                {
                    log.Error(exp);
                }
                // prevent serialreader from doing anything
                comPort.giveComport = true;

                log.Info("About to do dtr if needed");
                // reset on connect logic.
                if (Settings.Instance.GetBoolean("CHK_resetapmonconnect") == true)
                {
                    log.Info("set dtr rts to false");
                    comPort.BaseStream.DtrEnable = false;
                    comPort.BaseStream.RtsEnable = false;

                    comPort.BaseStream.toggleDTR();
                }

                comPort.giveComport = false;

                // setup to record new logs
                try
                {
                    Directory.CreateDirectory(Settings.Instance.LogDir);
                    lock (this)
                    {
                        // create log names
                        var dt = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss");
                        var tlog = Settings.Instance.LogDir + Path.DirectorySeparatorChar +
                                   dt + ".tlog";
                        var rlog = Settings.Instance.LogDir + Path.DirectorySeparatorChar +
                                   dt + ".rlog";

                        // check if this logname already exists
                        int a = 1;
                        while (File.Exists(tlog))
                        {
                            Thread.Sleep(1000);
                            // create new names with a as an index
                            dt = DateTime.Now.ToString("yyyy-MM-dd HH-mm-ss") + "-" + a.ToString();
                            tlog = Settings.Instance.LogDir + Path.DirectorySeparatorChar +
                                   dt + ".tlog";
                            rlog = Settings.Instance.LogDir + Path.DirectorySeparatorChar +
                                   dt + ".rlog";
                        }

                        //open the logs for writing
                        comPort.logfile =
                            new BufferedStream(File.Open(tlog, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None));
                        comPort.rawlogfile =
                            new BufferedStream(File.Open(rlog, FileMode.CreateNew, FileAccess.ReadWrite, FileShare.None));
                        log.Info("creating logfile " + dt + ".tlog");
                    }
                }
                catch (Exception exp2)
                {
                    log.Error(exp2);
                    CustomMessageBox.Show(Strings.Failclog);
                } // soft fail

                // reset connect time - for timeout functions
                connecttime = DateTime.Now;

                // do the connect
                comPort.Open(skipconnectcheck);

                if (!comPort.BaseStream.IsOpen)
                {
                    log.Info("comport is closed. existing connect");
                    try
                    {
                        IsConnected = false;
                        UpdateConnectIcon();
                        comPort.Close();
                    }
                    catch
                    {
                    }
                    return;
                }

                // set connected icon
                this.MenuConnect.Image = displayicons.disconnect;
            }
            catch (Exception ex)
            {
                log.Warn(ex);
                try
                {
                    IsConnected = false;
                    UpdateConnectIcon();
                    comPort.Close();
                }
                catch (Exception ex2)
                {
                    log.Warn(ex2);
                }
                CustomMessageBox.Show("Can not establish a connection\n\n" + ex.Message);
                return;
            }
        }

        public void doDisconnect(LinkInterface comPort)
        {
            log.Info("We are disconnecting");

            try
            {
                comPort.BaseStream.DtrEnable = false;
                comPort.Close();
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

            // now that we have closed the connection, cancel the connection stats
            // so that the 'time connected' etc does not grow, but the user can still
            // look at the now frozen stats on the still open form

            try
            {
                System.Threading.ThreadPool.QueueUserWorkItem((WaitCallback)delegate
               {
                   try
                   {
                       aeromagtec.Log.LogSort.SortLogs(Directory.GetFiles(Settings.Instance.LogDir, "*.tlog"));
                   }
                   catch
                   {
                   }
               }
                    );
            }
            catch
            {
            }

            this.MenuConnect.Image = global::aeromagtec.Properties.Resources.light_connect_icon;
        }

        /// <summary>
        /// 开始采集
        /// </summary>
        public void Startsynacp()
        {
            //timer_send.Enabled = false;
            //if (DataBinfswrite == null)
            //{
            //    OpenSaveUartDateFile();
            //}

            ////自动配置
            ////SetShow_sps();

            //ClearQueue();

            //errorcount = 0;

            //byte startflag = 0xa4;
            //DataPortSendCmd(startflag);
            //timer_send.Enabled = true;
            //Thread.Sleep(1000);
        }

        public void Stopacq()
        {
            ////SendFlag = true;
            //byte stopflag = 0xA1;
            ////SendMsg[0] = stopflag;
            ////停止命令
            //DataPortSendCmd(stopflag);
            //timer_send.Interval = 1000;
            ////关闭采集文件
            //CloseDateFile();
        }

        /// <summary>
        /// overriding the OnCLosing is a bit cleaner than handling the event, since it
        /// is this object.
        ///
        /// This happens before FormClosed
        /// </summary>
        /// <param name="e"></param>
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            // speed up tile saving on exit
            GMap.NET.GMaps.Instance.CacheOnIdleRead = false;
            GMap.NET.GMaps.Instance.BoostCacheEngine = true;

            log.Info("MainV2_FormClosing");

            Settings.Instance["MainHeight"] = this.Height.ToString();
            Settings.Instance["MainWidth"] = this.Width.ToString();
            Settings.Instance["MainMaximised"] = this.WindowState.ToString();

            Settings.Instance["MainLocX"] = this.Location.X.ToString();
            Settings.Instance["MainLocY"] = this.Location.Y.ToString();

            // close bases connection
            try
            {
                MainV2.comPort.logreadmode = false;
                //if (logfile != null)
                //    logfile.Close();

                //if (rawlogfile != null)
                //    rawlogfile.Close();

                //logfile = null;
                //rawlogfile = null;
            }
            catch
            {
            }

            // close all connections

            Utilities.adsb.Stop();

            GStreamer.StopAll();

            log.Info("closing vlcrender");
            try
            {
                while (vlcrender.store.Count > 0)
                    vlcrender.store[0].Stop();
            }
            catch
            {
            }

            log.Info("closing httpthread");

            // if we are waiting on a socket we need to force an abort
            httpserver.Stop();

            log.Info("sorting tlogs");
            //try
            //{
            //    System.Threading.ThreadPool.QueueUserWorkItem((WaitCallback)delegate
            //    {
            //        try
            //        {
            //            aeromagtec.Log.LogSort.SortLogs(Directory.GetFiles(Settings.Instance.LogDir, "*.tlog"));
            //        }
            //        catch
            //        {
            //        }
            //    }
            //        );
            //}
            //catch
            //{
            //}

            log.Info("closing MyView");

            // close all tabs
            //MyView.Dispose();

            log.Info("closing fd");
            try
            {
                FlightData.Dispose();
            }
            catch
            {
            }
            log.Info("closing fp");
            //try
            //{
            //    FlightPlanner.Dispose();
            //}
            //catch
            //{
            //}
            //log.Info("closing sim");
            //try
            //{
            //    Simulation.Dispose();
            //}
            //catch
            //{
            //}

            //try
            //{
            //    if (comPort.BaseStream.IsOpen)
            //        comPort.Close();
            //}
            //catch
            //{
            //} // i get alot of these errors, the port is still open, but not valid - user has unpluged usb

            // save config
            //SaveConfig();

            //Console.WriteLine(httpthread.IsAlive);

            log.Info("MainV2_FormClosing done");

            if (MONO)
                this.Dispose();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            base.OnFormClosed(e);

            Console.WriteLine("MainV2_FormClosed");
        }

        protected override void OnLoad(EventArgs e)
        {
            //MyView.AddScreen(new MainSwitcher.Screen("FlightData", FlightData, true));
            FlightData = new GCSViews.FlightData();
            FlightData.TopLevel = false;
            FlightData.Show();
            this.splitContainer1.Panel1.Controls.Clear();
            this.splitContainer1.Panel1.Controls.Add(FlightData);

            // for long running tasks using own threads.
            // for short use threadpool
            this.splitContainer1.Panel1.Resize += new EventHandler(splitContainer1_Panel1_Resize);
            this.SuspendLayout();
            //if (Program.Logo != null && Program.name == "VVVVZ")
            //{
            //    this.PerformLayout();
            //    MenuFlightPlanner_Click(this, e);
            //    MainMenu_ItemClicked(this, new ToolStripItemClickedEventArgs(MenuFlightPlanner));
            //}
            //else
            //{
            //    this.PerformLayout();
            //    log.Info("show FlightData");
            //    MenuFlightData_Click(this, e);
            //    log.Info("show FlightData... Done");
            //    MainMenu_ItemClicked(this, new ToolStripItemClickedEventArgs(MenuFlightData));
            //}

            // for long running tasks using own threads.
            // for short use threadpool

            //this.SuspendLayout();

            // setup http server
            try
            {
                log.Info("start http");
                httpthread = new Thread(new httpserver().listernforclients)
                {
                    Name = "tcp connect station",
                    IsBackground = true
                };
                httpthread.Start();
            }
            catch (Exception ex)
            {
                log.Error("Error starting TCP listener thread: ", ex);
                CustomMessageBox.Show(ex.ToString());
            }

            log.Info("start serialreader");
            // setup main serial reader
            serialreaderthread = new Thread(SerialReader)
            {
                IsBackground = true,
                Name = "Main Serial reader",
                Priority = ThreadPriority.AboveNormal
            };
            serialreaderthread.Start();

            //ThreadPool.QueueUserWorkItem(BGLoadAirports);

            //ThreadPool.QueueUserWorkItem(BGCreateMaps);

            //ThreadPool.QueueUserWorkItem(BGGetAlmanac);

            ThreadPool.QueueUserWorkItem(BGgetTFR);
            //数据解析线程
            //myThreadDataParser = new Thread(new ThreadStart(UartDataParser));//数据解析程序
            //myThreadDataParser.IsBackground = true;
            //myThreadDataParser.Priority = ThreadPriority.Highest;
            //myThreadDataParser.Start();

            //GPS数据解析线程

            //if (UDPserverStart == false)
            //{
            //    StartReceiveUDP_GPS();
            //    if (GPSsaveFileFs == null)
            //    {
            //        //OpenSaveGpsFile();
            //    }

            //    //UDPserverStart = true;
            //}

            // myThreadGPSDataParser = new Thread(new ThreadStart(UartGPSDataParser));//GPS数据解析程序
            // myThreadGPSDataParser.IsBackground = true;
            // myThreadGPSDataParser.Start();

            //自动打开两个串口
            //ConPort();
            //绘图设置
            //dataviewReset();
            //computer.Open();
            //开始更新时间和日期标签
            updateDateTimer.Start();
            this.updateDateTimer.Enabled = true;
        }

        private void BGgetTFR(object state)
        {
            try
            {
                tfr.tfrcache = Settings.GetUserDataDirectory() + "tfr.xml";
                tfr.GetTFRs();
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private void BGLoadAirports(object nothing)
        {
            // read airport list
            try
            {
                Utilities.Airports.ReadOurairports(Settings.GetRunningDirectory() +
                                                   "airports.csv");

                Utilities.Airports.checkdups = true;

                //Utilities.Airports.ReadOpenflights(Application.StartupPath + Path.DirectorySeparatorChar + "airports.dat");

                log.Info("Loaded " + Utilities.Airports.GetAirportCount + " airports");
            }
            catch
            {
            }
        }

        private void fullScreenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // full screen
            if (fullScreenToolStripMenuItem.Checked)
            {
                this.TopMost = true;
                this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
                this.WindowState = FormWindowState.Normal;
                this.WindowState = FormWindowState.Maximized;
            }
            else
            {
                this.TopMost = false;
                this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
                this.WindowState = FormWindowState.Maximized;
            }
        }

        private void MainV2_Resize(object sender, EventArgs e)
        {
            // mono - resize is called before the control is created
            float newx = (this.Width) / x;
            float newy = (this.Height) / y;
            x = this.Width;
            y = this.Height;
            //setControls(newx, newy, this);
            this.MainMenu.Width = this.Width - 16;
            splitContainer1.Dock = DockStyle.Fill;
            this.splitContainer1.Anchor = AnchorStyles.Bottom;
            this.splitContainer1.Anchor = AnchorStyles.Left;
            this.splitContainer1.Anchor = AnchorStyles.Right;
            this.splitContainer1.Anchor = AnchorStyles.Top;
            //this.splitContainer1.Width = this.Width - 6;
            this.statusStrip1.Dock = DockStyle.Bottom;
            this.splitContainer1.Height = this.Height - this.MainMenu.Height - 39;
            this.splitContainer1.Width = this.MainMenu.Width - 3;
            this.splitContainer1.SplitterDistance = this.splitContainer1.Height - this.statusStrip1.Height - 4;
            //this.splitContainer1.Width = (int)(newx * splitContainer1.Width);
            //splitContainer1.Height = (int)(newy * splitContainer1.Height);
        }

        private void MenuConnect_Click(object sender, EventArgs e)
        {
            comPort.giveComport = false;
            //------------------------------------------------------------
            //if (Connectfalg)
            //{
            //    ConPort();
            //    Connectfalg = false;
            //    this.MenuConnect.Image = displayicons.connect;
            //}
            //else
            //{
            //    Connectfalg = true;
            //    CloseTwoUart();
            //    this.MenuConnect.Image = displayicons.disconnect;
            //}
            //UpdateConnectIcon();

            //-------------------------------------------------------
            try
            {
                log.Info("Cleanup last logfiles");
                // cleanup from any previous sessions
                if (comPort.logfile != null)
                    comPort.logfile.Close();

                if (comPort.rawlogfile != null)
                    comPort.rawlogfile.Close();
            }
            catch (Exception ex)
            {
                CustomMessageBox.Show(Strings.ErrorClosingLogFile + ex.Message, Strings.ERROR);
            }

            comPort.logfile = null;
            comPort.rawlogfile = null;

            // decide if this is a connect or disconnect
            if (comPort.BaseStream.IsOpen)
            {
                doDisconnect(comPort);
            }
            else
            {
                doConnect(comPort, "TCP");
            }

            //var mav = new LinkInterface();

            //try
            //{
            //    MainV2.instance.doConnect(mav, "TCP");

            //    MainV2.Comports.Add(mav);

            //    //MainV2._connectionControl.UpdateSysIDS();
            //}
            //catch (Exception)
            //{
            //}
        }

        private void MenuFlightData_Click(object sender, EventArgs e)
        {
            //MyView.ShowScreen("FlightData");
            FlightData.Show();
            this.splitContainer1.Panel1.Controls.Clear();
            this.splitContainer1.Panel1.Controls.Add(FlightData);
            //FlightData.Dock = DockStyle.Fill;
        }

        private void SerialReader()
        {
            if (serialThread == true)
                return;
            serialThread = true;

            SerialThreadrunner.Reset();

            int minbytes = 0;

            DateTime speechcustomtime = DateTime.Now;

            DateTime speechlowspeedtime = DateTime.Now;

            DateTime linkqualitytime = DateTime.Now;

            while (serialThread)
            {
                try
                {
                    Thread.Sleep(1); // was 5

                    //try
                    //{
                    //    if (GCSViews.Terminal.comPort is MAVLinkSerialPort)
                    //    {
                    //    }
                    //    else
                    //    {
                    //        if (GCSViews.Terminal.comPort != null && GCSViews.Terminal.comPort.IsOpen)
                    //            continue;
                    //    }
                    //}
                    //catch (Exception ex)
                    //{
                    //    log.Error(ex);
                    //}

                    // update connect/disconnect button and info stats
                    try
                    {
                        UpdateConnectIcon();
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex);
                    }

                    // 30 seconds interval speech options

                    // speech for airspeed alerts

                    // speech altitude warning - message high warning

                    // not doing anything

                    // attenuate the link qualty over time
                    if ((DateTime.Now - MainV2.comPort.MAV.lastvalidpacket).TotalSeconds >= 1)
                    {
                        if (linkqualitytime.Second != DateTime.Now.Second)
                        {
                            MainV2.comPort.MAV.cs.linkqualitygcs = (ushort)(MainV2.comPort.MAV.cs.linkqualitygcs * 0.8f);
                            linkqualitytime = DateTime.Now;

                            // force redraw if there are no other packets are being read
                            GCSViews.FlightData.myhud.Invalidate();
                        }
                    }

                    // data loss warning - wait min of 10 seconds, ignore first 30 seconds of connect, repeat at 5 seconds interval

                    // get home point on armed status change.

                    // send a hb every seconds from gcs to ap

                    // if not connected or busy, sleep and loop
                    if (!comPort.BaseStream.IsOpen || comPort.giveComport == true)
                    {
                        if (!comPort.BaseStream.IsOpen)
                        {
                            // check if other ports are still open
                            foreach (var port in Comports)
                            {
                                if (port.BaseStream.IsOpen)
                                {
                                    Console.WriteLine("Main comport shut, swapping to other mav");
                                    comPort = port;
                                    break;
                                }
                            }
                        }

                        System.Threading.Thread.Sleep(100);
                    }

                    // read the interfaces
                    foreach (var port in Comports.ToArray())
                    {
                        if (!port.BaseStream.IsOpen)
                        {
                            // skip primary interface
                            if (port == comPort)
                                continue;

                            // modify array and drop out
                            Comports.Remove(port);
                            port.Dispose();
                            break;
                        }

                        while (port.BaseStream.IsOpen && port.BaseStream.BytesToRead > minbytes &&
                               port.giveComport == false && serialThread)
                        {
                            try
                            {
                                port.ReadPacket();
                            }
                            catch (Exception ex)
                            {
                                log.Error(ex);
                            }
                        }
                        // update currentstate of sysids on the port
                        foreach (var MAV in port.MAVlist)
                        {
                            try
                            {
                                MAV.cs.UpdateCurrentSettings(null, false, port, MAV);
                            }
                            catch (Exception ex)
                            {
                                log.Error(ex);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    //Tracking.AddException(e);
                    log.Error("Serial Reader fail :" + e.ToString());
                    try
                    {
                        comPort.Close();
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex);
                    }
                }
            }

            Console.WriteLine("SerialReader Done");
            SerialThreadrunner.Set();
        }

        private void setControls(float newx, float newy, Control cons)
        {
            //遍历窗体中的控件，重新设置控件的值
            foreach (Control con in cons.Controls)
            {
                //获取控件的Tag属性值，并分割后存储字符串数组
                if (con.Tag != null)
                {
                    string[] mytag = con.Tag.ToString().Split(new char[] { ';' });
                    //根据窗体缩放的比例确定控件的值
                    con.Width = Convert.ToInt32(System.Convert.ToSingle(mytag[0]) * newx);//宽度
                    con.Height = Convert.ToInt32(System.Convert.ToSingle(mytag[1]) * newy);//高度
                    con.Left = Convert.ToInt32(System.Convert.ToSingle(mytag[2]) * newx);//左边距
                    con.Top = Convert.ToInt32(System.Convert.ToSingle(mytag[3]) * newy);//顶边距
                    //Single currentSize = System.Convert.ToSingle(mytag[4]) * newy;//字体大小
                    //con.Font = new Font(con.Font.Name, currentSize, con.Font.Style, con.Font.Unit);
                    if (con.Controls.Count > 0)
                    {
                        setControls(newx, newy, con);
                    }
                }
            }
        }

        private void setTag(Control cons)
        {
            foreach (Control con in cons.Controls)
            {
                con.Tag = con.Width + ";" + con.Height + ";" + con.Left + ";" + con.Top + ";" + con.Font.Size;
                if (con.Controls.Count > 0)
                {
                    setTag(con);
                }
            }
        }

        //委托
        //数据读取线程
        //--------------------------------------------------------------------------------------------------------
        private void splitContainer1_Panel1_Resize(object sender, EventArgs e)
        {
            if (FlightData != null)
            {
                FlightData.Width = this.splitContainer1.Panel1.Width;
                FlightData.Height = this.splitContainer1.Panel1.Height;
            }
        }

        /// <summary>
        /// Used to fix the icon status for unexpected unplugs etc...
        /// </summary>
        private void UpdateConnectIcon()
        {
            if ((DateTime.Now - connectButtonUpdate).Milliseconds > 500)
            {
                //                        Console.WriteLine(DateTime.Now.Millisecond);
                if (comPort.BaseStream.IsOpen)
                {
                    if ((string)this.MenuConnect.Image.Tag != "Disconnect")
                    {
                        this.BeginInvoke((MethodInvoker)delegate
                        {
                            this.MenuConnect.Image = displayicons.disconnect;
                            //this.MenuConnect.Image.Tag = "Disconnect";
                            this.MenuConnect.Text = Strings.DISCONNECTc;
                            IsConnected = true;
                        });
                    }
                }
                else
                {
                    if (this.MenuConnect.Image != null && (string)this.MenuConnect.Image.Tag != "Connect")
                    {
                        this.BeginInvoke((MethodInvoker)delegate
                        {
                            this.MenuConnect.Image = displayicons.connect;
                            //this.MenuConnect.Image.Tag = "Connect";
                            this.MenuConnect.Text = Strings.CONNECTc;
                            IsConnected = false;
                            //if (_connectionStats != null)
                            //{
                            //    _connectionStats.StopUpdates();
                            //}
                        });
                    }

                    if (comPort.logreadmode)
                    {
                        IsConnected = false;
                    }
                }
                connectButtonUpdate = DateTime.Now;
            }
        }

        private void updateDateTimer_Tick(object sender, EventArgs e)
        {
            if (lastscreenupdate.AddMilliseconds(100) < DateTime.Now)
            {
                toolStripStatusLabel1.Text = MsgText;
            }
            else
            {
                MsgText = "";
            }
            lastscreenupdate = DateTime.Now;
        }

        public class burntkermitmenuicons : menuicons
        {
            public override Image bg
            {
                get { return global::aeromagtec.Properties.Resources.bgdark; }
            }

            public override Image config_tuning
            {
                get { return global::aeromagtec.Properties.Resources.light_tuningconfig_icon; }
            }

            public override Image connect
            {
                get { return global::aeromagtec.Properties.Resources.light_connect_icon; }
            }

            public override Image disconnect
            {
                get { return global::aeromagtec.Properties.Resources.light_disconnect_icon; }
            }

            public override Image donate
            {
                get { return global::aeromagtec.Properties.Resources.donate; }
            }

            public override Image fd
            {
                get { return global::aeromagtec.Properties.Resources.light_flightdata_icon; }
            }

            public override Image fp
            {
                get { return global::aeromagtec.Properties.Resources.light_flightplan_icon; }
            }

            public override Image help
            {
                get { return global::aeromagtec.Properties.Resources.light_help_icon; }
            }

            public override Image initsetup
            {
                get { return global::aeromagtec.Properties.Resources.light_initialsetup_icon; }
            }

            public override Image sim
            {
                get { return global::aeromagtec.Properties.Resources.light_simulation_icon; }
            }

            public override Image terminal
            {
                get { return global::aeromagtec.Properties.Resources.light_terminal_icon; }
            }

            public override Image wizard
            {
                get { return global::aeromagtec.Properties.Resources.wizardicon; }
            }
        }

        public class highcontrastmenuicons : menuicons
        {
            public override Image bg
            {
                get { return null; }
            }

            public override Image config_tuning
            {
                get { return global::aeromagtec.Properties.Resources.dark_tuningconfig_icon; }
            }

            public override Image connect
            {
                get { return global::aeromagtec.Properties.Resources.dark_connect_icon; }
            }

            public override Image disconnect
            {
                get { return global::aeromagtec.Properties.Resources.dark_disconnect_icon; }
            }

            public override Image donate
            {
                get { return global::aeromagtec.Properties.Resources.donate; }
            }

            public override Image fd
            {
                get { return global::aeromagtec.Properties.Resources.dark_flightdata_icon; }
            }

            public override Image fp
            {
                get { return global::aeromagtec.Properties.Resources.dark_flightplan_icon; }
            }

            public override Image help
            {
                get { return global::aeromagtec.Properties.Resources.dark_help_icon; }
            }

            public override Image initsetup
            {
                get { return global::aeromagtec.Properties.Resources.dark_initialsetup_icon; }
            }

            public override Image sim
            {
                get { return global::aeromagtec.Properties.Resources.dark_simulation_icon; }
            }

            public override Image terminal
            {
                get { return global::aeromagtec.Properties.Resources.dark_terminal_icon; }
            }

            public override Image wizard
            {
                get { return global::aeromagtec.Properties.Resources.wizardicon; }
            }
        }

        public abstract class menuicons
        {
            public abstract Image bg { get; }
            public abstract Image config_tuning { get; }
            public abstract Image connect { get; }
            public abstract Image disconnect { get; }
            public abstract Image donate { get; }
            public abstract Image fd { get; }
            public abstract Image fp { get; }
            public abstract Image help { get; }
            public abstract Image initsetup { get; }
            public abstract Image sim { get; }
            public abstract Image terminal { get; }
            public abstract Image wizard { get; }
        }

        //private void ShowConnectionStatsForm()
        //{
        //    if (this.connectionStatsForm == null || this.connectionStatsForm.IsDisposed)
        //    {
        //        // If the form has been closed, or never shown before, we need all new stuff
        //        this.connectionStatsForm = new Form
        //        {
        //            Width = 430,
        //            Height = 180,
        //            MaximizeBox = false,
        //            MinimizeBox = false,
        //            FormBorderStyle = FormBorderStyle.FixedDialog,
        //            Text = Strings.LinkStats
        //        };
        //        // Change the connection stats control, so that when/if the connection stats form is showing,
        //        // there will be something to see
        //        this.connectionStatsForm.Controls.Clear();
        //        _connectionStats = new ConnectionStats(comPort);
        //        this.connectionStatsForm.Controls.Add(_connectionStats);
        //        this.connectionStatsForm.Width = _connectionStats.Width;
        //    }
        //string convertGpstemp = null;
        [StructLayout(LayoutKind.Sequential)]
        internal class DEV_BROADCAST_HDR
        {
            internal Int32 dbch_size;
            internal Int32 dbch_devicetype;
            internal Int32 dbch_reserved;
        }

        private static class NativeMethods
        {
            public const uint ES_CONTINUOUS = 0x80000000;

            public const uint ES_SYSTEM_REQUIRED = 0x00000001;

            static public int SW_HIDE = 0;

            static public int SW_SHOWNORMAL = 1;

            // used to hide/show console window
            [DllImport("user32.dll")]
            public static extern int FindWindow(string szClass, string szTitle);

            [DllImport("kernel32.dll")]
            public static extern uint SetThreadExecutionState(uint esFlags);

            [DllImport("user32.dll")]
            public static extern int ShowWindow(int Handle, int showState);

            [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
            internal static extern IntPtr RegisterDeviceNotification
                (IntPtr hRecipient,
                    IntPtr NotificationFilter,
                    Int32 Flags);

            // Import SetThreadExecutionState Win32 API and necessary flags
        }
    }
}