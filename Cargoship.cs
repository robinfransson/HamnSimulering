using System;
using System.Collections.Generic;
using System.Text;

namespace HamnSimulering
{
    class Cargoship : Boat
    {
        public int CurrentCargo { get; set; }
        public Cargoship(string id, int weight, int topSpeedInKnots, int currentLoad, int daysSpent = 0) : base(id, weight, topSpeedInKnots, daysSpent)
        {
            MaxDaysAtHarbour = 6;
            CurrentCargo = currentLoad;
            SpecialProperty = $"{CurrentCargo} containers";
            SizeInSpots = 4f;
        }
    }
}
