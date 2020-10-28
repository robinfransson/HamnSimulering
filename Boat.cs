using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Linq;
using System.CodeDom;

namespace HamnSimulering
{
    class Boat
    {
        public Func<string> GetSpecialProperty => () =>
        {
            if (this is Rowboat rowboat) return $"{rowboat.MaxCapacity} passagerare";
            else if (this is Cargoship cargoship) return $"{cargoship.Containers} containers";
            else if (this is Catamaran catamaran) return $"{catamaran.NumberOfBeds} sängar";
            else if (this is Sailboat sailboat) return $"{sailboat.BoatLength} meter";
            else if (this is Motorboat motorboat) return $"{motorboat.Horsepowers} hästkrafter";
            else throw new NotImplementedException("Unsupported boat type: " +this.GetType());
        };
        public int[] AssignedSpotAtHarbour { get; set; }
        public Func<string> GetSpot => () =>
        {
            return AssignedSpotAtHarbour.Length < 2 ? $"{AssignedSpotAtHarbour[0] + 1}" : $"{AssignedSpotAtHarbour[0] + 1}-{AssignedSpotAtHarbour[1] + 1}";
            };
        public float SizeInSpots { get; set; }
        public string ModelID { get; set; }
        public int DaysSpentAtHarbour { get; set; }
        public string TopSpeedKMH => (float)Math.Round(TopSpeedKnots * 1.852, 0) + " km/h";
        public int TopSpeedKnots { get; set; }
        public int Weight { get; set; }
        public int MaxDaysAtHarbour { get; set; }
        public Func<string> GetBoatType => () =>
            {
                if (this is Rowboat) return "Roddbåt";
                else if (this is Cargoship) return "Lastfartyg";
                else if (this is Catamaran) return "Katamaran";
                else if (this is Sailboat) return "Segelbåt";
                else if(this is Motorboat) return "Motorbåt";
                else throw new NotImplementedException("Unsupported boat type: " + this.GetType());
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