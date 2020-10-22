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

        public Func<string> Spots => () => this is Sailboat ? $"{OccupiedSpots[0] + 1}" : $"{OccupiedSpots[0] + 1},{OccupiedSpots[1] + 1}";
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


        public void AssignSpot(string spot)
        {
            if(spot.Length == 1)
            {
                OccupiedSpots = new int[1] { Int32.Parse(spot) };
            }
            else
            {
                string[] spots = spot.Split(",");
                int start = Int32.Parse(spots[0]);
                int end = Int32.Parse(spots[1]);
                OccupiedSpots = new int[2] { start, end };
            }
        }
    }

}
