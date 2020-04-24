using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using aeromagtec.Controls;

namespace aeromagtec.GCSViews.ConfigurationView
{
    public partial class ConfigGPSInject : UserControl, IActivate, IDeactivate
    {
        public ConfigGPSInject()
        {
            InitializeComponent();
        }

        public void Activate()
        {
            gps.Activate();
        }

        public void Deactivate()
        {
            gps.Deactivate();
        }
    }
}