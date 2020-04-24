using System;
using System.Runtime.InteropServices;
using System.Data;
using aeromagtec.GCSViews;
using aeromagtec.Utilites;
using System.IO;

namespace aeromagtec.Utilities
{
    public class dataExp
    {
        public static UART_RMS_DATA uartdata;

        //===========================================================B
        // === 时间信息组
        private static int fs;           // 需要维持。 这里要求采样率是整数值 。(界面输入设置)

        private static double sdt;         // 需要维持。 1/fs;
        private Int64 N0;          // 需要维持。 接收机本次采集，开始采集时（计数值==1时）的数据绝对编号（相对于2020-01-10 00:00:00时,N0=1）
        private static DateTime DTref;     // 需要维持。 绝对参考时间 2020-01-01 00:00:00
        public static DateTime StartDateTime; // 需要维持。 非必须，可变为临时变量。维护时，可隐式的描述YY,MM,DD。
        public static int YY, MM, DD;     // 需要维持。首次读入本次采集的数据时，必须首先提供。(界面输入设置)。 *** 数据文件中无年月日，需要在初化时设定。比如从以前的文件中读取时需要设置。实时数据可从当前系统时间获取。

        //
        public Int64 Nany;        // 输出的样点编号。

        //
        public int iLastSec;       // 需要维持。 上个样点的秒时间，用于搜索。

        public bool GotStartTime;  // 需要维持。 用于表示搜索是否完成。是否已经获取到本次采集的起始时间，如果未获取，数据都被视为无效（因为无法获取数据绝对编号）。

        // === 数据检查信息组
        public int Nsec4Mean;                      // 设置用于计算均值的时间长度。(界面输入设置)

        public int NSampleMean;                    // 维持。
        public double CX1rawMean, CX2rawMean;      // 原始数据的均值。(界面输出显示)
        public double CX1Mean, CX2Mean;            // 好的数据的均值。(界面输出显示)
        public int BadDataCheckMethod;             // 判断好数据的方式。 (界面输入设置)。0：不检查数据好坏。1：用滑动窗均值作为基准。2：用用户设置均值作为基准。
        public double SetMeanCX1, SetMeanCX2;      // 手动设置的数 据均值。(界面输入设置)
        public double MaxRangOverMeanCX1, MaxRangOverMeanCX2;  // 数据偏离均值的最大值。(界面输入设置)。超过这个值被视为坏数据。

        //int DataCheckFlag;                  // 数据检查结果的标记输出(随样点输出)。0：正常数据。1：丢失数据（这里无法给出，暂时无定义）。2：超范围数据（坏数据）。 随数据输出。
        //
        private static int GotNumofData;

        public double[] CX1RawVector;      // 原始数据 求均值的数据向量
        public double[] CX2RawVector;      // 原始数据 求均值的数据向量
        public double[] CX1CheckVector;    // 求均值的数据向量
        public double[] CX2CheckVector;    // 求均值的数据向量
        public int vid;
        private static Int64 LastID;      // 需要维持。上一个正确接收的样点编号。
        private static double LastRawValueCX1, LastRawValueCX2;// 上一个好数据样点的数据值。
        private static double LastCheckValueCX1, LastCheckValueCX2;// 上一个好数据样点的数据值。
        public MyDataTable magdata = new MyDataTable();

        // ===========================================================

