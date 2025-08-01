﻿using System.Drawing;

namespace ST.Blazor.Leaflet.Samples.Data
{
    public record City
    {

        public string CoatOfArmsImageUrl { get; set; }

        public string Country { get; set; }

        public string Name { get; set; }

        public string Description { get; set; }

        public PointF Coordinates { get; set; }

    }
}
