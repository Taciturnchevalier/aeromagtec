using aeromagtec.Comms;
using aeromagtec.Controls;
using aeromagtec.Utilities;
using log4net;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive.Subjects;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using System.Data;
using aeromagtec.Mavlink;
using Timer = System.Timers.Timer;
using aeromagtec.GCSViews;
using FirebirdSql.Data.FirebirdClient;

namespace aeromagtec
{
    public class LinkInterface : MAVLink, IDisposable
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private ICommsSerial _baseStream;

        //public event EventHandler<BaseStream> OnPacketReceived;

        public dataExp newdataExp = new dataExp();

        public ICommsSerial BaseStream
        {
            get { return _baseStream; }
            set
            {
                // This is called every time user changes the port selection, so we need to make sure we cleanup
                // any previous objects so we don't leave the cleanup of system resources to the garbage collector.
                if (_baseStream != null)
                {
                    try
                    {
                        if (_baseStream.IsOpen)
                        {
                            _baseStream.Close();
                        }
                    }
                    catch { }
                    IDisposable dsp = _baseStream as IDisposable;
                    if (dsp != null)
                    {
                        try
                        {
                            dsp.Dispose();
                        }
                        catch { }
                    }
                }
                _baseStream = value;
            }
        }

        public static readonly byte[] Invalid;

        public ICommsSerial MirrorStream { get; set; }

        public bool MirrorStreamWrite { get; set; }

        public event EventHandler MavChanged;

        public event EventHandler CommsClose;

        private const int gcssysid = 255;

        public bool giveComport
        {
            get { return _giveComport; }
            set { _giveComport = value; }
        }

        private volatile bool _giveComport = false;

        private DateTime lastparamset = DateTime.MinValue;

        private FlightData flightdata;

        private int DataConvertLeftLength = 0;

        public void ExpFuninit()
        {
            int fs = Convert.ToInt16(flightdata.listBox1.SelectedItem.ToString());
            string[] temp = flightdata.StartDateTime.Text.Split(',');
            int yy = Convert.ToInt16(temp[0]);
            int mm = Convert.ToInt16(temp[1]);
            int dd = Convert.ToInt16(temp[2]);
            int nsec4mean = Convert.ToInt16(flightdata.Nsec4Mean.Text);
            int setmeancx1 = Convert.ToInt16(flightdata.SetMeanCX1.Text);
            int setmeancx2 = Convert.ToInt16(flightdata.SetMeanCX2.Text);
            int badckmodel = Convert.ToInt16(flightdata.listBox2.SelectedItem);
            int moc1 = Convert.ToInt16(flightdata.MaxRangOverMeanCX1.Text);
            int moc2 = Convert.ToInt16(flightdata.MaxRangOverMeanCX2.Text);
            if (true)
            {
                newdataExp.ReInitial(fs, yy, mm, dd, nsec4mean,
               setmeancx1, setmeancx2, badckmodel, moc1, moc2);
            }
        }

        public static byte[] converttemp = new byte[100];