        //===========================================================
        //public dataExp()
        //{
        //    // 时间检查组
        //    N0 = 0;
        //    DTref = Convert.ToDateTime("2020-01-01 00:00:00");
        //    iLastSec = -1;
        //    GotStartTime = false;
        //    fs = 160;
        //    sdt = 1.0F / fs;
        //    YY = 2020;
        //    MM = 1;
        //    DD = 1;
        //    // 数据检查组
        //    GotNumofData = 0;
        //    Nsec4Mean = 1;
        //    SetMeanCX1 = 48830; SetMeanCX2 = 49601;
        //    MaxRangOverMeanCX1 = 100;
        //    MaxRangOverMeanCX2 = 100;
        //    NSampleMean = fs * Nsec4Mean;
        //    vid = 0;
        //    LastID = 0;
        //    BadDataCheckMethod = 1;     // 0：不检查
        //    LastRawValueCX1 = -1; LastRawValueCX2 = -1;
        //    LastCheckValueCX1 = -1; LastCheckValueCX2 = -1;
        //}
        public void ReInitial(int SamplingRate, int yy, int mm, int dd,
            int nsec4mean, double setmeancx1, double setmeancx2, int badckmodel, double moc1, double moc2)
        {
            // 时间检查组
            N0 = 0;
            DTref = Convert.ToDateTime("2020-01-01 00:00:00");
            iLastSec = -1;
            GotStartTime = false;
            fs = SamplingRate;
            sdt = 1.0F / fs;
            YY = yy;
            MM = mm;
            DD = dd;
            // 数据检查组
            GotNumofData = 0;       // 已经计算
            Nsec4Mean = nsec4mean; // 1
            SetMeanCX1 = setmeancx1; SetMeanCX2 = setmeancx2;
            MaxRangOverMeanCX1 = moc1; // 100
            MaxRangOverMeanCX2 = moc2; // 100
            NSampleMean = fs * Nsec4Mean;
            CX1RawVector = new double[NSampleMean];
            CX2RawVector = new double[NSampleMean];
            CX1CheckVector = new double[NSampleMean];
            CX2CheckVector = new double[NSampleMean];
            vid = 0;
            LastID = 0;
            BadDataCheckMethod = badckmodel;     // 0：不检查  , 1: 检查
            LastRawValueCX1 = -1; LastRawValueCX2 = -1;
            LastCheckValueCX1 = -1; LastCheckValueCX2 = -1;
        }

        public void sampleID2DateTime(int sr, UInt64 SampleID, ref DateTime outDT, ref int us)
        {
            int K = (int)((SampleID - 1) / (UInt64)sr);
            int msk = (int)((SampleID - 1) % (UInt64)sr);
            us = (int)(msk * sdt * 1000000.0F);    // 样点的微秒值，整数
            outDT = DTref.AddSeconds(K);
            outDT = outDT.AddMilliseconds(us / 1000);
        }

        public void DateTime2sampleID(int sr, DateTime inDT, int us, ref UInt64 outSampleID)
        {
            TimeSpan ts = inDT - DTref;
            int SpanSec = (int)ts.TotalSeconds;
            outSampleID = (UInt64)SpanSec * (UInt64)sr;
            outSampleID += (UInt64)(us * sr / 1000000 + 1);
        }

