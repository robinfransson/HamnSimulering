using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace HamnSimulering
{
    class Harbour
    {
        public List<Boat> Port = new List<Boat>();

        const float TotalSpots = 32;
        public bool[] IsCurrentSpotTaken { get; set; }
        public int LargestSpot { get; set; }
        public float SpotsLeft => TotalSpots - Port.Sum(boat => boat.SizeInSpots);

        public Harbour()
        {

            IsCurrentSpotTaken = new bool[32];
            Array.Fill(IsCurrentSpotTaken, false);

        }

        public void UpdateLargestSpot()
        {
            LargestSpot = GetLargestSpot();
        }


        public void RemoveBoats(string table)
        {

            List<Boat> toRemove = Port.Where(boat => boat.DaysSpentAtHarbour == boat.MaxDaysAtHarbour).ToList();
            foreach(Boat boatToRemove in toRemove)
            {
                Remove(boatToRemove);
                BoatData.RemoveBoat(boatToRemove, table);
            }
        }

        public void Remove(Boat boat)
        {

            int start = boat.AssignedSpotAtHarbour[0];
            try
            {
                int end = boat.AssignedSpotAtHarbour[1];
                bool notRowboat = !(boat is Rowboat);
                bool anotherRowboat = Port.Count(otherBoat => otherBoat.AssignedSpotAtHarbour[0] == start && otherBoat is Rowboat) == 2;
                if (notRowboat || !anotherRowboat)
                {
                    for (int i = start; i <= end; i++)
                    {
                        IsCurrentSpotTaken[i] = false;
                    }
                }
            }
            catch (Exception e)
            {
                if (e is IndexOutOfRangeException)
                {
                    IsCurrentSpotTaken[start] = false;
                }
                else
                {
                    MessageBox.Show(e.Message);
                }
            }

            Port.Remove(boat);
        }



        private bool TryAddFromBottom(float spotsToTake, out int[] assignedSpot)
        {
            assignedSpot = new int[2];
            int currentSpot = IsCurrentSpotTaken.GetUpperBound(0);
            int spotsFound = 0;

            for(int i = currentSpot; i >= 0; i--) 
            {
                bool spotTaken = IsCurrentSpotTaken[i];
                if (spotTaken)
                {
                    spotsFound = 0;
                    currentSpot--;
                    continue;
                }
                else
                {
                    spotsFound++;
                }


                if (spotsFound == spotsToTake)
                {
                    //t.ex en segelbåt ska ha 2 platser, plats 1 och 2, 2 - (antal platser den ska ha (2)) = 0 0,1,2 blir 3 platser, därför plussar jag på 1 så det blir 1,2
                    int start = currentSpot;
                    int end = (currentSpot + (int)spotsToTake) - 1;
                    assignedSpot = new int[2] { start, end };
                    return true;
                }
                currentSpot--;
            }
            return false;
        }


        private bool TryAddFromTop(float spotsToTake, out int[] assignedSpot)
        {
            assignedSpot = new int[2];
            int currentSpot = 0;
            int spotsFound = 0;

            foreach(bool spotTaken in IsCurrentSpotTaken)
            {
                if(spotTaken)
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
                    //t.ex en segelbåt ska ha 2 platser, plats 1 och 2, 2 - (antal platser den ska ha (2)) = 0 0,1,2 blir 3 platser, därför plussar jag på 1 så det blir 1,2
                    int start = (currentSpot - (int)spotsToTake) + 1; 
                    int end = currentSpot;
                    assignedSpot = new int[2] {start, end};
                    return true;
                }
                currentSpot++;


            }
            return false;
        }

        



        public bool HasFreeSpots(Boat currentBoat, out int[] assignedSpot)
        {
            bool needsOneSpot = currentBoat.SizeInSpots <= 1f;
            if(currentBoat.SizeInSpots > LargestSpot)
            {
                assignedSpot = new int[1];
                return false;
            }
            else if (needsOneSpot)
            {
                return FindSpotForSmallBoat(currentBoat, out assignedSpot);
            }
            else
            {
                float boatSize = currentBoat.SizeInSpots;
                return boatSize % 2 == 0 ? TryAddFromTop(boatSize, out assignedSpot) : TryAddFromBottom(boatSize, out assignedSpot);
            }
        }




        private bool FindSpotForSmallBoat(Boat currentBoat, out int[] assignedSpot)
        {
            assignedSpot = new int[1];
            int currentSpot = 0;


            foreach (bool spotTaken in IsCurrentSpotTaken)
            {
                bool rowBoatOnSpot = Port.FirstOrDefault(boat => boat.AssignedSpotAtHarbour[0] == currentSpot) is Rowboat;
                bool spotIsUsedByRowboat = spotTaken && rowBoatOnSpot;
                bool isThereSpace = Port.Count(boat => boat.AssignedSpotAtHarbour[0] == currentSpot) < 2;

                bool rowBoatWillFit = spotIsUsedByRowboat && isThereSpace && currentBoat is Rowboat;

                if (rowBoatWillFit || !spotTaken)
                {
                    assignedSpot = new int[1] {currentSpot};
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
            while (i <= IsCurrentSpotTaken.GetUpperBound(0))
            {
                bool spot = IsCurrentSpotTaken[i];
                if (!spot)
                {
                    _temp++;
                }
                else
                {
                    largestSpot = CompareValues(_temp, largestSpot);
                    _temp = 0;
                }
                i++;
            }
            largestSpot = CompareValues(_temp, largestSpot);
            return largestSpot;
        }



        int CompareValues(int first, int second)
        {
            return first > second ? first : second;
        }


        public void UpdateSpots(Boat boat)
        {
            int start = boat.AssignedSpotAtHarbour[0];
            try
            {
                int end = boat.AssignedSpotAtHarbour[1];
                while (start <= end)
                {
                    IsCurrentSpotTaken[start] = true;
                    start++;
                }
            }
            catch(Exception e)
            {
                if (e is IndexOutOfRangeException)
                {
                    IsCurrentSpotTaken[start] = true;
                }
                else
                {
                    MessageBox.Show(e.Message);
                }
            }
        }
    }
}


































//private bool CanBoatFitInHarbour(float spotsToTake, out string assignedSpot)
//{
//    //if (this.LargestSpot < spotsToTake)
//    //{
//    //    return false;
//    //}
//    //else
//    //{
//    assignedSpot = "";
//    int currentSpot = 0;
//    int spotsFound = 0;
//    int endOfHarbour = IsCurrentSpotTaken.GetUpperBound(0);
//    //if spot is taken then continue loop from last spot that was checked occupied
//    while (currentSpot + spotsToTake <= endOfHarbour)
//    {
//        if (IsCurrentSpotTaken[currentSpot])
//        {
//            spotsFound = 0;
//            currentSpot++;
//            continue;
//        }
//        else
//        {
//            spotsFound++;
//            currentSpot++;
//        }
//        if (spotsFound == spotsToTake)
//        {

//            int start = currentSpot - (int)spotsToTake;
//            int end = currentSpot;
//            assignedSpot = $"{start},{end}";
//            return true;
//        }
//    }
//    return false;
//}
////}