        public void ReadPacket()
        {
            byte[] buffer = new byte[48 + 25];
            byte[] lefttemp = new byte[25];
            int length = 0;

            //MAVLinkMessage message = null;

            BaseStream.ReadTimeout = 1200; // 1200 ms between chars - the gps detection requires this.

            DateTime start = DateTime.Now;

            //Console.WriteLine(DateTime.Now.Millisecond + " SR0 " + BaseStream.BytesToRead);

            lock (readlock)
            {
                if (BaseStream.IsOpen || logreadmode)  //MainV2.IsConnected
                {
                    try
                    {
                        if (logreadmode)
                        {
                            int len = logplaybackfile.Read(buffer, 0, buffer.Length);
                            if (len == -1)
                            {
                                logreadmode = false;
                                logplaybackfile.BaseStream.Seek(0, SeekOrigin.Begin);
                            }
                            if (DataConvertLeftLength > 0)
                            {
                                //合并处理
                                byte[] tempnew = new byte[DataConvertLeftLength + buffer.Length];

                                //残留部分拷贝
                                Array.Copy(converttemp, tempnew, DataConvertLeftLength);

                                //拷贝队列中的部分
                                Array.Copy(buffer, 0, tempnew, DataConvertLeftLength, buffer.Length);

                                newdataExp.uart_rms_data_para(ref tempnew, ref lefttemp, ref DataConvertLeftLength);
                            }
                            else
                            {
                                newdataExp.uart_rms_data_para(ref buffer, ref lefttemp, ref DataConvertLeftLength);
                            }

                            lastlogread = DateTime.Now;
                        }
                        else
                        {
                            // time updated for internal reference
                            //MAV.cs.datetime = DateTime.Now;

                            DateTime to = DateTime.Now.AddMilliseconds(BaseStream.ReadTimeout);
                            while (BaseStream.IsOpen && BaseStream.BytesToRead <= 0)
                            {
                                if (DateTime.Now > to)
                                {
                                    log.InfoFormat("MAVLINK: 1 wait time out btr {0} len {1}", BaseStream.BytesToRead,
                                        length);
                                    throw new TimeoutException("Timeout");
                                }
                                Thread.Sleep(1);
                                Console.WriteLine(DateTime.Now.Millisecond + " SR0b " + BaseStream.BytesToRead);
                            }
                            Console.WriteLine(DateTime.Now.Millisecond + " SR1a " + BaseStream.BytesToRead);
                            if (BaseStream.IsOpen)
                            {
                                int len = BaseStream.Read(buffer, 0, buffer.Length);
                                if (len == 0)
                                {
                                    Thread.Sleep(10);
                                    return;
                                }
                                //giveComport = true;
                                if (DataConvertLeftLength > 0)
                                {
                                    //合并处理
                                    byte[] tempnew = new byte[DataConvertLeftLength + buffer.Length];

                                    //残留部分拷贝
                                    Array.Copy(converttemp, tempnew, DataConvertLeftLength);

                                    //拷贝队列中的部分
                                    Array.Copy(buffer, 0, tempnew, DataConvertLeftLength, buffer.Length);

                                    newdataExp.uart_rms_data_para(ref tempnew, ref lefttemp, ref DataConvertLeftLength);
                                    //bd.Insert(newdataExp.magdata);
                                    //newdataExp.magdata
                                }
                                else
                                {
                                    newdataExp.uart_rms_data_para(ref buffer, ref lefttemp, ref DataConvertLeftLength);
                                    //bd.Insert(newdataExp.magdata);
                                }
                                if (rawlogfile != null && rawlogfile.CanWrite)
                                    rawlogfile.Write(buffer, 0, buffer.Length);
                            }
                            Console.WriteLine(DateTime.Now.Millisecond + " SR1b " + BaseStream.BytesToRead);
                        }
                    }
                    catch (Exception e)
                    {
                        log.Info("Link readpacket read error: " + e.ToString());
                    }
                }
                else
                {
                    BaseStream.Close();
                }
            } // end readlock

            // resize the packet to the correct length

            // add byte count
            _bytesReceivedSubj.OnNext(buffer.Length);

            // update bps statistics
            if (_bpstime.Second != DateTime.Now.Second)
            {
                long btr = 0;
                if (BaseStream != null && BaseStream.IsOpen)
                {
                    btr = BaseStream.BytesToRead;
                }
                else if (logreadmode)
                {
                    btr = logplaybackfile.BaseStream.Length - logplaybackfile.BaseStream.Position;
                }
                //Console.Write("bps {0} loss {1} left {2} mem {3} mav2 {4} sign {5} mav1 {6} mav2 {7} signed {8}      \n", _bps1, MAV.synclost, btr,
                //    GC.GetTotalMemory(false) / 1024 / 1024.0, MAV.mavlinkv2, MAV.signing, _mavlink1count, _mavlink2count, _mavlink2signed);
                _bps2 = _bps1; // prev sec
                _bps1 = 0; // current sec
                _bpstime = DateTime.Now;
                // count
            }

            _bps1 += buffer.Length;

            // create a state for any sysid/compid includes gcs on log playback

            // once set it cannot be reverted

            // stat count

            //check if sig was included in packet, and we are not ignoring the signature (signing isnt checked else we wont enable signing)
            //logreadmode we always ignore signing as they would not be in the log if they failed the signature

            // packet is now verified

            // extract wp's/rally/fence/camera feedback/params from stream, including gcs packets on playback

            // if its a gcs packet - dont process further

            // update packet loss statistics

            // update last valid packet receive time
            //lastvalidpacket = DateTime.Now;

            //return message;
        }

