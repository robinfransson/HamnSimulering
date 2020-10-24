using System;
using System.Collections.Generic;
using System.Text;

namespace HamnSimulering
{
    class Rowboat : Boat
    {
        public int MaxCapacity { get; set; }
        public Rowboat(string id, int weight, int topSpeedInKnots, int passengers, int daysSpent=0, int[] spots=null) : base(id, weight, topSpeedInKnots, daysSpent, spots)
        {
            MaxCapacity = passengers;
            SizeInSpots = 0.5f;
            SpecialProperty = $"{MaxCapacity} personer";
            MaxDaysAtHarbour = 1;
        }

    }
}
