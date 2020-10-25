using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace HamnSimulering
{
    class Simulate
    {
        public static int boatsRejected = 0;
        public static int daysPassed = 0;
        public static int numberOfNewBoats = 5;
        public static void OneDay(WaitingBoats waitingBoats, Harbour leftHarbour, Harbour rightHarbour)
        {

            AddOneDay(leftHarbour, "LeftHarbour");
            AddOneDay(rightHarbour, "RightHarbour");

            TryToAddNewBoats(waitingBoats, leftHarbour, rightHarbour);
            AddToWaiting(waitingBoats);


            ListEmptySpots(leftHarbour, "LeftHarbour");
            ListEmptySpots(rightHarbour, "RightHarbour");


            UpdateDataTable(leftHarbour, "LeftHarbour");
            UpdateDataTable(rightHarbour, "RightHarbour");
            BoatData.UpdateVisitors(waitingBoats.Waiting);

            leftHarbour.UpdateLargestSpot();
            rightHarbour.UpdateLargestSpot();

            daysPassed++;
        }
        public static void AddOneDay(Harbour harbour, string table)
        {
            harbour.RemoveBoats(table);


            if (harbour.Port.Any())
            {
                foreach (Boat boat in harbour.Port)
                {
                    boat.DaysSpentAtHarbour++;
                }
            }
        }

        private static void TryToAddNewBoats(WaitingBoats waitingBoats, Harbour leftHarbour, Harbour rightHarbour)
        {
            foreach (Boat boat in waitingBoats.Waiting)
            {
                if (TryToDock(boat, leftHarbour, out int[] assignedSpot))
                {
                    boat.AssignedSpotAtHarbour = assignedSpot;
                    leftHarbour.Port.Add(boat);
                    leftHarbour.UpdateSpots(boat);
                    leftHarbour.UpdateLargestSpot();
                }
                else if (TryToDock(boat, rightHarbour, out assignedSpot))
                {
                    boat.AssignedSpotAtHarbour = assignedSpot;
                    rightHarbour.Port.Add(boat);
                    rightHarbour.UpdateSpots(boat);
                    rightHarbour.UpdateLargestSpot();
                }
                else
                {
                    boatsRejected++;
                }
            }
            if (waitingBoats.Waiting.Any())
            {
                waitingBoats.Waiting.Clear();
            }
        }
        private static bool TryToDock(Boat boat, Harbour harbour, out int[] assignedSpot)
        {
            return harbour.HasFreeSpots(boat, out assignedSpot);
        }

        public static void AddToWaiting(WaitingBoats waitingBoats)
        {
            waitingBoats.AddBoats(numberOfNewBoats);
            BoatData.UpdateVisitors(waitingBoats.Waiting);
        }


        public static void ListEmptySpots(Harbour harbour, string table)
        {
            BoatData.ListFreeSpots(harbour.IsCurrentSpotTaken, table);
        }
        public static void UpdateDataTable(Harbour harbour, string table)
        {
            BoatData.UpdateHarbour(harbour.Port, table);
        }
    }
}
