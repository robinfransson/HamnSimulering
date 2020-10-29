using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Navigation;

namespace HamnSimulering
{
    class Assigner
    {
        List<int[]> mergedPositions = new List<int[]>();
        List<int[]> positions = new List<int[]>();
        Harbour harbour;
        Func<int[], bool> errorCheck = (pos) =>
        {
            return pos[0] > pos[1];
        };
        public string AssignedHarbour
        {
            get
            {
                return harbour.Name;
            }
        }


        public Assigner(Harbour h)
        {
            harbour = h;
        }

        public void GetPositions()
        {
            //AssignedSpotAtHarbour[0] är första platsen som båten tar upp, så platserna sorteras på den för att när dom ska försöka slås ihop
            //hamnar de närliggande efter varandra i listan
            List<int[]> pos = harbour.RemovedBoats.OrderBy(boat => boat.AssignedSpot[0]).Select(value => value.AssignedSpot).ToList();
            positions = FixPositions(pos);
            MergePositions();
            if(mergedPositions.Any(errorCheck))
            {
                MessageBox.Show("Error!");
            }
        }



        void RemoveDuplicates()
        {
            if (mergedPositions.Any())
            {
                int lastSpotToCheck = mergedPositions.Max(pos => pos[1]);
                for (int i = 0; i < lastSpotToCheck; i++)
                {
                    bool duplicate = mergedPositions.Count(value => value[0] == i) > 1;
                    int[] toRemove = mergedPositions.FirstOrDefault(value => value[0] == i);
                    if (duplicate)
                    {
                        mergedPositions.Remove(toRemove);
                    }
                }
            }
        }

        public void ClearPositions()
        {
            mergedPositions.Clear();
            positions.Clear();

        }

        /// <summary>
        /// ger alla int[] 2 index, har den bara 1 från början blir det andra indexet samma som det första
        /// detta görs pga loopar som jämför första och andra värdet.
        /// </summary>
        /// <param name="positions"></param>
        /// <returns></returns>
        List<int[]> FixPositions(List<int[]> positionsBeforeFix)
        {
            List<int[]> fixedPositions = new List<int[]>();
            foreach (var pos in positionsBeforeFix)
            {
                if (pos.Length < 2)
                {
                    fixedPositions.Add(new int[2] { pos[0], pos[0] });
                }
                else
                {
                    fixedPositions.Add(pos);
                }
                if (pos != null)
                {
                    if (fixedPositions.Any(errorCheck))
                    {
                        MessageBox.Show("Error fix positions!");
                    }
                }
            }
            return fixedPositions;
        }

        /// <summary>
        /// "Lägger ihop" int[2], om i[1] + 1 = j[0]<br />
        /// exempel, {4,5} och {6,7} blir då {4,7} som läggs in i en lista.<br />
        /// och nästa gång den loopar kommer den kolla om nästa array går<br />
        /// att lägga ihop med den förra arrayen, gör det inte det åker den in som den är i listan.<br />
        /// (detta förutsätter att listan redan är sorterad)
        /// </summary>
        void MergePositions()
        {
            //räknar helt enkelt ut hur mycket större det ena talet är, för att sedan sortera dom i storleksordning
            Func<int[], int> distanceBetweenNumbers = (values) =>
            {
                return values[1] - values[0];
            };



            int[] previousPosition = null;
            foreach (int[] position in positions)
            {
                //första gången den körs har den inget värde att jämföra med
                //så jag sätter värdet prevValue till det första i listan med positioner
                //och lägger in det i mergedPositions och fortsätter loopen
                if (previousPosition is null)
                {
                    previousPosition = position;
                    mergedPositions.Add(previousPosition);
                    continue;
                }
                previousPosition = mergedPositions.LastOrDefault();

                int[] fixedValue = position;

                //kollar om det går att slå ihop dom
                if (previousPosition[1] + 1 == position[0])
                {
                    fixedValue = CombinePositions(previousPosition[0], position[1]);
                    mergedPositions.Add(fixedValue);
                }
                else
                {
                    mergedPositions.Add(position);
                }

                //om det första värdet i båda arrayerna är samma
                //har dom slagits ihop och då ska det föregående värdet
                //inte vara kvar i listan
                if (previousPosition[0] == fixedValue[0])
                {
                    mergedPositions.Remove(previousPosition);
                }
            }
            mergedPositions.OrderBy(distanceBetweenNumbers);
            RemoveDuplicates();
        }


        int[] CombinePositions(int startValue, int endValue)
        {
            return new int[2] { startValue, endValue };
        }



