using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Navigation;

namespace HamnSimulering
{
    class AssignHarbourSpot
    {
        List<int[]> mergedPositions = new List<int[]>();
        List<int[]> positions = new List<int[]>();
        Harbour harbour;
        public string AssignedHarbour
        {
            get
            {
                return harbour.HarbourName;
            }
        }

        bool showMessages = false;

        public AssignHarbourSpot(Harbour h)
        {
            harbour = h;
        }

        public void GetPositions()
        {
            List<int[]> pos = harbour.RemovedBoats.OrderBy(boat => boat.AssignedSpotAtHarbour[0]).Select(value => value.AssignedSpotAtHarbour).ToList();
            positions = FixPositions(pos);
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
            }
            return fixedPositions;
        }


        /// <summary>
        /// "Lägger ihop" int[] arrayer, om i[1] + 1 = j[0]<br />
        /// exempel, {4,5} och {6,7} blir då {4,7} som läggs in i en lista.<br />
        /// och nästa gång den loopar kommer den kolla om nästa array går<br />
        /// att lägga ihop med den förra arrayen, gör det inte det åker den in som den är i listan.<br />
        /// (detta förutsätter att listan redan är sorterad)
        /// </summary>
        public void MergePositions()
        {
            //räknar helt enkelt ut hur mycket större det ena talet är, för att sedan sortera dom i storleksordning
            Func<int[], int> distanceBetweenNumbers = (values) =>
            {
                return values[1] - values[0];
            };
            int[] prevValue = null;
            foreach (int[] nextValue in positions)
            {
                if (prevValue is null)
                {
                    prevValue = nextValue;
                    mergedPositions.Add(prevValue);
                    continue;
                }
                prevValue = mergedPositions.LastOrDefault();
                int[] fixedValue = VerifyRange(prevValue, nextValue);




                if (prevValue != null && prevValue[0] == fixedValue[0])
                {
                    mergedPositions.Remove(prevValue);
                }
                mergedPositions.Add(fixedValue);
            }
            mergedPositions.OrderBy(distanceBetweenNumbers);
        }






        int[] VerifyRange(int[] prevValue, int[] nextValue)
        {

            return prevValue[1] + 1 == nextValue[0] ? CombinePositions(prevValue[0], nextValue[1]) : nextValue;
        }

        int[] CombinePositions(int startValue, int endValue)
        {
            return new int[2] { startValue, endValue };
        }



        public void TryOldSpots(List<Boat> boats)
        {
            Func<int, int, bool> AllSpotsFree = (start, boatSize) =>
            {
                int end = start + boatSize;
                if(end > harbour.SpotIsTaken.GetUpperBound(0))
                {
                    return false;
                }
                for(int i = start; i <= end; i++)
                {
                    if(harbour.SpotIsTaken[i] == true)
                    {
                        return false;
                    }
                }
                return true;
            };



            foreach (int[] position in mergedPositions)
            {
                int max = position[1];
                foreach (Boat boat in boats)
                {
                    if (boat.AssignedSpotAtHarbour != null)
                    {
                        continue;
                    }
                    int start = position[0];

                    int boatSize = (int)boat.SizeInSpots - 1;
                    if (start > max || !AllSpotsFree(start, boatSize) || start + boatSize > max)
                    {
                        break;
                    }


                    int rowboatsOnSpot = harbour.Port.Count(row => row.AssignedSpotAtHarbour[0] == start && row is Rowboat);
                    int rowboatsLeftToAssign = boats.Count(boat => boat is Rowboat && boat.AssignedSpotAtHarbour == null);






                    if (boat is Rowboat && rowboatsOnSpot == 1 || !harbour.SpotIsTaken[start] && boat is Rowboat)
                    {
                        //hur många roddbåtar som det är kvar och hur många det är på platsen, är det 
                        //redan en roddbåt på platsen ska nästa båt försöka klämma sig in
                        //från nästa plats och är det här den sista ska samma sak ske
                        position[0] += rowboatsOnSpot == 1 || rowboatsLeftToAssign == 1 ? 1 : 0;
                        boat.AssignedSpotAtHarbour = new int[1] { start };
                        harbour.AddBoat(boat);
                        continue;
                    }


                    else if (harbour.SpotIsTaken[start])
                    {
                        position[0]++;
                        continue;
                    }



                    else if (boat is Motorboat)
                    {
                        boat.AssignedSpotAtHarbour = new int[1] { start };
                        position[0]++;
                        harbour.AddBoat(boat);
                        continue;
                    }



                    else
                    {
                        int newMinimum = start + boatSize;
                        boat.AssignedSpotAtHarbour = new int[2] { start, newMinimum };
                        position[0] = newMinimum;
                        harbour.AddBoat(boat);
                        continue;

                    }
                }
            }


        }