        ///////////////////////////////////////////////////////////////////////////////

        /// <summary>
        /// used to prevent comport access for exclusive use
        /// </summary>

        internal string plaintxtline = "";
        private string buildplaintxtline = "";

        public bool ReadOnly = false;

        //public TerrainFollow Terrain;

        private int _sysidcurrent = 0;

        public int sysidcurrent
        {
            get { return _sysidcurrent; }
            set
            {
                if (_sysidcurrent == value)
                    return;
                _sysidcurrent = value;
                if (MavChanged != null) MavChanged(this, null);
            }
        }

        private int _compidcurrent = 0;

        public int compidcurrent
        {
            get { return _compidcurrent; }
            set
            {
                if (_compidcurrent == value)
                    return;
                _compidcurrent = value;
                MavChanged?.Invoke(this, null);
            }
        }

        public MAVList MAVlist;

        public MAVState MAV
        {
            get { return MAVlist[sysidcurrent, compidcurrent]; }
            set { MAVlist[sysidcurrent, compidcurrent] = value; }
        }

        public double CONNECT_TIMEOUT_SECONDS = 30;

        /// <summary>
        /// progress form to handle connect and param requests
        /// </summary>
        private IProgressReporterDialogue frmProgressReporter;

        /// <summary>
        /// used for outbound packet sending
        /// </summary>
        internal int packetcount = 0;

        private readonly Subject<int> _bytesReceivedSubj = new Subject<int>();
        private readonly Subject<int> _bytesSentSubj = new Subject<int>();

        /// <summary>
        /// Observable of the count of bytes received, notified when the bytes themselves are received
        /// </summary>
        public IObservable<int> BytesReceived
        {
            get { return _bytesReceivedSubj; }
        }

        /// <summary>
        /// Observable of the count of bytes sent, notified when the bytes themselves are received
        /// </summary>
        public IObservable<int> BytesSent
        {
            get { return _bytesSentSubj; }
        }

        /// <summary>
        /// Observable of the count of packets skipped (on reception),
        /// calculated from periods where received packet sequence is not
        /// contiguous
        /// </summary>
        public Subject<int> WhenPacketLost { get; set; }

        public Subject<int> WhenPacketReceived { get; set; }

        /// <summary>
        /// used as a serial port write lock
        /// </summary>
        private volatile object objlock = new object();

        /// <summary>
        /// used for a readlock on readpacket
        /// </summary>
        private volatile object readlock = new object();

        /// <summary>
        /// enabled read from file mode
        /// </summary>
        public bool logreadmode
        {
            get { return _logreadmode; }
            set { _logreadmode = value; }
        }

        private bool _logreadmode = false;

        private BinaryReader _logplaybackfile;

        public DateTime lastlogread { get; set; }

        public BinaryReader logplaybackfile
        {
            get { return _logplaybackfile; }
            set
            {
                _logplaybackfile = value;
                if (_logplaybackfile != null && _logplaybackfile.BaseStream is FileStream)
                    log.Info("Logplaybackfile set " + ((FileStream)_logplaybackfile.BaseStream).Name);
                //MAVlist.Clear();
            }
        }

        public BufferedStream logfile { get; set; }

        public BufferedStream rawlogfile { get; set; }

        public FbConnection connection { get; set; }

        private int _bps1 = 0;
        private int _bps2 = 0;
        private DateTime _bpstime { get; set; }

        public LinkInterface()
        {
            // init fields
            MAVlist = new MAVList(this);
            this.BaseStream = new SerialPort();
            this.packetcount = 0;
            this._bytesReceivedSubj = new Subject<int>();
            this._bytesSentSubj = new Subject<int>();
            this.WhenPacketLost = new Subject<int>();
            this.WhenPacketReceived = new Subject<int>();
            this.readlock = new object();

            //this.mavlinkversion = 0;

            //this.debugmavlink = false;
            this.logreadmode = false;
            this.lastlogread = DateTime.MinValue;
            this._logplaybackfile = null;
            this.logfile = null;
            this.rawlogfile = null;
            this.connection = null;
        }

