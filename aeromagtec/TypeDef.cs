using System;
using System.Runtime.InteropServices;

namespace aeromagtec
{
    /// <summary>
    /// 20170104 RS422串口数据格式
    ///
    //一条数据16个字节
    //小端字节序
    //地地址低位，高地址高位
    //2+3+3+3+5
    //2：AA AA
    //3个通道24位数据
    //3+2：24bit+16bit
    //X+Y
    //数值+1
    //2.5/2^32*2
    //磁测转换系数
    //327680000*（Y+1）/(X+1)/3.498577*1000

    //327680000*（Y+1）/(X+1)/3498.577(PT)

    /// </summary>
    internal class TypeDef
    {
    }

    //接收的数据类型，用于显示或者解析
    public enum ReceivedDataType
    {
        CharType,
        HexType,
        StrutType
    };

    public enum SendDataType
    {
        CharType,
        HexType
    }

    //20170516增加一个光泵通道 20180402 50个字节
    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct UART_RMS_DATA
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] StartEndCode;

        public byte respond;//返回值

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]//fc
        public byte[] CH1_X;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] CH1_Y;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]//fc
        public byte[] CH2_X;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] CH2_Y;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]//X
        public byte[] CH3_X;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]//Y
        public byte[] CH4_Y;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]//Z
        public byte[] CH5_Z;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public byte[] voltage;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]
        public byte[] current;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]//4-5
        public byte[] latitude_integer;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]//4-5
        public byte[] latitude_decimals;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 2)]//4-5
        public byte[] longitude_integer;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]//4-5
        public byte[] longitude_decimals;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]//4-3
        public byte[] alt;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 1)]//2-1
        public byte[] states;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
        public byte[] count;

        public byte hour_gpsstate;
        public byte minute;
        public byte sec;
    }

    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    public struct TMAVSTATE
    {
        public double battery_voltage;
        public float current;
        public Byte satcount;
        public double lat;
        public double lng;
        public double alt;
        public Byte ocxo_states;
        public Byte work_states;
        public float ocxo_voltage;
        public Byte gps_fix_type;
        public Byte Hour;
        public Byte Minute;
        public Byte Sec;
        public int Count;
        public double mx;
        public double my;
        public double mz;
        public double mag01;
        public double mag02;
        public int errorcount;
    }

    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct state_box
    {
        public byte work_states;
        public byte ocxo_states;
        public float ocxo_voltage;
        public int count;
        public byte gps_fix_type;
        public byte stars_inuse;
        public double lat;
        public double lon;
        public double alt;
        public float voltage;
        public float current;
        public byte hour;
        public byte minute;
        public byte sec;
    }

    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct RMSInfo
    {
        public short day;                  // 日
        public short month;                // 月
        public short year;                 // 年 2001-2999
        public short hour;                 // 时
        public short minute;               // 分

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
        public string geo;              // 工作地区 - 地名

        public byte sec;              // 秒
        public byte met;          // 记录方法代码:

        //0-31
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 21)]
        public string standby1; // 备用

        public sbyte pro;          // ADC量化代码:
        public byte kan;          // number of channels 通道数
        public byte Tx_Onoff;				 // 发射开关zqm
        public uint Tx_Freq;					  // 发射频率

        public short Tx_percen;				//发射占空比
        public short Sub_Samplerate;               // 显示抽样倍率
        public float Tok;					// 发射电流 I(A)
        public uint standby4;				// 备用

        //32-71

        public byte Tgu;          // 发射波形: 1-doubling

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 19)]
        public string standby5; // 备用

        public short Nom;					// record of number 记录号与文件名相对应

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 6)]
        public string standby6; // 备用

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 20)]
        public string Fam;				// surname of the operator 操作员

        // 72-119

        public int Lps;					// number of points in record 记录的数据点数

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 18)]
        public string standby7;        // 备用

        public short boxid;					// MeterNum 仪器号

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 8)]
        public string standby8;        // 备用

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 48)]
        public string Com;

        // string of the comment 注释行

        //120-199
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 90)]
        public short[] Isw;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 132)]
        public string standby9;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 24)]
        public ChInfo[] channel;
    }

    // 406-511
    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]//按一个字节对齐
    internal struct ChInfo
    {
        public byte Idk;		//
        public byte Sensor_Gain; //磁棒增益：1:100  2:200 4:400 8:800
        public byte Ch_Gain;		// 通道增益代码:250：1/4;  1-4-8-16
        public byte Ufl;				// 测站号
        public short Pkt;				// 测点号
        public short Prf;				// 测线号

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 12)]
        public string standby1;

        public float X;				//coordinate 'x' (not use)
        public float Y;				//coordinate 'y' (not use)
        public float Z;				//coordinate 'z' (not use)
        public float Ecs;				// LSB

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 28)]
        public string standby2;
    }    // 64*24=1536

    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct tm
    {
        /*
         * the number of seconds after the minute, normally in the range
         * 0 to 59, but can be up to 60 to allow for leap seconds
         */
        public int tm_sec;
        /* the number of minutes after the hour, in the range 0 to 59*/
        public int tm_min;
        /* the number of hours past midnight, in the range 0 to 23 */
        public int tm_hour;
        /* the day of the month, in the range 1 to 31 */
        public int tm_mday;
        /* the number of months since January, in the range 0 to 11 */
        public int tm_mon;
        /* the number of years since 1900 */
        public int tm_year;
        /* the number of days since Sunday, in the range 0 to 6 */
        public int tm_wday;
        /* the number of days since January 1, in the range 0 to 365 */
        public int tm_yday;
        public int tm_isdst;
        public int tm_gmtoff;
        public int tm_zone;
    }

    [StructLayoutAttribute(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
    internal struct state_gps
    {
        public byte init_ok;//是否有效
        public ushort gpsweek;
        public uint msofweek;
        public int stars_inuse;
        public int fix_type;	/*1--NMEA_FIX_BAD,2--NMEA_FIX_2D,3--NMEA_FIX_3D */
        public double lat;        /**< Latitude in NDEG - +/-[degree][min].[sec/60] */
        public double lon;        /**< Longitude in NDEG - +/-[degree][min].[sec/60] */
        public double speed;      /**< Speed over the ground in kilometers/hour */
        public double direction;  /**< Track angle in degrees True */
        public double altitude;   /**< Antenna altitude above/below mean sea level (geoid) units:Meter*/
        public int stars_inview;

        //public tm cur_time;
        public DateTime utc_time;
    }
}