        public void TryOldSpots(List<Boat> boats)
        {
            //här kollas hamnen om båten får plats på den plats som loopas
            Func<int, int, bool> allSpotsFree = (start, boatSize) =>
            {
                int end = start + boatSize;

                //om båtens storlek plus startvärdet är större än hamnens kapacitet 
                if (end > harbour.SpotIsTaken.GetUpperBound(0))
                {
                    return false;
                }
                for (int i = start; i <= end; i++)
                {
                    //om hamnplatsen är upptagen
                    if (harbour.SpotIsTaken[i] == true)
                    {
                        return false;
                    }
                }
                //annars får båten plats
                return true;
            };



            foreach (int[] position in mergedPositions)
            {
                //
                int max = position[1];
                foreach (Boat boat in boats)
                {
                    //om båten redan har en plats ska nästa båt kollas direkt
                    if (boat.AssignedSpot != null)
                    {
                        continue;
                    }
                    int start = position[0];


                    //array index 0, därför -1
                    int boatSize = (int)boat.SizeInSpots - 1;

                    //eftersom listan med nya båtar är sorterade efter storleksordning kan man anta att
                    //nästa båt inte heller får plats, därav break
                    if (start > max || !allSpotsFree(start, boatSize) || start + boatSize > max)
                    {
                        break;
                    }


                    int rowboatsOnSpot = harbour.Port.Count(row => row.AssignedSpot[0] == start && row is Rowboat);
                    int rowboatsLeftToAssign = boats.Count(boat => boat is Rowboat && boat.AssignedSpot == null);






                    if (boat is Rowboat && rowboatsOnSpot == 1 || !harbour.SpotIsTaken[start] && boat is Rowboat)
                    {
                        //hur många roddbåtar som det är kvar och hur många det är på platsen, är det 
                        //redan en roddbåt på platsen så kommer inte det få plats en till, då plussas position[0] på med 1, så får nästa båt försöka klämma sig in
                        //från nästa plats och är det här den sista som inte har en hamnplats ska samma sak ske
                        position[0] += rowboatsOnSpot == 1 || rowboatsLeftToAssign == 1 ? 1 : 0;
                        boat.AssignedSpot = new int[1] { start };
                        harbour.AddBoat(boat);
                        continue;
                    }

                    //är platsen upptagen ska startpositionen för nästa båt bli 1 högre, 
                    else if (harbour.SpotIsTaken[start])
                    {
                        position[0]++;
                        continue;
                    }


                    //special regler för motorbåt, den ska bara få 1 hamnplats, och behöver inte en array med 2 index
                    else if (boat is Motorboat)
                    {
                        boat.AssignedSpot = new int[1] { start };
                        position[0]++;
                        harbour.AddBoat(boat);
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
                        harbour.AddBoat(boat);
                        continue;

                    }
                }
            }


        }
        public void GiveRowboatUnassignedSpot(List<Boat> rowboats)
        {
            int currentSpot = 0;

            rowboats = rowboats.Where(rowboat => rowboat.AssignedSpot == null).ToList();
            //medans det finns plats kvar och roddbåtar i listan
            //ska loopen köras
            while (harbour.SpotsLeft > 0 && rowboats.Any())
            {
                foreach (Boat rowboat in rowboats)
                {
                    int rowboatsOnSpot = harbour.Port.Count(row => row.AssignedSpot[0] == currentSpot && row is Rowboat);

                    //om det får plats en roddbåt eller om hamnplatsen är ledig
                    if (rowboatsOnSpot == 1 || !harbour.SpotIsTaken[currentSpot])
                    {
                        rowboat.AssignedSpot = new int[1] { currentSpot };
                        harbour.AddBoat(rowboat);
                        continue;
                    }
                    else
                    {
                        break;
                    }
                }
                currentSpot++;
                if (currentSpot > harbour.SpotIsTaken.GetUpperBound(0))
                {
                    break;
                }
                else
                {
                    rowboats = rowboats.Where(rowboat => rowboat.AssignedSpot == null).ToList();
                }
            }
        }








        /// <summary>
        /// Lägger till båtarna nerifrån
        /// </summary>
        /// <param name="boats"></param>
        /// <param name="harbour"></param>
        /// <returns></returns>
        public void TryAddFromBottom(List<Boat> boats)
        {
            foreach (Boat boat in boats)
            {
                if (boat.AssignedSpot != null)
                {
                    continue;
                }



                
                int spotsToTake = (int)boat.SizeInSpots;
                int currentSpot = harbour.SpotIsTaken.GetUpperBound(0);
                int spotsFound = 0;

                for (int i = currentSpot; i >= 0; i--)
                {
                    bool spotTaken = harbour.SpotIsTaken[i];

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
                        harbour.AddBoat(boat);
                        break;
                    }
                    currentSpot--;
                }
            }
        }
    }
}

































































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