        public LinkInterface(Stream logfileStream)
            : this()
        {
            logplaybackfile = new BinaryReader(logfileStream);
            logreadmode = true;
        }

        public void Close()
        {
            try
            {
                if (logfile != null)
                    logfile.Close();
            }
            catch
            {
            }
            try
            {
                if (rawlogfile != null)
                    rawlogfile.Close();
            }
            catch
            {
            }
            try
            {
                if (logplaybackfile != null)
                    logplaybackfile.Close();
            }
            catch
            {
            }

            try
            {
                if (BaseStream.IsOpen)
                    BaseStream.Close();
            }
            catch
            {
            }
            try
            {
                connection.Close();
            }
            catch
            {
            }
        }

        public void Open()
        {
            Open(false);
        }

        public void Open(bool skipconnectedcheck = false)
        {
            if (BaseStream.IsOpen && !skipconnectedcheck)
                return;

            //MAVlist.Clear();

            frmProgressReporter = new ProgressReporterDialogue
            {
                StartPosition = FormStartPosition.CenterScreen,
                Text = "Connecting..."
            };

            frmProgressReporter.DoWork += FrmProgressReporterDoWorkNOParams;
            frmProgressReporter.UpdateProgressAndStatus(-1, "Connecting...");
            //ThemeManager.ApplyThemeTo(frmProgressReporter);

            frmProgressReporter.RunBackgroundOperationAsync();

            frmProgressReporter.Dispose();
        }

