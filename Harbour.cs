using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Windows;
using System.Windows.Media.Media3D;

namespace HamnSimulering
{
    class Harbour
    {

        //List<Position> mergedPositions = new List<Position>();


        List<Port> Ports = new List<Port>();


        Func<Boat, bool> boatIsRowboat = (boat) => boat is Rowboat;
        Func<Boat, int, bool> anotherBoatOnSameSpot = (boat, spot) => boat.AssignedSpot[0] == spot;
        Func<Boat, int, bool> anotherRowboatOnSameSpot = (boat, spot) => boat.AssignedSpot[0] == spot && boat is Rowboat;
        Func<Boat, bool> notAssigned = (boat) => boat.AssignedSpot == null;
        Func<Rowboat, bool> notAssignedRowboat = (rowboat) => rowboat.AssignedSpot == null;
        Func<Boat, bool> isSmallBoat = (boat) => boat.AssignedSpot.Length == 1;
        Func<int[], int> firstSpot = (spots) => spots[0];
        Func<Boat, float> boatSize = (boat) => boat.Size;



        //hur stor platsen är
        Func<int[], int> portSpotSize = (values) =>
        {
            return values[1] - values[0];
        };
        /// <summary>
        /// här kollas hamnen om båten får plats på den plats som loopas
        /// </summary>
        Func<Port, int, int, int, bool> allSpotsFree = (port, start, max, boatSize) =>
        {
            int end = start + boatSize;

            if (end > port.OccupiedSpots.GetUpperBound(0) || end > max)
            {
                //om båtens storlek plus startvärdet är större än hamnens kapacitet
                //eller om båten är för stor för platsen som ska tilldelas
                return false;
            }

            for (int i = start; i <= end; i++)
            {
                //om hamnplatsen är upptagen
                if (port.OccupiedSpots[i] == true)
                {
                    return false;
                }
            }
            //annars får båten plats
            return true;
        };



        public void TryReusingPortSpots(List<Boat> boats)
        {
            foreach (Port port in Ports)
            {
                if (port.RemovedBoats.Any())
                {
                    List<int[]> spotsToTry = FetchPositions(port);
                    TryPortSpots(boats, port, spotsToTry);
                }
            }
        }

        public void GiveBoatsUnassignedPortSpots(List<Boat> boats)
        {
            if (boats.Any(notAssigned))
            {
                foreach (Port port in Ports)
                {
                    GiveBoatSemiEfficientParking(boats, port);
                }
            }

        }



        /// <summary>
        /// Placerar ut roddbåtar brevid varandra 
        /// </summary>
        /// <param name="boats"></param>
        public void PlaceRowboatsOnOccupiedSpots(List<Boat> boats)
        {
            foreach (Port port in Ports)
            {
                if (boats.Any(boat => boat is Rowboat && notAssigned(boat)))
                {
                    PlaceRowboatNextToAnother(boats, port);
                }
            }
        }
        public void AddRowboatsOnEmptySpot(List<Boat> boats)
        {
            foreach (Port port in Ports)
            {
                if (boats.Any(boat => boat is Rowboat && notAssigned(boat)))
                {
                    GiveRowboatUnassignedSpot(boats, port);
                }
            }
        }



        /// <summary>
        /// Lägger till en dag på varje båt i hamnen
        /// </summary>
        public void AddOneDay()
        {
            foreach (Port port in Ports)
            {
                port.CheckTimeOnBoats();


                if (port.Boats.Any())
                {
                    foreach (Boat boat in port.Boats)
                    {
                        boat.DaysSpentAtHarbour++;
                        BoatData.AddTimeToBoat(boat, port.PortName);
                    }
                }
            }
        }
        /// <summary>
        /// Listar alla lediga platser i varje hamn
        /// </summary>
        public void ListFreeSpots()
        {
            foreach(Port port in Ports)
            {
                BoatData.ListAllFreeSpotsInPort(port);
            }
        }

        /// <summary>
        /// Lägger till en kaj i listan med kajer
        /// </summary>
        /// <param name="p"></param>
        public void AddPort(Port p)
        {
            Ports.Add(p);
        }