        private void ChcekCXData(double CX1, double CX2, Int64 DataID, ref int CheckFlg)
        {
            // 数据检查组
            if (CX1 == 0 || CX2 == 0)
            {
                CX1 = LastCheckValueCX1;     // 上一次好的数据
                CX2 = LastCheckValueCX2;
                // 如果前一个也是不是好数据，则直接返回。即如果从开始一直是坏数据，则直接返回。
                if (LastCheckValueCX1 == 1 || LastCheckValueCX1 == -1)
                {
                    CheckFlg = 2;
                    return;
                }
            }
            // 从这开始数据肯定不是零，即前面先搜索到不为零的数据样点，才将数据压入缓存。
            //
            double LrawCX1, LrawCX2;
            double LCX1, LCX2;
            // 如果是第1个样点，不进行丢数据检查。
            if (LastID == 0)
            {
                CX1RawVector[vid] = CX1;
                CX2RawVector[vid] = CX2;
                CX1CheckVector[vid] = CX1;       // 放入第一个好的数据。 开始都认为是好的。
                CX2CheckVector[vid] = CX2;
                vid++;
                GotNumofData++;
                LastCheckValueCX1 = CX1;     // 则存在上一个好的数据（假设是好的），只用于计算均值，返回指示认为是坏的（控制后面的滤波处理）。
                LastCheckValueCX2 = CX2;

                CheckFlg = (BadDataCheckMethod == 0) ? 0 : 2;       // 如果是数据检查模式，则开始的一段数据视为坏数据。（控制后面的滤波处理）
                LastID = DataID;
                return;
            }
            // 样点编号是否连续
            int Nlost = (int)(DataID - LastID);
            // 如果有丢的数据,则使用上一个好的数据，先补全丢失的数据。
            for (int ii = 0; ii < Nlost - 1; ii++)
            {
                vid = vid % NSampleMean;
                // 先保存最左边一个点
                LrawCX1 = CX1RawVector[vid % NSampleMean];
                LrawCX2 = CX2RawVector[vid % NSampleMean];
                LCX1 = CX1CheckVector[vid % NSampleMean];
                LCX2 = CX2CheckVector[vid % NSampleMean];
                // 用上一个数据来填充。
                CX1RawVector[vid] = LastRawValueCX1;
                CX2RawVector[vid] = LastRawValueCX2;
                // 用上一个好的数据来填充。
                CX1CheckVector[vid] = LastCheckValueCX1;
                CX2CheckVector[vid] = LastCheckValueCX2;
                vid++;
                GotNumofData++;
                // 如果一段数据已经满。
                if (GotNumofData >= NSampleMean)
                {
                    GotNumofData = NSampleMean;
                    // 求均值
                    CX1Mean = CX1Mean - LCX1 / NSampleMean + CX1 / NSampleMean;
                    CX2Mean = CX2Mean - LCX2 / NSampleMean + CX2 / NSampleMean;
                }
            }
            // 数据检查方式
            if (BadDataCheckMethod == 2)
            {
                CX1Mean = SetMeanCX1;
                CX2Mean = SetMeanCX2;
            }
            // 当前数据样点, 先保存最左边一个点
            LastID = DataID;
            vid = vid % NSampleMean;
            LrawCX1 = CX1RawVector[vid % NSampleMean];    // 先保存最左边一个点
            LrawCX2 = CX2RawVector[vid % NSampleMean];
            LCX1 = CX1CheckVector[vid % NSampleMean];    // 先保存最左边一个点
            LCX2 = CX2CheckVector[vid % NSampleMean];
            // 数据量不足，均值向量未满
            if (GotNumofData < NSampleMean)
            {
                CX1RawVector[vid] = CX1;
                CX2RawVector[vid] = CX2;
                CX1CheckVector[vid] = CX1;
                CX2CheckVector[vid] = CX2;
                LastRawValueCX1 = CX1RawVector[vid];
                LastRawValueCX2 = CX2RawVector[vid];
                LastCheckValueCX1 = CX1CheckVector[vid];
                LastCheckValueCX2 = CX2CheckVector[vid];
                vid++;
                GotNumofData++;
                CheckFlg = (BadDataCheckMethod == 0) ? 0 : 2;       // 如果是数据检查模式，则开始的一段数据视为坏数据。（控制后面的滤波处理）

                if (GotNumofData >= NSampleMean)
                {
                    CX1rawMean = 0;
                    CX2rawMean = 0;
                    CX1Mean = 0;
                    CX2Mean = 0;
                    for (int ii = 0; ii < NSampleMean; ii++)
                    {
                        CX1rawMean += CX1RawVector[ii] / NSampleMean;
                        CX2rawMean += CX2RawVector[ii] / NSampleMean;
                        CX1Mean += CX1CheckVector[ii] / NSampleMean;
                        CX2Mean += CX2CheckVector[ii] / NSampleMean;
                    }
                }
            }
            else    // 缓存数据已满
            {
                // 原始数据
                CX1RawVector[vid] = CX1;
                CX2RawVector[vid] = CX2;
                // 数据幅度检查
                if ((Math.Abs(CX1 - CX1Mean) <= MaxRangOverMeanCX1) || (Math.Abs(CX2 - CX2Mean) <= MaxRangOverMeanCX2) || (BadDataCheckMethod == 0))
                {
                    // 合格数据或不检查
                    CX1CheckVector[vid] = CX1;
                    CX2CheckVector[vid] = CX2;
                    CheckFlg = 0;
                }
                else
                {
                    // 不合格数据，使用上一个好数据。
                    CX1CheckVector[vid] = LCX1;
                    CX2CheckVector[vid] = LCX2;
                    CheckFlg = (BadDataCheckMethod == 0) ? 0 : 2;
                }
                LastRawValueCX1 = CX1RawVector[vid];
                LastRawValueCX2 = CX2RawVector[vid];
                LastCheckValueCX1 = CX1CheckVector[vid];
                LastCheckValueCX2 = CX2CheckVector[vid];
                // 如果一段数据已经满。
                vid++;
                GotNumofData = NSampleMean;
                // 求均值
                CX1rawMean = CX1rawMean - LrawCX1 / NSampleMean + CX1 / NSampleMean;
                CX2rawMean = CX2rawMean - LrawCX2 / NSampleMean + CX2 / NSampleMean;
                CX1Mean = CX1Mean - LCX1 / NSampleMean + CX1 / NSampleMean;
                CX2Mean = CX2Mean - LCX2 / NSampleMean + CX2 / NSampleMean;
            }
        }

