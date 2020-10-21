using System;
using System.Collections.Generic;
using System.Text;

namespace HamnSimulering
{
    class Rowboat : Boat
    {
        public int MaxCapacity { get; set; }
        public Rowboat(string id, int weight, int topSpeedInKnots, int passengers, int daysSpent=0) : base(id, weight, topSpeedInKnots, daysSpent)
        {
            MaxCapacity = passengers;
            SizeInSpots = 0.5f;
            SpecialProperty = $"{MaxCapacity} personer";
            MaxDaysAtHarbour = 1;
        }

    }
}