        /// <summary>
        /// Tar bort den gamla kajen som har samma PortName och lägger in den nya
        /// </summary>
        /// <param name="newPort"></param>
        public void ReplacePort(Port newPort)
        {
            Port portToRemove = Ports.Where(port => port.PortName == newPort.PortName)
                                     .FirstOrDefault();

            Ports.Remove(portToRemove);
            Ports.Add(newPort);
        }



        private List<int[]> FetchPositions(Port port)
        {
            List<int[]> positions = port.RemovedBoats.Where(boat => !isSmallBoat(boat))
                                                     .Select(value => value.AssignedSpot)
                                                     .ToList();

            List<int[]> smallBoatPositions = port.RemovedBoats
                                            .Where(boat => isSmallBoat(boat))
                                            .Select(boat => boat.AssignedSpot)
                                            .ToList();


            //koden är uppbyggd så att alla int[] antas ha 2 index
            foreach (int[] position in smallBoatPositions)
            {
                positions.Add(new int[2] { position[0], position[0] });
            }

            //tar bort om det finns mer än 1 av samma 
            positions = positions.GroupBy(position => position[0])
                                 .Select(group => group.FirstOrDefault()) //väljer det första värdet i varje grupp, (sorterar ut dubletter (roddbåtar dvs))
                                 .OrderBy(firstSpot)// sorterar dom på 1a platsens värde så de platser som ligger nära varandra hamnar brevid varandra
                                 .ToList();

            return MergePositions(positions, port); 
        }





        private List<int[]> MergePositions(List<int[]> positions, Port port)
        {

            List<int[]> mergedSpots = new List<int[]>();

            mergedSpots.Add(positions.FirstOrDefault());


            //skippar det första värdet i listan för det redan är inlagt i den andra, så den inte jämför samma värden med varandra
            positions = positions.Skip(1)
                                 .ToList();

            foreach (int[] currentPosition in positions) //skippar första
            {
                int[] previousPosition = mergedSpots.LastOrDefault(); //hämtar det föregående värdet
                int[] nextValueToAdd;


                if (previousPosition[1] + 1 == currentPosition[0])//kollar om det går att slå ihop dom
                {
                    nextValueToAdd = CombinePositions(previousPosition[0], currentPosition[1]);
                }
                else
                {
                    nextValueToAdd = currentPosition;
                }

                mergedSpots.Add(nextValueToAdd);


                //om det första värdet i båda arrayerna är samma
                //har dom slagits ihop och då ska det föregående värdet
                //inte vara kvar i listan
                bool spotWasMerged = previousPosition[0] == nextValueToAdd[0];

                if (spotWasMerged)
                {
                    mergedSpots.Remove(previousPosition);
                }
            }

            return mergedSpots;
        }

        int[] CombinePositions(int startValue, int endValue)
        {
            return new int[2] { startValue, endValue };
        }


