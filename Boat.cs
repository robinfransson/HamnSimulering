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
        public int[] AssignedSpotAtHarbour { get; set; }

        public Func<string> GetSpot => () => this is Rowboat || this is Motorboat ? $"{AssignedSpotAtHarbour[0] + 1}" : $"{AssignedSpotAtHarbour[0] + 1}-{AssignedSpotAtHarbour[1] + 1}";
        public float SizeInSpots { get; set; }
        public string ModelID { get; set; }
        public int DaysSpentAtHarbour { get; set; }
        public string TopSpeedKMH => (float)Math.Round(TopSpeedKnots * 1.852, 2) + " km/h";
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
            AssignedSpotAtHarbour = spots;
            DaysSpentAtHarbour = daysSpent;
            ModelID = id;
            Weight = weight;
            TopSpeedKnots = topSpeedKnots;
        }


    }

}







//hade först en sträng med formatet 'plats1,plats2' ex. '1,4' som jag splittade och parsade till int, men gjorde så att mina funktioner skickar tillbaks int[] istället
//int[] GetAssignedSpots(string spots)
//{
//    string[] splitSpots = spots.Split(",");
//    if (splitSpots.Length < 2)
//    {
//        return new int[1] { Int32.Parse(spots) };
//    }
//    else
//    {
//        int start = Int32.Parse(splitSpots[0]);
//        int end = Int32.Parse(splitSpots[1]);
//        return new int[2] { start, end };
//    }
//}
//public void AssignSpot(string spots)
//{
//    AssignedSpotAtHarbour = GetAssignedSpots(spots);
//}