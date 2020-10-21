using System;
using System.Collections.Generic;
using System.Text;

namespace HamnSimulering
{
    class Sailboat : Boat
    {
        public int BoatLength { get; set; }
        public Sailboat(string id, int weight, int topSpeedInKnots, int length, int daysSpent = 0) : base(id, weight, topSpeedInKnots, daysSpent)
        {
            BoatLength = length;
            SpecialProperty = $"{BoatLength} meter lång";
            SizeInSpots = 2f;
            MaxDaysAtHarbour = 4;
        }
    }
}
