﻿using PredictStarNumberMod.Configuration;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PredictStarNumberMod
{
    internal class MapDataDeliverer
    {
        public static MapDataDeliverer Instance { get; set; }

        private string hash=string.Empty;
        private string mapType=string.Empty;

        public string Hash => hash;
        public string MapType => mapType;

        internal void SetHash(string hash)
        {
            this.hash = hash;
        }

        internal void SetMapType(string mapType)
        {
            this.mapType = mapType;
        }
    }
}