        public void TryPortSpots(List<Boat> boats, Port port, List<int[]> spots)
        {
            var positionsToCheck = spots.OrderBy(portSpotSize)
                                        .ToList();

            foreach (int[] position in positionsToCheck)
            {
                int max = position[1];
                List<Boat> boatsToCheck = boats.Where(notAssigned).OrderBy(boatSize).ToList();

                foreach (Boat boat in boatsToCheck)
                {
                    int currentSpot = position[0];

                    if (currentSpot > max)
                    {
                        break;
                    }
                    //array index 0, därför -1
                    int boatSize = (int)boat.Size - 1;

                    //eftersom listan med nya båtar är sorterade efter storleksordning kan man anta att
                    //nästa båt inte heller får plats, därav break
                    bool boatIsTooBig = !allSpotsFree(port, currentSpot, max, boatSize) || currentSpot + boatSize > max;

                    if (boatIsTooBig)
                    {
                        break;
                    }
                    bool spotTaken = port.OccupiedSpots[currentSpot];

                    int rowboatsOnSpot = port.Boats.Count(boat => anotherBoatOnSameSpot(boat, currentSpot) && boatIsRowboat(boat));
                    int rowboatsLeftToAssign = boatsToCheck.Count(boat => boatIsRowboat(boat) && notAssigned(boat));
                    bool rowboatCanPark = boatIsRowboat(boat) && (rowboatsOnSpot == 1 || !spotTaken);


                    if (rowboatCanPark)
                    {
                        //hur många roddbåtar som det är kvar och hur många det är på platsen, är det 
                        //redan en roddbåt på platsen så kommer inte det få plats en till, då plussas position[0] på med 1, så får nästa båt försöka klämma sig in
                        //från nästa plats och är det här den sista som inte har en hamnplats ska samma sak ske
                        position[0] += rowboatsOnSpot == 1 || rowboatsLeftToAssign == 1 ? 1 : 0;
                        boat.AssignedSpot = new int[1] { currentSpot };
                        port.AddBoat(boat);
                        continue;
                    }

                    //är platsen upptagen ska startpositionen för nästa båt bli 1 högre, 
                    else if (spotTaken)
                    {
                        position[0]++;
                        continue;
                    }


                    //special regler för motorbåt, den ska bara få 1 hamnplats, och behöver inte en array med 2 index
                    else if (boat is Motorboat)
                    {
                        boat.AssignedSpot = new int[1] { currentSpot };
                        position[0]++;
                        port.AddBoat(boat);
                        continue;
                    }


                    //annars är det en båt som ska ta mer än 1 plats
                    else
                    {
                        //och nästa båt ska börja kolla efter en plats
                        //från den sista båtens andra position + 1
                        //dvs om en båt fick plats {22,24} ska nästa börja kolla från 25
                        int end = currentSpot + boatSize;
                        int newMinimum = currentSpot + boatSize + 1;
                        boat.AssignedSpot = new int[2] { currentSpot, end };
                        position[0] = newMinimum;
                        port.AddBoat(boat);
                        continue;

                    }
                }
            }
        }


        public void PlaceRowboatNextToAnother(List<Boat> boats, Port port)
        {
            List<Rowboat> rowboatsToCheck = boats.Where(notAssigned).OfType<Rowboat>().ToList();
            List<Rowboat> dockedRowboatsWithSpace = port.Boats.Where(boatIsRowboat)
                                                    .OfType<Rowboat>()
                                                    .GroupBy(boat => boat.AssignedSpot[0])
                                                    .Where(group => group.Count() < 2)//där det är mindre än 2 på samma plats kan nästa få hamna
                                                    .Select(boatGroup => boatGroup.FirstOrDefault()) //väljer den första i varje grupp
                                                    .ToList();


            //så länge det finns roddbåtar att placera ut och roddbåtar som har plats för en till
            while (dockedRowboatsWithSpace.Any() && rowboatsToCheck.Any())
            {
                foreach(Rowboat rowboat in rowboatsToCheck)
                {
                    Rowboat otherRowboat = dockedRowboatsWithSpace.FirstOrDefault();
                    if(otherRowboat == null) //finns det ingen roddbåt i listan ska loopen avslutas
                    {
                        break;
                    }
                    int[] otherRowboatsSpot = otherRowboat.AssignedSpot; //ger den nya roddbåten samma position som den som redan är ute i kajen
                    rowboat.AssignedSpot = otherRowboatsSpot;
                    port.AddBoat(rowboat);

                    dockedRowboatsWithSpace.Remove(otherRowboat); //tar bort den ur listan så inte nästa får samma plats
                }
                rowboatsToCheck = rowboatsToCheck.Where(notAssignedRowboat).ToList();

            }
        }

