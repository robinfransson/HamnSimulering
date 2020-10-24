using System;
using System.Collections.Generic;
using System.Text;

namespace HamnSimulering
{
    class Cargoship : Boat
    {
        public int CurrentCargo { get; set; }
        public Cargoship(string id, int weight, int topSpeedInKnots, int currentLoad, int daysSpent = 0, int[] spots = null) : base(id, weight, topSpeedInKnots, daysSpent, spots)
        {
            MaxDaysAtHarbour = 6;
            CurrentCargo = currentLoad;
            SpecialProperty = $"{CurrentCargo} containers";
            SizeInSpots = 4f;
        }
    }
}
