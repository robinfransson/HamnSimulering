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
        static List<AssignHarbourSpot> assigners = new List<AssignHarbourSpot>();
        public static List<Boat> waitingBoats = new List<Boat>();

        public static int boatsRejected = 0;
        public static int boatsAccepted = 0;
        public static int daysPassed = 0;
        public static int numberOfNewBoats = 5;
        static List<Boat> rowboatsWaiting;
        static List<Boat> otherBoatsWaiting;
        static Func<Boat, bool> NotAssigned => (boat) => boat.AssignedSpotAtHarbour == null;
        static Action<List<Boat>> ShowWho => (boats) => {
            string info = "";
            if (boats.Any())
            {
                foreach (Boat boat in boats)
                {
                    info += $"{boat.GetBoatType()} {boat.ModelID} ({boat.SizeInSpots}) did not fit!\n";
                }
                MessageBox.Show(info);
            }
            };

        public static void SetupAssigner(Harbour harbour, bool reset=false)
        {
            if (!reset)
            {
                assigners.Add(new AssignHarbourSpot(harbour));
            }
            else
            {
                var assigner = assigners.FirstOrDefault(assigner => assigner.AssignedHarbour == harbour.HarbourName);
                if(assigner != null)
                {
                    assigners.Remove(assigner);
                    assigners.Add(new AssignHarbourSpot(harbour));
                }
                else
                {
                    throw new IndexOutOfRangeException();
                }
            }
        }


        static void GetPositions()
        {
            foreach (AssignHarbourSpot assigner in assigners)
            {
                assigner.GetPositions();
            }
        }

       //static void GiveUsedSpots(List<Boat> boats)
       //{
       //    foreach (AssignHarbourSpot assigner in assigners)
       //    {
       //        assigner.GivePositions__fix(boats);
       //    }
       //    
       //}

        static void TryAssignRowboat()
        {
            foreach (AssignHarbourSpot assigner in assigners)
            {
                //assigner.PlaceRowboat(rowboatsWaiting);
                assigner.GiveRowboatUnassignedSpot(rowboatsWaiting);
            }
        }

        static void CombineUsedSpots()
        {
            foreach (AssignHarbourSpot assigner in assigners)
            {
                assigner.MergePositions();
            }
        }

        static void TryUsedSpots()
        {
            foreach (AssignHarbourSpot assigner in assigners)
            {
                assigner.GivePositions(otherBoatsWaiting);
            }
        }

        static void AddFromBottom()
        {
            foreach (AssignHarbourSpot assigner in assigners)
            {
                assigner.TryAddFromBottom(otherBoatsWaiting);
            }

        }


        static void TestOldSpots()
        {
            foreach (AssignHarbourSpot assigner in assigners)
            {
                assigner.TryOldSpots(waitingBoats);
            }
        }


        static void AddRowBoats()
        {
            foreach(AssignHarbourSpot assigner in assigners)
            {

                assigner.GiveRowboatUnassignedSpot(rowboatsWaiting);
            }
        }

        public static void OneDay(Harbour leftHarbour, Harbour rightHarbour)
        {
            rowboatsWaiting = waitingBoats.Where(boat => boat is Rowboat).ToList();
            otherBoatsWaiting = waitingBoats.Where(boat => !(boat is Rowboat)).OrderBy(boat => boat.SizeInSpots).ToList();
            waitingBoats.OrderBy(boat => boat.SizeInSpots);
            //lägger till en dag på varje båt om det finns några i hamnen
            AddOneDay(leftHarbour);
            AddOneDay(rightHarbour);

            //min hamn logik
            //GetPositions();
            //TryAssignRowboat();
            //CombineUsedSpots();
            //TryUsedSpots();
            //AddFromBottom();





            GetPositions();
            CombineUsedSpots();
            TestOldSpots();
            AddRowBoats();
            AddFromBottom();



            //visa vilka båtar som inte fick plats och vilka det var
            boatsRejected += waitingBoats.Count(NotAssigned);
            boatsAccepted += numberOfNewBoats - waitingBoats.Count(NotAssigned);
            if (!MainWindow.automatic)
            {
                ShowWho(waitingBoats.Where(boat => boat.AssignedSpotAtHarbour == null).ToList());
            }


            waitingBoats.Clear();
            AddToWaiting();


            //uppdatera datatabellerna
            BoatData.UpdateVisitors(waitingBoats);
            UpdateData(leftHarbour);
            UpdateData(rightHarbour);


            daysPassed++;




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

        public static void AddToWaiting()
        {
            AddBoats(numberOfNewBoats);
            BoatData.UpdateVisitors(waitingBoats);
        }

        public static void AddBoats(int numberOfBoats)
        {
            Func<Boat> generateBoat = new Func<Boat>(() => Generate.RandomBoat());
            for (int i = 1; i <= numberOfBoats; i++)
            {
                waitingBoats.Add(generateBoat());
            }
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
