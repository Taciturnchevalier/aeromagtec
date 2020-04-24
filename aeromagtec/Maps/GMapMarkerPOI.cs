﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using GMap.NET;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using aeromagtec.Properties;

namespace aeromagtec.Maps
{
    [Serializable]
    public class GMapMarkerPOI : GMarkerGoogle
    {
        public GMapMarkerPOI(PointLatLng p)
            : base(p, GMarkerGoogleType.red_dot)
        {
        }
    }
}