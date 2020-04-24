using System;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SQLite;

namespace aeromagtec.Utilities
{
    public class MyDataTable
    {
        [Column("id"), PrimaryKey, NotNull]
        public Int64 Nany { get; set; }

        [Column("CheckFlg")]
        public Int32 CheckFlg { get; set; }

        [Column("Count")]
        public Int32 Count { get; set; }

        [Column("CX1")]
        public double CX1_data { get; set; }

        [Column("CX2")]
        public double CX2_data { get; set; }

        [Column("CX1rawMean ")]
        public double CX1rawMean { get; set; }

        [Column("CX1Mean")]
        public double CX1Mean { get; set; }

        [Column("CX2rawMean")]
        public double CX2rawMean { get; set; }

        [Column("CX2Mean")]
        public double CX2Mean { get; set; }

        [Column("mx")]
        public double mx { get; set; }

        [Column("my")]
        public double my { get; set; }

        [Column("mz")]
        public double mz { get; set; }

        [Column("lat")]
        public double lat { get; set; }

        [Column("lng")]
        public double lng { get; set; }

        [Column("alt")]
        public double alt { get; set; }

        [Column("x")]
        public double x { get; set; }

        [Column("y ")]
        public double y { get; set; }

        [Column("z")]
        public double z { get; set; }

        [Column("zone")]
        public double zone { get; set; }

        [Column("Hour")]
        public Int16 Hour { get; set; }

        [Column("Minute")]
        public Int16 Minute { get; set; }

        [Column("Sec")]
        public Int16 Sec { get; set; }

        [Column("voltage")]
        public float voltage { get; set; }

        [Column("current")]
        public float current { get; set; }

        [Column("ocxo_states")]
        public Int16 ocxo_states { get; set; }

        [Column("work_states")]
        public Int16 work_states { get; set; }

        [Column("ocxo_voltage")]
        public float ocxo_voltage { get; set; }

        [Column("satcount")]
        public Int16 satcount { get; set; }

        [Column("gps_fix_type")]
        public Int16 gps_fix_type { get; set; }
    }

    public class ReferSite : MyDataTable
    {
    }
}