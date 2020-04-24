using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace aeromagtec 
{   
    class refClass
    {

        public void FilePathString(string P_str_all)
        {
            // string P_str_all = openFileDialog1.FileName;//记录选择的文件全路径
            string P_str_path = //获取文件路径
                P_str_all.Substring(0, P_str_all.LastIndexOf("\\") + 1);
            string P_str_filename = //获取文件名
                P_str_all.Substring(P_str_all.LastIndexOf("\\") + 1,
                P_str_all.LastIndexOf(".") -
                (P_str_all.LastIndexOf("\\") + 1));
            string P_str_fileexc = //获取文件扩展名
                P_str_all.Substring(P_str_all.LastIndexOf(".") + 1,
                P_str_all.Length - P_str_all.LastIndexOf(".") - 1);
        }
        private void btn_GetTime_Click(object sender, EventArgs e)
        {
            string txt =
                DateTime.Now.ToString("d") + "\n" +//使用指定格式的字符串变量格式化日期字符串
                DateTime.Now.ToString("D") + "\n" +
                DateTime.Now.ToString("f") + "\n" +
                DateTime.Now.ToString("F") + "\n" +
                DateTime.Now.ToString("g") + "\n" +
                DateTime.Now.ToString("G") + "\n" +
                DateTime.Now.ToString("R") + "\n" +
                DateTime.Now.ToString("y") + "\n" +
                "当前系统时间为：" + DateTime.Now.ToString(//使用自定义格式格式化字符串
                "yyyy年MM月dd日 HH时mm分ss秒");
        }

      
        public static void WriteLog(string logText)
        {
#if true
            //string strLogFilePath = "C:\\NCLog.log";
           string strLogFilePath = System.IO.Directory.GetCurrentDirectory() + "\\Log.log";
            try
            {
                using (System.IO.StreamWriter logWriter = System.IO.File.AppendText(strLogFilePath))
                {
                    logWriter.Write(DateTime.Now.ToString() + ": ");
                    // logWriter.WriteLine(GetCurSourceFileName() + ": " + GetLineNum() + logText);
                    logWriter.WriteLine(logText);
                   Console.WriteLine(logText);
                }
            }
            catch
            {

            }
#endif

        }

        /// <summary>
        /// 取得当前源码的哪一行
        /// </summary>
        /// <returns></returns>
        public static int GetLineNum()
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(1, true);
            return st.GetFrame(0).GetFileLineNumber();
        }

        /// <summary>
        /// 取当前源码的源文件名
        /// </summary>
        /// <returns></returns>
        public static string GetCurSourceFileName()
        {
            System.Diagnostics.StackTrace st = new System.Diagnostics.StackTrace(1, true);

            return st.GetFrame(0).GetFileName();

        }
        /*
        static void Main(string[] args)
        {
            System.Console.WriteLine("Hello,World!");
            WriteLog("hello,world");
            try
            {
                int i = int.Parse("Error");//引发错误
            }
            catch (Exception e)
            {
                WriteLog(e.Message);
            }
        }*/
    }
}
