using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HamnSimulering
{
    class Harbour
    {
        public List<Boat> Port = new List<Boat>();

        const float TotalSpots = 32;
        public bool PreferSailboat { get; set; }
        private bool[] PortSpotsInUse { get; set; }
        public float SpotsLeft => TotalSpots - Port.Sum(boat => boat.SizeInSpots);

        public Harbour(bool preferSailBoat=false)
        {

            PortSpotsInUse = new bool[32];
            for (int spot = 0; spot <= PortSpotsInUse.GetUpperBound(0); spot++)
            {
                PortSpotsInUse[spot] = false;
            }
            PreferSailboat = preferSailBoat;

        }


        public bool TryAdd(Boat boat)
        {
            if (AreThereFreeSpots(boat, out int[] assignedSpot))
            {
                return true;
            }
            else
            {
                return false;
            }
        }


        public void Remove(Boat boat)
        {
            int actualStart = boat.OccupiedSpots[0] - 1;
            int actualEnd = boat.OccupiedSpots[1] - 1;
            for(int i = actualStart; i < actualEnd; i++)
            {
                PortSpotsInUse[i] = false;
            }
            Port.Remove(boat);
        }
        

        private bool CanBoatFitInHarbour(float spotsToTake, out int[] assignedSpot)
        {
            assignedSpot = new int[2];
            int currentSpot = 0;
            int spotsFound = 0;
            int timesRan = 0;
            int endOfHarbour = PortSpotsInUse.GetUpperBound(0);
            //if spot is taken then continue loop from last spot that was checked occupied
            while((spotsFound < spotsToTake) && (currentSpot + spotsToTake <= endOfHarbour))
            {
                if(PortSpotsInUse[currentSpot + timesRan])
                {
                    timesRan++;
                    currentSpot += timesRan;
                    timesRan = 0;
                }
                else
                {
                    spotsFound++;
                }
                if(spotsFound == spotsToTake)
                {
                    int start = currentSpot - (int)spotsToTake;
                    int end = currentSpot;
                    assignedSpot[0] = start;
                    assignedSpot[1] = end;
                    return true;
                }
            }
            return false;
        }
        private bool AreThereFreeSpots(Boat currentBoat, out int[] assignedSpot)
        {
            assignedSpot = new int[2];
            float boatSize = currentBoat.SizeInSpots;
            int spot = 1;
            bool currentBoatIsSailBoat = currentBoat is Sailboat;
            foreach (bool isCurrentSpotFree in PortSpotsInUse)
            {

                bool sailBoatWillFit = (Port.Count(boat => boat.OccupiedSpots[0] == spot && boat is Sailboat) < 2)
                                    || !Port.Any(boat => boat.OccupiedSpots[0] == spot);

                bool sailBoatCanDock = currentBoatIsSailBoat && sailBoatWillFit;




                if (isCurrentSpotFree && sailBoatCanDock)
                {
                    assignedSpot = new int[1] { spot };
                    return true;
                }
                if(currentBoat is Sailboat)
                {
                    continue;
                }
                else
                {
                    return CanBoatFitInHarbour(boatSize, out assignedSpot);
                }

            }
            return false;
        }
    }
}
