using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using log4net;
using System.IO;

namespace aeromagtec.Comms
{
    class MYTcpClient : CommsBase
    {
        private static readonly ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        
        static byte[] recbuffer = new byte[1024];
        public static Stream Stream;

        public static void Main()
        {
            try
            {
                //①创建一个Socket
                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                IPAddress LocalAddr = IPAddress.Parse(MainV2.IPadress);
                int port = MainV2.TCPPort;
                //②连接到指定服务器的指定端口
                socket.Connect(LocalAddr, port); //localhost代表本机

                log.Info("client:connect to server success!");

                //③实现异步接受消息的方法 客户端不断监听消息
                socket.BeginReceive(recbuffer, 0, recbuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveMessage), socket);

                //④接受用户输入，将消息发送给服务器端
                while (true)
                {
                    //var message = Console.ReadLine();
                    //var outputBuffer = MainV2.SendMsg;
                    //socket.BeginSend(outputBuffer, 0, outputBuffer.Length , SocketFlags.None, null, null);
                    //MainV2.SendFlag = false;
                }
            }
            catch (Exception ex)
            {
                log.Info("client:error " + ex.Message);
            }
            finally
            {
                Console.Read();
            }

        }

        #region 接收信息
        /// <summary>
        /// 接收信息
        /// </summary>
        /// <param name="ar"></param>
        public static void ReceiveMessage(IAsyncResult ar)
        {
            try
            {
                var socket = ar.AsyncState as Socket;

                //方法参考：http://msdn.microsoft.com/zh-cn/library/system.net.sockets.socket.endreceive.aspx
                var length = socket.EndReceive(ar);
                //读取出来消息内容

                Stream = new MemoryStream(recbuffer);
                MainV2.Data_br = new BinaryReader(Stream);
                MainV2.Data_bw = new BinaryWriter(Stream);
                //显示消息
                //log.Info(message);

                //接收下一个消息(因为这是一个递归的调用，所以这样就可以一直接收消息了）
                socket.BeginReceive(recbuffer, 0, recbuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveMessage), socket);
            }
            catch (Exception ex)
            {
                log.Info(ex.Message);
            }
        }
        #endregion
 


    }
}
