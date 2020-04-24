using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using GMap.NET;
using log4net;
using aeromagtec.Maps;
using aeromagtec.Utilities;
using Newtonsoft.Json;

namespace aeromagtec
{
    public class MAVState : MAVLink
    {
        private static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        [JsonIgnore]
        [IgnoreDataMember]
        public LinkInterface parent;

        public MAVState(LinkInterface LinkInterface, byte sysid, byte compid)
        {
            this.parent = LinkInterface;
            this.sysid = sysid;
            this.compid = compid;
            this.packetspersecond = new Dictionary<uint, double>();
            this.packetspersecondbuild = new Dictionary<uint, DateTime>();
            this.lastvalidpacket = DateTime.MinValue;
            sendlinkid = (byte)(new Random().Next(256));

            //this.param = new MAVLinkParamList();
            this.packets = new Dictionary<uint, MAVLinkMessage>();

            this.recvpacketcount = 0;
            this.VersionString = "";

            camerapoints.Clear();

            GMapMarkerOverlapCount.Clear();

            this.packetslost = 0f;
            this.packetsnotlost = 0f;
            this.packetlosttimer = DateTime.MinValue;
            cs.parent = this;
        }

        public float packetslost = 0;
        public float packetsnotlost = 0;
        public DateTime packetlosttimer = DateTime.MinValue;
        public float synclost = 0;

        // all
        public string VersionString { get; set; }

        public MAV_TYPE aptype { get; set; }

        public String apname { get; set; }

        /// <summary>
        /// mavlink 2 enable
        /// </summary>
        public bool mavlinkv2 = false;

        /// <summary>
        /// the static global state of the currently connected MAV
        /// </summary>
        public CurrentState cs = new CurrentState();

        private byte _sysid;

        /// <summary>
        /// mavlink remote sysid
        /// </summary>
        public byte sysid
        {
            get { return _sysid; }
            set { _sysid = value; }
        }

        /// <summary>
        /// mavlink remove compid
        /// </summary>
        public byte compid { get; set; }

        public byte linkid { get; set; }

        public byte sendlinkid { get; internal set; }

        public UInt64 timestamp { get; set; }

        /// <summary>
        /// storage for whole paramater list
        /// </summary>
        [JsonIgnore]
        [IgnoreDataMember]
        public MAVLinkParamList param { get; set; }

        [JsonIgnore]
        [IgnoreDataMember]
        public Dictionary<string, MAV_PARAM_TYPE> param_types = new Dictionary<string, MAV_PARAM_TYPE>();

        /// <summary>
        /// storage of a previous packet recevied of a specific type
        /// </summary>
        private Dictionary<uint, MAVLinkMessage> packets { get; set; }

        private object packetslock = new object();

        /// <summary>
        /// are we signing outgoing packets, and checking incomming packet signatures
        /// </summary>
        public bool signing { get; set; }

        /// <summary>
        /// mavlink 2 enable
        /// </summary>

        public MAVLinkMessage getPacket(uint mavlinkid)
        {
            //log.InfoFormat("getPacket {0}", (MAVLINK_MSG_ID)mavlinkid);
            lock (packetslock)
            {
                if (packets.ContainsKey(mavlinkid))
                {
                    return packets[mavlinkid];
                }
            }

            return null;
        }

        public void addPacket(MAVLinkMessage msg)
        {
            lock (packetslock)
            {
                packets[msg.msgid] = msg;
            }
        }

        public void clearPacket(uint mavlinkid)
        {
            lock (packetslock)
            {
                if (packets.ContainsKey(mavlinkid))
                {
                    packets[mavlinkid] = null;
                }
            }
        }

        public void Dispose()
        {
            //if (Proximity != null)
            //    Proximity.Dispose();
        }

        /// <summary>
        /// time seen of last mavlink packet
        /// </summary>
        public DateTime lastvalidpacket { get; set; }

        /// <summary>
        /// used to calc packets per second on any single message type - used for stream rate comparaison
        /// </summary>
        [JsonIgnore]
        [IgnoreDataMember]
        public Dictionary<uint, double> packetspersecond { get; set; }

        /// <summary>
        /// time last seen a packet of a type
        /// </summary>
        [JsonIgnore]
        [IgnoreDataMember]
        public Dictionary<uint, DateTime> packetspersecondbuild { get; set; }

        /// <summary>
        /// mavlink ap type
        /// </summary>

        /// <summary>
        /// used as a snapshot of what is loaded on the ap atm. - derived from the stream
        /// </summary>
        public ConcurrentDictionary<int, mavlink_mission_item_t> wps = new ConcurrentDictionary<int, mavlink_mission_item_t>();

        public ConcurrentDictionary<int, mavlink_rally_point_t> rallypoints = new ConcurrentDictionary<int, mavlink_rally_point_t>();

        public ConcurrentDictionary<int, mavlink_fence_point_t> fencepoints = new ConcurrentDictionary<int, mavlink_fence_point_t>();

        public List<mavlink_camera_feedback_t> camerapoints = new List<mavlink_camera_feedback_t>();

        public GMapMarkerOverlapCount GMapMarkerOverlapCount = new GMapMarkerOverlapCount(PointLatLng.Empty);

        internal int recvpacketcount = 0;
        public Int64 time_offset_ns { get; set; }
    }
}