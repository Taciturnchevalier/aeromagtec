using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using DirectShowLib;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using log4net;
using aeromagtec.Controls;
using aeromagtec.Utilities;
using OpenTK;
using WebCamService;
using ZedGraph;
using GMap.NET.WindowsPresentation;
using GMapMarker = GMap.NET.WindowsForms.GMapMarker;
using aeromagtec.Maps;
using GMapRoute = GMap.NET.WindowsForms.GMapRoute;
using System.Collections.Concurrent;
using System.Data;

//using LogAnalyzer = aeromagtec.Utilities.LogAnalyzer;
//using aeromagtec.Maps;

namespace aeromagtec.GCSViews
{
    public partial class FlightData : MyUserControl, IActivate, IDeactivate
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static bool threadrun;
        private int tickStart;
        private RollingPointPairList list1 = new RollingPointPairList(1200);
        private RollingPointPairList list2 = new RollingPointPairList(1200);
        private RollingPointPairList list3 = new RollingPointPairList(1200);
        private RollingPointPairList list4 = new RollingPointPairList(1200);
        private RollingPointPairList list5 = new RollingPointPairList(1200);
        private RollingPointPairList list6 = new RollingPointPairList(1200);
        private RollingPointPairList list7 = new RollingPointPairList(1200);
        private RollingPointPairList list8 = new RollingPointPairList(1200);
        private RollingPointPairList list9 = new RollingPointPairList(1200);
        private RollingPointPairList list10 = new RollingPointPairList(1200);

        private PropertyInfo list1item;
        private PropertyInfo list2item;
        private PropertyInfo list3item;
        private PropertyInfo list4item;
        private PropertyInfo list5item;
        private PropertyInfo list6item;
        private PropertyInfo list7item;
        private PropertyInfo list8item;
        private PropertyInfo list9item;
        private PropertyInfo list10item;

        private CurveItem list1curve;
        private CurveItem list2curve;
        private CurveItem list3curve;
        private CurveItem list4curve;
        private CurveItem list5curve;
        private CurveItem list6curve;
        private CurveItem list7curve;
        private CurveItem list8curve;
        private CurveItem list9curve;
        private CurveItem list10curve;

        internal static GMapOverlay tfrpolygons;
        public static GMapOverlay kmlpolygons;
        internal static GMapOverlay geofence;
        internal static GMapOverlay rallypointoverlay;
        internal static GMapOverlay photosoverlay;
        internal static GMapOverlay poioverlay = new GMapOverlay("POI"); // poi layer

        //ConcurrentQueue<byte[]> Uartdatacq = LinkInterface.Uartdatacq;

        private List<TabPage> TabListOriginal = new List<TabPage>();

        //private dataExp newdataExp = new dataExp();

        private Thread thisthread;
        private List<PointLatLng> trackPoints = new List<PointLatLng>();

        public static HUD myhud;
        public static myGMAP mymap;

        private bool playingLog;
        private double LogPlayBackSpeed = 1.0;

        private GMapMarker marker;

        //public SplitContainer MainHcopy;

        public static FlightData instance;

        //private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    if (CurrentGMapMarker == null || !(CurrentGMapMarker is GMapMarkerPOI))
        //        return;

        //    POI.POIDelete((GMapMarkerPOI)CurrentGMapMarker);
        //}

        internal GMapMarker CurrentGMapMarker;

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            MainV2.comPort.logreadmode = false;

            if (polygons != null)
                polygons.Dispose();
            if (routes != null)
                routes.Dispose();
            if (route != null)
                route.Dispose();
            if (marker != null)
                marker.Dispose();

