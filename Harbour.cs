﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace HamnSimulering
{
    class Harbour
    {
        public List<Boat> Port = new List<Boat>();


        public string HarbourName { get; set; }

        const float TotalSpots = 32;
        public bool[] IsCurrentSpotTaken { get; set; }
        public int LargestSpot { get; set; }

        public List<Boat> RemovedBoats = new List<Boat>();
        public float SpotsLeft => TotalSpots - Port.Sum(boat => boat.SizeInSpots);

        public Harbour(string name)
        {
            HarbourName = name;
            IsCurrentSpotTaken = new bool[32];
            Array.Fill(IsCurrentSpotTaken, false);

        }

        //public void UpdateLargestSpot()
        //{
        //    LargestSpot = GetLargestSpot();
        //}


        public void RemoveBoats()
        {
            RemovedBoats = Port.Where(boat => boat.DaysSpentAtHarbour == boat.MaxDaysAtHarbour).ToList();

            foreach (Boat boatToRemove in RemovedBoats)
            {
                Remove(boatToRemove);
            }
        }


        /// <summary>
        /// Sätter båtens position i hamnen till (true/false)  
        /// </summary>
        /// <param name="boat">Båten som tar 1 plats</param>
        /// <param name="value">true för upptagen, false för ledig, antalet roddbåtar kollas, är det fler än en (1) blir värdet true för platsen</param>
        void SetSingleSpot(Boat boat, bool value)
        {
            int spot = boat.AssignedSpotAtHarbour[0];
            if (boat is Rowboat && value == false)
            {
                bool anotherRowboat = Port.Any(otherBoat => otherBoat.AssignedSpotAtHarbour[0] == spot && otherBoat.ModelID != boat.ModelID && otherBoat is Rowboat);
                IsCurrentSpotTaken[spot] = anotherRowboat;
            }
            else
            {
                IsCurrentSpotTaken[spot] = value;
            }
        }



        /// <summary>
        /// Sätter båtens position i hamnen till (true/false) 
        /// </summary>
        /// <param name="boat">Båten som tar mer än 1 plats</param>
        /// <param name="value">true för upptagen, false för ledig</param>
        void SetManySpots(Boat boat, bool value)
        {
            int firstSpot = boat.AssignedSpotAtHarbour[0];
            int lastSpot = boat.AssignedSpotAtHarbour[1];
            for (int i = firstSpot; i <= lastSpot; i++)
            {
                IsCurrentSpotTaken[i] = value;
            }
        }
        public void Remove(Boat boat)
        {
            string boatType = boat.GetBoatType();
            switch (boatType)
            {
                case "Roddbåt":
                case "Motorbåt":
                    SetSingleSpot(boat, false);
                    break;
                default:
                    SetManySpots(boat, false);
                    break;
            }
            Port.Remove(boat);
            BoatData.RemoveBoat(boat, this.HarbourName);
        }


        /// <summary>
        /// försöker lägga in båten från sista platsen, fortsätter till sista eller tills den hittar en plats, returnerar true om en plats hittas, false om en plats inte hittas
        /// sätter även värdet på båtens AssignedSpotAtHarbour
        /// </summary>
        /// <param name="currentBoat"></param>
        /// <returns></returns>
        //private bool TryAddFromBottom(Boat currentBoat)
        //{
        //    //t.ex en segelbåt ska ha 2 platser, plats 1 och 2, 2 - (antal platser den ska ha (2)) = 0; 0,1,2 blir 3 platser, därför tar jag bort 1 så det blir 1,2
        //    int spotsToTake = (int)currentBoat.SizeInSpots - 1;
        //    int currentSpot = IsCurrentSpotTaken.GetUpperBound(0);
        //    int spotsFound = 0;

        //    for (int i = currentSpot; i >= 0; i--)
        //    {
        //        bool spotTaken = IsCurrentSpotTaken[i];
        //        if (spotTaken)
        //        {
        //            spotsFound = 0;
        //            currentSpot--;
        //            continue;
        //        }
        //        else
        //        {
        //            spotsFound++;
        //        }


        //        if (spotsFound == spotsToTake)
        //        {
        //            int start = currentSpot;
        //            int end = (currentSpot + spotsToTake);
        //            currentBoat.AssignedSpotAtHarbour = new int[2] { start, end };
        //            return true;
        //        }
        //        currentSpot--;
        //    }
        //    return false;
        //}

        ///// <summary>
        ///// försöker lägga in båten från första platsen, fortsätter ner sista eller tills den hittar en plats, returnerar true om en plats hittas, false om en plats inte hittas
        ///// sätter även värdet på båtens AssignedSpotAtHarbour
        ///// </summary>
        ///// <param name="currentBoat"></param>
        ///// <returns></returns>
        //private bool TryAddFromTop(Boat currentBoat)
        //{
        //    int spotsToTake = (int)currentBoat.SizeInSpots - 1;
        //    int currentSpot = 0;
        //    int spotsFound = 0;
        //    foreach (bool spotTaken in IsCurrentSpotTaken)
        //    {
        //        if (spotTaken)
        //        {
        //            spotsFound = 0;
        //            currentSpot++;
        //            continue;
        //        }
        //        else
        //        {
        //            spotsFound++;
        //        }


        //        if (spotsFound == spotsToTake)
        //        {
        //            int start = (currentSpot - spotsToTake);
        //            int end = currentSpot;
        //            currentBoat.AssignedSpotAtHarbour = new int[2] { start, end };
        //            return true;
        //        }
        //    }
        //    return false;
        //}


        bool FindUsedSpotForSmallBoat(Boat boat)
        {
            Boat sameType = RemovedBoats.FirstOrDefault(removedBoat => removedBoat.GetBoatType() == removedBoat.GetBoatType());
            List<Boat> suitableSpots = RemovedBoats.OrderBy(removedBoat => removedBoat.SizeInSpots).ToList();

            if (sameType != null)
            {
                boat.AssignedSpotAtHarbour = sameType.AssignedSpotAtHarbour;
                RemovedBoats.Remove(sameType);
                return true;
            }

            foreach (Boat differentType in suitableSpots)
            {
                int numberOfBoatsOnSpot = Port.Count(boat => boat.AssignedSpotAtHarbour[0] == differentType.AssignedSpotAtHarbour[0]);
                if (boat is Rowboat)
                {
                    int assignedSpot = differentType.AssignedSpotAtHarbour[0];
                    boat.AssignedSpotAtHarbour = new int[1] { assignedSpot };
                    RemovedBoats.Remove(differentType);
                    return true;
                }
                // om det är någon båt kvar kan inte motorbåten ställa sig där (då är det en roddbåt)
                else if (boat is Motorboat && numberOfBoatsOnSpot < 1)
                {
                    List<Boat> boatsToRemove = RemovedBoats.Where(boat => boat.AssignedSpotAtHarbour[0] == differentType.AssignedSpotAtHarbour[0]).ToList();
                    foreach (Boat boatOnSpot in boatsToRemove)
                    {
                        RemovedBoats.Remove(boatOnSpot);
                    }
                    boat.AssignedSpotAtHarbour[0] = differentType.AssignedSpotAtHarbour[0];
                    return true;
                }
            }
            return false;
        }
        bool FindMultipleAlreadyUsedSpots(Boat boat)
        {
            Boat oldBoatSameType = RemovedBoats.FirstOrDefault(oldBoatSameType => oldBoatSameType.GetBoatType() == boat.GetBoatType());
            Boat oldBoatDifferentType = RemovedBoats.Where(oldBoat => oldBoat.SizeInSpots >= boat.SizeInSpots)
                .OrderBy(oldBoat => oldBoat.SizeInSpots)
                .FirstOrDefault();

            // index börjar på 0 i en array, t.ex en båt som behöver 3 platser, behöver 2,3,4 (1 mindre än 2+SizeInSpots(3)) vilket hade blivit 2,3,4,5
            int spotsNeeded = (int)boat.SizeInSpots - 1;
            if (oldBoatSameType != null)
            {
                //finns det en båt av samma typ som precis tagits bort kan den här båten få den platsen
                boat.AssignedSpotAtHarbour = oldBoatSameType.AssignedSpotAtHarbour;
                RemovedBoats.Remove(oldBoatSameType);
                return true;
            }
            else if (oldBoatDifferentType != null)
            {
                //annars ska den här båten ta en del utav en plats
                int start = oldBoatDifferentType.AssignedSpotAtHarbour[0];
                int end = start + spotsNeeded;
                boat.AssignedSpotAtHarbour = new int[2] { start, end };
                RemovedBoats.Remove(oldBoatDifferentType);
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool CheckOldSpots(Boat boat)
        {

            if (boat is Rowboat || boat is Motorboat)
            {
                return FindUsedSpotForSmallBoat(boat);
            }
            else
            {
                return FindMultipleAlreadyUsedSpots(boat);
            }
        }


        //public bool HasFreeSpots(Boat currentBoat)
        //{
        //    bool needsOneSpot = currentBoat.SizeInSpots <= 1f;
        //    if (currentBoat.SizeInSpots > LargestSpot)
        //    {
        //        return false;
        //    }
        //    else if (needsOneSpot)
        //    {
        //        return FindSpotForSmallBoat(currentBoat);
        //    }
        //    else
        //    {
        //        return currentBoat.SizeInSpots % 2 != 0 ? TryAddFromTop(currentBoat) : TryAddFromBottom(currentBoat);
        //    }
        //}




        //private bool FindSpotForSmallBoat(Boat currentBoat)
        //{
        //    int currentSpot = 0;
        //    if (currentBoat is Rowboat)
        //    {

        //        List<Boat> otherRowboats = Port.Where(boat => boat.GetBoatType() == "Roddbåt").ToList();
        //        if (otherRowboats.Any())
        //        {
        //            foreach (Boat rowboat in otherRowboats)
        //            {
        //                bool rowBoatWillFit = otherRowboats.Count(boat => boat.AssignedSpotAtHarbour[0] == rowboat.AssignedSpotAtHarbour[0]) < 2;
        //                if (rowBoatWillFit)
        //                {
        //                    currentBoat.AssignedSpotAtHarbour = rowboat.AssignedSpotAtHarbour;
        //                    return true;
        //                }
        //            }
        //        }

        //    }
        //    foreach (bool spotTaken in IsCurrentSpotTaken)
        //    {

        //        if (!spotTaken)
        //        {

        //            currentBoat.AssignedSpotAtHarbour = new int[1] { currentSpot };
        //            return true;
        //        }
        //        currentSpot++;
        //    }

        //    return false;
        //}

        //int GetLargestSpot()
        //{
        //    int largestSpot = 0;
        //    int currentIteration = 0;
        //    int i = 0;
        //    while (i <= IsCurrentSpotTaken.GetUpperBound(0))
        //    {
        //        bool spotTaken = IsCurrentSpotTaken[i];
        //        i++;
        //        if (!spotTaken)
        //        {
        //            currentIteration++;
        //            continue;
        //        }
        //        if (currentIteration == 0)
        //        {
        //            continue;
        //        }
        //        else
        //        {
        //            largestSpot = CompareValues(currentIteration, largestSpot);
        //            currentIteration = 0;
        //        }
        //    }
        //    largestSpot = CompareValues(currentIteration, largestSpot);
        //    return largestSpot;
        //}



        //int CompareValues(int first, int second)
        //{
        //    return first > second ? first : second;
        //}


        public void AddBoat(Boat boat)
        {
            string boatType = boat.GetBoatType();
            switch (boatType)
            {
                case "Roddbåt":
                case "Motorbåt":
                    SetSingleSpot(boat, true);
                    break;
                default:
                    SetManySpots(boat, true);
                    break;
            }
            Port.Add(boat);
            //BoatData.UpdateHarbour__fix(this, boat);
            BoatData.UpdateHarbour(this, this.HarbourName);
            BoatData.ListFreeSpots(this);
            //BoatData.ListFreeSpots__fix(this);
        }

        public void UpdateSpots(Boat boat)
        {
            string boatType = boat.GetBoatType();
            switch (boatType)
            {
                case "Roddbåt":
                case "Motorbåt":
                    SetSingleSpot(boat, true);
                    break;
                default:
                    SetManySpots(boat, true);
                    break;
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
///





//assignedSpot = new int[0];
//oldSpot = new int[0];
//if (!TheseSpotsFirst.Any())
//{
//    return false;
//}


//foreach (int[] spot in TheseSpotsFirst)
//{
//    int spotRangeNeeded = (int)boat.SizeInSpots - 1;

//    if (boat is Rowboat || boat is Motorboat)
//    {
//        assignedSpot = new int[] { spot[0] };
//        oldSpot = spot;
//        return true;
//    }
//    else if (spot[0] + spotRangeNeeded <= spot[1])
//    {
//        int start = spot[0];
//        int end = start + spotRangeNeeded;
//        assignedSpot = new int[2] { start, end };
//        oldSpot = spot;
//        return true;
//    }
//}
//return false;