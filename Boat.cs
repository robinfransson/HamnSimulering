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

        public Func<string> GetSpot => () => this is Rowboat ? $"{OccupiedSpots[0] + 1}" : $"{OccupiedSpots[0] + 1}-{OccupiedSpots[1] + 1}";
        public float SizeInSpots { get; set; }
        public string ModelID { get; set; }
        public int DaysSpentAtHarbour { get; set; }
        public string TopSpeedKMH => (float)Math.Round(TopSpeedKnots * 0.53995680345572, 2) + " km/h";
        public int TopSpeedKnots { get; set; }
        public int Weight { get; set; }
        public int MaxDaysAtHarbour { get; set; }
        public Func<string> GetBoatType => () =>
            {
                if (this is Rowboat) return "Roddbåt";
                else if (this is Cargoship) return "Lastfartyg";
                else if (this is Catamaran) return "Katamaran";
                else if (this is Sailboat) return "Segelbåt";
                else return "Motorbåt";
            };

        public Boat(string id, int weight, int topSpeedKnots, int daysSpent = 0, int[] spots = null)
        {
            OccupiedSpots = spots;
            DaysSpentAtHarbour = daysSpent;
            ModelID = id;
            Weight = weight;
            TopSpeedKnots = topSpeedKnots;
        }

        int[] GetAssignedSpots(string spots)
        {
            string[] splitSpots = spots.Split(",");
            if (splitSpots.Length < 2)
            {
                return new int[1] { Int32.Parse(spots) };
            }
            else
            {
                int start = Int32.Parse(splitSpots[0]);
                int end = Int32.Parse(splitSpots[1]);
                return new int[2] { start, end };
            }
        }
        public void AssignSpot(string spots)
        {
            OccupiedSpots = GetAssignedSpots(spots);
        }
    }

}