            if (disposing && (components != null))
            {
                components.Dispose();
            }
        }

        public FlightData()
        {
            log.Info("Flightdata view Start");

            InitializeComponent();

            log.Info("Components Done");
            instance = this;
            //    _serializer = new DockStateSerializer(dockContainer1);
            //    _serializer.SavePath = Application.StartupPath + Path.DirectorySeparatorChar + "FDscreen.xml";
            //    dockContainer1.PreviewRenderer = new PreviewRenderer();
            //
            mymap = gMapControl1;
            myhud = hud1;
            //mymap.Paint += mymap_Paint;
            //mymap.Manager.UseMemoryCache = false;

            log.Info("Tunning Graph Settings");
            // setup default tuning graph
            if (Settings.Instance["Tuning_Graph_Selected"] != null)
            {
                string line = Settings.Instance["Tuning_Graph_Selected"].ToString();
                string[] lines = line.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string option in lines)
                {
                    using (var cb = new CheckBox { Name = option, Checked = true })
                    {
                        chk_box_CheckedChanged(cb, EventArgs.Empty);
                    }
                }
            }
            else
            {
                using (var cb = new CheckBox { Name = "mag01", Checked = true })
                {
                    chk_box_CheckedChanged(cb, EventArgs.Empty);
                }
                using (var cb = new CheckBox { Name = "mag02", Checked = true })
                {
                    chk_box_CheckedChanged(cb, EventArgs.Empty);
                }
                //using (var cb = new CheckBox { Name = "mx", Checked = true })
                //{
                //    chk_box_CheckedChanged2(cb, EventArgs.Empty);
                //}
                //using (var cb = new CheckBox { Name = "my", Checked = true })
                //{
                //    chk_box_CheckedChanged2(cb, EventArgs.Empty);
                //}
            }

            if (!string.IsNullOrEmpty(Settings.Instance["hudcolor"]))
            {
                hud1.hudcolor = Color.FromName(Settings.Instance["hudcolor"]);
            }

            log.Info("HUD Settings");
            foreach (string item in Settings.Instance.Keys)
            {
                if (item.StartsWith("hud1_useritem_"))
                {
                    string selection = item.Replace("hud1_useritem_", "");

                    CheckBox chk = new CheckBox();
                    chk.Name = selection;
                    chk.Checked = true;

                    HUD.Custom cust = new HUD.Custom();
                    cust.Header = Settings.Instance[item];
                    HUD.Custom.src = MainV2.comPort.MAV.cs;

                    addHudUserItem(ref cust, chk);
                }
            }

            // populate the unmodified base list
            //tabControlactions.TabPages.ForEach(i => { TabListOriginal.Add((TabPage)i); });

            log.Info("Graph Setup");
            CreateChart();

            // config map

            log.Info("Map Setup");
            gMapControl1.MapProvider = GMapProviders.GoogleChinaSatelliteMap;
            gMapControl1.RoutesEnabled = true;
            gMapControl1.CacheLocation = Settings.GetDataDirectory() +
                                             "gmapcache" + Path.DirectorySeparatorChar;
            gMapControl1.SetPositionByKeywords("BeiJing"); // 地图中心位置

            gMapControl1.MinZoom = 0;
            gMapControl1.MaxZoom = 24;
            gMapControl1.Zoom = 3;
            // map events
            gMapControl1.OnMapZoomChanged += gMapControl1_OnMapZoomChanged;

            gMapControl1.DisableFocusOnMouseEnter = true;

            gMapControl1.OnMarkerEnter += gMapControl1_OnMarkerEnter;
            gMapControl1.OnMarkerLeave += gMapControl1_OnMarkerLeave;

            gMapControl1.RoutesEnabled = true;
            gMapControl1.PolygonsEnabled = true;

            tfrpolygons = new GMapOverlay("tfrpolygons");
            gMapControl1.Overlays.Add(tfrpolygons);

            kmlpolygons = new GMapOverlay("kmlpolygons");
            gMapControl1.Overlays.Add(kmlpolygons);

            geofence = new GMapOverlay("geofence");
            gMapControl1.Overlays.Add(geofence);

            polygons = new GMapOverlay("polygons");
            gMapControl1.Overlays.Add(polygons);

            photosoverlay = new GMapOverlay("photos overlay");
            gMapControl1.Overlays.Add(photosoverlay);

            routes = new GMapOverlay("routes");
            gMapControl1.Overlays.Add(routes);

            rallypointoverlay = new GMapOverlay("rally points");
            gMapControl1.Overlays.Add(rallypointoverlay);

            gMapControl1.Overlays.Add(poioverlay);

            //MainV2.comPort.ParamListChanged += FlightData_ParentChanged;
        }

        protected override void OnInvalidated(InvalidateEventArgs e)
        {
            base.OnInvalidated(e);
            updateBindingSourceWork(); //???
        }

        // config map

        private GMapOverlay polygons;
        private GMapOverlay routes;
        private GMap.NET.WindowsForms.GMapRoute route;

        public void Activate()
        {
            log.Info("Activate Called");

            OnResize(EventArgs.Empty);

            if (MainV2.comPort.BaseStream.IsOpen || MainV2.comPort.logreadmode)
                ZedGraphTimer.Start();

            if (MainV2.MONO)
            {
                if (!hud1.Visible)
                    hud1.Visible = true;
                if (!hud1.Enabled)
                    hud1.Enabled = true;
            }

            for (int f = 1; f < 6; f++)
            {
                // load settings
                if (Settings.Instance["quickView" + f] != null)
                {
                    Control[] ctls = Controls.Find("quickView" + f, true);
                    if (ctls.Length > 0)
                    {
                        QuickView QV = (QuickView)ctls[0];

                        // set description and unit
                        string desc = Settings.Instance["quickView" + f];
                        QV.Tag = QV.desc;
                        QV.desc = MainV2.comPort.MAV.cs.GetNameandUnit(desc);

                        // set databinding for value
                        QV.DataBindings.Clear();
                        try
                        {
                            QV.DataBindings.Add(new Binding("number", bindingSource1,
                                Settings.Instance["quickView" + f], false));
                        }
                        catch (Exception ex)
                        {
                            log.Debug(ex);
                        }
                    }
                }
                else
                {
                    // if no config, update description on predefined
                    try
                    {
                        Control[] ctls = Controls.Find("quickView" + f, true);
                        if (ctls.Length > 0)
                        {
                            QuickView QV = (QuickView)ctls[0];
                            string desc = QV.desc;
                            QV.Tag = desc;
                            QV.desc = MainV2.comPort.MAV.cs.GetNameandUnit(desc);
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Debug(ex);
                    }
                }
            }
            // make sure the hud user items/warnings/checklist are using the current state
            HUD.Custom.src = MainV2.comPort.MAV.cs;

            if (Settings.Instance["maplast_lat"] != "")
            {
                try
                {
                    gMapControl1.Position = new PointLatLng(Settings.Instance.GetDouble("maplast_lat"),
                        Settings.Instance.GetDouble("maplast_lng"));
                    if (Math.Round(Settings.Instance.GetDouble("maplast_lat"), 1) == 0)
                    {
                        // no zoom in
                        Zoomlevel.Value = 3;
                    }
                    else
                    {
                        var zoom = Settings.Instance.GetFloat("maplast_zoom");
                        if (Zoomlevel.Maximum < (decimal)zoom)
                            zoom = (float)Zoomlevel.Maximum;
                        Zoomlevel.Value = (decimal)zoom;
                    }
                }
                catch
                {
                }
            }

            hud1.doResize();

            if (!Settings.Instance.ContainsKey("RecordFs"))
            {
                Settings.Instance["RecordFs"] = "160";
            }
            else
            {
                listBox1.SelectedItem = Settings.Instance["RecordFs"];
            }

            if (!Settings.Instance.ContainsKey("badckmodel"))
            {
                Settings.Instance["badckmodel"] = "1";
            }
            else
            {
                listBox2.SelectedItem = Settings.Instance["badckmodel"];
            }
        }

        public void Deactivate()
        {
            if (MainV2.MONO)
            {
                hud1.Dock = DockStyle.None;
                hud1.Size = new Size(5, 5);
                hud1.Enabled = false;
                hud1.Visible = false;
            }
            //     hud1.Location = new Point(-1000,-1000);

            Settings.Instance["maplast_lat"] = gMapControl1.Position.Lat.ToString();
            Settings.Instance["maplast_lng"] = gMapControl1.Position.Lng.ToString();
            Settings.Instance["maplast_zoom"] = gMapControl1.Zoom.ToString();
            Settings.Instance["RecordFs"] = listBox1.SelectedItem.ToString();
            Settings.Instance["badckmodel"] = listBox2.SelectedItem.ToString();
            ZedGraphTimer.Stop();
        }

        private void tabControl1_DrawItem(object sender, DrawItemEventArgs e)
        {
            // Draw the background of the ListBox control for each item.
            //e.DrawBackground();
            // Define the default color of the brush as black.
            Brush myBrush = Brushes.Black;

            LinearGradientBrush linear = new LinearGradientBrush(e.Bounds, Color.FromArgb(0x94, 0xc1, 0x1f),
                Color.FromArgb(0xcd, 0xe2, 0x96), LinearGradientMode.Vertical);

            e.Graphics.FillRectangle(linear, e.Bounds);

            // Draw the current item text based on the current Font
            // and the custom brush settings.
            e.Graphics.DrawString(((TabControl)sender).TabPages[e.Index].Text,
                e.Font, myBrush, e.Bounds, StringFormat.GenericDefault);
            // If the ListBox has focus, draw a focus rectangle around the selected item.
            e.DrawFocusRectangle();
        }

        private void gMapControl1_OnMapZoomChanged()
        {
            //try
            //{
            //    // Exception System.Runtime.InteropServices.SEHException: External component has thrown an exception.
            //    TRK_zoom.Value = (float)gMapControl1.Zoom;
            //    Zoomlevel.Value = Convert.ToDecimal(gMapControl1.Zoom);
            //}
            //catch
            //{
            //}

            center.Position = gMapControl1.Position;
        }

        private void gMapControl1_OnMarkerLeave(GMapMarker item)
        {
            CurrentGMapMarker = null;
        }

        private void gMapControl1_OnMarkerEnter(GMapMarker item)
        {
            CurrentGMapMarker = item;
        }

        private void FlightData_Load(object sender, EventArgs e)
        {
            POI.POIModified += POI_POIModified;

            // update tabs displayed

            gMapControl1.EmptyTileColor = Color.Gray;

            Zoomlevel.Minimum = gMapControl1.MapProvider.MinZoom;
            Zoomlevel.Maximum = 24;
            Zoomlevel.Value = Convert.ToDecimal(gMapControl1.Zoom);

            if (Settings.Instance.ContainsKey("russian_hud"))
            {
                hud1.Russian = Settings.Instance.GetBoolean("russian_hud");
            }

            hud1.doResize();

            thisthread = new Thread(mainloop);
            thisthread.Name = "FD Mainloop";
            thisthread.IsBackground = true;
            thisthread.Start();
        }

        private void tfr_GotTFRs(object sender, EventArgs e)
        {
            Invoke((Action)delegate
            {
                foreach (var item in tfr.tfrs)
                {
                    List<List<PointLatLng>> points = item.GetPaths();

                    foreach (var list in points)
                    {
                        GMap.NET.WindowsForms.GMapPolygon poly = new GMap.NET.WindowsForms.GMapPolygon(list, item.NAME);

                        poly.Fill = new SolidBrush(Color.FromArgb(30, Color.Blue));

                        tfrpolygons.Polygons.Add(poly);
                    }
                }
                tfrpolygons.IsVisibile = MainV2.ShowTFR;
            });
        }

        private void POI_POIModified(object sender, EventArgs e)
        {
            POI.UpdateOverlay(poioverlay);
        }

        private void mainloop()
        {
            threadrun = true;
            EndPoint Remote = new IPEndPoint(IPAddress.Any, 0);

            DateTime tracklast = DateTime.Now.AddSeconds(0);

            DateTime tunning = DateTime.Now.AddSeconds(0);

            DateTime mapupdate = DateTime.Now.AddSeconds(0);

            //DateTime vidrec = DateTime.Now.AddSeconds(0);

            DateTime waypoints = DateTime.Now.AddSeconds(0);

            DateTime updatescreen = DateTime.Now;

            DateTime tsreal = DateTime.Now;
            double taketime = 0;
            double timeerror = 0;

            while (!IsHandleCreated)
                Thread.Sleep(1000);

            while (threadrun)
            {
                // 发送完数据 giveComport= true 需修正这部分代码
                if (MainV2.comPort.giveComport)
                {
                    Thread.Sleep(5);
                    updateBindingSource();
                    continue;
                }

                if (!MainV2.comPort.logreadmode)
                    Thread.Sleep(10); // max is only ever 10 hz but we go a little faster to empty the serial queue

                if (this.IsDisposed)
                {
                    threadrun = false;
                    break;
                }

                // 日志读取 修改
                // log playback
                if (MainV2.comPort.logreadmode && MainV2.comPort.logplaybackfile != null)
                {
                    if (MainV2.comPort.BaseStream.IsOpen)
                    {
                        MainV2.comPort.logreadmode = false;
                        try
                        {
                            MainV2.comPort.logplaybackfile.Close();
                        }
                        catch
                        {
                            log.Error("Failed to close logfile");
                        }
                        MainV2.comPort.logplaybackfile = null;
                    }

                    //Console.WriteLine(DateTime.Now.Millisecond);

                    if (updatescreen.AddMilliseconds(300) < DateTime.Now)
                    {
                        try
                        {
                            updatePlayPauseButton(true);
                            updateLogPlayPosition();
                        }
                        catch
                        {
                            log.Error("Failed to update log playback pos");
                        }
                        updatescreen = DateTime.Now;
                    }

                    //Console.WriteLine(DateTime.Now.Millisecond + " done ");

                    DateTime logplayback = MainV2.comPort.lastlogread;
                    try
                    {
                        if (!MainV2.comPort.BaseStream.IsOpen)
                            MainV2.comPort.ReadPacket();
                        //UartDataParser();

                        //MainV2.comPort.MAV.cs.UpdateCurrentSettings(bindingSource1);
                        //MainV2.comPort.MAV.cs.UpdateCurrentSettings(bindingSource1); //readPacket(logplaybackfile); //读取数据
                        // update currentstate of sysids on the port
                        foreach (var MAV in MainV2.comPort.MAVlist)
                        {
                            try
                            {
                                MAV.cs.UpdateCurrentSettings(null, false, MainV2.comPort, MAV);
                            }
                            catch (Exception ex)
                            {
                                log.Error(ex);
                            }
                        }
                    }
                    catch
                    {
                        log.Error("Failed to read data packet");
                    }

                    double act = (lastlogread - logplayback).TotalMilliseconds;

                    if (act > 9999 || act < 0)
                        act = 0;

                    double ts = 0;
                    if (LogPlayBackSpeed == 0)
                        LogPlayBackSpeed = 0.01;
                    try
                    {
                        ts = Math.Min((act / LogPlayBackSpeed), 1000);
                    }
                    catch
                    {
                    }

                    double timetook = (DateTime.Now - tsreal).TotalMilliseconds;
                    if (timetook != 0)
                    {
                        //Console.WriteLine("took: " + timetook + "=" + taketime + " " + (taketime - timetook) + " " + ts);
                        //Console.WriteLine(MainV2.comPort.lastlogread.Second + " " + DateTime.Now.Second + " " + (MainV2.comPort.lastlogread.Second - DateTime.Now.Second));
                        //if ((taketime - timetook) < 0)
                        {
                            timeerror += (taketime - timetook);
                            if (ts != 0)
                            {
                                ts += timeerror;
                                timeerror = 0;
                            }
                        }
                        if (Math.Abs(ts) > 1000)
                            ts = 1000;
                    }

                    taketime = ts;
                    tsreal = DateTime.Now;

                    if (ts > 0 && ts < 1000)
                        Thread.Sleep((int)ts);

                    tracklast = tracklast.AddMilliseconds(ts - act);
                    tunning = tunning.AddMilliseconds(ts - act);

                    if (tracklast.Month != DateTime.Now.Month)
                    {
                        tracklast = DateTime.Now;
                        tunning = DateTime.Now;
                    }

                    try
                    {
                        if (MainV2.comPort.logplaybackfile != null &&
                            MainV2.comPort.logplaybackfile.BaseStream.Position ==
                            MainV2.comPort.logplaybackfile.BaseStream.Length)
                        {
                            MainV2.comPort.logreadmode = false;
                        }
                    }
                    catch
                    {
                        MainV2.comPort.logreadmode = false;
                    }
                }
                else
                {
                    // ensure we know to stop
                    if (MainV2.comPort.logreadmode)
                        MainV2.comPort.logreadmode = false;
                    updatePlayPauseButton(false);

                    if (!playingLog && MainV2.comPort.logplaybackfile != null)
                    {
                        continue;
                    }
                }

                try
                {
                    updateBindingSource();
                    // Console.WriteLine(DateTime.Now.Millisecond + " done ");

                    // battery warning.
                    float warnvolt = Settings.Instance.GetFloat("speechbatteryvolt");
                    float warnpercent = Settings.Instance.GetFloat("speechbatterypercent");

                    if (MainV2.comPort.MAV.cs.battery_voltage <= warnvolt)
                    {
                        hud1.lowvoltagealert = true;
                    }
                    else if ((MainV2.comPort.MAV.cs.battery_remaining) < warnpercent)
                    {
                        hud1.lowvoltagealert = true;
                    }
                    else
                    {
                        hud1.lowvoltagealert = false;
                    }

                    // update opengltest

                    // udpate tunning tab   //&& CB_tuning.Checked
                    Updategraph();

                    // update map
                    if (tracklast.AddSeconds(1.2) < DateTime.Now)
                    {
                        Updatemymap();
                    }
                }
                catch (Exception ex)
                {
                    log.Error(ex);
                    //Tracking.AddException(ex);
                    Console.WriteLine("FD Main loop exception " + ex);
                }
            }
            Console.WriteLine("FD Main loop exit");
        }

        private void RefreshGarpg()
        {
            if (MainV2.comPort.BaseStream.IsOpen || MainV2.comPort.logreadmode)
            {
                ZedGraphTimer.Enabled = true;
                ZedGraphTimer.Start();
                zg1.Visible = true;
                zg1.Refresh();
            }
            else
            {
                ZedGraphTimer.Enabled = false;
                ZedGraphTimer.Stop();
                zg1.Visible = false;
            }
        }

        private void Expinit()
        {
            LinkInterface comPort = MainV2.Comports[0];
            int fs = Convert.ToInt16(listBox1.SelectedItem.ToString());
            string[] temp = StartDateTime.Text.Split(',');
            int yy = Convert.ToInt16(temp[0]);
            int mm = Convert.ToInt16(temp[1]);
            int dd = Convert.ToInt16(temp[2]);
            int nsec4mean = Convert.ToInt16(Nsec4Mean.Text);
            double setmeancx1 = Convert.ToDouble(SetMeanCX1.Text);
            double setmeancx2 = Convert.ToDouble(SetMeanCX2.Text);
            int badckmodel = Convert.ToInt16(listBox2.SelectedItem);
            double moc1 = Convert.ToDouble(MaxRangOverMeanCX1.Text);
            double moc2 = Convert.ToDouble(MaxRangOverMeanCX2.Text);
            foreach (var port in MainV2.Comports.ToArray())
            {
                if (!port.BaseStream.IsOpen)
                {
                    // skip primary interface
                    if (port == comPort)
                        continue;

                    // modify array and drop out
                    MainV2.Comports.Remove(port);
                    port.Dispose();
                    break;
                }

                while (port.BaseStream.IsOpen)
                {
                    try
                    {
                        comPort.newdataExp.ReInitial(fs, yy, mm, dd, nsec4mean,
                setmeancx1, setmeancx2, badckmodel, moc1, moc2); ;
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex);
                    }
                }
            }
        }

        private double ConvertToDouble(object input)
        {
            if (input.GetType() == typeof(float))
            {
                return (float)input;
            }
            if (input.GetType() == typeof(double))
            {
                return (double)input;
            }
            if (input.GetType() == typeof(ulong))
            {
                return (ulong)input;
            }
            if (input.GetType() == typeof(long))
            {
                return (long)input;
            }
            if (input.GetType() == typeof(int))
            {
                return (int)input;
            }
            if (input.GetType() == typeof(uint))
            {
                return (uint)input;
            }
            if (input.GetType() == typeof(short))
            {
                return (short)input;
            }
            if (input.GetType() == typeof(ushort))
            {
                return (ushort)input;
            }
            if (input.GetType() == typeof(bool))
            {
                return (bool)input ? 1 : 0;
            }
            if (input.GetType() == typeof(string))
            {
                double ans = 0;
                if (double.TryParse((string)input, out ans))
                {
                    return ans;
                }
            }
            if (input is Enum)
            {
                return Convert.ToInt32(input);
            }

            if (input == null)
                throw new Exception("Bad Type Null");
            else
                throw new Exception("Bad Type " + input.GetType().ToString());
        }

        private void updateClearRoutesMarkers()
        {
            Invoke((MethodInvoker)delegate
            {
                routes.Markers.Clear();
            });
        }

        private void setMapBearing()
        {
            Invoke((MethodInvoker)delegate { gMapControl1.Bearing = (int)((MainV2.comPort.MAV.cs.yaw + 360) % 360); });
        }

        // to prevent cross thread calls while in a draw and exception
        private void updateClearRoutes()
        {
            // not async
            Invoke((MethodInvoker)delegate
            {
                routes.Routes.Clear();
                routes.Routes.Add(route);
            });
        }

        // to prevent cross thread calls while in a draw and exception
        private void updateClearMissionRouteMarkers()
        {
            // not async
            Invoke((MethodInvoker)delegate
            {
                polygons.Routes.Clear();
                polygons.Markers.Clear();
                //routes.Markers.Clear();
            });
        }

        private void updateRoutePosition()
        {
            // not async
            Invoke((MethodInvoker)delegate
            {
                gMapControl1.UpdateRouteLocalPosition(route);
            });
        }

        private void addMissionRouteMarker(GMapMarker marker)
        {
            // not async
            Invoke((MethodInvoker)delegate
            {
                routes.Markers.Add(marker);
            });
        }

        private void addMissionPhotoMarker(GMapMarker marker)
        {
            // not async
            Invoke((MethodInvoker)delegate
            {
                photosoverlay.Markers.Add(marker);
            });
        }

        private void updatePlayPauseButton(bool playing)
        {
            if (playing)
            {
                if (BUT_playlog.Text == "暂停")
                    return;

                BeginInvoke((MethodInvoker)delegate
                {
                    try
                    {
                        BUT_playlog.Text = "暂停";
                    }
                    catch
                    {
                    }
                });
            }
            else
            {
                if (BUT_playlog.Text == "播放")
                    return;

                BeginInvoke((MethodInvoker)delegate
                {
                    try
                    {
                        BUT_playlog.Text = "播放";
                    }
                    catch
                    {
                    }
                });
            }
        }

        private DateTime lastscreenupdate = DateTime.Now;
        private object updateBindingSourcelock = new object();
        private volatile int updateBindingSourcecount;
        private string updateBindingSourceThreadName = "";

        private void updateBindingSource()
        {
            //  run at 25 hz.
            if (lastscreenupdate.AddMilliseconds(40) < DateTime.Now)
            {
                lock (updateBindingSourcelock)
                {
                    // this is an attempt to prevent an invoke queue on the binding update on slow machines
                    if (updateBindingSourcecount > 0)
                    {
                        if (lastscreenupdate < DateTime.Now.AddSeconds(-5))
                        {
                            updateBindingSourcecount = 0;
                        }
                        return;
                    }

                    updateBindingSourcecount++;
                    updateBindingSourceThreadName = Thread.CurrentThread.Name;
                }

                this.BeginInvokeIfRequired((MethodInvoker)delegate
                {
                    updateBindingSourceWork();

                    lock (updateBindingSourcelock)
                    {
                        updateBindingSourcecount--;
                    }
                });
            }
        }

        private void updateBindingSourceWork()
        {
            //修改
            try
            {
                if (this.Visible)
                {
                    //Console.Write("bindingSource1 ");
                    MainV2.comPort.MAV.cs.UpdateCurrentSettings(bindingSource1);
                    //Console.Write("bindingSourceHud ");
                    //MainV2.comPort.MAV.cs.UpdateCurrentSettings(bindingSource3);
                    //Console.WriteLine("DONE ");

                    //if (tabControl1.SelectedTab == tabPage1)
                    //{
                    //    MainV2.comPort.MAV.cs.UpdateCurrentSettings(bindingSource1);
                    //}
                    //else if (tabControl1.SelectedTab == tabPage3)
                    //{
                    //    MainV2.comPort.MAV.cs.UpdateCurrentSettings(bindingSource1);
                    //}
                    //else if (tabControlactions.SelectedTab == tabGauges)
                    //{
                    //    MainV2.comPort.MAV.cs.UpdateCurrentSettings(bindingSourceGaugesTab);
                    //}
                    //else if (tabControlactions.SelectedTab == tabPagePreFlight)
                    //{
                    //    MainV2.comPort.MAV.cs.UpdateCurrentSettings(bindingSourceGaugesTab);
                    //}
                }
                else
                {
                    //Console.WriteLine("Null Binding");
                    MainV2.comPort.MAV.cs.UpdateCurrentSettings(bindingSource1);
                }
                lastscreenupdate = DateTime.Now;
            }
            catch (Exception ex)
            {
                log.Error(ex);
                //Tracking.AddException(ex);
            }
        }

        /// <summary>
        /// Try to reduce the number of map position changes generated by the code
        /// </summary>
        private DateTime lastmapposchange = DateTime.MinValue;

        private void updateMapPosition(PointLatLng currentloc)
        {
            Invoke((MethodInvoker)delegate
            {
                try
                {
                    if (lastmapposchange.Second != DateTime.Now.Second)
                    {
                        if (Math.Abs(currentloc.Lat - gMapControl1.Position.Lat) > 0.0001 || Math.Abs(currentloc.Lng - gMapControl1.Position.Lng) > 0.0001)
                        {
                            gMapControl1.Position = currentloc;
                        }
                        lastmapposchange = DateTime.Now;
                    }
                    //hud1.Refresh();
                }
                catch
                {
                }
            });
        }

        private void updateMapZoom(int zoom)
        {
            Invoke((MethodInvoker)delegate
            {
                try
                {
                    gMapControl1.Zoom = zoom;
                }
                catch
                {
                }
            });
        }

        private void updateLogPlayPosition()
        {
            BeginInvoke((MethodInvoker)delegate
            {
                try
                {
                    if (tracklog.Visible)
                        tracklog.Value =
                            (int)
                                (MainV2.comPort.logplaybackfile.BaseStream.Position /
                                 (double)MainV2.comPort.logplaybackfile.BaseStream.Length * 100);
                    if (lbl_logpercent.Visible)
                        lbl_logpercent.Text =
                            (MainV2.comPort.logplaybackfile.BaseStream.Position /
                             (double)MainV2.comPort.logplaybackfile.BaseStream.Length).ToString("0.00%");

                    if (lbl_playbackspeed.Visible)
                        lbl_playbackspeed.Text = "x " + LogPlayBackSpeed;
                }
                catch
                {
                }
            });
        }

        private void addpolygonmarker(string tag, double lng, double lat, int alt, Color? color, GMapOverlay overlay)
        {
            try
            {
                PointLatLng point = new PointLatLng(lat, lng);
                GMarkerGoogle m = new GMarkerGoogle(point, GMarkerGoogleType.green);
                m.ToolTipMode = MarkerTooltipMode.Always;
                m.ToolTipText = tag;
                m.Tag = tag;

                GMapMarkerRect mBorders = new GMapMarkerRect(point);
                {
                    mBorders.InnerMarker = m;
                    try
                    {
                        mBorders.wprad =
                            (int)(Settings.Instance.GetFloat("TXT_WPRad") / CurrentState.multiplierdist);
                    }
                    catch
                    {
                    }
                    if (color.HasValue)
                    {
                        mBorders.Color = color.Value;
                    }
                }

                Invoke((MethodInvoker)delegate
                {
                    overlay.Markers.Add(m);
                    overlay.Markers.Add(mBorders);
                });
            }
            catch (Exception)
            {
            }
        }

        private void addpolygonmarkerred(string tag, double lng, double lat, int alt, Color? color, GMapOverlay overlay)
        {
            try
            {
                PointLatLng point = new PointLatLng(lat, lng);
                GMarkerGoogle m = new GMarkerGoogle(point, GMarkerGoogleType.red);
                m.ToolTipMode = MarkerTooltipMode.Always;
                m.ToolTipText = tag;
                m.Tag = tag;

                GMapMarkerRect mBorders = new GMapMarkerRect(point);
                {
                    mBorders.InnerMarker = m;
                }

                Invoke((MethodInvoker)delegate
                {
                    overlay.Markers.Add(m);
                    overlay.Markers.Add(mBorders);
                });
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// used to redraw the polygon
        /// </summary>
        private void RegeneratePolygon()
        {
            List<PointLatLng> polygonPoints = new List<PointLatLng>();

            if (routes == null)
                return;

            foreach (GMapMarker m in polygons.Markers)
            {
                if (m is GMapMarkerRect)
                {
                    m.Tag = polygonPoints.Count;
                    polygonPoints.Add(m.Position);
                }
            }

            if (polygonPoints.Count < 2)
                return;

            GMap.NET.WindowsForms.GMapRoute homeroute = new GMapRoute("homepath");
            homeroute.Stroke = new Pen(Color.Yellow, 2);
            homeroute.Stroke.DashStyle = DashStyle.Dash;
            // add first point past home
            homeroute.Points.Add(polygonPoints[1]);
            // add home location
            homeroute.Points.Add(polygonPoints[0]);
            // add last point
            homeroute.Points.Add(polygonPoints[polygonPoints.Count - 1]);

            GMapRoute wppath = new GMapRoute("wp path");
            wppath.Stroke = new Pen(Color.Yellow, 4);
            wppath.Stroke.DashStyle = DashStyle.Custom;

            for (int a = 1; a < polygonPoints.Count; a++)
            {
                wppath.Points.Add(polygonPoints[a]);
            }

            Invoke((MethodInvoker)delegate
            {
                polygons.Routes.Add(homeroute);
                polygons.Routes.Add(wppath);
            });
        }

        private void hud_UserItem(object sender, EventArgs e)
        {
            Form selectform = new Form
            {
                Name = "select",
                Width = 50,
                Height = 50,
                Text = "Display This",
                AutoSize = true,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false,
                AutoScroll = true
            };
            //ThemeManager.ApplyThemeTo(selectform);

            object thisBoxed = MainV2.comPort.MAV.cs;
            Type test = thisBoxed.GetType();

            int max_length = 0;
            List<string> fields = new List<string>();

            foreach (var field in test.GetProperties())
            {
                // field.Name has the field's name.
                object fieldValue = field.GetValue(thisBoxed, null); // Get value
                if (fieldValue == null)
                    continue;

                // Get the TypeCode enumeration. Multiple types get mapped to a common typecode.
                TypeCode typeCode = Type.GetTypeCode(fieldValue.GetType());

                if (
                    !(typeCode == TypeCode.Single || typeCode == TypeCode.Double || typeCode == TypeCode.Int32 ||
                      typeCode == TypeCode.UInt16))
                    continue;

                max_length = Math.Max(max_length, TextRenderer.MeasureText(field.Name, selectform.Font).Width);
                fields.Add(field.Name);
            }
            max_length += 15;
            fields.Sort();

            int col_count = (int)(Screen.FromControl(this).Bounds.Width * 0.8f) / max_length;
            int row_count = fields.Count / col_count + ((fields.Count % col_count == 0) ? 0 : 1);
            int row_height = 20;
            //selectform.MinimumSize = new Size(col_count * max_length, row_count * row_height);

            for (int i = 0; i < fields.Count; i++)
            {
                CheckBox chk_box = new CheckBox
                {
                    Text = fields[i],
                    Name = fields[i],
                    Tag = "custom",
                    Location = new Point(5 + (i / row_count) * (max_length + 5), 2 + (i % row_count) * row_height),
                    Size = new Size(max_length, row_height),
                    Checked = hud1.CustomItems.ContainsKey(fields[i])
                };
                chk_box.CheckedChanged += chk_box_hud_UserItem_CheckedChanged;
                if (chk_box.Checked)
                    chk_box.BackColor = Color.Green;
                selectform.Controls.Add(chk_box);
            }

            selectform.Shown += (o, args) =>
            {
                selectform.Controls.ForEach(a =>
                {
                    if (a is CheckBox && ((CheckBox)a).Checked)
                        ((CheckBox)a).BackColor = Color.Green;
                });
            };

            selectform.ShowDialog(this);
        }

        private void addHudUserItem(ref HUD.Custom cust, CheckBox sender)
        {
            setupPropertyInfo(ref cust.Item, (sender).Name, MainV2.comPort.MAV.cs);

            hud1.CustomItems[(sender).Name] = cust;

            hud1.Invalidate();
        }

        /// <summary>
        /// 初始化绘图控件
        /// </summary>
        /// <param ></param>
        public void CreateChart()
        {
            List<ZedGraphControl> Graphs = new List<ZedGraphControl>();
            Graphs.Add(zg1);
            Graphs.Add(zg2);
            Graphs.Add(zg3);
            Graphs.Add(zg4);
            Graphs.Add(zg5);

            foreach (ZedGraphControl graph in Graphs)
            {
                graph.GraphPane.XAxis.Title.Text = "时间(s)";
                graph.GraphPane.XAxis.MajorGrid.IsVisible = true;
                graph.GraphPane.XAxis.Scale.Min = 0;
                graph.GraphPane.XAxis.Scale.Max = 5;

                graph.GraphPane.YAxis.Title.Text = "磁场强度(nT)";
                // turn off the opposite tics so the Y tics don't show up on the Y2 axis
                graph.GraphPane.YAxis.MajorTic.IsOpposite = false;
                graph.GraphPane.YAxis.MinorTic.IsOpposite = false;
                // Don't display the Y zero line
                graph.GraphPane.YAxis.MajorGrid.IsZeroLine = false;
                // Align the Y axis labels so they are flush to the axis
                graph.GraphPane.YAxis.Scale.Align = AlignP.Inside;
                graph.GraphPane.Chart.Fill = new Fill(Color.White, Color.LightGray, 45.0f);
            }

            //
            zg1.GraphPane.Title.Text = "磁场原始数据";
            zg2.GraphPane.Title.Text = "时域滤波结果1";
            zg3.GraphPane.Title.Text = "时域滤波结果2";
            zg4.GraphPane.Title.Text = "时域滤波结果3";
            zg5.GraphPane.Title.Text = "OBF";

            // Sample at 50ms intervals
            ZedGraphTimer.Interval = 200;
            //timer1.Enabled = true;
            //timer1.Start();

            // Calculate the Axis Scale Ranges
            //zgc.AxisChange();

            tickStart = Environment.TickCount;
        }

        //private void CB_tuning_CheckedChanged(object sender, EventArgs e)
        //{
        //    if (CB_tuning.Checked)
        //    {
        //        ZedGraphTimer.Enabled = true;
        //        ZedGraphTimer.Start();
        //        zg1.Visible = true;
        //        zg1.Refresh();
        //    }
        //    else
        //    {
        //        ZedGraphTimer.Enabled = false;
        //        ZedGraphTimer.Stop();
        //        //zg1.Visible = false;
        //    }
        //}

        private void timer1_Tick(object sender, EventArgs e)
        {
            try
            {
                // Make sure that the curvelist has at least one curve
                if (zg1.GraphPane.CurveList.Count <= 0)
                    return;

                // Get the first CurveItem in the graph
                LineItem curve = zg1.GraphPane.CurveList[0] as LineItem;
                if (curve == null)
                    return;

                // Get the PointPairList
                IPointListEdit list = curve.Points as IPointListEdit;
                // If this is null, it means the reference at curve.Points does not
                // support IPointListEdit, so we won't be able to modify it
                // 数据修改 ，在这里数据处理？？？？
                if (list == null)
                    return;

                // Time is measured in seconds
                double time = (Environment.TickCount - tickStart) / 1000.0;

                // Keep the X scale at a rolling 30 second interval, with one
                // major step between the max X value and the end of the axis
                Scale xScale = zg1.GraphPane.XAxis.Scale;
                if (time > xScale.Max - xScale.MajorStep)
                {
                    xScale.Max = time + xScale.MajorStep;
                    xScale.Min = xScale.Max - 10.0;
                }

                // Make sure that the curvelist has at least one curve
                //if (zg2.GraphPane.CurveList.Count <= 0)
                //    return;

                //// Get the first CurveItem in the graph
                //LineItem curve2 = zg1.GraphPane.CurveList[0] as LineItem;
                //if (curve == null)
                //    return;

                //// Get the PointPairList
                //IPointListEdit list2 = curve.Points as IPointListEdit;
                //// If this is null, it means the reference at curve.Points does not
                //// support IPointListEdit, so we won't be able to modify it
                //// 数据修改 ，在这里数据处理？？？？
                //if (list == null)
                //    return;

                // Time is measured in seconds
                //double time = (Environment.TickCount - tickStart) / 1000.0;

                // Keep the X scale at a rolling 30 second interval, with one
                // major step between the max X value and the end of the axis
                Scale xScale2 = zg2.GraphPane.XAxis.Scale;
                if (time > xScale2.Max - xScale2.MajorStep)
                {
                    xScale2.Max = time + xScale2.MajorStep;
                    xScale2.Min = xScale2.Max - 10.0;
                }

                // Make sure the Y axis is rescaled to accommodate actual data
                zg1.AxisChange();
                zg2.AxisChange();
                // Force a redraw

                zg1.Invalidate();
                zg2.Invalidate();
            }
            catch
            {
            }
        }

        /// <summary>
        /// 开始同步采集
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button2_Click(object sender, EventArgs e)
        {
            //MainV2.instance.Startsynacp();
            LinkInterface comPort = MainV2.Comports[0];
            button2.Enabled = false;
            Expinit();
            byte startflag = 0xa4;
            foreach (var port in MainV2.Comports.ToArray())
            {
                if (!port.BaseStream.IsOpen)
                {
                    // skip primary interface
                    if (port == comPort)
                        continue;

                    // modify array and drop out
                    MainV2.Comports.Remove(port);
                    port.Dispose();
                    break;
                }

                while (port.BaseStream.IsOpen)
                {
                    try
                    {
                        comPort.sendPacket(startflag);
                        Thread.Sleep(20);
                        comPort.sendPacket(startflag);
                        comPort.giveComport = false;
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex);
                    }
                }
            }
        }

        /// <summary>
        /// 停止采集
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button4_Click(object sender, EventArgs e)
        {
            //MainV2.instance.Stopacq();
            LinkInterface comPort = MainV2.Comports[0];
            button2.Enabled = true;
            button3.Enabled = true;
            byte stopflag = 0xA1;
            foreach (var port in MainV2.Comports.ToArray())
            {
                if (!port.BaseStream.IsOpen)
                {
                    // skip primary interface
                    if (port == comPort)
                        continue;

                    // modify array and drop out
                    MainV2.Comports.Remove(port);
                    port.Dispose();
                    break;
                }

                while (port.BaseStream.IsOpen)
                {
                    try
                    {
                        comPort.sendPacket(stopflag);
                        Thread.Sleep(20);
                        comPort.sendPacket(stopflag);
                    }
                    catch (Exception ex)
                    {
                        log.Error(ex);
                    }
                }
            }
        }

        /// <summary>
        /// 读取本地数据文件
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button7_Click(object sender, EventArgs e)
        {
            LBL_logfn.Text = "";

            if (MainV2.comPort.logplaybackfile != null)
            {
                try
                {
                    MainV2.comPort.logplaybackfile.Close();
                    MainV2.comPort.logplaybackfile = null;
                }
                catch
                {
                }
            }

            using (OpenFileDialog fd = new OpenFileDialog())
            {
                fd.AddExtension = true;
                fd.Filter = "Telemetry log (*.Ts)|*.Ts;*.Ts.*";
                fd.InitialDirectory = Settings.Instance.LogDir;
                fd.DefaultExt = ".tlog";
                DialogResult result = fd.ShowDialog();
                string file = fd.FileName;

                LoadLogFile(file);
                Expinit();
            }
        }

        //本地数据读取

        public void LoadLogFile(string file)
        {
            if (file != "")
            {
                try
                {
                    //BUT_clear_track_Click(null, null);

                    //MainV2.comPort.logreadmode  = true;
                    MainV2.comPort.logplaybackfile = new BinaryReader(File.OpenRead(file));
                    MainV2.comPort.lastlogread = DateTime.MinValue;

                    LBL_logfn.Text = Path.GetFileName(file);

                    log.Info("Open logfile " + file);

                    tracklog.Value = 0;
                    tracklog.Minimum = 0;
                    tracklog.Maximum = 100;
                }
                catch
                {
                    CustomMessageBox.Show(Strings.PleaseLoadValidFile, Strings.ERROR);
                }
            }
            else
            {
            }
        }

        public DateTime lastlogread { get; set; }

        // 播放速度调节
        private void myButton1_Click(object sender, EventArgs e)
        {
            LogPlayBackSpeed = double.Parse(((MyButton)sender).Tag.ToString(), CultureInfo.InvariantCulture);
            lbl_playbackspeed.Text = "x " + LogPlayBackSpeed;
        }

        public void BUT_playlog_Click(object sender, EventArgs e)
        {
            if (MainV2.comPort.logreadmode)
            {
                MainV2.comPort.logreadmode = false;
                ZedGraphTimer.Stop();
                playingLog = false;
            }
            else
            {
                // BUT_clear_track_Click(sender, e);
                MainV2.comPort.logreadmode = true;
                list1.Clear();
                list2.Clear();
                list3.Clear();
                list4.Clear();
                list5.Clear();
                list6.Clear();
                list7.Clear();
                list8.Clear();
                list9.Clear();
                list10.Clear();
                tickStart = Environment.TickCount;

                zg1.GraphPane.XAxis.Scale.Min = 0;
                zg1.GraphPane.XAxis.Scale.Max = 1;
                zg2.GraphPane.XAxis.Scale.Min = 0;
                zg2.GraphPane.XAxis.Scale.Max = 1;
                ZedGraphTimer.Start();
                playingLog = true;
            }
        }

        private GMapMarker center = new GMarkerGoogle(new PointLatLng(0.0, 0.0), GMarkerGoogleType.none);

        private void gMapControl1_OnPositionChanged(PointLatLng point)
        {
            center.Position = point;

            UpdateOverlayVisibility();
        }

        private void UpdateOverlayVisibility()
        {
            // change overlay visability
            if (gMapControl1.ViewArea != null)
            {
                var bounds = gMapControl1.ViewArea;
                bounds.Inflate(1, 1);

                foreach (var poly in kmlpolygons.Polygons)
                {
                    if (bounds.Contains(poly.Points[0]))
                        poly.IsVisible = true;
                    else
                        poly.IsVisible = false;
                }
            }
        }

        // 绘制参数选择
        private void zg1_DoubleClick(object sender, EventArgs e)
        {
            string formname = "select";
            Form selectform = Application.OpenForms[formname];
            if (selectform != null)
            {
                selectform.WindowState = FormWindowState.Minimized;
                selectform.Show();
                selectform.WindowState = FormWindowState.Normal;
                return;
            }

            selectform = new Form
            {
                Name = formname,
                Width = 50,
                Height = 550,
                Text = "Graph This"
            };

            int x = 10;
            int y = 10;

            {
                CheckBox chk_box = new CheckBox();
                chk_box.Text = "Logarithmic";
                chk_box.Name = "Logarithmic";
                chk_box.Location = new Point(x, y);
                chk_box.Size = new Size(100, 20);
                chk_box.CheckedChanged += chk_log_CheckedChanged;

                selectform.Controls.Add(chk_box);
            }

            //ThemeManager.ApplyThemeTo(selectform);

            y += 20;

            object thisBoxed = MainV2.comPort.MAV.cs;
            Type test = thisBoxed.GetType();

            int max_length = 0;
            List<string> fields = new List<string>();

            foreach (var field in test.GetProperties())
            {
                // field.Name has the field's name.
                object fieldValue;
                TypeCode typeCode;
                try
                {
                    fieldValue = field.GetValue(thisBoxed, null); // Get value

                    if (fieldValue == null)
                        continue;

                    // Get the TypeCode enumeration. Multiple types get mapped to a common typecode.
                    typeCode = Type.GetTypeCode(fieldValue.GetType());
                }
                catch
                {
                    continue;
                }

                if (!(typeCode == TypeCode.Single || typeCode == TypeCode.Double ||
                    typeCode == TypeCode.Int32 || typeCode == TypeCode.UInt16))
                    continue;

                max_length = Math.Max(max_length, TextRenderer.MeasureText(field.Name, selectform.Font).Width);
                fields.Add(field.Name);
            }
            max_length += 15;
            fields.Sort();

            foreach (var field in fields)
            {
                CheckBox chk_box = new CheckBox();

                //ThemeManager.ApplyThemeTo(chk_box);

                if (list1item != null && list1item.Name == field)
                {
                    chk_box.Checked = true;
                    chk_box.BackColor = Color.Green;
                }
                if (list2item != null && list2item.Name == field)
                {
                    chk_box.Checked = true;
                    chk_box.BackColor = Color.Green;
                }
                if (list3item != null && list3item.Name == field)
                {
                    chk_box.Checked = true;
                    chk_box.BackColor = Color.Green;
                }
                if (list4item != null && list4item.Name == field)
                {
                    chk_box.Checked = true;
                    chk_box.BackColor = Color.Green;
                }
                if (list5item != null && list5item.Name == field)
                {
                    chk_box.Checked = true;
                    chk_box.BackColor = Color.Green;
                }
                if (list6item != null && list6item.Name == field)
                {
                    chk_box.Checked = true;
                    chk_box.BackColor = Color.Green;
                }
                if (list7item != null && list7item.Name == field)
                {
                    chk_box.Checked = true;
                    chk_box.BackColor = Color.Green;
                }
                if (list8item != null && list8item.Name == field)
                {
                    chk_box.Checked = true;
                    chk_box.BackColor = Color.Green;
                }
                if (list9item != null && list9item.Name == field)
                {
                    chk_box.Checked = true;
                    chk_box.BackColor = Color.Green;
                }
                if (list10item != null && list10item.Name == field)
                {
                    chk_box.Checked = true;
                    chk_box.BackColor = Color.Green;
                }

                chk_box.Text = field;
                chk_box.Name = field;
                chk_box.Tag = "custom";
                chk_box.Location = new Point(x, y);
                chk_box.Size = new Size(100, 20);
                chk_box.CheckedChanged += chk_box_CheckedChanged;

                selectform.Controls.Add(chk_box);

                x += 0;
                y += 20;

                if (y > selectform.Height - 50)
                {
                    x += 100;
                    y = 10;

                    selectform.Width = x + 100;
                }
            }

            selectform.Shown += (o, args) =>
            {
                selectform.Controls.ForEach(a =>
                {
                    if (a is CheckBox && ((CheckBox)a).Checked)
                        ((CheckBox)a).BackColor = Color.Green;
                });
            };

            selectform.Show();
        }

        private void zg2_DoubleClick(object sender, EventArgs e)
        {
            string formname = "select";
            Form selectform = Application.OpenForms[formname];
            if (selectform != null)
            {
                selectform.WindowState = FormWindowState.Minimized;
                selectform.Show();
                selectform.WindowState = FormWindowState.Normal;
                return;
            }

            selectform = new Form
            {
                Name = formname,
                Width = 50,
                Height = 550,
                Text = "Graph This"
            };

            int x = 10;
            int y = 10;

            {
                CheckBox chk_box = new CheckBox();
                chk_box.Text = "Logarithmic";
                chk_box.Name = "Logarithmic";
                chk_box.Location = new Point(x, y);
                chk_box.Size = new Size(100, 20);
                chk_box.CheckedChanged += chk_log_CheckedChanged;

                selectform.Controls.Add(chk_box);
            }

            //ThemeManager.ApplyThemeTo(selectform);

            y += 20;

            object thisBoxed = MainV2.comPort.MAV.cs;
            Type test = thisBoxed.GetType();

            int max_length = 0;
            List<string> fields = new List<string>();

            foreach (var field in test.GetProperties())
            {
                // field.Name has the field's name.
                object fieldValue;
                TypeCode typeCode;
                try
                {
                    fieldValue = field.GetValue(thisBoxed, null); // Get value

                    if (fieldValue == null)
                        continue;

                    // Get the TypeCode enumeration. Multiple types get mapped to a common typecode.
                    typeCode = Type.GetTypeCode(fieldValue.GetType());
                }
                catch
                {
                    continue;
                }

                if (!(typeCode == TypeCode.Single || typeCode == TypeCode.Double ||
                    typeCode == TypeCode.Int32 || typeCode == TypeCode.UInt16))
                    continue;

                max_length = Math.Max(max_length, TextRenderer.MeasureText(field.Name, selectform.Font).Width);
                fields.Add(field.Name);
            }
            max_length += 15;
            fields.Sort();

            foreach (var field in fields)
            {
                CheckBox chk_box = new CheckBox();

                //ThemeManager.ApplyThemeTo(chk_box);

                if (list1item != null && list1item.Name == field)
                {
                    chk_box.Checked = true;
                    chk_box.BackColor = Color.Green;
                }
                if (list2item != null && list2item.Name == field)
                {
                    chk_box.Checked = true;
                    chk_box.BackColor = Color.Green;
                }
                if (list3item != null && list3item.Name == field)
                {
                    chk_box.Checked = true;
                    chk_box.BackColor = Color.Green;
                }
                if (list4item != null && list4item.Name == field)
                {
                    chk_box.Checked = true;
                    chk_box.BackColor = Color.Green;
                }
                if (list5item != null && list5item.Name == field)
                {
                    chk_box.Checked = true;
                    chk_box.BackColor = Color.Green;
                }
                if (list6item != null && list6item.Name == field)
                {
                    chk_box.Checked = true;
                    chk_box.BackColor = Color.Green;
                }
                if (list7item != null && list7item.Name == field)
                {
                    chk_box.Checked = true;
                    chk_box.BackColor = Color.Green;
                }
                if (list8item != null && list8item.Name == field)
                {
                    chk_box.Checked = true;
                    chk_box.BackColor = Color.Green;
                }
                if (list9item != null && list9item.Name == field)
                {
                    chk_box.Checked = true;
                    chk_box.BackColor = Color.Green;
                }
                if (list10item != null && list10item.Name == field)
                {
                    chk_box.Checked = true;
                    chk_box.BackColor = Color.Green;
                }

                chk_box.Text = field;
                chk_box.Name = field;
                chk_box.Tag = "custom";
                chk_box.Location = new Point(x, y);
                chk_box.Size = new Size(100, 20);
                chk_box.CheckedChanged += chk_box_CheckedChanged2;

                selectform.Controls.Add(chk_box);

                x += 0;
                y += 20;

                if (y > selectform.Height - 50)
                {
                    x += 100;
                    y = 10;

                    selectform.Width = x + 100;
                }
            }

            selectform.Shown += (o, args) =>
            {
                selectform.Controls.ForEach(a =>
                {
                    if (a is CheckBox && ((CheckBox)a).Checked)
                        ((CheckBox)a).BackColor = Color.Green;
                });
            };

            selectform.Show();
        }

        //void mymap_Paint(object sender, PaintEventArgs e)
        //{
        //    distanceBar1.DoPaintRemote(e);
        //}

        private void chk_log_CheckedChanged(object sender, EventArgs e)
        {
            if (((CheckBox)sender).Checked)
            {
                zg1.GraphPane.YAxis.Type = AxisType.Log;
            }
            else
            {
                zg1.GraphPane.YAxis.Type = AxisType.Linear;
            }
        }

        private void chk_box_CheckedChanged(object sender, EventArgs e)
        {
            //ThemeManager.ApplyThemeTo((Control)sender);

            if (((CheckBox)sender).Checked)
            {
                ((CheckBox)sender).BackColor = Color.Green;

                if (list1item == null)
                {
                    if (setupPropertyInfo(ref list1item, ((CheckBox)sender).Name, MainV2.comPort.MAV.cs))
                    {
                        list1.Clear();
                        list1curve = zg1.GraphPane.AddCurve(((CheckBox)sender).Name, list1, Color.Red, SymbolType.None);
                    }
                }
                else if (list2item == null)
                {
                    if (setupPropertyInfo(ref list2item, ((CheckBox)sender).Name, MainV2.comPort.MAV.cs))
                    {
                        list2.Clear();
                        list2curve = zg1.GraphPane.AddCurve(((CheckBox)sender).Name, list2, Color.Blue, SymbolType.None);
                    }
                }
                else if (list3item == null)
                {
                    if (setupPropertyInfo(ref list3item, ((CheckBox)sender).Name, MainV2.comPort.MAV.cs))
                    {
                        list3.Clear();
                        list3curve = zg1.GraphPane.AddCurve(((CheckBox)sender).Name, list3, Color.Green,
                            SymbolType.None);
                    }
                }
                else if (list4item == null)
                {
                    if (setupPropertyInfo(ref list4item, ((CheckBox)sender).Name, MainV2.comPort.MAV.cs))
                    {
                        list4.Clear();
                        list4curve = zg1.GraphPane.AddCurve(((CheckBox)sender).Name, list4, Color.Orange,
                            SymbolType.None);
                    }
                }
                else if (list5item == null)
                {
                    if (setupPropertyInfo(ref list5item, ((CheckBox)sender).Name, MainV2.comPort.MAV.cs))
                    {
                        list5.Clear();
                        list5curve = zg1.GraphPane.AddCurve(((CheckBox)sender).Name, list5, Color.Yellow,
                            SymbolType.None);
                    }
                }
                else if (list6item == null)
                {
                    if (setupPropertyInfo(ref list6item, ((CheckBox)sender).Name, MainV2.comPort.MAV.cs))
                    {
                        list6.Clear();
                        list6curve = zg1.GraphPane.AddCurve(((CheckBox)sender).Name, list6, Color.Magenta,
                            SymbolType.None);
                    }
                }
                else if (list7item == null)
                {
                    if (setupPropertyInfo(ref list7item, ((CheckBox)sender).Name, MainV2.comPort.MAV.cs))
                    {
                        list7.Clear();
                        list7curve = zg1.GraphPane.AddCurve(((CheckBox)sender).Name, list7, Color.Purple,
                            SymbolType.None);
                    }
                }
                else if (list8item == null)
                {
                    if (setupPropertyInfo(ref list8item, ((CheckBox)sender).Name, MainV2.comPort.MAV.cs))
                    {
                        list8.Clear();
                        list8curve = zg1.GraphPane.AddCurve(((CheckBox)sender).Name, list8, Color.LimeGreen,
                            SymbolType.None);
                    }
                }
                else if (list9item == null)
                {
                    if (setupPropertyInfo(ref list9item, ((CheckBox)sender).Name, MainV2.comPort.MAV.cs))
                    {
                        list9.Clear();
                        list9curve = zg1.GraphPane.AddCurve(((CheckBox)sender).Name, list9, Color.Cyan, SymbolType.None);
                    }
                }
                else if (list10item == null)
                {
                    if (setupPropertyInfo(ref list10item, ((CheckBox)sender).Name, MainV2.comPort.MAV.cs))
                    {
                        list10.Clear();
                        list10curve = zg1.GraphPane.AddCurve(((CheckBox)sender).Name, list10, Color.Violet,
                            SymbolType.None);
                    }
                }
                else
                {
                    CustomMessageBox.Show("Max 10 at a time.");
                    ((CheckBox)sender).Checked = false;
                }

                string selected = "";
                try
                {
                    foreach (var curve in zg1.GraphPane.CurveList)
                    {
                        selected = selected + curve.Label.Text + "|";
                    }
                }
                catch
                {
                }
                Settings.Instance["Tuning_Graph_Selected"] = selected;
            }
            else
            {
                ((CheckBox)sender).BackColor = Color.Transparent;

                // reset old stuff
                if (list1item != null && list1item.Name == ((CheckBox)sender).Name)
                {
                    list1item = null;
                    zg1.GraphPane.CurveList.Remove(list1curve);
                }
                if (list2item != null && list2item.Name == ((CheckBox)sender).Name)
                {
                    list2item = null;
                    zg1.GraphPane.CurveList.Remove(list2curve);
                }
                if (list3item != null && list3item.Name == ((CheckBox)sender).Name)
                {
                    list3item = null;
                    zg1.GraphPane.CurveList.Remove(list3curve);
                }
                if (list4item != null && list4item.Name == ((CheckBox)sender).Name)
                {
                    list4item = null;
                    zg1.GraphPane.CurveList.Remove(list4curve);
                }
                if (list5item != null && list5item.Name == ((CheckBox)sender).Name)
                {
                    list5item = null;
                    zg1.GraphPane.CurveList.Remove(list5curve);
                }
                if (list6item != null && list6item.Name == ((CheckBox)sender).Name)
                {
                    list6item = null;
                    zg1.GraphPane.CurveList.Remove(list6curve);
                }
                if (list7item != null && list7item.Name == ((CheckBox)sender).Name)
                {
                    list7item = null;
                    zg1.GraphPane.CurveList.Remove(list7curve);
                }
                if (list8item != null && list8item.Name == ((CheckBox)sender).Name)
                {
                    list8item = null;
                    zg1.GraphPane.CurveList.Remove(list8curve);
                }
                if (list9item != null && list9item.Name == ((CheckBox)sender).Name)
                {
                    list9item = null;
                    zg1.GraphPane.CurveList.Remove(list9curve);
                }
                if (list10item != null && list10item.Name == ((CheckBox)sender).Name)
                {
                    list10item = null;
                    zg1.GraphPane.CurveList.Remove(list10curve);
                }
            }
        }

        private void chk_box_CheckedChanged2(object sender, EventArgs e)
        {
            //ThemeManager.ApplyThemeTo((Control)sender);

            if (((CheckBox)sender).Checked)
            {
                ((CheckBox)sender).BackColor = Color.Green;

                if (list1item == null)
                {
                    if (setupPropertyInfo(ref list1item, ((CheckBox)sender).Name, MainV2.comPort.MAV.cs))
                    {
                        list1.Clear();
                        list1curve = zg2.GraphPane.AddCurve(((CheckBox)sender).Name, list1, Color.Red, SymbolType.None);
                    }
                }
                else if (list2item == null)
                {
                    if (setupPropertyInfo(ref list2item, ((CheckBox)sender).Name, MainV2.comPort.MAV.cs))
                    {
                        list2.Clear();
                        list2curve = zg2.GraphPane.AddCurve(((CheckBox)sender).Name, list2, Color.Blue, SymbolType.None);
                    }
                }
                else if (list3item == null)
                {
                    if (setupPropertyInfo(ref list3item, ((CheckBox)sender).Name, MainV2.comPort.MAV.cs))
                    {
                        list3.Clear();
                        list3curve = zg2.GraphPane.AddCurve(((CheckBox)sender).Name, list3, Color.Green,
                            SymbolType.None);
                    }
                }
                else if (list4item == null)
                {
                    if (setupPropertyInfo(ref list4item, ((CheckBox)sender).Name, MainV2.comPort.MAV.cs))
                    {
                        list4.Clear();
                        list4curve = zg2.GraphPane.AddCurve(((CheckBox)sender).Name, list4, Color.Orange,
                            SymbolType.None);
                    }
                }
                else if (list5item == null)
                {
                    if (setupPropertyInfo(ref list5item, ((CheckBox)sender).Name, MainV2.comPort.MAV.cs))
                    {
                        list5.Clear();
                        list5curve = zg2.GraphPane.AddCurve(((CheckBox)sender).Name, list5, Color.Yellow,
                            SymbolType.None);
                    }
                }
                else if (list6item == null)
                {
                    if (setupPropertyInfo(ref list6item, ((CheckBox)sender).Name, MainV2.comPort.MAV.cs))
                    {
                        list6.Clear();
                        list6curve = zg2.GraphPane.AddCurve(((CheckBox)sender).Name, list6, Color.Magenta,
                            SymbolType.None);
                    }
                }
                else if (list7item == null)
                {
                    if (setupPropertyInfo(ref list7item, ((CheckBox)sender).Name, MainV2.comPort.MAV.cs))
                    {
                        list7.Clear();
                        list7curve = zg2.GraphPane.AddCurve(((CheckBox)sender).Name, list7, Color.Purple,
                            SymbolType.None);
                    }
                }
                else if (list8item == null)
                {
                    if (setupPropertyInfo(ref list8item, ((CheckBox)sender).Name, MainV2.comPort.MAV.cs))
                    {
                        list8.Clear();
                        list8curve = zg2.GraphPane.AddCurve(((CheckBox)sender).Name, list8, Color.LimeGreen,
                            SymbolType.None);
                    }
                }
                else if (list9item == null)
                {
                    if (setupPropertyInfo(ref list9item, ((CheckBox)sender).Name, MainV2.comPort.MAV.cs))
                    {
                        list9.Clear();
                        list9curve = zg2.GraphPane.AddCurve(((CheckBox)sender).Name, list9, Color.Cyan, SymbolType.None);
                    }
                }
                else if (list10item == null)
                {
                    if (setupPropertyInfo(ref list10item, ((CheckBox)sender).Name, MainV2.comPort.MAV.cs))
                    {
                        list10.Clear();
                        list10curve = zg2.GraphPane.AddCurve(((CheckBox)sender).Name, list10, Color.Violet,
                            SymbolType.None);
                    }
                }
                else
                {
                    CustomMessageBox.Show("Max 10 at a time.");
                    ((CheckBox)sender).Checked = false;
                }

                string selected = "";
                try
                {
                    foreach (var curve in zg2.GraphPane.CurveList)
                    {
                        selected = selected + curve.Label.Text + "|";
                    }
                }
                catch
                {
                }
                Settings.Instance["Tuning_Graph_Selected"] = selected;
            }
            else
            {
                ((CheckBox)sender).BackColor = Color.Transparent;

                // reset old stuff
                if (list1item != null && list1item.Name == ((CheckBox)sender).Name)
                {
                    list1item = null;
                    zg2.GraphPane.CurveList.Remove(list1curve);
                }
                if (list2item != null && list2item.Name == ((CheckBox)sender).Name)
                {
                    list2item = null;
                    zg2.GraphPane.CurveList.Remove(list2curve);
                }
                if (list3item != null && list3item.Name == ((CheckBox)sender).Name)
                {
                    list3item = null;
                    zg2.GraphPane.CurveList.Remove(list3curve);
                }
                if (list4item != null && list4item.Name == ((CheckBox)sender).Name)
                {
                    list4item = null;
                    zg2.GraphPane.CurveList.Remove(list4curve);
                }
                if (list5item != null && list5item.Name == ((CheckBox)sender).Name)
                {
                    list5item = null;
                    zg2.GraphPane.CurveList.Remove(list5curve);
                }
                if (list6item != null && list6item.Name == ((CheckBox)sender).Name)
                {
                    list6item = null;
                    zg2.GraphPane.CurveList.Remove(list6curve);
                }
                if (list7item != null && list7item.Name == ((CheckBox)sender).Name)
                {
                    list7item = null;
                    zg2.GraphPane.CurveList.Remove(list7curve);
                }
                if (list8item != null && list8item.Name == ((CheckBox)sender).Name)
                {
                    list8item = null;
                    zg2.GraphPane.CurveList.Remove(list8curve);
                }
                if (list9item != null && list9item.Name == ((CheckBox)sender).Name)
                {
                    list9item = null;
                    zg2.GraphPane.CurveList.Remove(list9curve);
                }
                if (list10item != null && list10item.Name == ((CheckBox)sender).Name)
                {
                    list10item = null;
                    zg2.GraphPane.CurveList.Remove(list10curve);
                }
            }
        }

        private bool setupPropertyInfo(ref PropertyInfo input, string name, object source)
        {
            Type test = source.GetType();

            foreach (var field in test.GetProperties())
            {
                if (field.Name == name)
                {
                    input = field;
                    return true;
                }
            }

            return false;
        }

        private void quickView1_DoubleClick(object sender, EventArgs e)
        {
            QuickView qv = (QuickView)sender;

            Form selectform = new Form
            {
                Name = "select",
                Width = 50,
                Height = 50,
                Text = "Display This",
                AutoSize = true,
                StartPosition = FormStartPosition.CenterParent,
                MaximizeBox = false,
                MinimizeBox = false,
                AutoScroll = true
            };
            //ThemeManager.ApplyThemeTo(selectform);

            object thisBoxed = MainV2.comPort.MAV.cs;
            Type test = thisBoxed.GetType();

            int max_length = 0;
            List<string> fields = new List<string>();

            foreach (var field in test.GetProperties())
            {
                // field.Name has the field's name.
                object fieldValue = field.GetValue(thisBoxed, null); // Get value
                if (fieldValue == null)
                    continue;

                // Get the TypeCode enumeration. Multiple types get mapped to a common typecode.
                TypeCode typeCode = Type.GetTypeCode(fieldValue.GetType());

                if (
                    !(typeCode == TypeCode.Single || typeCode == TypeCode.Double || typeCode == TypeCode.Int32 ||
                      typeCode == TypeCode.UInt16))
                    continue;

                max_length = Math.Max(max_length, TextRenderer.MeasureText(field.Name, selectform.Font).Width);
                fields.Add(field.Name);
            }
            max_length += 15;
            fields.Sort();

            int col_count = (int)(Screen.FromControl(this).Bounds.Width * 0.8f) / max_length;
            int row_count = fields.Count / col_count + ((fields.Count % col_count == 0) ? 0 : 1);
            int row_height = 20;
            //selectform.MinimumSize = new Size(col_count * max_length, row_count * row_height);

            for (int i = 0; i < fields.Count; i++)
            {
                CheckBox chk_box = new CheckBox
                {
                    // dont change to ToString() = null exception
                    Checked = qv.Tag != null && qv.Tag.ToString() == fields[i],
                    Text = fields[i],
                    Name = fields[i],
                    Tag = qv,
                    Location = new Point(5 + (i / row_count) * (max_length + 5), 2 + (i % row_count) * row_height),
                    Size = new Size(max_length, row_height)
                };
                chk_box.CheckedChanged += chk_box_quickview_CheckedChanged;
                if (chk_box.Checked)
                    chk_box.BackColor = Color.Green;
                selectform.Controls.Add(chk_box);
            }

            selectform.Shown += (o, args) =>
            {
                selectform.Controls.ForEach(a =>
                {
                    if (a is CheckBox && ((CheckBox)a).Checked)
                        ((CheckBox)a).BackColor = Color.Green;
                });
            };

            selectform.ShowDialog(this);
        }

        private void chk_box_quickview_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkbox = (CheckBox)sender;

            if (checkbox.Checked)
            {
                // save settings
                Settings.Instance[((QuickView)checkbox.Tag).Name] = checkbox.Name;

                // set description
                string desc = checkbox.Name;
                ((QuickView)checkbox.Tag).Tag = desc;

                desc = MainV2.comPort.MAV.cs.GetNameandUnit(desc);

                ((QuickView)checkbox.Tag).desc = desc;

                // set databinding for value
                ((QuickView)checkbox.Tag).DataBindings.Clear();
                ((QuickView)checkbox.Tag).DataBindings.Add(new Binding("number", bindingSource1, checkbox.Name,
                    true));

                // close selection form
                ((Form)checkbox.Parent).Close();
            }
        }

        private void chk_box_hud_UserItem_CheckedChanged(object sender, EventArgs e)
        {
            CheckBox checkbox = (CheckBox)sender;

            if (checkbox.Checked)
            {
                checkbox.BackColor = Color.Green;

                HUD.Custom cust = new HUD.Custom();
                HUD.Custom.src = MainV2.comPort.MAV.cs;

                string prefix = checkbox.Name + ": ";
                if (Settings.Instance["hud1_useritem_" + checkbox.Name] != null)
                    prefix = Settings.Instance["hud1_useritem_" + checkbox.Name];

                if (DialogResult.Cancel == InputBox.Show("Header", "Please enter your item prefix", ref prefix))
                {
                    checkbox.Checked = false;
                    return;
                }

                Settings.Instance["hud1_useritem_" + checkbox.Name] = prefix;

                cust.Header = prefix;

                addHudUserItem(ref cust, checkbox);
            }
            else
            {
                checkbox.BackColor = Color.Transparent;

                if (hud1.CustomItems.ContainsKey(checkbox.Name))
                    hud1.CustomItems.Remove(checkbox.Name);

                Settings.Instance.Remove("hud1_useritem_" + checkbox.Name);
                hud1.Invalidate();
            }
        }

        private void Updategraph()
        {
            double time = (Environment.TickCount - tickStart) / 1000.0;
            if (list1item != null)
                list1.Add(time, ConvertToDouble(list1item.GetValue(MainV2.comPort.MAV.cs, null)));
            if (list2item != null)
                list2.Add(time, ConvertToDouble(list2item.GetValue(MainV2.comPort.MAV.cs, null)));
            if (list3item != null)
                list3.Add(time, ConvertToDouble(list3item.GetValue(MainV2.comPort.MAV.cs, null)));
            if (list4item != null)
                list4.Add(time, ConvertToDouble(list4item.GetValue(MainV2.comPort.MAV.cs, null)));
            if (list5item != null)
                list5.Add(time, ConvertToDouble(list5item.GetValue(MainV2.comPort.MAV.cs, null)));
            if (list6item != null)
                list6.Add(time, ConvertToDouble(list6item.GetValue(MainV2.comPort.MAV.cs, null)));
            if (list7item != null)
                list7.Add(time, ConvertToDouble(list7item.GetValue(MainV2.comPort.MAV.cs, null)));
            if (list8item != null)
                list8.Add(time, ConvertToDouble(list8item.GetValue(MainV2.comPort.MAV.cs, null)));
            if (list9item != null)
                list9.Add(time, ConvertToDouble(list9item.GetValue(MainV2.comPort.MAV.cs, null)));
            if (list10item != null)
                list10.Add(time, ConvertToDouble(list10item.GetValue(MainV2.comPort.MAV.cs, null)));
        }

        private DateTime waypoints = DateTime.Now.AddSeconds(0);
        private DateTime mapupdate = DateTime.Now.AddSeconds(0);

        private void Updatemymap()
        {
            if (Settings.Instance.GetBoolean("CHK_maprotation"))
            {
                // dont holdinvalidation here
                setMapBearing();
            }

            if (route == null)
            {
                route = new GMap.NET.WindowsForms.GMapRoute(trackPoints, "track");
                routes.Routes.Add(route);
            }

            PointLatLng currentloc = new PointLatLng(MainV2.comPort.MAV.cs.lat, MainV2.comPort.MAV.cs.lng);

            gMapControl1.HoldInvalidation = true;

            int numTrackLength = Settings.Instance.GetInt32("NUM_tracklength");
            // maintain route history length
            if (route.Points.Count > numTrackLength)
            {
                route.Points.RemoveRange(0,
                    route.Points.Count - numTrackLength);
            }
            // add new route point
            if (MainV2.comPort.MAV.cs.lat != 0 && MainV2.comPort.MAV.cs.lng != 0)
            {
                route.Points.Add(currentloc);
            }

            updateRoutePosition();

            // update programed wp course
            if (waypoints.AddSeconds(5) < DateTime.Now)
            {
                //Console.WriteLine("Doing FD WP's");
                updateClearMissionRouteMarkers();

                //float dist = 0;
                //float travdist = 0;

                // optional on Flight data
                if (MainV2.ShowAirports)
                {
                    // airports
                    foreach (var item in Airports.getAirports(gMapControl1.Position).ToArray())
                    {
                        try
                        {
                            rallypointoverlay.Markers.Add(new GMapMarkerAirport(item)
                            {
                                ToolTipText = item.Tag,
                                ToolTipMode = MarkerTooltipMode.OnMouseOver
                            });
                        }
                        catch (Exception e)
                        {
                            log.Error(e);
                        }
                    }
                }
                waypoints = DateTime.Now;
            }

            //updateClearRoutesMarkers();

            if (route.Points.Count == 0 || route.Points[route.Points.Count - 1].Lat != 0 &&
                (mapupdate.AddSeconds(3) < DateTime.Now))
            {
                updateMapPosition(currentloc);
                mapupdate = DateTime.Now;
            }

            if (route.Points.Count == 1 && gMapControl1.Zoom == 3) // 3 is the default load zoom
            {
                updateMapPosition(currentloc);
                updateMapZoom(17);
            }
            //}

            gMapControl1.HoldInvalidation = false;

            if (gMapControl1.Visible)
            {
                gMapControl1.Invalidate();
            }
        }

        [DllImport("CppFilterWrapper.dll", CharSet = CharSet.Unicode, CallingConvention = CallingConvention.Cdecl)]
        public static extern int VSShowFilterSetWin(double RS, double[] FilterCoef, ref int FilterLN, ref int Delay);

        /// <summary>
        /// 滤波器设计
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void FliterDisn_Click(object sender, EventArgs e)
        {
            double RS = double.Parse(listBox1.Text);
            int Flicoflen = int.Parse(textBox5.Text);
            double[] FilterCoef = new double[Flicoflen];
            int FilterLN = int.Parse(textBox4.Text);
            int Delay = 0;
            int irr = VSShowFilterSetWin(RS, FilterCoef, ref FilterLN, ref Delay);
            label22.Text = FilterCoef[FilterLN / 2].ToString();
            label23.Text = Delay.ToString();
            label24.Text = FilterLN.ToString();
            label25.Text = irr.ToString();
        }

        private void text_keypress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            if (!(Char.IsNumber(e.KeyChar)) && e.KeyChar != (char)8)
            {
                e.Handled = true;
            }
        }

        private void StartDateTime_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        {
            if (!(Char.IsNumber(e.KeyChar)) && e.KeyChar != (char)8 && e.KeyChar != ',')
            {
                e.Handled = true;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void tracklog_Scroll(object sender, EventArgs e)
        {
            try
            {
                if (route != null)
                    route.Points.Clear();

                MainV2.comPort.lastlogread = DateTime.MinValue;
                MainV2.comPort.MAV.cs.ResetInternals();

                if (MainV2.comPort.logplaybackfile != null)
                    MainV2.comPort.logplaybackfile.BaseStream.Position =
                        (long)(MainV2.comPort.logplaybackfile.BaseStream.Length * (tracklog.Value / 100.0));

                updateLogPlayPosition();
            }
            catch
            {
            } // ignore any invalid
        }

        /// <summary>
        /// 滤波器选择
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void myButton6_Click(object sender, EventArgs e)
        {
            string FliterTab = ((MyButton)sender).Tag.ToString();
            switch (FliterTab)
            {
                case "A":
                    myLabel1.Text = "";
                    //LBL_logfn.Text = Path.GetFileName(file);
                    break;

                case "B":
                    myLabel2.Text = "";
                    break;

                case "C":
                    myLabel3.Text = "";
                    break;

                case "D":
                    myLabel4.Text = "";
                    break;

                case "E":
                    myLabel5.Text = "";
                    break;

                case "F":
                    myLabel6.Text = "";
                    break;
            }

            using (OpenFileDialog fd = new OpenFileDialog())
            {
                fd.AddExtension = true;
                fd.Filter = "Fliter file (*.Ts)|*.Ts;*.Ts.*";
                fd.InitialDirectory = Settings.Instance.LogDir;
                fd.DefaultExt = ".tlog";
                DialogResult result = fd.ShowDialog();
                string file = fd.FileName;

                // 滤波器读取
            }
        }

        /// <summary>
        /// 测试采集
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void button3_Click(object sender, EventArgs e)
        {
            Expinit();
            byte startflag = 0xa4;
            MainV2.comPort.sendPacket(startflag);
            Thread.Sleep(20);
            MainV2.comPort.sendPacket(startflag);
        }
    }
}