        //===========================================================E
        public DataTable RawDataTable()
        {
            DataTable RawDataTable = new DataTable("RawDataTable");
            // Define all the columns once.
            DataColumn[] cols ={
                                  new DataColumn("Nany",typeof(Int64)),
                                  new DataColumn("CheckFlg",typeof(Int16)),
                                  new DataColumn("Count",typeof(Int32)),
                                  new DataColumn("CX1_data",typeof(double)),
                                  new DataColumn("CX2_data",typeof(double)),
                                  new DataColumn("CX1rawMean",typeof(double)),
                                  new DataColumn("CX1Mean",typeof(double)),
                                  new DataColumn("CX2rawMean",typeof(double)),
                                  new DataColumn("CX2Mean",typeof(double)),
                                  new DataColumn("mx",typeof(double)),
                                  new DataColumn("my",typeof(double)),
                                  new DataColumn("mz",typeof(double)),
                                  new DataColumn("lat",typeof(double)),
                                  new DataColumn("lng",typeof(double)),
                                  new DataColumn("alt",typeof(double)),
                                  new DataColumn("x",typeof(double)),
                                  new DataColumn("y",typeof(double)),
                                  new DataColumn("zone",typeof(double)),
                                  new DataColumn("Hour",typeof(Int16)),
                                  new DataColumn("Minute",typeof(Int16)),
                                  new DataColumn("Sec",typeof(Int16)),
                                  new DataColumn("voltage",typeof(double)),
                                  new DataColumn("current",typeof(float)),
                                  new DataColumn("work_states",typeof(Int16)),
                                  new DataColumn("ocxo_states",typeof(Int16)),
                                  new DataColumn("ocxo_voltage",typeof(Int16)),
                                  new DataColumn("satcount",typeof(Int16)),
                                  new DataColumn("gps_fix_type",typeof(Int16))
                              };

            RawDataTable.Columns.AddRange(cols);
            RawDataTable.PrimaryKey = new DataColumn[] { RawDataTable.Columns["Nany"] };
            return RawDataTable;
        }

        //DataTable rawData = RawDataTable();
        public static object BytesToStuct(byte[] bytes, int offset, Type type)
        {
            //得到结构体的大小
            int size = Marshal.SizeOf(type);
            //byte数组长度小于结构体的大小
            if (size > bytes.Length)
            {
                //返回"
                return null;
            }
            //分配结构体大小的内存空间
            IntPtr structPtr = Marshal.AllocHGlobal(size);
            //将byte数组拷到分配好的内存空间
            Marshal.Copy(bytes, offset, structPtr, size);
            //将内存空间转换为目标结构"
            object obj = Marshal.PtrToStructure(structPtr, type);
            //释放内存空间
            Marshal.FreeHGlobal(structPtr);
            //返回结构"
            return obj;
        }

