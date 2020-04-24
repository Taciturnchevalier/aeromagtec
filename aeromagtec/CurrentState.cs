using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.ComponentModel;
using aeromagtec.Utilities;
using log4net;
using aeromagtec.Attributes;
using aeromagtec;
using System.Collections;
using System.Linq;
using System.Runtime.Serialization;
using DirectShowLib;
using Newtonsoft.Json;

namespace aeromagtec
{
    public class CurrentState : ICloneable
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public event EventHandler csCallBack;

        [JsonIgnore]
        [IgnoreDataMember]
        public MAVState parent;

        public int lastautowp = -1;

        // multipliers
        public static float multiplierdist = 1;

        public static string DistanceUnit = "";
        public static float multiplierspeed = 1;
        public static string SpeedUnit = "m";

        public static double toDistDisplayUnit(double input)
        {
            return input * multiplierdist;
        }

        public static double toSpeedDisplayUnit(double input)
        {
            return input * multiplierspeed;
        }

        public static double fromDistDisplayUnit(double input)
        {
            return input / multiplierdist;
        }

        public static double fromSpeedDisplayUnit(double input)
        {
            return input / multiplierspeed;
        }

        // 系统需要参数定义
        [DisplayText("ocxo state")]
        public Int16 ocxo_states { get; set; }

        [DisplayText("ocxo voltage")]
        public float ocxo_voltage { get; set; }

        [DisplayText("work state")]
        public Int16 work_states { get; set; }

        [DisplayText("gps_fix_type")]
        public Int16 gps_fix_type { get; set; }

        [DisplayText("hour")]
        public byte Hour { get; set; }

        [DisplayText("Minute")]
        public byte Minute { get; set; }

        [DisplayText("Sec")]
        public byte Sec { get; set; }

        [DisplayText("time")]
        public DateTime time { get; set; }

        [DisplayText("Count")]
        public int Count { get; set; }

        // mag
        [DisplayText("Mag X")]
        public double mx { get; set; }

        [DisplayText("Mag Y")]
        public double my { get; set; }

        [DisplayText("Mag Z")]
        public double mz { get; set; }

        [DisplayText("Mag Field")]
        public double magfield
        {
            get { return (float)Math.Sqrt(Math.Pow(mx, 2) + Math.Pow(my, 2) + Math.Pow(mz, 2)); }
        }

        [DisplayText("CH01")]
        public float mag01 { get; set; }

        [DisplayText("CH02")]
        public float mag02 { get; set; }

        // orientation - rads
        [DisplayText("Roll (deg)")]
        public float roll { get; set; }

        [DisplayText("Pitch (deg)")]
        public float pitch { get; set; }

        [DisplayText("Yaw (deg)")]
        public float yaw
        {
            get { return _yaw; }
            set
            {
                if (value < 0)
                {
                    _yaw = value + 360;
                }
                else
                {
                    _yaw = value;
                }
            }
        }

        private float _yaw = 0;

        // position
        [DisplayText("Latitude")]
        public double lat { get; set; }

        [DisplayText("Longitude")]
        public double lng { get; set; }

        [DisplayText("Altitude (dist)")]
        public float alt { get; set; }

        private DateTime lastalt = DateTime.MinValue;

        [DisplayText("Gps Status")]
        public float gpsstatus { get; set; }

        [DisplayText("Gps HDOP")]
        public float gpshdop { get; set; }

        [DisplayText("Sat Count")]
        public Int16 satcount { get; set; }

        public DateTime gpstime { get; set; }

        [DisplayText("Dist Traveled (dist)")]
        public float distTraveled { get; set; }

        //battery
        [DisplayText("Bat Voltage (V)")]
        public float battery_voltage
        {
            get { return _battery_voltage; }
            set
            {
                if (_battery_voltage == 0) _battery_voltage = value;
                _battery_voltage = value * 0.4f + _battery_voltage * 0.6f;
            }
        }

        internal float _battery_voltage;

        [DisplayText("Bat Remaining (%)")]
        public int battery_remaining
        {
            get { return _battery_remaining; }
            set
            {
                _battery_remaining = value;
                if (_battery_remaining < 0 || _battery_remaining > 100) _battery_remaining = 0;
            }
        }

