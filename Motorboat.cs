using System;
using System.Collections.Generic;
using System.Text;

namespace HamnSimulering
{
    class Motorboat : Boat
    {
        public int Horsepowers { get; set; }
        public Motorboat(string id, int weight, int topSpeedInKnots, int horsePower, int daysSpent = 0) : base(id, weight, topSpeedInKnots, daysSpent)
        {
            MaxDaysAtHarbour = 3;
            Horsepowers = horsePower;
            SpecialProperty = $"{horsePower} hästkrafter";
            SizeInSpots = 1f;
        }
    }
}
