using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net;
using System.Text;
using log4net;
using log4net.Config;
using GMap.NET.MapProviders;
using System.Threading;
using System.Drawing;
using System.Management;
using aeromagtec.Utilities;
using System.IO;
using aeromagtec.Comms;
using aeromagtec.Controls;
using SQLite;

namespace aeromagtec
{
    public static class Program
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Program));

        public static DateTime starttime = DateTime.Now;

        public static string name { get; internal set; }

        public static bool WindowsStoreApp { get { return Application.ExecutablePath.Contains("WindowsApps"); } }

        internal static Thread Thread;

        public static Image Logo = null;

        public static Image IconFile = null;

        public static string[] args = new string[] { };

        public static Bitmap SplashBG = null;

        public static string[] names = new string[] { "VVVVZ" };
        public static bool MONO = false;

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        private static void Main(string[] args)
        {
            Program.args = args;
            Console.WriteLine(
                "If your error is about Microsoft.DirectX.DirectInput, please install the latest directx redist from here http://www.microsoft.com/en-us/download/details.aspx?id=35 \n\n");
            Thread = Thread.CurrentThread;
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            XmlConfigurator.Configure();
            log.Info("******************* Logging Configured *******************");
            Application.SetCompatibleTextRenderingDefault(false);

            ServicePointManager.DefaultConnectionLimit = 10;

            Application.ThreadException += Application_ThreadException;

            AppDomain.CurrentDomain.UnhandledException +=
                new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            if (args.Length > 0 && args[0] == "/update")
            {
                //Utilities.Update.DoUpdate();
                return;
            }

            name = "aeromagtec";
            try
            {
                if (File.Exists(Settings.GetRunningDirectory() + "logo.txt"))
                    name = File.ReadAllLines(Settings.GetRunningDirectory() + "logo.txt",
                        Encoding.UTF8)[0];
            }
            catch
            {
            }

            if (File.Exists(Settings.GetRunningDirectory() + "logo.png"))
                Logo = new Bitmap(Settings.GetRunningDirectory() + "logo.png");

            //if (File.Exists(Settings.GetRunningDirectory() + "icon.png"))
            //{
            //    // 128*128
            //    IconFile = new Bitmap(Settings.GetRunningDirectory() + "icon.png");
            //}
            //else
            //{
            //    IconFile = aeromagtec.Properties.Resources.mpdesktop.ToBitmap();
            //}

            if (File.Exists(Settings.GetRunningDirectory() + "splashbg.jpg")) // 600*375
                SplashBG = new Bitmap(Settings.GetRunningDirectory() + "splashbg.jpg");
            // setup settings provider
            aeromagtec.Comms.CommsBase.Settings += CommsBase_Settings;
            aeromagtec.Comms.CommsBase.InputBoxShow += CommsBaseOnInputBoxShow;

            var databasePath = Path.Combine(Settings.Instance.DataDir, "MyData.sqlite");
            var db = new SQLiteConnection(databasePath);
            db.CreateTable<MyDataTable>();
            db.CreateTable<ReferSite>();

            // set the cache provider to my custom version
            GMap.NET.GMaps.Instance.PrimaryCache = new Maps.MyImageCache();
            // add my custom map providers
            GMapProviders.List.Add(Maps.WMSProvider.Instance);
            GMapProviders.List.Add(Maps.Custom.Instance);
            GMapProviders.List.Add(Maps.Earthbuilder.Instance);
            GMapProviders.List.Add(Maps.Statkart_Topo2.Instance);
            GMapProviders.List.Add(Maps.Eniro_Topo.Instance);
            GMapProviders.List.Add(Maps.MapBox.Instance);
            GMapProviders.List.Add(Maps.MapboxNoFly.Instance);

            // add proxy settings
            GMapProvider.WebProxy = WebRequest.GetSystemWebProxy();
            GMapProvider.WebProxy.Credentials = CredentialCache.DefaultCredentials;

            WebRequest.DefaultWebProxy = WebRequest.GetSystemWebProxy();
            WebRequest.DefaultWebProxy.Credentials = CredentialCache.DefaultNetworkCredentials;

            try
            {
                Thread.CurrentThread.Name = "Base Thread";
                Application.Run(new MainV2());
            }
            catch (Exception ex)
            {
                log.Fatal("Fatal app exception", ex);
                Console.WriteLine(ex.ToString());

                Console.WriteLine("\nPress any key to exit!");
                Console.ReadLine();
            }
        }

        private static inputboxreturn CommsBaseOnInputBoxShow(string title, string prompttext, ref string text)
        {
            var ans = InputBox.Show(title, prompttext, ref text);

            if (ans == DialogResult.Cancel || ans == DialogResult.Abort)
                return inputboxreturn.Cancel;
            if (ans == DialogResult.OK)
                return inputboxreturn.OK;

            return inputboxreturn.NotSet;
        }

        private static string CommsBase_Settings(string name, string value, bool set = false)
        {
            if (set)
            {
                Settings.Instance[name] = value;
                return value;
            }

            if (Settings.Instance.ContainsKey(name))
            {
                return Settings.Instance[name].ToString();
            }

            return "";
        }

        private static void handleException(Exception ex)
        {
            if (ex.Message == "Safe handle has been closed")
            {
                return;
            }

            if (MainV2.instance != null && MainV2.instance.IsDisposed)
                return;

            //aeromagtec.Utilities.Tracking.AddException(ex);

            log.Debug(ex.ToString());

            //GetStackTrace(ex);

            // hyperlinks error
            if (ex.Message == "Requested registry access is not allowed." ||
                ex.ToString().Contains("System.Windows.Forms.LinkUtilities.GetIELinkBehavior"))
            {
                return;
            }
            if (ex.Message == "The port is closed.")
            {
                CustomMessageBox.Show("Serial connection has been lost");
                return;
            }
            if (ex.Message == "A device attached to the system is not functioning.")
            {
                CustomMessageBox.Show("Serial connection has been lost");
                return;
            }
            if (ex.GetType() == typeof(OpenTK.Graphics.GraphicsContextException))
            {
                CustomMessageBox.Show("Please update your graphics card drivers. Failed to create opengl surface\n" + ex.Message);
                return;
            }
            if (ex.GetType() == typeof(MissingMethodException) || ex.GetType() == typeof(TypeLoadException))
            {
                CustomMessageBox.Show("Please Update - Some older library dlls are causing problems\n" + ex.Message);
                return;
            }
            if (ex.GetType() == typeof(ObjectDisposedException) || ex.GetType() == typeof(InvalidOperationException))
            // something is trying to update while the form, is closing.
            {
                log.Error(ex);
                return; // ignore
            }
            if (ex.GetType() == typeof(FileNotFoundException) || ex.GetType() == typeof(BadImageFormatException))
            // i get alot of error from people who click the exe from inside a zip file.
            {
                CustomMessageBox.Show(
                    "You are missing some DLL's. Please extract the zip file somewhere. OR Use the update feature from the menu " +
                    ex.ToString());
                // return;
            }
            // windows and mono
            if (ex.StackTrace != null && ex.StackTrace.Contains("System.IO.Ports.SerialStream.Dispose") ||
                ex.StackTrace != null && ex.StackTrace.Contains("System.IO.Ports.SerialPortStream.Dispose"))
            {
                log.Error(ex);
                return; // ignore
            }

            log.Info("Th Name " + Thread.Name);

            DialogResult dr =
                CustomMessageBox.Show("An error has occurred\n" + ex.ToString() + "\n\nReport this Error???",
                    "Send Error", MessageBoxButtons.YesNo);
        }

        private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
        {
            Exception ex = e.Exception;

            handleException(ex);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var list = AppDomain.CurrentDomain.ReflectionOnlyGetAssemblies();

            log.Error(list);

            handleException((Exception)e.ExceptionObject);
        }
    }
}