        private int _battery_remaining;

        [DisplayText("Bat Current (Amps)")]
        public float current
        {
            get { return _current; }
            set
            {
                if (_lastcurrent == DateTime.MinValue) _lastcurrent = datetime;
                // case for no sensor
                if (value == -0.01f)
                {
                    _current = 0;
                    return;
                }
                battery_usedmah += ((value * 1000.0) * (datetime - _lastcurrent).TotalHours);
                _current = value;
                _lastcurrent = datetime;
            }
        } //current may to be below zero - recuperation in arduplane

        private float _current;

        [DisplayText("Bat Watts")]
        public float watts
        {
            get { return battery_voltage * current; }
        }

        private DateTime _lastcurrent = DateTime.MinValue;

        [DisplayText("Bat efficiency (mah/km)")]
        public double battery_mahperkm { get { return battery_usedmah / (distTraveled / 1000.0f); } }

        [DisplayText("Bat km left EST (km)")]
        public double battery_kmleft { get { return (((100.0f / (100.0f - battery_remaining)) * battery_usedmah) - battery_usedmah) / battery_mahperkm; } }

        [DisplayText("Bat used EST (mah)")]
        public double battery_usedmah { get; set; }

        public double battery_temp { get; set; }

        public double HomeAlt
        {
            get { return HomeLocation.Alt; }
            set { }
        }

        private static PointLatLngAlt _homelocation = new PointLatLngAlt();

        public PointLatLngAlt HomeLocation
        {
            get { return _homelocation; }
            set { _homelocation = value; }
        }

        private PointLatLngAlt _movingbase = new PointLatLngAlt();

        public PointLatLngAlt MovingBase
        {
            get { return _movingbase; }
            set
            {
                if (_movingbase.Lat != value.Lat || _movingbase.Lng != value.Lng || _movingbase.Alt
                    != value.Alt)
                    _movingbase = value;
            }
        }

        private static PointLatLngAlt _trackerloc = new PointLatLngAlt();

        public PointLatLngAlt TrackerLocation
        {
            get
            {
                if (_trackerloc.Lng != 0) return _trackerloc;
                return HomeLocation;
            }
            set { _trackerloc = value; }
        }

        public PointLatLngAlt Location
        {
            get { return new PointLatLngAlt(lat, lng, alt); }
        }

        [DisplayText("GroundCourse (deg)")]
        public float groundcourse
        {
            get { return _groundcourse; }
            set
            {
                if (value < 0)
                {
                    _groundcourse = value + 360;
                }
                else
                {
                    _groundcourse = value;
                }
            }
        }

        private float _groundcourse = 0;

        [DisplayText("Bearing Target (deg)")]
        public float nav_bearing { get; set; }

        [DisplayText("Bearing Target (deg)")]
        public float target_bearing { get; set; }

        // turn radius
        [DisplayText("Turn Radius (dist)")]
        public float radius
        {
            get
            {
                if (groundspeed <= 1) return 0;
                return ((groundspeed * groundspeed) / (float)(9.8f * Math.Tan(roll * MathHelper.deg2rad)));
            }
        }

        public float groundspeed { get; set; }
        //public float GeoFenceDist
        //{
        //    get
        //    {
        //        try
        //        {
        //            float disttotal = 99999;
        //            PointLatLngAlt lineStartLatLngAlt = null;
        //            var R = 6371e3;
        //            // close loop
        //            var list = MainV2.MAVSTATE.fencepoints.ToList();
        //            if (list.Count > 0)
        //            {
        //                // remove return location
        //                list.RemoveAt(0);
        //            }

        //            // check all segments
        //            foreach (var mavlinkFencePointT in list)
        //            {
        //                if (lineStartLatLngAlt == null)
        //                {
        //                    lineStartLatLngAlt = new PointLatLngAlt(mavlinkFencePointT.Value.lat,
        //                        mavlinkFencePointT.Value.lng);
        //                    continue;
        //                }

        //                // crosstrack distance
        //                var lineEndLatLngAlt = new PointLatLngAlt(mavlinkFencePointT.Value.lat, mavlinkFencePointT.Value.lng);

        //                var lineDist = lineStartLatLngAlt.GetDistance2(lineEndLatLngAlt);

