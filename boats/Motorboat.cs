using System;
using System.Collections.Generic;
using System.Text;

namespace HamnSimulering
{
    class Motorboat : Boat
    {
        public int Horsepowers { get; set; }
        public Motorboat(string id, int weight, int topSpeedInKnots, int horsePower, int daysSpent = 0, int[] spots = null, string port = null) : base(id, weight, topSpeedInKnots, daysSpent, spots, port)
        {
            MaxDaysAtHarbour = 3;
            Horsepowers = horsePower;
            Size = 1f;
        }
    }
}
