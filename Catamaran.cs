using System;
using System.Collections.Generic;
using System.Text;

namespace HamnSimulering
{
    class Catamaran : Boat
    {
        public int NumberOfBeds { get; set; }
        public Catamaran(string id, int weight, int topSpeedInKnots, int beds, int daysSpent = 0) : base(id, weight, topSpeedInKnots, daysSpent)
        {
            NumberOfBeds = beds;
            SpecialProperty = $"{NumberOfBeds} sängar";
            SizeInSpots = 3;
            MaxDaysAtHarbour = 5;
        }
    }
}