        /// <summary>
        /// Så länge det finns plats och roddbåtar kommer loopen att köras tills den hittar en plats
        /// </summary>
        /// <param name="rowboats"></param>
        /// <param name="port"></param>
        public void GiveRowboatUnassignedSpot(List<Boat> boats, Port port)
        {
            int currentSpot = 0;
            List<Rowboat> rowboatsToCheck = boats.OfType<Rowboat>().Where(notAssignedRowboat).ToList();
            
            //medans det finns plats kvar och roddbåtar i listan
            //ska loopen köras
            while (port.SpotsLeft > 0f && rowboatsToCheck.Any())
            {
                foreach (Rowboat rowboat in rowboatsToCheck)
                {
                    int rowboatsOnSpot = port.Boats.Count(boat => anotherRowboatOnSameSpot(boat, currentSpot));
                    bool currentSpotTaken = port.OccupiedSpots[currentSpot];

                    //för att en roddbåt ska kunna parkera måste det antingen vara exakt 1 där eller så ska platsen vara ledig
                    bool rowboatCanPark = rowboatsOnSpot == 1 || !currentSpotTaken;


                    if (rowboatCanPark)
                    {
                        rowboat.AssignedSpot = new int[1] { currentSpot };
                        port.AddBoat(rowboat);
                        continue;
                    }
                    else if(rowboatsOnSpot == 2 || currentSpotTaken)
                    {
                        break;
                    }
                }

                currentSpot++;
                rowboatsToCheck = rowboatsToCheck.Where(notAssignedRowboat).ToList();
                if(currentSpot == port.OccupiedSpots.GetUpperBound(0))
                {
                    break;
                }
            }
        }

        static int[] PortSpotArray(string spots)
        {

            string[] currentSpots = spots.Split(',').SkipLast(1).ToArray(); //skippar sista index för att jag vet att det kommer vara en tom stäng

            int lastIndex = currentSpots.GetUpperBound(0);
            int start = int.Parse(currentSpots[0]);
            int end = int.Parse(currentSpots[lastIndex]);

            int[] portSpot = new int[2] { start, end }; //returnerar arrayen med endast start och slut positionen
            return portSpot;
        }

        static List<int[]> FreePortSpots(Port port)
        {
            List<int[]> freeSpots = new List<int[]>();
            string spotsFound = "";
            for (int i = 0; i <= port.OccupiedSpots.GetUpperBound(0); i++)
            {
                bool spotTaken = port.OccupiedSpots[i];
                if (spotTaken)
                {
                    spotsFound += ";"; //separerar strängen med ett semicolon så jag kan veta vart den lediga platsen tar slut
                }
                else
                {
                    spotsFound += i + ",";
                }
            }
            string[] spots = spotsFound.Split(';');
            foreach (string spot in spots)
            {
                if (spot != "")
                {
                    freeSpots.Add(PortSpotArray(spot));
                }
            }
            return freeSpots;

        }

        public void GiveBoatSemiEfficientParking(List<Boat> boats, Port port)
        {
            List<int[]> freeSpots = FreePortSpots(port);
            //List<int[]> freeSpots = FreePortSpots2(port);
            List<Boat> boatsToCheck = boats.Where(notAssigned).OrderBy(boatSize).ToList();
            foreach (Boat boat in boatsToCheck)
            {
                //t.ex plats 1 till 3 för en katamaran 3-1 = 2 katamaranens storlek = 3 därför -1
                int[] givenPortSpot = freeSpots.Where(spot => spot[1] - spot[0] == boat.Size-1).FirstOrDefault();
                if(givenPortSpot == null)//finns det ingen plats som matchar exakt ska nästa kollas
                {
                    continue;
                }
                else
                {
                    //motorbåten ska endast ha 1 plats listad
                    boat.AssignedSpot = boat is Motorboat ? new int[1] { givenPortSpot[0] } : givenPortSpot;
                    port.AddBoat(boat);
                    freeSpots.Remove(givenPortSpot); //tar bort platsen ur listan
                    continue;
                }
            }
            boatsToCheck = boats.Where(notAssigned).ToList();
            if (freeSpots.Any() && boatsToCheck.Any())
            {
                TryPortSpots(boatsToCheck, port, freeSpots);
            }

            
        }



    }
}














































//public void GetPositionsFromRemovedBoats()
//{
//    foreach (Port port in Ports)
//    {
//        if (port.RemovedBoats.Any())
//        {
//            FetchPositions(port);
//        }
//    }
//}

//public void TestSpotsFromRemovedBoats(List<Boat> boats)
//{
//    foreach (Port port in Ports)
//    {
//        if (boats.Any())
//        {
//            TryOldSpots(boats, port);
//        }
//    }
//}