        public void uart_rms_data_para(ref byte[] insoure, ref byte[] lefttemp, ref int DataConvertLeftLength)
        {
            //============================================B
            uartdata = new UART_RMS_DATA();
            int StructLen = 48;
            double adconvert = 1.0;
            DataTable rawData = RawDataTable();
            DataRow row1 = rawData.NewRow();
            int errorcount = 0;
            //============================================E

            //每次最多只做一次显示的转换
            //int StructLen = Marshal.SizeOf(typeof(UART_RMS_DATA));
            int TempDateSize = insoure.Length;
            int index1 = -1;
            int index2 = -1;
            //匹配分析的起始位置点
            int searchindex = 0;
            bool findflag = false;
            while (searchindex + StructLen <= TempDateSize)
            {
                //有一个完整的结构体
                if (TempDateSize >= StructLen)
                {
                    //SIZE -1 后面有+1 不能越界
                    //第一个aaaa
                    findflag = false;
                    for (int i = searchindex; i < TempDateSize - 3; i++)
                    {
                        if (insoure[i] == 0xAA && insoure[i + 1] == 0xAA && insoure[i + 2] == 0x55 && insoure[i + 3] == 0x55)
                        {
                            index1 = i;
                            findflag = true;
                            break;
                        }
                    }
                    //没有找到头直接结束
                    if (findflag == false)
                    {
                        searchindex = 0;
                        break;
                    }
                    //够解析一个吗？
                    if (TempDateSize - index1 < StructLen)
                    {
                        searchindex = 0;
                        break;
                    }

                    //第二个aaaa
                    //SIZE -1 后面有+1 不能越界
                    for (int i = index1 + 4; i < TempDateSize - 3; i++)
                    {
                        if (insoure[i] == 0xAA && insoure[i + 1] == 0xAA && insoure[i + 2] == 0x55 && insoure[i + 3] == 0x55)
                        {
                            index2 = i;

                            break;
                        }
                    }
                    if (index2 == -1)
                    {
                        searchindex = index1;
                        //没有找到第二个
                        break;
                    }
                    if (index2 == index1)
                    {
                        searchindex = index1;
                        break;
                    }
                    if (index2 > index1)
                    {
                        searchindex = index2;
                    }
                    if (index2 - index1 == StructLen)
                    {
                        //UART_RMS_DATA uartdata = (UART_RMS_DATA)BytesStruct.BytesToStuct(insoure, index1, typeof(UART_RMS_DATA));
                        UART_RMS_DATA uartdata = (UART_RMS_DATA)BytesToStuct(insoure, index1, typeof(UART_RMS_DATA));

                        int Fxdata, Cxdata = 0;

                        //24BIT 加8位变成32位，符号位就自动升到高位了
                        byte[] tempdata = new byte[4];
                        //tempdata[0] = 0;
                        // tempdata[1] = 0;
                        Array.Clear(tempdata, 0, 4);
                        Array.Copy(uartdata.voltage, 0, tempdata, 2, 2);
                        Array.Reverse(tempdata);
                        //voltageshow = BitConverter.ToInt32(tempdata, 0) * 25.0f /1000/ 16.0f;
                        MainV2.comPort.MAV.cs.battery_voltage = BitConverter.ToInt32(tempdata, 0) * 25.0f / 1000 / 16.0f;
                        //tempdata[0] = 0;
                        //tempdata[1] = 0;
                        Array.Clear(tempdata, 0, 2);
                        Array.Copy(uartdata.current, 0, tempdata, 2, 2);
                        Array.Reverse(tempdata);
                        //currentshow = BitConverter.ToInt32(tempdata, 0)/16.0f * 25.0f /10000;
                        MainV2.comPort.MAV.cs.current = BitConverter.ToInt32(tempdata, 0) / 16.0f * 25.0f / 10000;
                        MainV2.comPort.MAV.cs.satcount = (Byte)(BitConverter.ToInt32(tempdata, 0) & 0x0f);

                        Array.Clear(tempdata, 0, 4);
                        Array.Copy(uartdata.latitude_integer, 0, tempdata, 2, 2);
                        Array.Reverse(tempdata);
                        //voltageshow = BitConverter.ToInt32(tempdata, 0) * 25.0f /1000/ 16.0f;
                        int latitude_integer = BitConverter.ToInt32(tempdata, 0);
                        Array.Clear(tempdata, 0, 4);
                        Array.Copy(uartdata.latitude_decimals, 0, tempdata, 1, 3);
                        Array.Reverse(tempdata);
                        //voltageshow = BitConverter.ToInt32(tempdata, 0) * 25.0f /1000/ 16.0f;
                        int latitude_decimals = BitConverter.ToInt32(tempdata, 0);
                        //string latstring = latitude_integer.ToString() + "." + latitude_decimals.ToString();

                        MainV2.comPort.MAV.cs.lat = latitude_integer / 100 + latitude_integer % 100 / 60.0 + latitude_decimals / 100000.0 / 60.0;

                        Array.Clear(tempdata, 0, 4);
                        Array.Copy(uartdata.longitude_integer, 0, tempdata, 2, 2);
                        Array.Reverse(tempdata);
                        //voltageshow = BitConverter.ToInt32(tempdata, 0) * 25.0f /1000/ 16.0f;
                        int longitude_integer = BitConverter.ToInt32(tempdata, 0);
                        Array.Clear(tempdata, 0, 4);
                        Array.Copy(uartdata.longitude_decimals, 0, tempdata, 1, 3);
                        Array.Reverse(tempdata);
                        //voltageshow = BitConverter.ToInt32(tempdata, 0) * 25.0f /1000/ 16.0f;
                        int longitude_decimals = BitConverter.ToInt32(tempdata, 0);

                        //string lonstring = longitude_integer.ToString() + "." + longitude_decimals.ToString();

                        MainV2.comPort.MAV.cs.lng = longitude_integer / 100 + longitude_integer % 100 / 60.0 + longitude_decimals / 100000.0 / 60.0;

                        Array.Clear(tempdata, 0, 4);
                        Array.Copy(uartdata.alt, 0, tempdata, 1, 3);
                        Array.Reverse(tempdata);
                        //currentshow = BitConverter.ToInt32(tempdata, 0)/16.0f * 25.0f /10000;
                        MainV2.comPort.MAV.cs.alt = BitConverter.ToInt32(tempdata, 0) / 10000.0f;
                        //字节反转  高4位中的最低位晶振同步状态

                        MainV2.comPort.MAV.cs.ocxo_states = (Byte)((uartdata.states[0] >> 4) & 0x1);
                        MainV2.comPort.MAV.cs.work_states = (Byte)((uartdata.states[0] >> 7));
                        MainV2.comPort.MAV.cs.ocxo_voltage = ((uartdata.states[0] & 0x0f) * 256) * 5 / 4096.0f;

                        MainV2.comPort.MAV.cs.gps_fix_type = (Byte)(uartdata.hour_gpsstate >> 4);
                        MainV2.comPort.MAV.cs.Hour = (Byte)(uartdata.hour_gpsstate & 0xf);
                        MainV2.comPort.MAV.cs.Minute = (Byte)(uartdata.minute);
                        MainV2.comPort.MAV.cs.Sec = (Byte)uartdata.sec;

                        Array.Clear(tempdata, 0, 4);
                        Array.Copy(uartdata.count, 0, tempdata, 1, 3);
                        Array.Reverse(tempdata);
                        //voltageshow = BitConverter.ToInt32(tempdata, 0) * 25.0f /1000/ 16.0f;
                        MainV2.comPort.MAV.cs.Count = BitConverter.ToInt32(tempdata, 0);

                        Array.Clear(tempdata, 0, 4);
                        Array.Copy(uartdata.CH3_X, 0, tempdata, 1, 3);
                        // Array.Reverse(tempdata);
                        MainV2.comPort.MAV.cs.mx = BitConverter.ToInt32(tempdata, 0) * adconvert;
                        Array.Copy(uartdata.CH4_Y, 0, tempdata, 1, 3);
                        // Array.Reverse(tempdata);
                        MainV2.comPort.MAV.cs.my = BitConverter.ToInt32(tempdata, 0) * adconvert;
                        Array.Copy(uartdata.CH5_Z, 0, tempdata, 1, 3);
                        //  Array.Reverse(tempdata);
                        MainV2.comPort.MAV.cs.mz = BitConverter.ToInt32(tempdata, 0) * adconvert;

                        //xyzdata = 20.0 * Math.Sqrt(ch3_x_data * ch3_x_data + ch4_y_data * ch4_y_data + ch5_z_data * ch5_z_data);

                        Cxdata = uartdata.CH1_X[1] + uartdata.CH1_X[0] * 256;
                        Fxdata = uartdata.CH1_Y[2] + uartdata.CH1_Y[1] * 256 + uartdata.CH1_Y[0] * 65536;

                        if (Fxdata == 0 || Cxdata == 0)
                        {
                            MainV2.comPort.MAV.cs.mag01 = 0;
                        }
                        else
                        {
                            //327680000*（Y+1）/(X+1)/3.498577 (nT)327680000 换为196608000
                            MainV2.comPort.MAV.cs.mag01 = 320000000f * (Cxdata + 1) / (Fxdata + 1) / 3.498577f;
                        }

                        Fxdata = uartdata.CH2_Y[2] + uartdata.CH2_Y[1] * 256 + uartdata.CH2_Y[0] * 65536;
                        Cxdata = uartdata.CH2_X[1] + uartdata.CH2_X[0] * 256;

                        if (Fxdata == 0 || Cxdata == 0)
                        {
                            MainV2.comPort.MAV.cs.mag02 = 0;
                        }
                        else
                        {
                            MainV2.comPort.MAV.cs.mag02 = 320000000f * (Cxdata + 1) / (Fxdata + 1) / 3.498577f;
                        }

                        //文件存储使用
                        /*
                         cqfileline1.Enqueue(ch1data);

                        */

                        //更新界面
                        // this.Invoke(UpdateTextHandler, "UartDataParser");

                        //===========================================================B
                        //Console.WriteLine(MainV2.comPort.MAV.cs.Count.ToString() + "  Sec:" + MainV2.comPort.MAV.cs.Sec.ToString());
                        //未初始化，不执行
                        if (fs == 0)
                            return;

                        if (GotStartTime)
                        {
                            Nany = (Int64)(N0 + (Int64)MainV2.comPort.MAV.cs.Count - 1);

                            // debug begin
                            //DateTime AnyDateTime = new DateTime();
                            //int us = 0;
                            //sampleID2DateTime(fs, Nany, ref AnyDateTime, ref us);
                            //UInt64 NanyR = 0;
                            //DateTime2sampleID(fs, AnyDateTime, us, ref NanyR);
                            //Console.Write(MainV2.comPort.MAV.cs.Count.ToString() + "  Sec:" + MainV2.comPort.MAV.cs.Sec.ToString() + "  Nany:" + Nany.ToString() + " " + AnyDateTime.ToString("yyyy-MM-dd,hh:mm:ss fff") + " us:" + us.ToString() + " N:" + NanyR.ToString());
                            //Console.WriteLine(MainV2.comPort.MAV.cs.Count.ToString() + "  Sec:" + MainV2.comPort.MAV.cs.Sec.ToString()+ "  Nany:" + Nany.ToString());
                            // debug end
                        }
                        else
                        {
                            if (iLastSec >= 0)
                            {
                                // 已经获得之前的秒时间
                                //
                                if (iLastSec != MainV2.comPort.MAV.cs.Sec && GotStartTime == false)         // 如果是变秒的样点。并且尚未获得采集开始时间StartDateTime。
                                {
                                    int n = MainV2.comPort.MAV.cs.Count;
                                    int K = (int)Math.Round((n - 1) / fs * 1.0);            // 秒脉冲时刻，采集经历的总秒数。

                                    //DateTime NewDateTime = new DateTime(YY, MM, DD, MainV2.comPort.MAV.cs.Hour-7, MainV2.comPort.MAV.cs.Minute-12, MainV2.comPort.MAV.cs.Sec-21, 0);
                                    DateTime NewDateTime = new DateTime(YY, MM, DD, MainV2.comPort.MAV.cs.Hour, MainV2.comPort.MAV.cs.Minute, MainV2.comPort.MAV.cs.Sec, 0);
                                    StartDateTime = NewDateTime.AddSeconds(-K);

                                    TimeSpan ts = StartDateTime - DTref;
                                    N0 = (Int64)(ts.TotalSeconds * fs + 1);

                                    GotStartTime = true;

                                    Nany = (Int64)(N0 + (Int64)MainV2.comPort.MAV.cs.Count - 1);

                                    //debug begin
                                    //DateTime AnyDateTime = new DateTime();
                                    //int us = 0;
                                    //sampleID2DateTime(fs, Nany, ref AnyDateTime, ref us);
                                    //UInt64 NanyR = 0;
                                    //DateTime2sampleID(fs, AnyDateTime, us, ref NanyR);
                                    //Console.Write(MainV2.comPort.MAV.cs.Count.ToString() + "  Sec:" + MainV2.comPort.MAV.cs.Sec.ToString() + "  Nany:" + Nany.ToString() + " " + AnyDateTime.ToString("yyyy-MM-dd,hh:mm:ss fff") + " us:" + us.ToString() + " N:" + NanyR.ToString());
                                    //Console.WriteLine(MainV2.comPort.MAV.cs.Count.ToString() + "  Sec:" + MainV2.comPort.MAV.cs.Sec.ToString()+ "  Nany:" + Nany.ToString());
                                    // debug end
                                }
                            }
                            else
                            {
                                iLastSec = MainV2.comPort.MAV.cs.Sec;        // 获取这段数据的新1个样点时间秒时间，用于之后确定秒时间的变化点。
                                Nany = 0;
                            }
                        }
                        //
                        int CheckFlg = -1;
                        if (GotStartTime)
                        {
                            ChcekCXData(MainV2.comPort.MAV.cs.mag01, MainV2.comPort.MAV.cs.mag02, Nany, ref CheckFlg);
                            Console.Write("  CheckFlg: " + CheckFlg.ToString() + "  RawMean1: " + CX1rawMean.ToString() + "  CheckMean1: " + CX1Mean.ToString() + "\n");
                        }
                        //===========================================================E
                        double X, Y, Zone;
                        X = 0.0; Y = 0.0; Zone = 0.0;
                        bool ok = WGS84toUTM.LatLonToUTM(MainV2.comPort.MAV.cs.lat, MainV2.comPort.MAV.cs.lng, ref X, ref Y, ref Zone);

                        magdata.Nany = Nany;
                        magdata.CheckFlg = CheckFlg;
                        magdata.Count = MainV2.comPort.MAV.cs.Count;
                        magdata.CX1_data = MainV2.comPort.MAV.cs.mag01;
                        magdata.CX2_data = MainV2.comPort.MAV.cs.mag02;
                        magdata.CX1rawMean = CX1rawMean;
                        magdata.CX1Mean = CX1Mean;
                        magdata.CX2rawMean = CX2rawMean;
                        magdata.CX2Mean = CX2Mean;
                        magdata.mx = MainV2.comPort.MAV.cs.mx;
                        magdata.my = MainV2.comPort.MAV.cs.my;
                        magdata.mz = MainV2.comPort.MAV.cs.mz;
                        magdata.lat = MainV2.comPort.MAV.cs.lat;
                        magdata.lng = MainV2.comPort.MAV.cs.lng;
                        magdata.alt = MainV2.comPort.MAV.cs.alt;
                        magdata.x = X;
                        magdata.y = Y;
                        magdata.zone = Zone;
                        magdata.Hour = MainV2.comPort.MAV.cs.Hour;
                        magdata.Minute = MainV2.comPort.MAV.cs.Minute;
                        magdata.Sec = MainV2.comPort.MAV.cs.Sec;
                        magdata.voltage = MainV2.comPort.MAV.cs.battery_voltage;
                        magdata.current = MainV2.comPort.MAV.cs.current;
                        magdata.work_states = MainV2.comPort.MAV.cs.work_states;
                        magdata.ocxo_states = MainV2.comPort.MAV.cs.ocxo_states;
                        magdata.ocxo_voltage = MainV2.comPort.MAV.cs.ocxo_voltage;
                        magdata.satcount = MainV2.comPort.MAV.cs.satcount;
                        magdata.gps_fix_type = MainV2.comPort.MAV.cs.gps_fix_type;
                    }
                    //长度不符合说明数据有丢失或错误
                    else
                    {
                        searchindex = index2;
                        errorcount++;
                    }
                }
                else
                {
                    errorcount++;
                    break;
                }
            }
            //拷贝剩余
            if ((TempDateSize - searchindex > 0))
            {
                DataConvertLeftLength = TempDateSize - searchindex;
                //大小不对 数据有错误
                if (DataConvertLeftLength > lefttemp.Length)
                {
                    DataConvertLeftLength = 0;
                    errorcount++;
                    //MessageBox.Show(DataConvertLeftLength.ToString());
                }
                else
                {
                    //残留部分拷贝
                    Array.Copy(insoure, searchindex, lefttemp, 0, DataConvertLeftLength);
                }
            }
        }
    }
}