        /// <summary>
        /// gives a rowboat the next free spot that is found.
        /// also checks if another can fit on the spot.
        /// any rowboat that is not added to the harbour
        /// returns without an assigned spot.
        /// </summary>
        /// <param name="rowboats">list of boats</param>
        /// <param name="harbour"></param>
        /// <returns></returns>
        public void GiveRowboatUnassignedSpot(List<Boat> rowboats)
        {

            //TryOldSpots(rowboats);
            int currentSpot = 0;

            while (harbour.SpotsLeft > 0 && rowboats.Any())
            {
                foreach (Boat rowboat in rowboats)
                {
                    if (rowboat.AssignedSpotAtHarbour != null)
                    {
                        continue;
                    }
                    int rowboatsOnSpot = harbour.Port.Count(row => row.AssignedSpotAtHarbour[0] == currentSpot && row is Rowboat);

                    if (rowboatsOnSpot == 1 || !harbour.SpotIsTaken[currentSpot])
                    {
                        rowboat.AssignedSpotAtHarbour = new int[1] { currentSpot };
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
                    rowboats = rowboats.Where(rowboat => rowboat.AssignedSpotAtHarbour == null).ToList();
                }
            }
        }


        /// <summary>
        /// places a rowboat next to another, if there are any, else it doesnt do anything
        /// </summary>
        /// <param name="rowboats"></param>
        /// <param name="harbour"></param>
        public void PlaceRowboat(List<Boat> rowboats)
        {
            List<Boat> rowboatsAlreadyDocked = harbour.Port.Where(boat => boat is Rowboat).ToList();
            foreach (Boat dockedRowboat in rowboatsAlreadyDocked)
            {
                foreach (Boat rowboat in rowboats)
                {
                    if (rowboat.AssignedSpotAtHarbour != null)
                    {
                        continue;
                    }
                    int spot = dockedRowboat.AssignedSpotAtHarbour[0];
                    int rowboatsOnSpot = harbour.Port.Count(row => row.AssignedSpotAtHarbour[0] == spot);
                    if (rowboatsOnSpot == 1)
                    {
                        rowboat.AssignedSpotAtHarbour = dockedRowboat.AssignedSpotAtHarbour;
                        harbour.AddBoat(rowboat);
                        break;
                    }

                }

                List<int[]> rowboatAssigned = rowboats.Where(boat => boat.AssignedSpotAtHarbour != null).Select(value => value.AssignedSpotAtHarbour).ToList();
                foreach (int[] spot in rowboatAssigned)
                {
                    //har fixat till alla positioner till 2 index, så här behövs det skapas en ny array
                    //med samma layout
                    int[] toRemove = new int[2] { spot[0], spot[0] };
                    positions.Remove(toRemove);
                    //mergedPositions.Remove(spot);
                }

            }
        }






        /// <summary>
        /// försöker ge redan använda platser till de nya båtarna (utom roddbåt)
        /// </summary>
        /// <param name="newBoats">Listan med nya båtar</param>
        public void GivePositions(List<Boat> newBoats)
        {
            foreach (int[] position in mergedPositions)
            {

                int max = position[1];

                foreach (Boat boat in newBoats)
                {
                    if (boat.AssignedSpotAtHarbour != null)
                    {
                        continue;
                    }
                    if (position[0] > max)
                    {
                        break;
                    }


                    int start = position[0];
                    if (harbour.SpotIsTaken[start])
                    {
                        position[0]++;
                        continue;
                    }


                    if (boat is Motorboat)
                    {
                        boat.AssignedSpotAtHarbour = new int[1] { start };
                        position[0]++;
                        harbour.AddBoat(boat);
                    }
                    else
                    {
                        int boatSize = (int)boat.SizeInSpots - 1;

                        if (start + boatSize > max)
                        {
                            break;
                        }
                        else
                        {
                            int newMinimum = start + boatSize;
                            boat.AssignedSpotAtHarbour = new int[2] { start, newMinimum };
                            position[0] = newMinimum;
                            harbour.AddBoat(boat);
                        }
                    }
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
            //t.ex en segelbåt ska ha 2 platser, plats 1 och 2, 2 - (antal platser den ska ha (2)) = 0; 0,1,2 blir 3 platser, därför tar jag bort 1 så det blir 1,2
            foreach (Boat boat in boats)
            {
                if (boat.AssignedSpotAtHarbour != null)
                {
                    continue;
                }



                int spotsToTake = (int)boat.SizeInSpots;
                int currentSpot = harbour.SpotIsTaken.GetUpperBound(0);
                int spotsFound = 0;

                for (int i = currentSpot; i >= 0; i--)
                {
                    bool spotTaken = harbour.SpotIsTaken[i];
                    if (spotTaken)
                    {
                        spotsFound = 0;
                        currentSpot--;//platsen var upptagen
                        continue;
                    }
                    else
                    {
                        spotsFound++;
                    }


                    if (spotsFound == spotsToTake)
                    {
                        int start = currentSpot;
                        int end = (currentSpot + spotsToTake) - 1;

                        boat.AssignedSpotAtHarbour = boat is Motorboat ? new int[1] { start } : new int[2] { start, end };
                        harbour.AddBoat(boat);
                        break;
                    }
                    currentSpot--;
                }
            }
        }


        void CouldNotGetASpot(Boat boat, string caller)
        {
            if (showMessages)
            {
                MessageBox.Show($"{boat.GetBoatType()} {boat.ModelID} could not get a spot!\n{caller} @ {harbour.HarbourName}");
            }
        }
        /// <summary>
        /// försöker lägga in båten från första platsen, fortsätter ner sista eller tills den hittar en plats, returnerar true om en plats hittas, false om en plats inte hittas
        /// sätter även värdet på båtens AssignedSpotAtHarbour
        /// </summary>
        /// <param name="boats">listan med båtar</param>
        /// <returns></returns>
        private void TryAddFromTop(List<Boat> boats, Harbour harbour)
        {
            foreach (Boat boat in boats)
            {
                int spotsToTake = (int)boat.SizeInSpots;
                int currentSpot = 0;
                int spotsFound = 0;
                foreach (bool spotTaken in harbour.SpotIsTaken)
                {
                    if (spotTaken)
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
                        int start = (currentSpot - spotsToTake);
                        int end = currentSpot;
                        boat.AssignedSpotAtHarbour = new int[2] { start, end };
                        continue;
                    }
                }
            }
        }
    }
}
