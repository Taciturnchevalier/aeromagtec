using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Threading;

namespace aeromagtec.mav
{
    public class mav
    {
        private CurrentState boxstate = new CurrentState();

        private int packetlen = 48;

        //10/2^24/256 MV
        private double adconvert = 0.0000023283064365386962890625f;

        //残留数据
        private int DataConvertLeftLength = 0;

        private int framelen = Marshal.SizeOf(typeof(UART_RMS_DATA));
        private int ConvertTempSize = 48; // packetlen
        private const int StructLen = 48;//Marshal.SizeOf(typeof(UART_RMS_DATA))20->25->50

        //保存残留数据的数组
        private byte[] converttemp = new byte[100];//40->100

        private int errorcount = 0;//错误计数
        private BinaryReader Data_br = null;
        private BinaryWriter Data_bw = null;

        /// <summary>
        /// AA55为数据头  aaaa 20170208换为4个AA
        /// </summary>
        /// <param name="insoure">解析数据源</param>
        /// <param name="lefttemp"> 剩余部分</param>
        public void uart_rms_data_para(ref byte[] insoure, ref byte[] lefttemp)
        {
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
                        UART_RMS_DATA uartdata = (UART_RMS_DATA)BytesStruct.BytesToStuct(insoure, index1, typeof(UART_RMS_DATA));

                        int Fxdata, Cxdata = 0;

                        //24BIT 加8位变成32位，符号位就自动升到高位了
                        byte[] tempdata = new byte[4];
                        //tempdata[0] = 0;
                        // tempdata[1] = 0;
                        Array.Clear(tempdata, 0, 4);
                        Array.Copy(uartdata.voltage, 0, tempdata, 2, 2);
                        Array.Reverse(tempdata);
                        //voltageshow = BitConverter.ToInt32(tempdata, 0) * 25.0f /1000/ 16.0f;
                        boxstate.battery_voltage = BitConverter.ToInt32(tempdata, 0) * 25.0f / 1000 / 16.0f;
                        //tempdata[0] = 0;
                        //tempdata[1] = 0;
                        Array.Clear(tempdata, 0, 2);
                        Array.Copy(uartdata.current, 0, tempdata, 2, 2);
                        Array.Reverse(tempdata);
                        //currentshow = BitConverter.ToInt32(tempdata, 0)/16.0f * 25.0f /10000;
                        boxstate.current = BitConverter.ToInt32(tempdata, 0) / 16.0f * 25.0f / 10000;
                        boxstate.satcount = (Byte)(BitConverter.ToInt32(tempdata, 0) & 0x0f);

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
                        string latstring = latitude_integer.ToString() + "." + latitude_decimals.ToString();
                        boxstate.lat = Convert.ToDouble(latstring);

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

                        string lonstring = longitude_integer.ToString() + "." + longitude_decimals.ToString();
                        boxstate.lng = Convert.ToDouble(lonstring);

                        Array.Clear(tempdata, 0, 4);
                        Array.Copy(uartdata.alt, 0, tempdata, 1, 3);
                        Array.Reverse(tempdata);
                        //currentshow = BitConverter.ToInt32(tempdata, 0)/16.0f * 25.0f /10000;
                        boxstate.alt = BitConverter.ToInt32(tempdata, 0) / 10000.0f;
                        //字节反转  高4位中的最低位晶振同步状态

                        boxstate.ocxo_states = (Byte)((uartdata.states[0] >> 4) & 0x1);
                        boxstate.work_states = (Byte)((uartdata.states[0] >> 7));
                        boxstate.ocxo_voltage = ((uartdata.states[0] & 0x0f) * 256) * 5 / 4096.0f;

                        boxstate.gpsstatus = (Byte)(uartdata.hour_gpsstate >> 4);
                        boxstate.Hour = (Byte)(uartdata.hour_gpsstate & 0xf);
                        boxstate.Minute = (Byte)(uartdata.minute);
                        boxstate.Sec = (Byte)uartdata.sec;

                        Array.Clear(tempdata, 0, 4);
                        Array.Copy(uartdata.count, 0, tempdata, 1, 3);
                        Array.Reverse(tempdata);
                        //voltageshow = BitConverter.ToInt32(tempdata, 0) * 25.0f /1000/ 16.0f;
                        boxstate.Count = BitConverter.ToInt32(tempdata, 0);

                        Array.Clear(tempdata, 0, 4);
                        Array.Copy(uartdata.CH3_X, 0, tempdata, 1, 3);
                        // Array.Reverse(tempdata);
                        boxstate.mx = BitConverter.ToInt32(tempdata, 0) * adconvert;
                        Array.Copy(uartdata.CH4_Y, 0, tempdata, 1, 3);
                        // Array.Reverse(tempdata);
                        boxstate.my = BitConverter.ToInt32(tempdata, 0) * adconvert;
                        Array.Copy(uartdata.CH5_Z, 0, tempdata, 1, 3);
                        //  Array.Reverse(tempdata);
                        boxstate.mz = BitConverter.ToInt32(tempdata, 0) * adconvert;

                        //xyzdata = 20.0 * Math.Sqrt(ch3_x_data * ch3_x_data + ch4_y_data * ch4_y_data + ch5_z_data * ch5_z_data);

                        Cxdata = uartdata.CH1_X[1] + uartdata.CH1_X[0] * 256;
                        Fxdata = uartdata.CH1_Y[2] + uartdata.CH1_Y[1] * 256 + uartdata.CH1_Y[0] * 65536;

                        if (Fxdata == 0 || Cxdata == 0)
                        {
                            boxstate.mag01 = 0;
                        }
                        else
                        {
                            //327680000*（Y+1）/(X+1)/3.498577 (nT)327680000 换为196608000
                            boxstate.mag01 = 320000000f * (Cxdata + 1) / (Fxdata + 1) / 3.498577f;
                        }

                        Fxdata = uartdata.CH2_Y[2] + uartdata.CH2_Y[1] * 256 + uartdata.CH2_Y[0] * 65536;
                        Cxdata = uartdata.CH2_X[1] + uartdata.CH2_X[0] * 256;

                        if (Fxdata == 0 || Cxdata == 0)
                        {
                            boxstate.mag02 = 0;
                        }
                        else
                        {
                            boxstate.mag02 = 320000000f * (Cxdata + 1) / (Fxdata + 1) / 3.498577f;
                        }
                        //327680000*（Y+1）/(X+1)/3.498577 (nT)327680000 换为196608000

                        //ch3_x_Showline.Enqueue(ch3_x_data);
                        //ch4_y_Showline.Enqueue(ch4_y_data);
                        //ch5_z_Showline.Enqueue(ch5_z_data);

                        //cqShowline24.Enqueue(ch1_optical_pump_data);
                        //cqShowline25.Enqueue(ch2_optical_pump_data);

                        //文件存储使用
                        /*
                         cqfileline1.Enqueue(ch1data);
                         cqfileline2.Enqueue(ch2data);
                         cqfileline3.Enqueue(ch3data);
                         cqfileline24.Enqueue(optical_pump_data1);
                         cqfileline25.Enqueue(optical_pump_data2);
                         cqfilelinetime.Enqueue(uart_gps_state);

                         cqfileline4.Enqueue(ch4data);
                         cqfileline5.Enqueue(ch5data);
                         cqfileline6.Enqueue(ch6data);

                         cqfileline7.Enqueue(ch7data);
                         cqfileline8.Enqueue(ch8data);
                         cqfileline9.Enqueue(ch9data);

                        */

                        //更新界面
                        // this.Invoke(UpdateTextHandler, "UartDataParser");
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

        /// <summary>
        /// 数据解析程序
        /// </summary>
        public void UartDataParser()
        {
            while (true)
            {
                try
                {
                    //队列中有数据
                    if (Uartdatacq.Count > 0)
                    {
                        byte[] temp;
                        while (Uartdatacq.TryDequeue(out temp))
                        {
                            //合并满足最小解析长度
                            if (temp.Length + DataConvertLeftLength >= ConvertTempSize)
                            {
                                //第一步数据合并将上次残留的数据合并到现在

                                //存在残留数据
                                if (DataConvertLeftLength > 0)
                                {
                                    //合并处理
                                    byte[] tempnew = new byte[DataConvertLeftLength + temp.Length];

                                    //残留部分拷贝
                                    Array.Copy(converttemp, tempnew, DataConvertLeftLength);

                                    //拷贝队列中的部分
                                    Array.Copy(temp, 0, tempnew, DataConvertLeftLength, temp.Length);
                                    DataConvertLeftLength = 0;
                                    //合并解析tempnew
                                    //解析生成有效数据，残留部分拷贝到converttemp
                                    uart_rms_data_para(ref tempnew, ref converttemp);

                                    //残留部分拷贝
                                }
                                else
                                {
                                    //直接处理
                                    uart_rms_data_para(ref temp, ref converttemp);
                                }
                            }
                            //长度不够
                            else
                            {
                                //残留部分拷贝
                                Array.Copy(temp, 0, converttemp, DataConvertLeftLength, temp.Length);
                                DataConvertLeftLength += temp.Length;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    //出现错误 重置剩余计数
                    DataConvertLeftLength = 0;
                    MessageBox.Show(ex.ToString());
                }
                Thread.Sleep(10);
            }
        }

        /// 降度分秒格式经纬度转换为小数经纬度
        /// </summary>
        /// <param name="_Value">度分秒经纬度</param>
        /// <returns>小数经纬度</returns>
        private static double GPSTransforming(string _Value)
        {
            double Ret = 0.0;
            string[] TempStr = _Value.Split('.');
            string x = TempStr[0].Substring(0, TempStr[0].Length - 2);
            string y = TempStr[0].Substring(TempStr[0].Length - 2, 2);
            string z = TempStr[1].Substring(0, 4);
            Ret = Convert.ToDouble(x) + Convert.ToDouble(y) / 60 + Convert.ToDouble(z) / 600000;
            return Ret;
        }

        /// <summary>
        /// 输入字符串，剩余字符串，解析出的结果
        /// -1：格式错误 0：没有有效数据 2：有效数据
        /// 修改为GNRMC
        /// </summary>
        /// <param name="insoure"></param>
        /// <param name="lefttemp"></param>
        /// <param name="uart_gps_state"></param>
        private int uart_Gps_data_para(ref string insoure, ref string lefttemp, ref state_gps uart_gps_state)
        {
            try
            {
                int iRet = 0;
                string handerStr = "$GN";
                //直接去掉不连续的部分，每包从GPRMC开始
                //string handerStr = "$GNRMC";
                //去掉其实的无用数据
                int findHander = insoure.IndexOf(handerStr);//看是否含有GPS串头
                if (findHander == -1)
                {
                    lefttemp = insoure;
                    return -1;
                }
                int findEndHander = insoure.LastIndexOf("\r\n");//结束 没有则返回-1
                if (findEndHander == -1)
                {
                    lefttemp = insoure;
                    return -1;
                }
                //剩余部分
                lefttemp = insoure.Substring(findEndHander, insoure.Length - findEndHander);
                //有效部分
                insoure = insoure.Substring(findHander, findEndHander - findHander);
                //按换行分出行数
                string[] striparr = insoure.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < striparr.Length; i++)
                {
                    string _RecString = striparr[i];
                    string[] seg = _RecString.Split(',');
                    string temp;
                    switch (seg[0])
                    {
                        case "$GNRMC":
                            if (seg.Length >= 12)
                            {
                                temp = seg[2];

                                if (temp != "")
                                {
                                    uart_gps_state.init_ok = (byte)(temp == "V" ? 0 : 1);//状态 A为有效

                                    //if (uart_gps_state.init_ok == 0)
                                    {
                                        //iRet = -1;
                                        //V为无效直接返回
                                        // break;
                                    }
                                }
                                if (seg[3] != "" && seg[4] != "")
                                {
                                    if (seg[4] == "N")
                                    {
                                        uart_gps_state.lat = GPSTransforming(seg[3]);
                                    }
                                    else
                                    {
                                        uart_gps_state.lat = -GPSTransforming(seg[3]);
                                    }
                                }
                                if (seg[6] != "" && seg[7] != "")
                                {
                                    if (seg[6] == "E")
                                    {
                                        uart_gps_state.lon = GPSTransforming(seg[5]);
                                    }
                                    else
                                    {
                                        uart_gps_state.lon = -GPSTransforming(seg[5]);
                                    }
                                }
                                temp = seg[7];

                                if (temp != "")
                                {
                                    uart_gps_state.speed = Convert.ToDouble(temp) * 1.852;//速度 节转KM/h
                                }
                                temp = seg[8];
                                if (temp != "")
                                {
                                    uart_gps_state.direction = Convert.ToDouble(temp);
                                }
                                temp = (seg[9] == "" ? "" : string.Format("20{0}-{1}-{2} {3}:{4}:{5}", seg[9].Substring(4), seg[9].Substring(2, 2), seg[9].Substring(0, 2), seg[1].Substring(0, 2), seg[1].Substring(2, 2), seg[1].Substring(4)));
                                if (temp != "")
                                {
                                    uart_gps_state.utc_time = DateTime.ParseExact(temp, "yyyy-MM-dd HH:mm:ss.ff", System.Globalization.CultureInfo.CurrentCulture);
                                }
                                //获得有效数据
                                iRet = 2;
                            }

                            break;

                        case "$GPVTG":
                            break;

                        case "$GNGGA":

                            temp = seg[7];
                            if (temp != "")
                            {
                                uart_gps_state.stars_inuse = Convert.ToInt32(temp);
                            }
                            else
                            {
                                uart_gps_state.stars_inuse = 0;
                            }
                            temp = seg[9];
                            if (temp != "")
                            {
                                uart_gps_state.altitude = Convert.ToDouble(temp);
                                iRet = 2;
                            }

                            break;

                        case "$GNGSA":

                            temp = seg[2];
                            if (temp != "")
                            {
                                uart_gps_state.fix_type = Convert.ToInt32(temp);//1:bad 2 :2d 3:3d
                                iRet = 2;
                            }
                            break;

                        case "$GPGSV":
                            break;

                        case "$GPGLL":
                            break;

                        case "$GPZDA":
                            break;

                        default:
                            iRet = -1;
                            // MessageBox.Show(seg[0]);
                            break;
                    }
                }
                return iRet;
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.ToString());
                return -1;
            }
        }

        private string convertGpstemp = null;

        //GPS状体结构体
        private state_gps uart_gps_state = new state_gps();

        /// <summary>
        /// GPS数据解析程序
        /// </summary>
        private void UartGPSDataParser()
        {
        }

        //存储的数据文件的文件名
        private string Datasavefilename = "";

        private System.IO.FileStream DataBinfswrite = null;

        // System.IO.FileStream UartTxtfswrite = null;
        private BinaryWriter UartBinWriter = null;

        private DateTime TimeStart;

        //打开数据文件，文件名按时间命名同时写入数据头
        private int OpenSaveUartDateFile()
        {
            try
            {
                string directoryName;
                directoryName = Application.StartupPath;
                string path = directoryName.ToString() + "\\ACQDATA";

                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                string saveSafeFileName = null;
                TimeStart = new DateTime(System.DateTime.Now.Ticks);
                saveSafeFileName = System.DateTime.Now.Year.ToString("0000") + System.DateTime.Now.Month.ToString("00") + System.DateTime.Now.Day.ToString("00") + System.DateTime.Now.Hour.ToString("00") + System.DateTime.Now.Minute.ToString("00") + System.DateTime.Now.Second.ToString("00") + "_" + SETsps + ".dat";

                string saveFileName = path + "\\" + saveSafeFileName;
                //Datasavefilename = saveFileName;
                DataBinfswrite = new FileStream(saveFileName, FileMode.Append);
                Datasavefilename = saveSafeFileName;
                UartBinWriter = new BinaryWriter(DataBinfswrite);
                //saveInfo = true;

                //写入数据头
                /*RMSInfo RmsFileHead = new RMSInfo();
                HeadInfoInit(RmsFileHead);
                RmsFileHead.year = (short)System.DateTime.Now.Year;
                RmsFileHead.month = (short)System.DateTime.Now.Month;
                RmsFileHead.day = (short)System.DateTime.Now.Day;
                RmsFileHead.hour = (short)System.DateTime.Now.Hour;
                RmsFileHead.minute = (short)System.DateTime.Now.Minute;
                RmsFileHead.sec = (byte)System.DateTime.Now.Second;
                byte[] temp1 = BytesStruct.StructToBytes(RmsFileHead, 2048);

                UartBinWriter.Write(temp1, 0, 2048);*/

                return 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
                return -1;
            }
        }

        /// <summary>
        /// 初始化构造一个头段 目前大部分为空
        /// </summary>
        /// <returns></returns>
        private bool HeadInfoInit(RMSInfo RmsFileHead)
        {
            RmsFileHead.day = 0;
            RmsFileHead.month = 0;
            RmsFileHead.year = 0;
            RmsFileHead.hour = 0;
            RmsFileHead.minute = 0;
            RmsFileHead.geo = "";
            RmsFileHead.met = 0;
            RmsFileHead.standby1 = "";
            RmsFileHead.pro = 0;
            RmsFileHead.kan = 3;

            RmsFileHead.Com = "";

            RmsFileHead.channel = new ChInfo[24];
            for (int i = 0; i < RmsFileHead.kan; i++)
            {
                RmsFileHead.channel[i].Idk = 2;

                RmsFileHead.channel[i].Ufl = 42;
                RmsFileHead.channel[i].Pkt = 299;
                RmsFileHead.channel[i].Prf = 1;
                RmsFileHead.channel[i].X = 115.6571f;
                RmsFileHead.channel[i].Y = 38.70437f;
                RmsFileHead.channel[i].Z = 4.056086f;
                RmsFileHead.channel[i].Ecs = 0.26754f;
            }

            return true;
        }

        //发送控制命令
        /// <summary>
        /// 目前控制命令只有一个字节
        /// </summary>
        /// <param name="cmd"></param>
        //private void DataPortSendCmd(byte cmd)
        //{
        //    if (Data_bw != null)
        //    {
        //        totalSendBytes += 1;
        //        try
        //        {
        //            byte[] temp = new byte[1];
        //            temp[0] = cmd;
        //            //DataSerialPort.Write(ch, 0, ch.Length);
        //            //AsyncSendMessage(BitConverter.GetBytes(cmd));
        //            AsyncSendMessage(temp);
        //            //sendStatusLabel.Text = totalSendBytes.ToString() + "字节";
        //        }
        //        catch (Exception ee)
        //        {
        //            MessageBox.Show(ee.ToString());
        //        }

        //    }

        //}

        // Construct a ConcurrentQueue.建立一个队列当缓冲区
        private ConcurrentQueue<byte[]> Uartdatacq = new ConcurrentQueue<byte[]>();

        //GPS的数据解析队列
        private ConcurrentQueue<byte[]> UartGPScq = new ConcurrentQueue<byte[]>();

        private string SETsps;
        private int totalSendBytes;
    }
}