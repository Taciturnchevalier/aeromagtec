using System;
using System.Reflection;
using System.Windows.Forms;
using log4net;
using aeromagtec.Controls;
using aeromagtec.Controls.BackstageView;

//using aeromagtec.GCSViews.ConfigurationView;
using aeromagtec.Utilities;
using System.Resources;

namespace aeromagtec.GCSViews
{
    public partial class InitialSetup : MyUserControl, IActivate
    {
        internal static readonly ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static string lastpagename = "";

        public InitialSetup()
        {
            InitializeComponent();
        }

        //public bool isConnected
        //{
        //    //get { return MainV2.ConectisSUCC; }
        //}

        //public bool isDisConnected
        //{
        //    //get { return !MainV2.ConectisSUCC; }
        //}

        private BackstageViewPage AddBackstageViewPage(Type userControl, string headerText, bool enabled = true,
    BackstageViewPage Parent = null, bool advanced = false)
        {
            try
            {
                if (enabled)
                    return backstageView.AddPage(userControl, headerText, Parent, advanced);
            }
            catch (Exception ex)
            {
                log.Error(ex);
                return null;
            }

            return null;
        }

        public void Activate()
        {
        }
    }
}