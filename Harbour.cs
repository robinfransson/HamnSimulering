using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;

namespace HamnSimulering
{
    class Harbour
    {
        public List<Boat> Port = new List<Boat>();

        const float TotalSpots = 32;
        public bool PreferSailboat { get; set; }
        private bool[] SpotsInUse { get; set; }
        public float SpotsLeft => TotalSpots - Port.Sum(boat => boat.SizeInSpots);

        public Harbour(bool preferSailBoat = false)
        {

            SpotsInUse = new bool[32];
            Array.Fill(SpotsInUse, false);
            PreferSailboat = preferSailBoat;

        }


        public void Remove(Boat boat)
        {
            int start = boat.OccupiedSpots[0];
            if (boat.SizeInSpots < 2f)
            {
                SpotsInUse[start] = false;
            }
            else
            {
                int end = boat.OccupiedSpots[1] - 1;
                for (int i = start; i <= end; i++)
                {
                    SpotsInUse[i] = false;
                }
            }
            Port.Remove(boat);
        }


        private bool CanBoatFitInHarbour(float spotsToTake, out string assignedSpot)
        {
            assignedSpot = "";
            int currentSpot = 0;
            int spotsFound = 0;
            int endOfHarbour = SpotsInUse.GetUpperBound(0);
            //if spot is taken then continue loop from last spot that was checked occupied
            while (currentSpot + spotsToTake <= endOfHarbour)
            {
                if (SpotsInUse[currentSpot])
                {
                    spotsFound = 0;
                    currentSpot++;
                    continue;
                }
                else
                {
                    spotsFound++;
                }
                if (spotsFound == spotsToTake)
                {

                    int start = currentSpot - (int)spotsToTake + 1;
                    int end = currentSpot;
                    assignedSpot = $"{start},{end}";
                    return true;
                }
                currentSpot++;
            }
            return false;
        }
        public bool AreThereFreeSpots(Boat currentBoat, out string assignedSpot)
        {
            if (currentBoat is Rowboat)
            {
                return FindSpotForRowboat(out assignedSpot);
            }
            else
            {
                float boatSize = currentBoat.SizeInSpots;
                return CanBoatFitInHarbour(boatSize, out assignedSpot);
            }
        }

        private bool FindSpotForRowboat(out string assignedSpot)
        {
            assignedSpot = "";
            int spot = 0;
            foreach (bool spotTaken in SpotsInUse)
            {
                bool rowBoatWillFit = (Port.Count(boat => boat.OccupiedSpots[0] == spot && boat is Sailboat) < 2);

                Boat otherBoatOnSpot = Port.FirstOrDefault(boat => boat.OccupiedSpots[0] == spot);
                if (rowBoatWillFit || !Port.Any(boat => boat.OccupiedSpots[0] == spot))
                {
                    assignedSpot = $"{spot}";
                    return true;
                }
                spot++;
            }
            return false;
        }

        public void UpdateSpots(Boat boat)
        {
            if (boat.OccupiedSpots.GetUpperBound(0) < 1)
            {
                int actualSpot = boat.OccupiedSpots[0];
                SpotsInUse[actualSpot] = true;

            }
            else
            {
                int current = boat.OccupiedSpots[0];
                int end = boat.OccupiedSpots[1];
                do
                {
                    SpotsInUse[current] = true;
                    current++;
                } while (current <= end);
            }
        }
    }
}
