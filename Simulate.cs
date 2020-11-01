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
        public static Harbour harbour = new Harbour();
        public static List<Boat> waitingBoats = new List<Boat>();

        DispatcherTimer automaticTimer;
        public static int boatsRejected = 0;
        public static int boatsAccepted = 0;
        public static int daysPassed = 0;
        public static int boatsPerDay = 5;
        static List<Boat> rowboatsWaiting;
        static List<Boat> otherBoatsWaiting;
        static Func<Boat, bool> NotAssigned => (boat) => boat.AssignedSpot == null;
        static Action<List<Boat>> ShowWho => (boats) => {
            string info = "";
            if (boats.Any())
            {
                foreach (Boat boat in boats)
                {
                    info += $"{boat.GetBoatType()} {boat.ModelID} (size {boat.Size} spots) did not fit!\n";
                }
                MessageBox.Show(info);
            }
        };



        public static void SetAutoTimerSpeed(int seconds, int milliseconds)
        {

        }

        public static void OneDay()
        {
            rowboatsWaiting = waitingBoats.Where(boat => boat is Rowboat).ToList();
            otherBoatsWaiting = waitingBoats.Where(boat => !(boat is Rowboat)).OrderBy(boat => boat.Size).ToList();
            



            //lägger till en dag på varje båt om det finns några vid kajen
            harbour.AddOneDay();


            harbour.GetPositions();


            harbour.TestOldSpots(waitingBoats);

            harbour.AddRowBoats(rowboatsWaiting);
            
            harbour.AddFromBottom(otherBoatsWaiting);
            
            harbour.ClearPositions();



            boatsRejected += waitingBoats.Count(NotAssigned);
            boatsAccepted += boatsPerDay - waitingBoats.Count(NotAssigned);


            //visa vilka båtar som inte fick plats om inte automatic är true
            if (!MainWindow.automatic)
            {
                ShowWho(waitingBoats.Where(NotAssigned).ToList());
            }

            //rensar väntande båtar och lägger till nya
            waitingBoats.Clear();
            AddToWaiting();

            ///////////////////////////////här ändrade jag updatedata commented !!!
            //uppdatera datatabellerna.
            BoatData.UpdateVisitors(waitingBoats);



            //UpdateData(leftHarbour);
            //UpdateData(rightHarbour);


            daysPassed++;


        }




        public static void ClearWaiting()
        {
            waitingBoats.Clear();
        }


        

        public static void AddToWaiting(int? ammount=null)
        {
            if (ammount != null)
            {
                boatsPerDay = (int)ammount;
            }
            //lägger till eller tar bort båtar om inte antalet båtar i listan
            //är samma som båtar per dag
            while(waitingBoats.Count != boatsPerDay)
            {
                if(waitingBoats.Count < boatsPerDay)
                {

                    AddBoats(boatsPerDay);
                }
                else
                {
                    var boat = waitingBoats.LastOrDefault();
                    waitingBoats.Remove(boat);
                }
            }
            //och uppdaterar tabellen när den är p
            BoatData.UpdateVisitors(waitingBoats);
        }

        public static void AddBoats(int numberOfBoats)
        {
            
            for (int i = 1; i <= numberOfBoats; i++)
            {
                waitingBoats.Add(Generate.RandomBoat());
            }
        }
        public static void UpdateData(Port port)
        {
            BoatData.ListFreeSpotsPort(port);

            BoatData.UpdatePort(port);
            //BoatData.UpdateHarbour(harbour);
            //BoatData.ListFreeSpots(harbour);
        }
    }
}











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
