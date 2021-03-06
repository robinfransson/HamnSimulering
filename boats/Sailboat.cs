﻿using System;
using System.Collections.Generic;
using System.Text;

namespace HamnSimulering
{
    class Sailboat : Boat
    {
        public int BoatLength { get; set; }
        public Sailboat(string id, int weight, int topSpeedInKnots, int length, int daysSpent = 0, int[] spots = null, string port=null) : base(id, weight, topSpeedInKnots, daysSpent, spots, port)
        {
            BoatLength = length;
            Size = 2f;
            MaxDaysAtHarbour = 4;
        }
    }
}
