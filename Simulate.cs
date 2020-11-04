using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Windows;
using System.CodeDom;
using System.Windows.Threading;

namespace HamnSimulering
{
    class Simulate
    {
        //public static Harbour harbour = new Harbour();
        public static Harbour harbour = new Harbour();
        public static List<Boat> waitingBoats = new List<Boat>();

        public static int BoatsRejected { get; set; }
        public static int BoatsAccepted { get; set; }
        public static int DaysPassed { get; set; }
        public static int BoatsPerDay { get; set; }






        public static void OneDay(bool isAuto)
        {

            //vilka båtar som inte har fått en plats tilldelad
            Func<Boat, bool> notAssigned = (boat) => boat.AssignedSpot == null;

            //visar en ruta med vilka båtar som inte fick plats
            Action<List<Boat>> showWho = (boats) => {
                string info = "";
                    foreach (Boat boat in boats)
                    {
                        info += $"{boat.GetBoatType()} {boat.ModelID} (size {boat.Size} spots) did not fit!\n";
                    }
                    MessageBox.Show(info);
                
            };

            



            //lägger till en dag på varje båt om det finns några vid kajen
            harbour.AddOneDay();

            //hämtar positionerna från de båtarna som lämnat hamnen nyligen
            harbour.TryReusingPortSpots(waitingBoats);

            //försöker dela ut de platser GetPositions() hämtade
            //harbour.TestSpotsFromRemovedBoats(waitingBoats);

            //lägger till roddbåtar brevid andra roddbåtar
            harbour.PlaceRowboatsOnOccupiedSpots(waitingBoats);

            //lägger till roddbåtar på nya platser
            harbour.AddRowboatsOnEmptySpot(waitingBoats);

            //sedan de andra båtarna
            harbour.GiveBoatsUnassignedPortSpots(waitingBoats);
            

            ////rensar listorna med positioner
            //harbour.ClearPositions();

            //skriver ut lediga platser
            harbour.ListFreeSpots();


            //vilka båtar som inte har en tilldelad plats

            int rejectedBoats = waitingBoats.Count(notAssigned);
            BoatsRejected += rejectedBoats;

            //vilka båtar som har en tilldelad plats
            BoatsAccepted += BoatsPerDay - rejectedBoats;

            //visa vilka båtar som inte fick plats om inte automatic är true
            if (!isAuto && rejectedBoats > 0)
            {
                showWho(waitingBoats.Where(notAssigned).ToList());
            }

            //rensar väntande båtar och lägger till nya
            waitingBoats.Clear();
            AddBoats();


            //uppdaterar datatabellen för listan av väntande båtar.
            BoatData.UpdateVisitors(waitingBoats);

            harbour.ListFreeSpots();

            DaysPassed++;

        }

        public static void ClearWaiting()
        {
            waitingBoats.Clear();
        }


        public static void AddBoats()
        {

            //lägger till eller tar bort båtar om inte antalet båtar i listan
            //är samma som båtar per dag
            while (waitingBoats.Count != BoatsPerDay)
            {
                if (waitingBoats.Count < BoatsPerDay)
                {
                    waitingBoats.Add(Generate.RandomBoat());
                }
                else
                {
                    var boat = waitingBoats.LastOrDefault();
                    waitingBoats.Remove(boat);
                }
            }
            //och uppdaterar tabellen
            BoatData.UpdateVisitors(waitingBoats);
        }

        public static void AddToWaiting(int ammount)
        {
            BoatsPerDay = ammount;
            AddBoats();
        }

    }
}


























//public static void UpdateData(Port port)
//{
//    BoatData.ListFreeSpotsPort(port);

//    BoatData.UpdatePort(port);
//}







//ListEmptySpots(leftHarbour);
//ListEmptySpots(rightHarbour);
//List<Boat> waiting = waitingBoats.Waiting;

//List<Boat> rowboatsWaiting = waiting.Where(boat => boat is Rowboat).ToList();
//List<Boat> otherBoatsWaiting = waiting.Where(boat => !(boat is Rowboat)).OrderBy(boat => boat.SizeInSpots).ToList();
//assignLeft = new AssignHarbourSpot(leftHarbour);
//assignLeft.GetPositions();
//assignRight = new AssignHarbourSpot(rightHarbour);
//assignRight.GetPositions();


//assignLeft.PlaceRowboat(rowboatsWaiting);
//assignRight.PlaceRowboat(rowboatsWaiting);

//assignLeft.MergePositions();
//assignRight.MergePositions();

//assignLeft.GiveRowboatUnassignedSpot(rowboatsWaiting);
//assignRight.GiveRowboatUnassignedSpot(rowboatsWaiting);

//assignLeft.GivePositions(otherBoatsWaiting);
//assignRight.GivePositions(otherBoatsWaiting);

//assignLeft.TryAddFromBottom(otherBoatsWaiting);

//assignRight.TryAddFromBottom(otherBoatsWaiting);



//AddOneDay(leftHarbour);
//AddOneDay(rightHarbour);

//TryToAddNewBoats(waitingBoats, leftHarbour, rightHarbour);
////ListEmptySpots(leftHarbour);
////ListEmptySpots(rightHarbour);


//static List<Boat> TryPlacingBoats(Harbour harbour, WaitingBoats waitingBoats, AssignHarbourSpot assign)
//{
//
//    List<Boat> leftoverBoats = assign.FindBestPlacement(harbour, waitingBoats);
//
//
//    return leftoverBoats;
//
//}


//private static bool TryToDock(Boat boat, Harbour harbour)
//{
//    return harbour.HasFreeSpots(boat);
//}



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