//public void FetchPositions(Port port)
//{
//    List<int[]> positions = port.RemovedBoats.Where(boat => !isSmallBoat(boat))
//                                             .Select(value => value.AssignedSpot)
//                                             .ToList();

//    List<int[]> smallBoatPositions = port.RemovedBoats
//                                    .Where(boat => isSmallBoat(boat))
//                                    .Select(boat => boat.AssignedSpot)
//                                    .ToList();


//    //koden är uppbyggd så att alla int[] antas ha 2 index
//    foreach (int[] position in smallBoatPositions)
//    {
//        positions.Add(new int[2] { position[0], position[0] });
//    }

//    //tar bort om det finns mer än 1 av samma 
//    positions = positions.GroupBy(position => position[0])
//                         .Select(group => group.FirstOrDefault()) //väljer det första värdet i varje grupp, (sorterar ut dubletter (roddbåtar dvs))
//                         .OrderBy(firstSpot)// sorterar dom på 1a platsens värde så de platser som ligger nära varandra hamnar brevid varandra
//                         .ToList();



//    MergePositions(positions, port);
//}





//void MergePositions(List<int[]> positions, Port port)
//{

//    List<int[]> mergedSpots = new List<int[]>();

//    mergedSpots.Add(positions.FirstOrDefault());


//    //skippar det första värdet i listan för det redan är inlagt i den andra, så den inte jämför samma värden med varandra
//    positions = positions.Skip(1)
//                         .ToList();

//    foreach (int[] currentPosition in positions) //skippar första
//    {

//        int[] previousPosition = mergedSpots.LastOrDefault(); //hämtar det föregående värdet
//        int[] nextValueToAdd;


//        if (previousPosition[1] + 1 == currentPosition[0])//kollar om det går att slå ihop dom
//        {
//            nextValueToAdd = CombinePositions(previousPosition[0], currentPosition[1]);
//        }
//        else
//        {
//            nextValueToAdd = currentPosition;
//        }

//        mergedSpots.Add(nextValueToAdd);


//        //om det första värdet i båda arrayerna är samma
//        //har dom slagits ihop och då ska det föregående värdet
//        //inte vara kvar i listan
//        bool spotWasMerged = previousPosition[0] == nextValueToAdd[0];

//        if (spotWasMerged)
//        {
//            mergedSpots.Remove(previousPosition);
//        }
//    }

//    //loopar igenom listan med de positioner som eventuellt har slagits ihop, och lägger till port.PortName
//    //för att kunna se vart ifrån platserna kom
//    foreach (int[] mergedSpot in mergedSpots)
//    {
//        mergedPositions.Add(new Position(mergedSpot, port.PortName));
//    }
//}

//public void TryOldSpots(List<Boat> boats, Port port)
//{


//    var positionsToCheck = mergedPositions.Where(pos => pos.Port == port.PortName)
//                                                 .Select(pos => pos.Spot)
//                                                 .ToList();

//    TryPortSpots(boats, port, positionsToCheck);
//}
//public void MergeOldAndFreeSpots(List<Boat> boats, Port port)
//{
//    List<int[]> spotsToCheck = new List<int[]>();
//    List<int[]> positions = port.RemovedBoats.Where(boat => !isSmallBoat(boat))
//                                              .Select(value => value.AssignedSpot)
//                                              .ToList();

//    List<int[]> smallBoatPositions = port.RemovedBoats
//                                    .Where(boat => isSmallBoat(boat))
//                                    .Select(boat => boat.AssignedSpot)
//                                    .ToList();


//    //koden är uppbyggd så att alla int[] antas ha 2 index
//    foreach (int[] position in smallBoatPositions)
//    {
//        positions.Add(new int[2] { position[0], position[0] });
//    }

//    positions = positions.GroupBy(position => position[0])
//                         .Select(group => group.FirstOrDefault()) //väljer det första värdet i varje grupp, (sorterar ut dubletter (roddbåtar dvs))
//                         .OrderBy(firstSpot)// sorterar dom på 1a platsens värde så de platser som ligger nära varandra hamnar brevid varandra
//                         .ToList();
//    foreach (int[] position in positions)
//    {
//        int start = position[0];
//        int end = position[1];
//        bool foundEnd = false;
//        bool foundStart = false;
//        while(!foundEnd && !foundStart)
//        {
//            if(!foundEnd)
//            {

