using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;

namespace HamnSimulering
{
    class Harbour
    {

        List<Position> mergedPositions = new List<Position>();


        List<Port> ports = new List<Port>();




        public void GetPositionsFromRemovedBoats()
        {
            foreach (Port port in ports)
            {
                if (port.RemovedBoats.Any())
                {
                    FetchPositions(port);
                }
            }
        }

        public void AddFromBottom(List<Boat> boats)
        {
            foreach (Port port in ports)
            {
                if (boats.Any())
                {
                    TryAddFromBottom(boats, port);
                }
            }

        }


        public void TestOldSpots(List<Boat> boats)
        {
            foreach (Port port in ports)
            {
                if (boats.Any())
                {
                    TryOldSpots(boats, port);
                }
            }
        }


        public void AddRowBoats(List<Rowboat> rowboats)
        {
            foreach (Port port in ports)
            {
                if (rowboats.Any())
                {
                    GiveRowboatUnassignedSpot(rowboats, port);
                }
            }
        }



        /// <summary>
        /// Lägger till en dag på varje båt i hamnen
        /// </summary>
        public void AddOneDay()
        {
            foreach (Port port in ports)
            {
                port.CheckTimeOnBoats();


                if (port.boats.Any())
                {
                    foreach (Boat boat in port.boats)
                    {
                        boat.DaysSpentAtHarbour++;
                        BoatData.AddTimeToBoat(boat, port.PortName);
                    }
                }
            }
        }

        public void ListFreeSpots()
        {
            foreach(Port port in ports)
            {
                BoatData.ListFreeSpotsPort(port);
            }
        }

        /// <summary>
        /// Lägger till en kaj i listan med kajer
        /// </summary>
        /// <param name="p"></param>
        public void AddPort(Port p)
        {
            ports.Add(p);
        }

        /// <summary>
        /// Tar bort den gamla kajen som har samma PortName och lägger in den nya
        /// </summary>
        /// <param name="newPort"></param>
        public void ReplacePort(Port newPort)
        {
            Port portToRemove = ports.Where(port => port.PortName == newPort.PortName).FirstOrDefault();
            ports.Remove(portToRemove);
            ports.Add(newPort);
        }





        public void FetchPositions(Port port)
        {
            List<int[]> positions = port.RemovedBoats.Where(boat => boat.AssignedSpot.Length == 2)
                                                     .Select(value => value.AssignedSpot)
                                                     .ToList();

            List<int[]> smallBoatPositions = port.RemovedBoats
                                            .Where(boat => boat.AssignedSpot.Length == 1)
                                            .Select(boat => boat.AssignedSpot)
                                            .ToList();

            //koden är uppbyggd så att alla int[] antas ha 2 index
            foreach(int[] position in smallBoatPositions)
            {
                positions.Add(new int[2] { position[0], position[0] });
            }

            //tar bort om det finns mer än 1 av samma 
            positions = positions.GroupBy(position => position[0])
                                 .Select(group => group.FirstOrDefault()) //väljer det första värdet i varje grupp, (sorterar ut dubletter (roddbåtar dvs))
                                 .OrderBy(position => position[0]).ToList();

            MergePositions(port, positions);
        }





        void MergePositions(Port p, List<int[]> positions)
        {

            List<int[]> mergedPortSpots = new List<int[]>
            {


                //lägger in det första värdet från listan med de borttagna båtarnas positioner i listan som ska innehålla
                //de som har blivit ihopsatta så att loopen har något att jämföra med
                positions.FirstOrDefault()
            };


            //skippar det första värdet i listan för det redan är inlagt i den andra, så den inte jämför samma värden med varandra
            positions = positions.Skip(1).ToList();

            foreach (int[] currentPosition in positions) //skippar första
            {
                //hämtar det föregående värdet
                int[] previousPosition = mergedPortSpots.LastOrDefault();

                int[] nextValueToAdd;


                //kollar om det går att slå ihop dom
                if (previousPosition[1] + 1 == currentPosition[0])
                {
                    nextValueToAdd = CombinePositions(previousPosition[0], currentPosition[1]);
                }
                else
                {
                    nextValueToAdd = currentPosition;
                }

                mergedPortSpots.Add(nextValueToAdd);

                //om det första värdet i båda arrayerna är samma
                //har dom slagits ihop och då ska det föregående värdet
                //inte vara kvar i listan
                if (previousPosition[0] == nextValueToAdd[0])
                {
                    mergedPortSpots.Remove(previousPosition);
                }
            }

            //loopar igenom listan med de positioner som eventuellt har slagits ihop, och lägger till port.PortName
            //för att kunna se vart ifrån platserna kom
            foreach (int[] mergedSpot in mergedPortSpots)
            {

                mergedPositions.Add(new Position(mergedSpot, p.PortName));
            }
        }

        /// <summary>
        /// Rensar listan med gamla kajplatser
        /// </summary>
        public void ClearPositions()
        {
            mergedPositions.Clear();
        }


        int[] CombinePositions(int startValue, int endValue)
        {
            return new int[2] { startValue, endValue };
        }



