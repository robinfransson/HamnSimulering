using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Linq;

namespace HamnSimulering
{
    class Boat
    {
        public string SpecialProperty { get; set; }
        public int[] OccupiedSpots { get; set; }
        public float SizeInSpots { get; set; }
        public string ModelID { get; set; }
        public int DaysSpentAtHarbour { get; set; }
        public float TopSpeedKMH => (float)Math.Round(TopSpeedKnots * 0.53995680345572, 2);
        public int TopSpeedKnots { get; set; }
        public int Weight { get; set; }
        public int MaxDaysAtHarbour { get; set; }

        public Boat(string id, int weight, int topSpeedKnots, int daysSpent = 0)
        {
            DaysSpentAtHarbour = daysSpent;
            ModelID = id;
            Weight = weight;
            TopSpeedKnots = topSpeedKnots;
        }

        public override string ToString()
        {
            Boat boat = this;
            string info = $"Modell: {boat.ModelID} Vikt: {boat.Weight} Topphastighet: Knop:{boat.TopSpeedKnots} KMH: {boat.TopSpeedKMH} Dagar vid hamnen: {boat.DaysSpentAtHarbour} ";
            if(boat is Rowboat rowboat)
            {
                info += $"Platser: {rowboat.MaxCapacity}";
            }
            else if (boat is Cargoship cargoship)
            {
                info += $"Last: {cargoship.CurrentCargo}";
            }
            else if (boat is Motorboat motorboat)
            {
                info += $"HK: {motorboat.Horsepowers}";
            }
            else if (boat is Sailboat sailboat)
            {
                info += $"HK: {sailboat.BoatLength}";
            }
            else
            {
                info += $"Sängar: {((Catamaran)boat).NumberOfBeds}";
            }
            return info;
        }
    }

}