//                bool doneStart = false;
//                int currentSpot = 
//                while(!doneStart)
//                {
//                    if(port.OccupiedSpots[i] || position.Any(pos => pos[1] ==)
//                    {
//                        position[0] = i;
//                    }
//                }
//            }
//        }
//    }


//        //tar bort om det finns mer än 1 av samma 

//}



///// <summary>
///// Lägger till båtarna nerifrån
///// </summary>
///// <param name="boats"></param>
///// <param name="harbour"></param>
///// <returns></returns>
//public void TryAddFromBottom(List<Boat> boats, Port port)
//{
//    List<Boat> boatsToCheck = boats.Where(notAssigned).ToList();
//    foreach (Boat boat in boatsToCheck)
//    {
//        int spotsToTake = (int)boat.Size;

//        int currentSpot = port.OccupiedSpots.GetUpperBound(0);//eftersom loopen börjar från slutet ska currentSpot vara arrayens max

//        int spotsFound = 0;

//        for (int i = currentSpot; i >= 0; i--)
//        {
//            bool spotTaken = port.OccupiedSpots[i];

//            //platsen var upptagen
//            if (spotTaken)
//            {
//                spotsFound = 0;
//                currentSpot--;
//                continue;
//            }
//            else
//            {
//                spotsFound++;
//            }


//            if (spotsFound == spotsToTake)
//            {
//                int start = currentSpot;
//                int end = (currentSpot + spotsToTake) - 1; //tar bort 1 för att 

//                //en motorbåt behöver endast en plats
//                boat.AssignedSpot = boat is Motorboat ? new int[1] { start } : new int[2] { start, end };
//                port.AddBoat(boat);
//                break;
//            }
//            currentSpot--;
//        }
//    }
//}

//List<int[]> FixPositions(List<int[]> positionsBeforeFix)
//{
//    List<int[]> fixedPositions = new List<int[]>();
//    foreach (var pos in positionsBeforeFix)
//    {
//        if (pos.Length < 2)
//        {
//            fixedPositions.Add(new int[2] { pos[0], pos[0] });
//        }
//        else
//        {
//            fixedPositions.Add(pos);
//        }
//    }
//    return fixedPositions;
//}
// public void GetPositions(Port p)
//{
//    //om första indexet är större än det andra har något blivit tokigt
//    Func<int[], bool> errorCheck = (pos) =>
//    {
//        return pos[0] > pos[1];
//    };

//    //AssignedSpotAtHarbour[0] är första platsen som båten tar upp, så platserna sorteras på den för att när dom ska försöka slås ihop
//    //hamnar de närliggande efter varandra i listan
//    List<int[]> pos = p.RemovedBoats
//                        .OrderBy(boat => boat.AssignedSpot[0])
//                        .Select(value => value.AssignedSpot)
//                        .ToList();



//    //ger alla båtars plats[] 2 index ->
//    removedBoatsPositions = FixPositions(pos, p);
//    MergePositions__fix(p);
//}

///// <summary>
///// "Lägger ihop" båtplatser om dom är brevid varandra<br />
///// exempel, {4,5} och {6,7} blir då {4,7} som läggs in i en lista.<br />
///// och nästa gång den loopar kommer den kolla om nästa array går<br />
///// att lägga ihop med den förra arrayen, gör det inte det åker den oförändrad i listan.<br />
///// (detta förutsätter att listan redan är sorterad)
///// </summary>
//void MergePositions__fix(Port p)
//{


//    List<int[]> position____fix = removedBoatsPositions.Where(pos => pos.Port == p.PortName).Select(pos => pos.Spot).ToList();
//    //räknar helt enkelt ut hur mycket större det ena talet är, för att sedan sortera dom i storleksordning


//    List<int[]> merged = new List<int[]>(); 