        public void TryOldSpots(List<Boat> boats, Port port)
        {

            //hur stor platsen är
            Func<int[], int> distanceBetweenNumbers = (values) =>
            {
                return values[1] - values[0];
            };

            //här kollas hamnen om båten får plats på den plats som loopas
            Func<int, int, bool> allSpotsFree = (start, boatSize) =>
            {
                int end = start + boatSize;

                //om båtens storlek plus startvärdet är större än hamnens kapacitet 
                if (end > port.OccupiedSpots.GetUpperBound(0))
                {
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


            var positionsToCheck = mergedPositions.Where(pos => pos.Port == port.PortName)
                                                         .Select(pos => pos.Spot)
                                                         .OrderBy(distanceBetweenNumbers)
                                                         .ToList();


            foreach (int[] position in positionsToCheck)
            {
                
                int max = position[1];

                List<Boat> boatsToCheck = boats.Where(boat => boat.AssignedSpot == null).ToList();

                foreach (Boat boat in boatsToCheck)
                {
                    int start = position[0];

                    if(start > max)
                    {
                        break;
                    }

                    //array index 0, därför -1
                    int boatSize = (int)boat.Size - 1;

                    //eftersom listan med nya båtar är sorterade efter storleksordning kan man anta att
                    //nästa båt inte heller får plats, därav break
                    bool boatIsTooBig = !allSpotsFree(start, boatSize) || start + boatSize > max;

                    if (boatIsTooBig)
                    {
                        break;
                    }


                    int rowboatsOnSpot = port.boats.Count(boat => boat.AssignedSpot[0] == start && boat is Rowboat);
                    int rowboatsLeftToAssign = boatsToCheck.Count(boat => boat is Rowboat && boat.AssignedSpot == null);
                    bool rowboatCanPark = boat is Rowboat && rowboatsOnSpot == 1 || !port.OccupiedSpots[start] && boat is Rowboat;


                    if (rowboatCanPark)
                    {
                        //hur många roddbåtar som det är kvar och hur många det är på platsen, är det 
                        //redan en roddbåt på platsen så kommer inte det få plats en till, då plussas position[0] på med 1, så får nästa båt försöka klämma sig in
                        //från nästa plats och är det här den sista som inte har en hamnplats ska samma sak ske
                        position[0] += rowboatsOnSpot == 1 || rowboatsLeftToAssign == 1 ? 1 : 0;
                        boat.AssignedSpot = new int[1] { start };
                        port.AddBoat(boat);
                        continue;
                    }

                    //är platsen upptagen ska startpositionen för nästa båt bli 1 högre, 
                    else if (port.OccupiedSpots[start])
                    {
                        position[0]++;
                        continue;
                    }


                    //special regler för motorbåt, den ska bara få 1 hamnplats, och behöver inte en array med 2 index
                    else if (boat is Motorboat)
                    {
                        boat.AssignedSpot = new int[1] { start };
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
                        int end = start + boatSize;
                        int newMinimum = start + boatSize + 1;
                        boat.AssignedSpot = new int[2] { start, end };
                        position[0] = newMinimum;
                        port.AddBoat(boat);
                        continue;

                    }
                }
            }
        }



        /// <summary>
        /// Så länge det finns plats och roddbåtar kommer loopen att köras tills den hittar en plats
        /// </summary>
        /// <param name="rowboats"></param>
        /// <param name="port"></param>
        public void GiveRowboatUnassignedSpot(List<Rowboat> rowboats, Port port)
        {
            int currentSpot = 0;
            List<Rowboat> rowboatsToCheck = rowboats.Where(rowboat => rowboat.AssignedSpot == null).ToList();
            //medans det finns plats kvar och roddbåtar i listan
            //ska loopen köras
            while (port.SpotsLeft > 0 && rowboatsToCheck.Any())
            {
                foreach (Rowboat rowboat in rowboatsToCheck)
                {
                    int rowboatsOnSpot = port.boats.Count(row => row.AssignedSpot[0] == currentSpot && row is Rowboat);
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
                rowboatsToCheck = rowboats.Where(rowboat => rowboat.AssignedSpot == null).ToList();
            }
        }








        /// <summary>
        /// Lägger till båtarna nerifrån
        /// </summary>
        /// <param name="boats"></param>
        /// <param name="harbour"></param>
        /// <returns></returns>
        public void TryAddFromBottom(List<Boat> boats, Port port)
        {
            List<Boat> boatsToCheck = boats.Where(boat => boat.AssignedSpot == null).ToList();
            foreach (Boat boat in boatsToCheck)
            {
                //här borde det inte finnas några roddbåtar så jag castar om båtstorleken till en int
                int spotsToTake = (int)boat.Size;

                //eftersom loopen börjar från slutet ska currentSpot vara arrayens max
                int currentSpot = port.OccupiedSpots.GetUpperBound(0);
                int spotsFound = 0;

                for (int i = currentSpot; i >= 0; i--)
                {
                    bool spotTaken = port.OccupiedSpots[i];

                    //platsen var upptagen
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
                        //t.ex en segelbåt ska ha 2 platser, plats 1 och 2, 2 - (antal platser den ska ha (2)) = 0; 0,1,2 blir 3 platser, därför tar jag bort 1 så det blir 0,1
                        int start = currentSpot;
                        int end = (currentSpot + spotsToTake) - 1;

                        boat.AssignedSpot = boat is Motorboat ? new int[1] { start } : new int[2] { start, end };
                        port.AddBoat(boat);
                        break;
                    }
                    currentSpot--;
                }
            }
        }

    }
}
















































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
