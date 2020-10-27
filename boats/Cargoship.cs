using System;
using System.Collections.Generic;
using System.Text;

namespace HamnSimulering
{
    class Cargoship : Boat
    {
        public int Containers { get; set; }
        public Cargoship(string id, int weight, int topSpeedInKnots, int currentLoad, int daysSpent = 0, int[] spots = null) : base(id, weight, topSpeedInKnots, daysSpent, spots)
        {
            MaxDaysAtHarbour = 6;
            Containers = currentLoad;
            SizeInSpots = 4f;
        }
    }
}
