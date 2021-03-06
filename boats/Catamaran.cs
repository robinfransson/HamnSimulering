﻿using System;
using System.Collections.Generic;
using System.Text;

namespace HamnSimulering
{
    class Catamaran : Boat
    {
        public int NumberOfBeds { get; set; }
        public Catamaran(string id, int weight, int topSpeedInKnots, int beds, int daysSpent = 0, int[] spots = null, string port = null) : base(id, weight, topSpeedInKnots, daysSpent, spots, port)
        {
            NumberOfBeds = beds;
            Size = 3f;
            MaxDaysAtHarbour = 3;
        }
    }
}