//    int[] previousPosition = null;
//    foreach (int[] position in position____fix)
//    {
//        //första gången den körs har den inget värde att jämföra med
//        //så jag sätter värdet prevValue till det första i listan med positioner
//        //och lägger in det i mergedPositions och fortsätter loopen
//        if (previousPosition is null)
//        {
//            previousPosition = position;
//            merged.Add(previousPosition);
//            continue;
//        }
//        previousPosition = merged.LastOrDefault();

//        int[] fixedValue = position;

//        //kollar om det går att slå ihop dom
//        if (previousPosition[1] + 1 == position[0])
//        {
//            fixedValue = CombinePositions(previousPosition[0], position[1]);
//            merged.Add(fixedValue);
//        }
//        else
//        {
//            merged.Add(position);
//        }

//        //om det första värdet i båda arrayerna är samma
//        //har dom slagits ihop och då ska det föregående värdet
//        //inte vara kvar i listan
//        if (previousPosition[0] == fixedValue[0])
//        {
//            merged.Remove(previousPosition);
//        }
//    }
//    foreach(int[] mergedposition__ in merged)
//    {

//        mergedRemovedPositions.Add(new Position(mergedposition__, p.PortName));
//    }
//}








//        void CouldNotGetASpot(Boat boat, string caller)
//        {
//            if (showMessages)
//            {
//                MessageBox.Show($"{boat.GetBoatType()} {boat.ModelID} could not get a spot!\n{caller} @ {harbour.HarbourName}");
//            }
//        }
//        /// <summary>
//        /// försöker lägga in båten från första platsen, fortsätter ner sista eller tills den hittar en plats, returnerar true om en plats hittas, false om en plats inte hittas
//        /// sätter även värdet på båtens AssignedSpotAtHarbour
//        /// </summary>
//        /// <param name="boats">listan med båtar</param>
//        /// <returns></returns>
//        private void TryAddFromTop(List<Boat> boats, Harbour harbour)
//        {
//            foreach (Boat boat in boats)
//            {
//                int spotsToTake = (int)boat.SizeInSpots;
//                int currentSpot = 0;
//                int spotsFound = 0;
//                foreach (bool spotTaken in harbour.SpotIsTaken)
//                {
//                    if (spotTaken)
//                    {
//                        spotsFound = 0;
//                        currentSpot++;
//                        continue;
//                    }
//                    else
//                    {
//                        spotsFound++;
//                    }


//                    if (spotsFound == spotsToTake)
//                    {
//                        int start = (currentSpot - spotsToTake);
//                        int end = currentSpot;
//                        boat.AssignedSpotAtHarbour = new int[2] { start, end };
//                        continue;
//                    }
//                }
//            }
//        }
//    }
//}

///// <summary>
///// försöker ge redan använda platser till de nya båtarna (utom roddbåt)
///// </summary>
///// <param name="newBoats">Listan med nya båtar</param>
//public void GivePositions(List<Boat> newBoats)
//{
//    foreach (int[] position in mergedPositions)
//    {

//        int max = position[1];

//        foreach (Boat boat in newBoats)
//        {
//            if (boat.AssignedSpotAtHarbour != null)
//            {
//                continue;
//            }
//            if (position[0] > max)
//            {
//                break;
//            }


//            int start = position[0];
//            if (harbour.SpotIsTaken[start])
//            {
//                position[0]++;
//                continue;
//            }


//            if (boat is Motorboat)
//            {
//                boat.AssignedSpotAtHarbour = new int[1] { start };
//                position[0]++;
//                harbour.AddBoat(boat);
//            }
//            else
//            {
//                int boatSize = (int)boat.SizeInSpots - 1;

//                if (start + boatSize > max)
//                {
//                    break;
//                }
//                else
//                {
//                    int newMinimum = start + boatSize;
//                    boat.AssignedSpotAtHarbour = new int[2] { start, newMinimum };
//                    position[0] = newMinimum;
//                    harbour.AddBoat(boat);
//                }
//            }
//        }
//    }
//}