        private void ProgressWorkerEventArgs_CancelRequestChanged(object sender, PropertyChangedEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void FrmProgressReporterDoWorkNOParams(object sender, ProgressWorkerEventArgs e, object passdata = null)
        {
            OpenBg(sender, false, e);
        }

        private void OpenBg(object PRsender, bool getparams, ProgressWorkerEventArgs progressWorkerEventArgs)
        {
            frmProgressReporter.UpdateProgressAndStatus(-1, "正在连接.....");

            giveComport = true;

            byte[] buffer = new byte[1024];

            if (BaseStream is SerialPort)
            {
                // allow settings to settle - previous dtr
                Thread.Sleep(1000);
            }

            //Terrain = new TerrainFollow(this);

            try
            {
                BaseStream.ReadBufferSize = 1024;

                lock (objlock) // so we dont have random traffic
                {
                    log.Info("Open port with " + BaseStream.PortName + " " + BaseStream.BaudRate);

                    if (BaseStream is UdpSerial)
                    {
                        progressWorkerEventArgs.CancelRequestChanged += (o, e) =>
                        {
                            ((UdpSerial)BaseStream).CancelConnect = true;
                            ((ProgressWorkerEventArgs)o)
                                .CancelAcknowledged = true;
                        };
                    }

                    BaseStream.Open();
                    // 读取数据 待修改
                    //BaseStream.DiscardInBuffer();

                    // other boards seem to have issues if there is no delay? posible bootloader timeout issue
                    if (BaseStream is SerialPort)
                    {
                        Thread.Sleep(1000);
                    }
                }

                //List<MAVLinkMessage> hbhistory = new List<MAVLinkMessage>();

                DateTime start = DateTime.Now;
                DateTime deadline = start.AddSeconds(CONNECT_TIMEOUT_SECONDS);

                var countDown = new Timer { Interval = 1000, AutoReset = false };
                countDown.Elapsed += (sender, e) =>
                {
                    int secondsRemaining = (deadline - e.SignalTime).Seconds;
                    frmProgressReporter.UpdateProgressAndStatus(-1, string.Format(Strings.Trying, secondsRemaining));
                    if (secondsRemaining > 0) countDown.Start();
                };
                countDown.Start();

                int count = 0;

                while (true)
                {
                    if (progressWorkerEventArgs.CancelRequested)
                    {
                        progressWorkerEventArgs.CancelAcknowledged = true;
                        countDown.Stop();
                        if (BaseStream.IsOpen)
                            BaseStream.Close();
                        giveComport = false;
                        return;
                    }

                    log.Info(DateTime.Now.Millisecond + " Start connect loop ");

                    if (DateTime.Now > deadline)
                    {
                        //if (Progress != null)
                        //    Progress(-1, "No Heartbeat Packets");
                        countDown.Stop();
                        this.Close();
                    }

                    Thread.Sleep(1);

                    //BaseStream.Read(buffer, count, 1);
                    count++;
                    // 解析数据，获取 sysid ,comid

                    sysidcurrent = 1;
                    compidcurrent = 50;

                    // if we get no data, try enableing rts/cts
                    //if (buffer.Length == 0 && BaseStream is SerialPort)
                    //{
                    //    BaseStream.RtsEnable = !BaseStream.RtsEnable;
                    //}

                    if (count > 2)
                    {
                        break;
                    }

                    SetupMavConnect(1, 50, "AIR");
                }

                countDown.Stop();

                if (frmProgressReporter.doWorkArgs.CancelAcknowledged == true)
                {
                    giveComport = false;
                    if (BaseStream.IsOpen)
                        BaseStream.Close();
                    return;
                }
            }
            catch (Exception e)
            {
                try
                {
                    BaseStream.Close();
                }
                catch
                {
                }
                giveComport = false;
                if (string.IsNullOrEmpty(progressWorkerEventArgs.ErrorMessage))
                    progressWorkerEventArgs.ErrorMessage = Strings.ConnectFailed;
                log.Error(e);
                Console.WriteLine(e.ToString());
                throw;
            }
            //frmProgressReporter.Close();
            giveComport = false;
            frmProgressReporter.UpdateProgressAndStatus(100, Strings.Done);
            //log.Info("Done open " + MAV.sysid + " " + MAV.compid);
            //MAV.packetslost = 0;
            //MAV.synclost = 0;
        }

        private void SetupMavConnect(int sysid, int compid, string compname)
        {
            sysidcurrent = sysid;
            compidcurrent = compid;

            MAVlist[sysid, compid].aptype = (MAV_TYPE)2;
            MAVlist[sysid, compid].apname = compname;

            MAVlist[sysid, compid].sysid = (byte)sysid;
            MAVlist[sysid, compid].compid = (byte)compid;
            //MAVlist[sysid, compid].recvpacketcount = seq;
        }

        public void sendPacket(byte indata)
        {
            generatePacket(indata);
            return;
        }

        private void generatePacket(byte indata)
        {
            //uses currently targeted mavs sysid and compid
            generatePacket(indata, false);
        }

        /// <summary>
        /// Generate a Mavlink Packet and write to serial
        /// </summary>
        /// <param name="messageType">type number = MAVLINK_MSG_ID</param>
        /// <param name="indata">byte data</param>
        public void generatePacket(byte indata, bool forcesigning = false)
        {
            giveComport = true;

            if (!BaseStream.IsOpen)
            {
                return;
            }

            lock (objlock)
            {
                // 现在就一个字节
                //byte data = indata;
                int i = 5;
                byte[] packet = new byte[i];

                packet[0] = Convert.ToByte("AA", 16);
                packet[1] = Convert.ToByte("AA", 16);
                packet[2] = Convert.ToByte("55", 16);
                packet[3] = Convert.ToByte("55", 16);
                packet[4] = indata;

                if (BaseStream.IsOpen)
                {
                    BaseStream.Write(packet, 0, i);
                    _bytesSentSubj.OnNext(i);
                }

                try
                {
                    if (logfile != null && logfile.CanWrite)
                    {
                        lock (logfile)
                        {
                            byte[] datearray =
                                BitConverter.GetBytes(
                                    (UInt64)((DateTime.UtcNow - new DateTime(1970, 1, 1)).TotalMilliseconds * 1000));
                            Array.Reverse(datearray);
                            logfile.Write(datearray, 0, datearray.Length);
                            logfile.Write(packet, 0, i);
                        }
                    }
                }
                catch
                {
                }
            }
        }

        public bool Write(string line)
        {
            lock (objlock)
            {
                BaseStream.Write(line);
            }
            _bytesSentSubj.OnNext(line.Length);
            return true;
        }

        public void Dispose()
        {
            if (_bytesReceivedSubj != null)
                _bytesReceivedSubj.Dispose();
            if (_bytesSentSubj != null)
                _bytesSentSubj.Dispose();
            //if (MAVlist != null)
            //    MAVlist.Dispose();

            this.Close();

            //Terrain = null;

            MirrorStream = null;

            logreadmode = false;
            logplaybackfile = null;
        }
    }
}