        //                var distToLocation = lineStartLatLngAlt.GetDistance2(Location);
        //                var bearToLocation = lineStartLatLngAlt.GetBearing(Location);
        //                var lineBear = lineStartLatLngAlt.GetBearing(lineEndLatLngAlt);

        //                var angle = bearToLocation - lineBear;
        //                if (angle < 0)
        //                    angle += 360;

        //                var alongline = Math.Cos(angle * MathHelper.deg2rad) * distToLocation;

        //                // check to see if our point is still within the line length
        //                if (alongline > lineDist)
        //                {
        //                    lineStartLatLngAlt = lineEndLatLngAlt;
        //                    continue;
        //                }

        //                var dXt2 = Math.Sin(angle * MathHelper.deg2rad) * distToLocation;

        //                var dXt = Math.Asin(Math.Sin(distToLocation / R) * Math.Sin(angle * MathHelper.deg2rad)) * R;

        //                disttotal = (float)Math.Min(disttotal, Math.Abs(dXt2));

        //                lineStartLatLngAlt = lineEndLatLngAlt;
        //            }

        //            // check also distance from the points - because if we are outside the polygon, we may be on a corner segment
        //            foreach (var mavlinkFencePointT in list)
        //            {
        //                var pathpoint = new PointLatLngAlt(mavlinkFencePointT.Value.lat, mavlinkFencePointT.Value.lng);
        //                var dXt2 = pathpoint.GetDistance(Location);
        //                disttotal = (float)Math.Min(disttotal, Math.Abs(dXt2));
        //            }

        //            return disttotal;
        //        }
        //        catch
        //        {
        //            return 0;
        //        }
        //    }
        //}

        [DisplayText("Dist to Home (dist)")]
        public float DistToHome
        {
            get
            {
                if (lat == 0 && lng == 0 || TrackerLocation.Lat == 0)
                    return 0;

                // shrinking factor for longitude going to poles direction
                double rads = Math.Abs(TrackerLocation.Lat) * 0.0174532925;
                double scaleLongDown = Math.Cos(rads);
                double scaleLongUp = 1.0f / Math.Cos(rads);

                //DST to Home
                double dstlat = Math.Abs(TrackerLocation.Lat - lat) * 111319.5;
                double dstlon = Math.Abs(TrackerLocation.Lng - lng) * 111319.5 * scaleLongDown;
                return (float)Math.Sqrt((dstlat * dstlat) + (dstlon * dstlon)) * multiplierdist;
            }
        }

        [DisplayText("Dist to Moving Base (dist)")]
        public float DistFromMovingBase
        {
            get
            {
                if (lat == 0 && lng == 0 || MovingBase == null)
                    return 0;

                // shrinking factor for longitude going to poles direction
                double rads = Math.Abs(MovingBase.Lat) * 0.0174532925;
                double scaleLongDown = Math.Cos(rads);
                double scaleLongUp = 1.0f / Math.Cos(rads);

                //DST to Home
                double dstlat = Math.Abs(MovingBase.Lat - lat) * 111319.5;
                double dstlon = Math.Abs(MovingBase.Lng - lng) * 111319.5 * scaleLongDown;
                return (float)Math.Sqrt((dstlat * dstlat) + (dstlon * dstlon)) * multiplierdist;
            }
        }

        [DisplayText("Elevation to Mav (deg)")]
        public float ELToMAV
        {
            get
            {
                float dist = DistToHome / multiplierdist;

                if (dist < 5)
                    return 0;

                float altdiff = (float)(alt - TrackerLocation.Alt);

                float angle = (float)(Math.Atan(altdiff / dist) * MathHelper.rad2deg);

                return angle;
            }
        }

        [DisplayText("Bearing to Mav (deg)")]
        public float AZToMAV
        {
            get
            {
                // shrinking factor for longitude going to poles direction
                double rads = Math.Abs(TrackerLocation.Lat) * 0.0174532925;
                double scaleLongDown = Math.Cos(rads);
                double scaleLongUp = 1.0f / Math.Cos(rads);

                //DIR to Home
                double dstlon = (TrackerLocation.Lng - lng); //OffSet_X
                double dstlat = (TrackerLocation.Lat - lat) * scaleLongUp; //OffSet Y
                double bearing = 90 + (Math.Atan2(dstlat, -dstlon) * 57.295775); //absolut home direction
                if (bearing < 0) bearing += 360; //normalization
                //bearing = bearing - 180;//absolut return direction
                //if (bearing < 0) bearing += 360;//normalization

                float dist = DistToHome / multiplierdist;

                if (dist < 5)
                    return 0;

                return (float)bearing;
            }
        }