///// <summary>
///// places a rowboat next to another, if there are any, else it doesnt do anything
///// </summary>
///// <param name="rowboats"></param>
///// <param name="harbour"></param>
//public void PlaceRowboat(List<Boat> rowboats)
//{
//    List<Boat> rowboatsAlreadyDocked = harbour.Port.Where(boat => boat is Rowboat).ToList();
//    foreach (Boat dockedRowboat in rowboatsAlreadyDocked)
//    {
//        foreach (Boat rowboat in rowboats)
//        {
//            if (rowboat.AssignedSpotAtHarbour != null)
//            {
//                continue;
//            }
//            int spot = dockedRowboat.AssignedSpotAtHarbour[0];
//            int rowboatsOnSpot = harbour.Port.Count(row => row.AssignedSpotAtHarbour[0] == spot);
//            if (rowboatsOnSpot == 1)
//            {
//                rowboat.AssignedSpotAtHarbour = dockedRowboat.AssignedSpotAtHarbour;
//                harbour.AddBoat(rowboat);
//                break;
//            }

//        }

//        List<int[]> rowboatAssigned = rowboats.Where(boat => boat.AssignedSpotAtHarbour != null).Select(value => value.AssignedSpotAtHarbour).ToList();
//        foreach (int[] spot in rowboatAssigned)
//        {
//            //har fixat till alla positioner till 2 index, så här behövs det skapas en ny array
//            //med samma layout
//            int[] toRemove = new int[2] { spot[0], spot[0] };
//            positions.Remove(toRemove);
//            //mergedPositions.Remove(spot);
//        }

//    }
//}


//foreach (int[] position in positionsToCheck)
//{

//    int max = position[1];
//    List<Boat> boatsToCheck = boats.Where(notAssigned).ToList();

//    foreach (Boat boat in boatsToCheck)
//    {
//        int currentSpot = position[0];

//        if(currentSpot > max)
//        {
//            break;
//        }

//        //array index 0, därför -1
//        int boatSize = (int)boat.Size - 1;

//        //eftersom listan med nya båtar är sorterade efter storleksordning kan man anta att
//        //nästa båt inte heller får plats, därav break
//        bool boatIsTooBig = !allSpotsFree(port, currentSpot, max, boatSize) || currentSpot + boatSize > max;

//        if (boatIsTooBig)
//        {
//            break;
//        }
//        bool spotTaken = port.OccupiedSpots[currentSpot];

//        int rowboatsOnSpot = port.Boats.Count(boat => anotherBoatOnSameSpot(boat, currentSpot) && boatIsRowboat(boat));
//        int rowboatsLeftToAssign = boatsToCheck.Count(boat => boatIsRowboat(boat) && notAssigned(boat));
//        bool rowboatCanPark = boatIsRowboat(boat) && (rowboatsOnSpot == 1 || !spotTaken);


//        if (rowboatCanPark)
//        {
//            //hur många roddbåtar som det är kvar och hur många det är på platsen, är det 
//            //redan en roddbåt på platsen så kommer inte det få plats en till, då plussas position[0] på med 1, så får nästa båt försöka klämma sig in
//            //från nästa plats och är det här den sista som inte har en hamnplats ska samma sak ske
//            position[0] += rowboatsOnSpot == 1 || rowboatsLeftToAssign == 1 ? 1 : 0;
//            boat.AssignedSpot = new int[1] { currentSpot };
//            port.AddBoat(boat);
//            continue;
//        }

//        //är platsen upptagen ska startpositionen för nästa båt bli 1 högre, 
//        else if (spotTaken)
//        {
//            position[0]++;
//            continue;
//        }


//        //special regler för motorbåt, den ska bara få 1 hamnplats, och behöver inte en array med 2 index
//        else if (boat is Motorboat)
//        {
//            boat.AssignedSpot = new int[1] { currentSpot };
//            position[0]++;
//            port.AddBoat(boat);
//            continue;
//        }


//        //annars är det en båt som ska ta mer än 1 plats
//        else
//        {
//            //och nästa båt ska börja kolla efter en plats
//            //från den sista båtens andra position + 1
//            //dvs om en båt fick plats {22,24} ska nästa börja kolla från 25
//            int end = currentSpot + boatSize;
//            int newMinimum = currentSpot + boatSize + 1;
//            boat.AssignedSpot = new int[2] { currentSpot, end };
//            position[0] = newMinimum;
//            port.AddBoat(boat);
//            continue;

//        }
//    }
//}