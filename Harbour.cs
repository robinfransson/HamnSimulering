using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace HamnSimulering
{
    class Harbour
    {
        public List<Boat> Port = new List<Boat>();

        const float TotalSpots = 32;
        public bool[] SpotsInUse { get; set; }
        public int LargestSpot { get; set; }
        public float SpotsLeft => TotalSpots - Port.Sum(boat => boat.SizeInSpots);

        public Harbour()
        {

            SpotsInUse = new bool[32];
            Array.Fill(SpotsInUse, false);

        }

        public void UpdateLargestSpot()
        {
            LargestSpot = GetLargestSpot();
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
                int end = boat.OccupiedSpots[1];
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
            if (this.LargestSpot < spotsToTake)
            {
                return false;
            }
            else
            {
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
        }



        public bool HasFreeSpots(Boat currentBoat, out string assignedSpot)
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
            int currentSpot = 0;
            foreach (bool spotTaken in SpotsInUse)
            {
                bool spotIsUsedByRowboat = spotTaken && Port.FirstOrDefault(boat => boat.OccupiedSpots[0] == currentSpot) is Rowboat;
                bool rowBoatWillFit = spotIsUsedByRowboat && Port.Count(boat => boat.OccupiedSpots[0] == currentSpot) < 2;

                if (rowBoatWillFit || !spotTaken)
                {
                    assignedSpot = $"{currentSpot}";
                    return true;
                }
                currentSpot++;
            }
            return false;
        }

        int GetLargestSpot()
        {
            int largestSpot = 0;
            int _temp = 0;
            int i = 0;
            while(i <= SpotsInUse.GetUpperBound(0))
            {
                bool spot = SpotsInUse[i];
                if (!spot)
                {
                    _temp++;
                }
                else
                {
                    if (_temp > largestSpot)
                        largestSpot = _temp;
                    _temp = 0;
                }
                i++;
            }
            if (_temp > largestSpot)
                largestSpot = _temp;
            _temp = 0;
            return largestSpot;
        }


        public void UpdateSpots(Boat boat)
        {
            if (boat.OccupiedSpots.GetUpperBound(0) < 1)
            {
                int assignedSpot = boat.OccupiedSpots[0];
                SpotsInUse[assignedSpot] = true;

            }
            else
            {
                int start = boat.OccupiedSpots[0];
                int end = boat.OccupiedSpots[1];
                while (start <= end)
                {
                    SpotsInUse[start] = true;
                    start++;
                }
            }
        }
    }
}
