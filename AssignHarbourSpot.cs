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
            foreach(var pos in positionsBeforeFix)
            {
                if(pos.Length < 2)
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
            int[] prevValue = null;
            foreach (int[] nextValue in positions)
            {
                int[] fixedValue = null;
                if (prevValue is null)
                {
                    prevValue = nextValue;
                    mergedPositions.Add(prevValue);
                    continue;
                }
                fixedValue = VerifyRange(prevValue, nextValue);
                 



                int[] previousMax = mergedPositions.LastOrDefault();
                if (previousMax != null && previousMax[0] == fixedValue[0])
                {
                    mergedPositions.Remove(previousMax);
                }
                mergedPositions.Add(fixedValue);
            }
        }






        int[] VerifyRange(int[] prevValue, int[] nextValue)
        {
            
                return prevValue[1] + 1 == nextValue[0] ? CombinePositions(prevValue[0], nextValue[1]) : nextValue;
        }

        int[] CombinePositions(int startValue, int endValue)
        {
            return new int[2] { startValue, endValue };
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
            int currentSpot = 0;
            foreach (Boat rowboat in rowboats)
            {
                if (rowboat.AssignedSpotAtHarbour != null)
                {
                    continue;
                }



                int rowboatsOnSpot = harbour.Port.Count(row => row.AssignedSpotAtHarbour[0] == currentSpot && row is Rowboat);

                if (rowboatsOnSpot == 1 || !harbour.IsCurrentSpotTaken[currentSpot])
                {
                    rowboat.AssignedSpotAtHarbour = new int[1] { currentSpot };
                    harbour.AddBoat(rowboat);
                    continue;
                }
                else
                {
                    currentSpot++;
                }

                if (rowboat.AssignedSpotAtHarbour == null)
                {
                    CouldNotGetASpot(rowboat, "Give rowboat unassigned spot");
                }
            }
        }


        /// <summary>
        /// places a rowboat next to another, if there are any, else it doesnt do ano
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
                    if(dockedRowboat.AssignedSpotAtHarbour.Length > 1)
                    {

                    }
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
                    

                    if (rowboat.AssignedSpotAtHarbour == null)
                    {
                        CouldNotGetASpot(rowboat, "Give rowboat spot from another that left");
                    }
                }

                List<int[]> rowboatAssigned = rowboats.Where(boat => boat.AssignedSpotAtHarbour != null).Select(value => value.AssignedSpotAtHarbour).ToList();
                foreach (int[] spot in rowboatAssigned)
                {
                    positions.Remove(spot);
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
                    if(harbour.IsCurrentSpotTaken[start])
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
                        if(start + boatSize > max && mergedPositions.LastOrDefault() == position)
                        {

                            CouldNotGetASpot(boat, $"Reused spot from other boat last tried was ({start}, {max})");
                            break;
                        }
                        else if (start + boatSize > max)
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
            ///   returns a list of boats without an assigned spot
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
                int currentSpot = harbour.IsCurrentSpotTaken.GetUpperBound(0);
                int spotsFound = 0;

                for (int i = currentSpot; i >= 0; i--)
                {
                    bool spotTaken = harbour.IsCurrentSpotTaken[i];
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
                        int start = currentSpot;
                        int end = (currentSpot + spotsToTake) - 1;
                        boat.AssignedSpotAtHarbour = new int[2] { start, end };
                        harbour.AddBoat(boat);
                        break;
                    }
                    currentSpot--;
                }
                if (boat.AssignedSpotAtHarbour == null)
                {
                    CouldNotGetASpot(boat, "Add from bottom");
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
                int spotsToTake = (int)boat.SizeInSpots - 1;
                int currentSpot = 0;
                int spotsFound = 0;
                foreach (bool spotTaken in harbour.IsCurrentSpotTaken)
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
