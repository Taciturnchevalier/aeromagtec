

//10/2^24/256 MV
        public static double adconvert = 0.0000023283064365386962890625f;
        
        //保存残留数据的数组
        public static byte[] converttemp = new byte[100];

        public static int ConvertTempSize = packetlen;

        public static BinaryReader Data_br = null;

        //private TcpClient Gpsclient = null;
        public static BinaryWriter Data_bw = null;

        //残留数据
        public static int DataConvertLeftLength = 0;
        
         // Construct a ConcurrentQueue.建立一个队列当缓冲区
        public static ConcurrentQueue<byte[]> Uartdatacq = new ConcurrentQueue<byte[]>();
        
          //----------------------------------------------------------------------------
        /// <summary>
        /// 张启卯代码部分
        /// </summary>
        //配置参数
        private const string configfilename = "AMSconfig.xml";

        private const int packetlen = 48;

        private const int StructLen = packetlen;
        
          //GPS的数据解析队列
        //private ConcurrentQueue<byte[]> UartGPScq = new ConcurrentQueue<byte[]>();
        //用于存储文件
        // Construct a ConcurrentQueue.建立一个安全队列当缓冲区 时间
        private ConcurrentQueue<state_gps> cqfilelinetime = new ConcurrentQueue<state_gps>();

        private System.IO.FileStream DataBinfswrite = null;

        //网络端口
        private TcpClient Dataclient = null;

        //连接任务
        private BackgroundWorker DataconnectWork = new BackgroundWorker();

        //存储的数据文件的文件名
        private string Datasavefilename = "";

        private int framelen = Marshal.SizeOf(typeof(UART_RMS_DATA));

        private int GPSPort = 5049;

        private System.IO.BinaryWriter GPSsaveFileFs = null;
         //解析数据
        //Thread myThreadDataParser;
        //解析GPS
        //Thread myThreadGPSDataParser;
        //保存文本文件
        private Thread myThreadTXTsave;

        private Thread myThreadUDPReceive;
        
         private delegateFunction ReadDataFileDF;

        //private bool UDPserverStart = false;
        private UdpClient receiveUdpClient = null;
          //GPS 走UDP
        //解析的数据
        //state_box boxstate = new state_box();
        private string SETsps = "80";
        
          // System.IO.FileStream UartTxtfswrite = null;
        private BinaryWriter UartBinWriter = null;

        //public static byte[] SendMsg;
        /// <summary>
        /// Active Comport interface
        /// </summary>
        //Controls.MainSwitcher MyView;
        
        
         public delegate int delegateFunction(BinaryReader readLineSReader, ConcurrentQueue<byte[]> Uartdatacq);

        private delegate void ReceiveMessageDelegate(out byte[] receiveMessage);

        private delegate void SendMessageDelegate(byte[] message);
        
          /// <summary>
        /// AA55为数据头  aaaa 20170208换为4个AA
        /// </summary>
        /// <param name="insoure">解析数据源</param>
        /// <param name="lefttemp"> 剩余部分</param>
        public static void uart_rms_data_para(ref byte[] insoure, ref byte[] lefttemp)
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
                        comPort.MAV.cs.battery_voltage = BitConverter.ToInt32(tempdata, 0) * 25.0f / 1000 / 16.0f;
                        //tempdata[0] = 0;
                        //tempdata[1] = 0;
                        Array.Clear(tempdata, 0, 2);
                        Array.Copy(uartdata.current, 0, tempdata, 2, 2);
                        Array.Reverse(tempdata);
                        //currentshow = BitConverter.ToInt32(tempdata, 0)/16.0f * 25.0f /10000;
                        comPort.MAV.cs.current = BitConverter.ToInt32(tempdata, 0) / 16.0f * 25.0f / 10000;
                        comPort.MAV.cs.satcount = (Byte)(BitConverter.ToInt32(tempdata, 0) & 0x0f);

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

                        comPort.MAV.cs.lat = latitude_integer / 100 + latitude_integer % 100 / 60.0 + latitude_decimals / 100000.0 / 60.0;

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

                        comPort.MAV.cs.lng = longitude_integer / 100 + longitude_integer % 100 / 60.0 + longitude_decimals / 100000.0 / 60.0;

                        Array.Clear(tempdata, 0, 4);
                        Array.Copy(uartdata.alt, 0, tempdata, 1, 3);
                        Array.Reverse(tempdata);
                        //currentshow = BitConverter.ToInt32(tempdata, 0)/16.0f * 25.0f /10000;
                        comPort.MAV.cs.alt = BitConverter.ToInt32(tempdata, 0) / 10000.0f;
                        //字节反转  高4位中的最低位晶振同步状态

                        comPort.MAV.cs.ocxo_states = (Byte)((uartdata.states[0] >> 4) & 0x1);
                        comPort.MAV.cs.work_states = (Byte)((uartdata.states[0] >> 7));
                        comPort.MAV.cs.ocxo_voltage = ((uartdata.states[0] & 0x0f) * 256) * 5 / 4096.0f;

                        comPort.MAV.cs.gps_fix_type = (Byte)(uartdata.hour_gpsstate >> 4);
                        comPort.MAV.cs.Hour = (Byte)(uartdata.hour_gpsstate & 0xf);
                        comPort.MAV.cs.Minute = (Byte)(uartdata.minute);
                        comPort.MAV.cs.Sec = (Byte)uartdata.sec;

                        Array.Clear(tempdata, 0, 4);
                        Array.Copy(uartdata.count, 0, tempdata, 1, 3);
                        Array.Reverse(tempdata);
                        //voltageshow = BitConverter.ToInt32(tempdata, 0) * 25.0f /1000/ 16.0f;
                        comPort.MAV.cs.Count = BitConverter.ToInt32(tempdata, 0);

                        Array.Clear(tempdata, 0, 4);
                        Array.Copy(uartdata.CH3_X, 0, tempdata, 1, 3);
                        // Array.Reverse(tempdata);
                        comPort.MAV.cs.mx = BitConverter.ToInt32(tempdata, 0) * adconvert;
                        Array.Copy(uartdata.CH4_Y, 0, tempdata, 1, 3);
                        // Array.Reverse(tempdata);
                        comPort.MAV.cs.my = BitConverter.ToInt32(tempdata, 0) * adconvert;
                        Array.Copy(uartdata.CH5_Z, 0, tempdata, 1, 3);
                        //  Array.Reverse(tempdata);
                        comPort.MAV.cs.mz = BitConverter.ToInt32(tempdata, 0) * adconvert;

                        //xyzdata = 20.0 * Math.Sqrt(ch3_x_data * ch3_x_data + ch4_y_data * ch4_y_data + ch5_z_data * ch5_z_data);

                        Cxdata = uartdata.CH1_X[1] + uartdata.CH1_X[0] * 256;
                        Fxdata = uartdata.CH1_Y[2] + uartdata.CH1_Y[1] * 256 + uartdata.CH1_Y[0] * 65536;

                        if (Fxdata == 0 || Cxdata == 0)
                        {
                            comPort.MAV.cs.pump_data = 0;
                        }
                        else
                        {
                            //327680000*（Y+1）/(X+1)/3.498577 (nT)327680000 换为196608000
                            comPort.MAV.cs.pump_data = 320000000f * (Cxdata + 1) / (Fxdata + 1) / 3.498577f;
                        }

                        Fxdata = uartdata.CH2_Y[2] + uartdata.CH2_Y[1] * 256 + uartdata.CH2_Y[0] * 65536;
                        Cxdata = uartdata.CH2_X[1] + uartdata.CH2_X[0] * 256;

                        if (Fxdata == 0 || Cxdata == 0)
                        {
                            comPort.MAV.cs.pump_data02 = 0;
                        }
                        else
                        {
                            comPort.MAV.cs.pump_data02 = 320000000f * (Cxdata + 1) / (Fxdata + 1) / 3.498577f;
                        }
                        //327680000*（Y+1）/(X+1)/3.498577 (nT)327680000 换为196608000

                        //ch3_x_Showline.Enqueue(ch3_x_data);
                        //ch4_y_Showline.Enqueue(ch4_y_data);
                        //ch5_z_Showline.Enqueue(ch5_z_data);

                        //cqShowline24.Enqueue(ch1_optical_pump_data);
                        //cqShowline25.Enqueue(ch2_optical_pump_data);
                        //DataSet pumpysydata = new DataSet("pumpysydata");
                        //pumpysydata.Tables.Add(Rawtable());
                        //DataTable Rawdata = Rawtable();

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

        //错误计数
        /// <summary>
        /// 数据解析程序
        /// </summary>
        public static void UartDataParser()
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
        
         public void SendUDPData(IPAddress remoteIp, byte[] bytes)
        {
            //byte[] bytes = StructToBytes(obj);
            IPEndPoint iep = new IPEndPoint(remoteIp, GPSPort);//localAddress
            try
            {
                if (receiveUdpClient.Client != null)
                {
                    receiveUdpClient.Send(bytes, bytes.Length, iep);
                }
            }
            catch (Exception ex)//没有连接网络时候的异常需要提示
            {
                refClass.WriteLog(ex.ToString());
            }
        }

        public void StartReceiveUDP_GPS()
        {
            myThreadUDPReceive = new Thread(ReceiveUDPData);
            //将线程设为后台运"
            // myThreadUDPReceive.IsBackground = true;
            myThreadUDPReceive.Start();
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

        /// <summary>异步向服务器端发送数据</summary>
        private void AsyncSendMessage(byte[] message)
        {
            SendMessageDelegate d = new SendMessageDelegate(SendMessage);
            IAsyncResult result = d.BeginInvoke(message, null, null);
            while (result.IsCompleted == false)
            {
                if (isExit)
                {
                    return;
                }
                Thread.Sleep(50);
            }
            SendMessageStates states = new SendMessageStates();
            states.d = d;
            states.result = result;
            Thread t = new Thread(FinishAsyncSendMessage);
            t.IsBackground = true;
            t.Start(states);
        }
        
         //Marshal.SizeOf(typeof(UART_RMS_DATA))20->25->50
        private void ClearDoubleDataQueue(ref ConcurrentQueue<double> cq)
        {
            if (cq.Count > 0)
            {
                //MessageBox.Show(cq.ToString() + ":" + cq.Count.ToString());
                double temp;
                while (cq.TryDequeue(out temp))
                {
                }
            }
        }

        //如果队列有数据就清空
        private void ClearQueue()
        {
            if (Uartdatacq.Count > 0)
            {
                byte[] temp;
                while (Uartdatacq.TryDequeue(out temp))
                {
                }
            }

            //ClearDoubleDataQueue(ref cqShowline0);
            //ClearDoubleDataQueue(ref ch3_x_Showline);
            //ClearDoubleDataQueue(ref ch4_y_Showline);
            //ClearDoubleDataQueue(ref ch5_z_Showline);

            if (cqfilelinetime.Count > 0)
            {
                state_gps temp;
                while (cqfilelinetime.TryDequeue(out temp))
                {
                }
            }
        }

        /// <summary>
        /// 关闭GPS和数据文件
        /// </summary>
        private void CloseDateFile()
        {
            if (DataBinfswrite != null)
            {
                //saveInfo = false;
                UartBinWriter.Close();
                DataBinfswrite.Close();
                DataBinfswrite = null;
            }
        }

        /// <summary>
        /// 关闭两个端口
        /// </summary>
        private void CloseTwoUart()
        {
            if (Dataclient != null)
            {
                IsConnected = false;
                //Data_br.Close();
                //Data_bw.Close();
                Dataclient.Close();
            }
            //if (Gpsclient != null)
            //{
            //    isExit = true;
            //    Gps_br.Close();
            //    Gps_bw.Close();
            //    Gpsclient.Close();
            //}
        }

        private void ConPort()
        {
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

            myinputbox inp = new myinputbox("输入IP地址");
            DialogResult dr = inp.ShowDialog();
            if (dr == DialogResult.OK && inp.Value.Length > 0)
            {
                string[] iparry = inp.Value.Split(':');
                IPadress = iparry[0];
                TCPPort = Convert.ToInt32(iparry[1]);
            }
            else
            {
                MessageBox.Show("请先配置IP地址");
                return;
            }

            DataconnectWork.RunWorkerAsync();
            connecttime = DateTime.Now;
        }

        /// <summary>异步方式与服务器进行连接</summary>
        private void DataconnectWork_DoWork(object sender, DoWorkEventArgs e)
        {
            Dataclient = new TcpClient();

            IAsyncResult result = Dataclient.BeginConnect(IPadress, TCPPort, null, null);
            while (result.IsCompleted == false)
            {
                Thread.Sleep(100);
                MsgText = "连接中.....";
                //statusDisplayToolStripStatusLabel.Text += ".";
            }
            try
            {
                Dataclient.EndConnect(result);
                e.Result = "success";
            }
            catch (Exception ex)
            {
                e.Result = ex.Message;
                return;
            }
        }

        /// <summary>异步方式与服务器完成连接操作后的处理</summary>
        private void DataconnectWork_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result.ToString() == "success")
            {
                if (Dataclient != null)
                {
                    MsgText = "数据端口连接成功！";
                }

                // AddStatus("连接成功");
                //获取网络流
                NetworkStream networkStream = Dataclient.GetStream();
                //将网络流作为二进制读写对象
                Data_br = new BinaryReader(networkStream);
                Data_bw = new BinaryWriter(networkStream);
                //Thread threadReceive = new Thread(new ThreadStart(ReceiveDataByte));
                //threadReceive.IsBackground = true;
                //threadReceive.Start();
                //Thread.Sleep(100);
                //AsyncSendMessage(textBoxSend.Text + "\r\n");
                IsConnected = true;
            }
            else
            {
                Data_br = null;
                Data_bw = null;

                IsConnected = false;
                MsgText = "连接失败！";
            }
        }

        //发送控制命令
        /// <summary>
        /// 目前控制命令只有一个字节
        /// </summary>
        /// <param name="cmd"></param>
        private void DataPortSendCmd(byte cmd)
        {
            if (Data_bw != null)
            {
                totalSendBytes += 1;
                try
                {
                    byte[] temp = new byte[1];
                    temp[0] = cmd;
                    //DataSerialPort.Write(ch, 0, ch.Length);
                    //AsyncSendMessage(BitConverter.GetBytes(cmd));
                    AsyncSendMessage(temp);
                    //sendStatusLabel.Text = totalSendBytes.ToString() + "字节";
                }
                catch (Exception ee)
                {
                    MessageBox.Show(ee.ToString());
                }
            }
        }

        /// <summary>处理接收的服务器端数据</summary>
        private void FinishAsyncSendMessage(object obj)
        {
            SendMessageStates states = (SendMessageStates)obj;
            states.d.EndInvoke(states.result);
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

        private void LoadConfig(string fileName)
        {
            SharedPreferences sp = new SharedPreferences(fileName);
            if (sp.ConfigFileExists)
            {
                try
                {
                    IPadress = sp.GetString("IPAdress", "192.168.0.7");
                    TCPPort = sp.GetInt32("DataPort", 5050);
                    GPSPort = sp.GetInt32("GPSPort", 5049);
                    //读取设置的采样率
                    SETsps = sp.GetString("ACQSPS", "800");
                    //读取设置的样点数
                    //textBox_mpoint.Text = sp.GetString("MPOINT", "800");
                    //comboBox_subsampling_rate.SelectedItem = sp.GetString("SUBRATE", "1");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
            }
            else
            {//载入默认设置
                ResetToDefaultSettings();
            }
        }
        
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

        private int ReadBinFileData(BinaryReader readLineSReader, ConcurrentQueue<byte[]> Uartdatacq)
        {
            do
            {
                //随机大小读取
                Random randonmum = new Random();
                int readcount = randonmum.Next(20, 100);
                byte[] temparray = new byte[readcount];
                if (readLineSReader.Read(temparray, 0, temparray.Length) == temparray.Length)
                {
                    Uartdatacq.Enqueue(temparray);
                }
                else
                {
                    break;
                }
            } while (true);

            return 0;
        }

        /// <summary>处理接收的服务器端数据</summary>
        private void ReceiveDataByte()
        {
            byte[] receiveString = null;
            while (isExit == false)
            {
                ReceiveMessageDelegate d = new ReceiveMessageDelegate(ReceiveMessage);
                IAsyncResult result = d.BeginInvoke(out receiveString, null, null);
                //使用轮询方式来判断异步操作是否完成
                while (result.IsCompleted == false)
                {
                    if (isExit)
                    {
                        break;
                    }
                    Thread.Sleep(250);
                }
                //获取Begin方法的返回值和所有输入/输出参数
                d.EndInvoke(out receiveString, result);

                if (receiveString == null)
                {
                    continue;
                }
                // Console.WriteLine(receiveString.Length);
            }
            Application.Exit();
        }

        private void ReceiveMessage(out byte[] receiveMessage)
        {
            receiveMessage = null;
            try
            {
                int size = Dataclient.Available;
                if (size >= StructLen)
                {
                    receiveMessage = Data_br.ReadBytes(size);
                    if (receiveMessage.Length == size)
                    {
                        //已经停止就不要数据了
                        // if (acqisend == false)
                        {
                            //其他线程再去解析数据
                            Uartdatacq.Enqueue(receiveMessage);
                        }
                        // 保存文件
                        //文件没有创建则自动创建
                        if (DataBinfswrite != null)
                        {
                            UartBinWriter.Write(receiveMessage);
                        }
                    }
                    else
                    {//接收错误
                        errorcount++;
                    }
                }
                totalDataReceivedBytes += size;
            }
            catch (Exception ex)
            {
                //statusStrip1.Text(ex.Message);
                MsgText = ex.Message;
            }
        }

        private void ReceiveUDPData()
        {
            try
            {
                receiveUdpClient = new UdpClient(5050);

                IPEndPoint remote = new IPEndPoint(IPAddress.Parse(IPadress), GPSPort);
                while (receiveUdpClient != null && isExit != false)
                {
                    //关闭udpClient时此句会产生异常
                    if (receiveUdpClient.Available <= 0)
                    {
                        Thread.Sleep(10);
                        continue;
                    }

                    byte[] receiveBytes = receiveUdpClient.Receive(ref remote);//阻塞的方式接收数
                    if (receiveBytes.Length > 0)
                    {
                        //UartGPScq.Enqueue(receiveBytes);
                        if (GPSsaveFileFs != null)
                        {
                            GPSsaveFileFs.Write(receiveBytes);
                        }
                    }
                    else
                    {
                        Thread.Sleep(10);
                        continue;
                    }
                    //IPAddress remoteip = remote.Address;
                    //两个格式不同数据包大小 状态包数据更大
                }
                //UDP线程中断
                receiveUdpClient.Close();// receiveUdpClient.Receive以阻塞方式接收数据因此要先关闭连"
                receiveUdpClient = null;
                //UDPserverStart = false;
            }
            catch (Exception ex)
            {
                refClass.WriteLog(ex.ToString());
            }
        }

        /// <summary>
        /// 默认端口参数设置
        /// </summary>
        private void ResetToDefaultSettings()
        {
            IPadress = "192.168.0.7";
            TCPPort = 5050;
            GPSPort = 5049;
            //读取设置的采样率
            //comboBox_SETsps.SelectedItem = "800";
            //读取设置的样点数
            //textBox_mpoint.Text = "800";
            //comboBox_subsampling_rate.SelectedItem = "1";
        }

        private void SaveConfig(string fileName)
        {
            //以xml方式来保存
            Editor editor = new Editor();
            try
            {
                editor.PutString("IPAdress", "192.168.0.7");
                editor.PutString("DataPort", "5050");
                editor.PutString("GPSPort", "5049");
                ////保存设置的采用率
                //editor.PutString("ACQSPS", SETsps);
                ////保存设置的绘图点数设置
                //editor.PutInt32("MPOINT", Convert.ToInt32(this.textBox_mpoint.Text));
                ////保存设置的绘图抽样倍率
                //editor.PutInt32("SUBRATE", Convert.ToInt32(this.comboBox_subsampling_rate.SelectedItem));
                //editor.PutInt32("DataPortBaudRate", Convert.ToInt32(DataSerialPort.BaudRate));
                SharedPreferences sp = new SharedPreferences(fileName);
                //记得调用该方法将上述内容一次写入并保存。
                sp.Save(editor);
                // MessageBox.Show("保存成功！");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }

        /// <summary>向服务器端发送数据</summary>
        private void SendMessage(byte[] message)
        {
            try
            {
                Data_bw.Write(message);
                Data_bw.Flush();
            }
            catch
            {
                // AddStatus("发送失败!");
            }
        }
        
         /// <summary>
        /// 发送配置到串口
        /// </summary>
        private void SetShow_sps()
        {
            try
            {
                timer_send.Interval = 1000 / Convert.ToInt32(SETsps);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        
         //OpenHardwareMonitor.Hardware.Computer computer = new OpenHardwareMonitor.Hardware.Computer();
        /// <summary>
        /// 设置提示信息
        /// </summary>
        private void settips()
        {
            //ToolTip.SetToolTip(groupBox4, "GPS的相关状态");
            //ToolTip.SetToolTip(button_starttestacq, "开始采集前请确认采样率设置");
        }
        
          //GPS状体结构体
        //state_gps uart_gps_state = new state_gps();
        /// <summary>
        /// GPS数据解析程序
        /// </summary>
        private void UartGPSDataParser()
        {
            #if false
            state_gps uart_gps_temp = new state_gps();
            while (true)
            {
                try
                {
                    //队列中有数据
                    if (UartGPScq.Count > 0)
                    {
                        string temp;

                        while (UartGPScq.TryDequeue(out temp))
                        {
                            //第一步数据合并将上次残留的数据
                            if (convertGpstemp != null)
                            {
                                //合并
                                string tempnew = convertGpstemp + temp;
                                convertGpstemp = null;
                                //只去有效数据
                                /* state_gps gps_statetmep = new state_gps();
                                  if (uart_Gps_data_para(ref tempnew, ref convertGpstemp, ref gps_statetmep) == 2)
                                  {
                                      uart_gps_state = gps_statetmep;
                                  }*/

                                uart_Gps_data_para(ref tempnew, ref convertGpstemp, ref uart_gps_temp);
                                if (uart_gps_temp.utc_time.Year > 2010)
                                {
                                    uart_gps_state = uart_gps_temp;
                                }
                                //cqfileGPS.Enqueue(uart_gps_state);
                                //解析
                                //剩余部分赋值到convertGpstemp
                            }
                            else
                            {
                                //直接解析
                                //剩余部分赋值到convertGpstemp
                                /* state_gps gps_statetmep = new state_gps();
                                 if (uart_Gps_data_para(ref temp, ref convertGpstemp, ref gps_statetmep) == 2)
                                 {
                                     uart_gps_state = gps_statetmep;
                                 }*/

                                uart_Gps_data_para(ref temp, ref convertGpstemp, ref uart_gps_temp);
                                if (uart_gps_temp.utc_time.Year > 2010)
                                {
                                    uart_gps_state = uart_gps_temp;
                                }
                            }
                            //uartgpsinfo = GPSAnalysisClass.GPRMCAnalysis(temp);
                        }
                    }
                }
                catch (Exception ex)
                {
                    //出现错误 重置剩余计数
                    convertGpstemp = null;
                    MessageBox.Show(ex.ToString());
                }
                Thread.Sleep(10);
            }
            #endif
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

       //    this.connectionStatsForm.Show();
        /// <summary>发送信息状态的数据结构</summary>
        private struct SendMessageStates
        {
            public SendMessageDelegate d;
            public IAsyncResult result;
        }


        