        [DisplayText("Sonar Range (meters)")]
        public float sonarrange
        {
            get { return (float)toDistDisplayUnit(_sonarrange); }
            set { _sonarrange = value; }
        }

        private float _sonarrange = 0;

        [DisplayText("Sonar Voltage (Volt)")]
        public float sonarvoltage { get; set; }

        // current firmware

        //public float freemem { get; set; }
        //public float load { get; set; }
        //public float brklevel { get; set; }
        public bool armed { get; set; }

        // stats
        public ushort packetdropremote { get; set; }

        public ushort linkqualitygcs { get; set; }

        [DisplayText("Voltage Flags")]
        public uint voltageflag { get; set; }

        public ushort i2cerrors { get; set; }

        public double timesincelastshot { get; set; }

        // requested stream rates
        public byte rateattitude { get; set; }

        public byte rateposition { get; set; }
        public byte ratestatus { get; set; }
        public byte ratesensors { get; set; }
        public byte raterc { get; set; }

        internal static byte rateattitudebackup { get; set; }
        internal static byte ratepositionbackup { get; set; }
        internal static byte ratestatusbackup { get; set; }
        internal static byte ratesensorsbackup { get; set; }
        internal static byte ratercbackup { get; set; }

        // reference
        public DateTime datetime { get; set; }

        public bool connected
        {
            get { return (MainV2.comPort.BaseStream.IsOpen || MainV2.comPort.logreadmode); }
        }

        public float campointa { get; set; }

        public float campointb { get; set; }

        public float campointc { get; set; }

        public PointLatLngAlt GimbalPoint { get; set; }

        public float gimballat
        {
            get
            {
                if (GimbalPoint == null) return 0;
                return (float)GimbalPoint.Lat;
            }
        }

        public float gimballng
        {
            get
            {
                if (GimbalPoint == null) return 0;
                return (float)GimbalPoint.Lng;
            }
        }

        public bool landed { get; set; }

        public bool terrainactive { get; set; }

        // rc override
        public short rcoverridech1;//{ get; set; }

        public short rcoverridech2;// { get; set; }

        internal bool batterymonitoring = false;

        // for calc of sitl speedup
        internal DateTime lastimutime = DateTime.MinValue;

        internal double imutime = 0;

        internal bool MONO = false;

        static CurrentState()
        {
            // set default telemrates
            rateattitudebackup = 4;
            ratepositionbackup = 2;
            ratestatusbackup = 2;
            ratesensorsbackup = 2;
            ratercbackup = 2;
        }

        public CurrentState()
        {
            ResetInternals();

            var t = Type.GetType("Mono.Runtime");
            MONO = (t != null);
        }

        public void ResetInternals()
        {
            lock (this)
            {
                rateattitude = rateattitudebackup;
                rateposition = ratepositionbackup;
                ratestatus = ratestatusbackup;
                ratesensors = ratesensorsbackup;
                raterc = ratercbackup;
                datetime = DateTime.MinValue;
                battery_usedmah = 0;
                _lastcurrent = DateTime.MinValue;
                distTraveled = 0;
                //timeInAir = 0;
            }
        }

        public List<string> GetItemList()
        {
            List<string> ans = new List<string>();

            object thisBoxed = this;
            Type test = thisBoxed.GetType();

            // public instance props
            PropertyInfo[] props = test.GetProperties();

            //props

            foreach (var field in props)
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

                ans.Add(field.Name);
            }

