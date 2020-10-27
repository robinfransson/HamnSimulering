using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Windows;
using System.CodeDom;

namespace HamnSimulering
{
    class Simulate
    {
        static AssignHarbourSpot assignLeft;
        static AssignHarbourSpot assignRight;

        public static int boatsRejected = 0;
        public static int daysPassed = 0;
        public static int numberOfNewBoats = 5;
        static Func<Boat, bool> NotAssigned => (b) => b.AssignedSpotAtHarbour == null;


        public static void OneDay(WaitingBoats waitingBoats, Harbour leftHarbour, Harbour rightHarbour)
        {
            //ListEmptySpots(leftHarbour);
            //ListEmptySpots(rightHarbour);





            assignLeft = new AssignHarbourSpot(leftHarbour);
            assignLeft.GetPositions();
            assignRight = new AssignHarbourSpot(rightHarbour);
            assignRight.GetPositions();

            List<Boat> waiting = waitingBoats.Waiting;

            List<Boat> rowboatsWaiting = waiting.Where(boat => boat is Rowboat).ToList();
            List<Boat> otherBoatsWaiting = waiting.Where(boat => !(boat is Rowboat)).ToList();

            assignLeft.PlaceRowboat(rowboatsWaiting);
            assignRight.PlaceRowboat(rowboatsWaiting);

            assignLeft.MergePositions();
            assignRight.MergePositions();

            assignLeft.GiveRowboatUnassignedSpot(rowboatsWaiting);
            assignRight.GiveRowboatUnassignedSpot(rowboatsWaiting);

            assignLeft.GivePositions(otherBoatsWaiting);
            assignRight.GivePositions(otherBoatsWaiting);

            assignLeft.TryAddFromBottom(otherBoatsWaiting);

            assignRight.TryAddFromBottom(otherBoatsWaiting);

            //lägg ihop de som inte har blivit tilldelade en plats
            otherBoatsWaiting.AddRange(rowboatsWaiting);
            boatsRejected += otherBoatsWaiting.Count(boat => NotAssigned(boat));


            AddOneDay(leftHarbour);
            AddOneDay(rightHarbour);

            //AddOneDay(leftHarbour);
            //AddOneDay(rightHarbour);

            //TryToAddNewBoats(waitingBoats, leftHarbour, rightHarbour);
            waitingBoats.Waiting.Clear();
            AddToWaiting(waitingBoats);
            ////ListEmptySpots(leftHarbour);
            ////ListEmptySpots(rightHarbour);
            BoatData.UpdateVisitors(waitingBoats.Waiting);

            UpdateData(leftHarbour);
            UpdateData(rightHarbour);


            daysPassed++;
        }

        //static List<Boat> TryPlacingBoats(Harbour harbour, WaitingBoats waitingBoats, AssignHarbourSpot assign)
        //{
        //
        //    List<Boat> leftoverBoats = assign.FindBestPlacement(harbour, waitingBoats);
        //
        //
        //    return leftoverBoats;
        //
        //}

        public static void AddOneDay(Harbour harbour)
        {
            harbour.RemoveBoats();


            if (harbour.Port.Any())
            {
                foreach (Boat boat in harbour.Port)
                {
                    boat.DaysSpentAtHarbour++;
                }
            }
        }

        //private static bool TryToDock(Boat boat, Harbour harbour)
        //{
        //    return harbour.HasFreeSpots(boat);
        //}

        public static void AddToWaiting(WaitingBoats waitingBoats)
        {
            waitingBoats.AddBoats(numberOfNewBoats);
            BoatData.UpdateVisitors(waitingBoats.Waiting);
        }


        public static void ListEmptySpots(Harbour harbour)
        {
            BoatData.ListFreeSpots(harbour);
        }
        public static void UpdateData(Harbour harbour)
        {
            BoatData.UpdateHarbour(harbour, harbour.HarbourName);
            BoatData.ListFreeSpots(harbour);
        }
    }
}



















//static List<Boat> AssignBoatsNewSpots(List<Boat> boats, Harbour harbour)
//{
//    .TryAddFromBottom(boats, harbour);
//    return boats.Where(boat => boat.AssignedSpotAtHarbour == null).ToList();
//}

//static List<Boat> AssignRowboatsNewSpots(List<Boat> rowboats, Harbour harbour)
//{
//    rowboats = assign.GiveRowboatUnassignedSpot(rowboats, harbour);
//    return rowboats;
//}
//private static void TryToAddNewBoats(WaitingBoats waitingBoats, Harbour leftHarbour, Harbour rightHarbour)
//{
//    foreach (Boat boat in waitingBoats.Waiting.OrderBy(boat => boat.SizeInSpots))
//    {
//        if (leftHarbour.CheckOldSpots(boat))
//        {
//            leftHarbour.Port.Add(boat);
//            leftHarbour.UpdateSpots(boat);
//            leftHarbour.UpdateLargestSpot();
//        }
//        else if (rightHarbour.CheckOldSpots(boat))
//        {
//            rightHarbour.Port.Add(boat);
//            rightHarbour.UpdateSpots(boat);
//            rightHarbour.UpdateLargestSpot();
//        }
//        else if (TryToDock(boat, leftHarbour))
//        {
//            leftHarbour.Port.Add(boat);
//            leftHarbour.UpdateSpots(boat);
//            leftHarbour.UpdateLargestSpot();
//        }
//        else if (TryToDock(boat, rightHarbour))
//        {
//            rightHarbour.Port.Add(boat);
//            rightHarbour.UpdateSpots(boat);
//            rightHarbour.UpdateLargestSpot();
//        }
//        else
//        {
//            UpdateData(leftHarbour, "LeftHarbour");
//            UpdateData(rightHarbour, "RightHarbour");

//            //MessageBox.Show($"{boat.GetBoatType()} {boat.ModelID} couldn't get a spot! \n It needed {boat.SizeInSpots} spots!");
//            boatsRejected++;
//        }
//    }
//    if (waitingBoats.Waiting.Any())
//    {
//        waitingBoats.Waiting.Clear();
//    }
//}