            return ans;
        }

        private DateTime lastupdate = DateTime.Now;

        private DateTime lastsecondcounter = DateTime.Now;
        private PointLatLngAlt lastpos = new PointLatLngAlt();

        private DateTime lastdata = DateTime.MinValue;

        public string GetNameandUnit(string name)
        {
            string desc = name;
            try
            {
                var typeofthing = typeof(CurrentState).GetProperty(name);
                if (typeofthing != null)
                {
                    var attrib = typeofthing.GetCustomAttributes(false);
                    if (attrib.Length > 0)
                        desc = ((Attributes.DisplayTextAttribute)attrib[0]).Text;
                }
            }
            catch
            {
            }

            if (desc.Contains("(dist)"))
            {
                desc = desc.Replace("(dist)", "(" + CurrentState.DistanceUnit + ")");
            }
            else if (desc.Contains("(speed)"))
            {
                desc = desc.Replace("(speed)", "(" + CurrentState.SpeedUnit + ")");
            }

            return desc;
        }

        /// <summary>
        /// use for main serial port only
        /// </summary>
        /// <param name="bs"></param>
        public void UpdateCurrentSettings(System.Windows.Forms.BindingSource bs)
        {
            UpdateCurrentSettings(bs, false, MainV2.comPort, MainV2.comPort.MAV);
        }

        /// <summary>
        /// Use the default sysid
        /// </summary>
        /// <param name="bs"></param>
        /// <param name="updatenow"></param>
        /// <param name="mavinterface"></param>
        public void UpdateCurrentSettings(System.Windows.Forms.BindingSource bs, bool updatenow,
            LinkInterface mavinterface)
        {
            UpdateCurrentSettings(bs, updatenow, mavinterface, mavinterface.MAV);
        }

        public void UpdateCurrentSettings(System.Windows.Forms.BindingSource bs, bool updatenow,
            LinkInterface mavinterface, MAVState MAV)
        {
            lock (this)
            {
                if (updatenow) //
                {
                    lastupdate = DateTime.Now;

                    //MainV2.comPort.UartDataParser();

                    //check if valid mavinterface
                    //if (parent != null && parent.packetsnotlost != 0)
                    //{
                    //    if ((DateTime.Now - parent.lastvalidpacket).TotalSeconds > 10)
                    //    {
                    //        linkqualitygcs = 0;
                    //    }
                    //    else
                    //    {
                    //        linkqualitygcs =
                    //            (ushort)((parent.packetsnotlost / (parent.packetsnotlost + parent.packetslost)) * 100.0);
                    //    }

                    //    if (linkqualitygcs > 100)
                    //        linkqualitygcs = 100;
                    //}

                    if (datetime.Second != lastsecondcounter.Second)
                    {
                        lastsecondcounter = datetime;

                        if (lastpos.Lat != 0 && lastpos.Lng != 0)
                        {
                            // 应该判断接口打开后初始化distTraveled=0
                            distTraveled += (float)lastpos.GetDistance(new PointLatLngAlt(lat, lng, 0, "")) *
                                            multiplierdist;
                            lastpos = new PointLatLngAlt(lat, lng, 0, "");
                        }
                        else
                        {
                            lastpos = new PointLatLngAlt(lat, lng, 0, "");
                        }
                    }

                    // re-request streams
                }

                try
                {
                    if (csCallBack != null)
                        csCallBack(this, null);
                }
                catch
                {
                }

                //Console.Write(DateTime.Now.Millisecond + " start ");
                // update form
                try
                {
                    if (bs != null)
                    {
                        bs.DataSource = this;
                        //bs.DataSource = dataExp.RawDataTable();
                        bs.ResetBindings(false);

                        return;
                        /*

                        sw.Start();
                        bs.SuspendBinding();
                        bs.Clear();
                        bs.ResumeBinding();
                        bs.Add(this);
                        sw.Stop();
                        elaps = sw.Elapsed;
                        Console.WriteLine("2 " + elaps.ToString("0.#####") + " done ");

                        sw.Start();
                        if (bs.Count > 100)
                            bs.Clear();
                        bs.Add(this);
                        sw.Stop();
                        elaps = sw.Elapsed;
                        Console.WriteLine("3 " + elaps.ToString("0.#####") + " done ");
                        */
                    }
                }
                catch
                {
                    log.InfoFormat("CurrentState Binding error");
                }
            }
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        //public void dowindcalc()
        //{
        //    //Wind Fixed gain Observer
        //    //Ryan Beall
        //    //8FEB10

        //    double Kw = 0.010; // 0.01 // 0.10

        //    if (airspeed < 1 || groundspeed < 1)
        //        return;

        //    double Wn_error = airspeed*Math.Cos((yaw)*MathHelper.deg2rad)*Math.Cos(pitch*MathHelper.deg2rad) -
        //                      groundspeed*Math.Cos((groundcourse)*MathHelper.deg2rad) - Wn_fgo;
        //    double We_error = airspeed*Math.Sin((yaw)*MathHelper.deg2rad)*Math.Cos(pitch*MathHelper.deg2rad) -
        //                      groundspeed*Math.Sin((groundcourse)*MathHelper.deg2rad) - We_fgo;

        //    Wn_fgo = Wn_fgo + Kw*Wn_error;
        //    We_fgo = We_fgo + Kw*We_error;

        //    double wind_dir = (Math.Atan2(We_fgo, Wn_fgo)*(180/Math.PI));
        //    double wind_vel = (Math.Sqrt(Math.Pow(We_fgo, 2) + Math.Pow(Wn_fgo, 2)));

        //    wind_dir = (wind_dir + 360)%360;

        //    this.wind_dir = (float) wind_dir; // (float)(wind_dir * 0.5 + this.wind_dir * 0.5);
        //    this.wind_vel = (float) wind_vel; // (float)(wind_vel * 0.5 + this.wind_vel * 0.5);

        //    //Console.WriteLine("Wn_error = {0}\nWe_error = {1}\nWn_fgo =    {2}\nWe_fgo =  {3}\nWind_dir =    {4}\nWind_vel =    {5}\n",Wn_error,We_error,Wn_fgo,We_fgo,wind_dir,wind_vel);

        //    //Console.WriteLine("wind_dir: {0} wind_vel: {1}    as {4} yaw {5} pitch {6} gs {7} cog {8}", wind_dir, wind_vel, Wn_fgo, We_fgo , airspeed,yaw,pitch,groundspeed,groundcourse);

        //    //low pass the outputs for better results!
        //}

        /// <summary>
        /// derived from MAV_SYS_STATUS_SENSOR
        /// </summary>
        public class Mavlink_Sensors
        {
            private BitArray bitArray = new BitArray(32);

            public bool seen = false;

            public Mavlink_Sensors()
            {
                //var item = MAVLink.MAV_SYS_STATUS_SENSOR._3D_ACCEL;
            }

            public Mavlink_Sensors(uint p)
            {
                seen = true;
                bitArray = new BitArray(new int[] { (int)p });
            }

            public bool gyro
            {
                get { return bitArray[ConvertValuetoBitmaskOffset((int)MAVLink.MAV_SYS_STATUS_SENSOR._3D_GYRO)]; }
                set { bitArray[ConvertValuetoBitmaskOffset((int)MAVLink.MAV_SYS_STATUS_SENSOR._3D_GYRO)] = value; }
            }

            public bool accelerometer
            {
                get { return bitArray[ConvertValuetoBitmaskOffset((int)MAVLink.MAV_SYS_STATUS_SENSOR._3D_ACCEL)]; }
                set { bitArray[ConvertValuetoBitmaskOffset((int)MAVLink.MAV_SYS_STATUS_SENSOR._3D_ACCEL)] = value; }
            }

            public bool compass
            {
                get { return bitArray[ConvertValuetoBitmaskOffset((int)MAVLink.MAV_SYS_STATUS_SENSOR._3D_MAG)]; }
                set { bitArray[ConvertValuetoBitmaskOffset((int)MAVLink.MAV_SYS_STATUS_SENSOR._3D_MAG)] = value; }
            }

            public bool barometer
            {
                get { return bitArray[ConvertValuetoBitmaskOffset((int)MAVLink.MAV_SYS_STATUS_SENSOR.ABSOLUTE_PRESSURE)]; }
                set { bitArray[ConvertValuetoBitmaskOffset((int)MAVLink.MAV_SYS_STATUS_SENSOR.ABSOLUTE_PRESSURE)] = value; }
            }

            public bool differential_pressure
            {
                get { return bitArray[ConvertValuetoBitmaskOffset((int)MAVLink.MAV_SYS_STATUS_SENSOR.DIFFERENTIAL_PRESSURE)]; }
                set { bitArray[ConvertValuetoBitmaskOffset((int)MAVLink.MAV_SYS_STATUS_SENSOR.DIFFERENTIAL_PRESSURE)] = value; }
            }

            public bool gps
            {
                get { return bitArray[ConvertValuetoBitmaskOffset((int)MAVLink.MAV_SYS_STATUS_SENSOR.GPS)]; }
                set { bitArray[ConvertValuetoBitmaskOffset((int)MAVLink.MAV_SYS_STATUS_SENSOR.GPS)] = value; }
            }

            public bool optical_flow
            {
                get { return bitArray[ConvertValuetoBitmaskOffset((int)MAVLink.MAV_SYS_STATUS_SENSOR.OPTICAL_FLOW)]; }
                set { bitArray[ConvertValuetoBitmaskOffset((int)MAVLink.MAV_SYS_STATUS_SENSOR.OPTICAL_FLOW)] = value; }
            }

            public bool VISION_POSITION
            {
                get { return bitArray[ConvertValuetoBitmaskOffset((int)MAVLink.MAV_SYS_STATUS_SENSOR.VISION_POSITION)]; }
                set { bitArray[ConvertValuetoBitmaskOffset((int)MAVLink.MAV_SYS_STATUS_SENSOR.VISION_POSITION)] = value; }
            }

            public bool LASER_POSITION
            {
                get { return bitArray[ConvertValuetoBitmaskOffset((int)MAVLink.MAV_SYS_STATUS_SENSOR.LASER_POSITION)]; }
                set { bitArray[ConvertValuetoBitmaskOffset((int)MAVLink.MAV_SYS_STATUS_SENSOR.LASER_POSITION)] = value; }
            }

            public bool GROUND_TRUTH
            {
                get { return bitArray[ConvertValuetoBitmaskOffset((int)MAVLink.MAV_SYS_STATUS_SENSOR.EXTERNAL_GROUND_TRUTH)]; }
                set { bitArray[ConvertValuetoBitmaskOffset((int)MAVLink.MAV_SYS_STATUS_SENSOR.EXTERNAL_GROUND_TRUTH)] = value; }
            }

            public bool rate_control
            {
                get { return bitArray[ConvertValuetoBitmaskOffset((int)MAVLink.MAV_SYS_STATUS_SENSOR.ANGULAR_RATE_CONTROL)]; }
                set { bitArray[ConvertValuetoBitmaskOffset((int)MAVLink.MAV_SYS_STATUS_SENSOR.ANGULAR_RATE_CONTROL)] = value; }
            }

            public bool attitude_stabilization
            {
                get { return bitArray[ConvertValuetoBitmaskOffset((int)MAVLink.MAV_SYS_STATUS_SENSOR.ATTITUDE_STABILIZATION)]; }
                set { bitArray[ConvertValuetoBitmaskOffset((int)MAVLink.MAV_SYS_STATUS_SENSOR.ATTITUDE_STABILIZATION)] = value; }
            }

            public bool yaw_position
            {
                get { return bitArray[ConvertValuetoBitmaskOffset((int)MAVLink.MAV_SYS_STATUS_SENSOR.YAW_POSITION)]; }
                set { bitArray[ConvertValuetoBitmaskOffset((int)MAVLink.MAV_SYS_STATUS_SENSOR.YAW_POSITION)] = value; }
            }

            public bool altitude_control
            {
                get { return bitArray[ConvertValuetoBitmaskOffset((int)MAVLink.MAV_SYS_STATUS_SENSOR.Z_ALTITUDE_CONTROL)]; }
                set { bitArray[ConvertValuetoBitmaskOffset((int)MAVLink.MAV_SYS_STATUS_SENSOR.Z_ALTITUDE_CONTROL)] = value; }
            }

            public bool xy_position_control
            {
                get { return bitArray[ConvertValuetoBitmaskOffset((int)MAVLink.MAV_SYS_STATUS_SENSOR.XY_POSITION_CONTROL)]; }
                set { bitArray[ConvertValuetoBitmaskOffset((int)MAVLink.MAV_SYS_STATUS_SENSOR.XY_POSITION_CONTROL)] = value; }
            }

            public bool motor_control
            {
                get { return bitArray[ConvertValuetoBitmaskOffset((int)MAVLink.MAV_SYS_STATUS_SENSOR.MOTOR_OUTPUTS)]; }
                set { bitArray[ConvertValuetoBitmaskOffset((int)MAVLink.MAV_SYS_STATUS_SENSOR.MOTOR_OUTPUTS)] = value; }
            }

            public bool rc_receiver
            {
                get { return bitArray[ConvertValuetoBitmaskOffset((int)MAVLink.MAV_SYS_STATUS_SENSOR.RC_RECEIVER)]; }
                set { bitArray[ConvertValuetoBitmaskOffset((int)MAVLink.MAV_SYS_STATUS_SENSOR.RC_RECEIVER)] = value; }
            }

            public bool gyro2
            {
                get { return bitArray[ConvertValuetoBitmaskOffset((int)MAVLink.MAV_SYS_STATUS_SENSOR._3D_GYRO2)]; }
                set { bitArray[ConvertValuetoBitmaskOffset((int)MAVLink.MAV_SYS_STATUS_SENSOR._3D_GYRO2)] = value; }
            }

            public bool accel2
            {
                get { return bitArray[ConvertValuetoBitmaskOffset((int)MAVLink.MAV_SYS_STATUS_SENSOR._3D_ACCEL2)]; }
                set { bitArray[ConvertValuetoBitmaskOffset((int)MAVLink.MAV_SYS_STATUS_SENSOR._3D_ACCEL2)] = value; }
            }

            public bool mag2
            {
                get { return bitArray[ConvertValuetoBitmaskOffset((int)MAVLink.MAV_SYS_STATUS_SENSOR._3D_MAG2)]; }
                set { bitArray[ConvertValuetoBitmaskOffset((int)MAVLink.MAV_SYS_STATUS_SENSOR._3D_MAG2)] = value; }
            }

            public bool geofence
            {
                get { return bitArray[ConvertValuetoBitmaskOffset((int)MAVLink.MAV_SYS_STATUS_SENSOR.MAV_SYS_STATUS_GEOFENCE)]; }
                set { bitArray[ConvertValuetoBitmaskOffset((int)MAVLink.MAV_SYS_STATUS_SENSOR.MAV_SYS_STATUS_GEOFENCE)] = value; }
            }

            public bool ahrs
            {
                get { return bitArray[ConvertValuetoBitmaskOffset((int)MAVLink.MAV_SYS_STATUS_SENSOR.MAV_SYS_STATUS_AHRS)]; }
                set { bitArray[ConvertValuetoBitmaskOffset((int)MAVLink.MAV_SYS_STATUS_SENSOR.MAV_SYS_STATUS_AHRS)] = value; }
            }

            public bool terrain
            {
                get { return bitArray[ConvertValuetoBitmaskOffset((int)MAVLink.MAV_SYS_STATUS_SENSOR.MAV_SYS_STATUS_TERRAIN)]; }
                set { bitArray[ConvertValuetoBitmaskOffset((int)MAVLink.MAV_SYS_STATUS_SENSOR.MAV_SYS_STATUS_TERRAIN)] = value; }
            }

            public bool logging
            {
                get { return bitArray[ConvertValuetoBitmaskOffset((int)MAVLink.MAV_SYS_STATUS_SENSOR.MAV_SYS_STATUS_LOGGING)]; }
                set { bitArray[ConvertValuetoBitmaskOffset((int)MAVLink.MAV_SYS_STATUS_SENSOR.MAV_SYS_STATUS_LOGGING)] = value; }
            }

            private int ConvertValuetoBitmaskOffset(int input)
            {
                int offset = 0;
                for (int a = 0; a < sizeof(int) * 8; a++)
                {
                    offset = 1 << a;
                    if (input == offset)
                        return a;
                }
                return 0;
            }

            public uint Value
            {
                get
                {
                    int[] array = new int[1];
                    bitArray.CopyTo(array, 0);
                    return (uint)array[0];
                }
                set
                {
                    seen = true;
                    bitArray = new BitArray(new int[] { (int)value });
                }
            }

            public override string ToString()
            {
                return Convert.ToString(Value, 2);
            }
        }
    }
}