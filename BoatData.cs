using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace HamnSimulering
{

    class BoatData
    {

        static DataSet boatInfo = new DataSet("BoatInfo");



       
        /// <summary>
        /// sorterar raderna i hamnarna per det första talet i "Plats"
        /// </summary>
        /// <param name="table">Vilken tabell som ska visas</param>
        /// <returns></returns>
        public static DataView BoatDataViewer(string table)
        {
            //i brist på bättre lösning
            if(table == "WaitingBoats")
            {
                return boatInfo.Tables[table].DefaultView;
            }
            else
            {
                Func<string, char, int> splitColumnData = 
                    ((data, splitAt) => int.Parse(data.Split(splitAt)[0]));



                var boats = boatInfo.Tables[table].AsEnumerable();
                var q1 = boats.OrderBy(row => splitColumnData(row.Field<string>("Plats"), '-'));
                return q1.AsDataView();
            }
        }

        /// <summary>
        /// skapar tabeller och 
        /// </summary>
        public static void SetupDataTables()
        {
            for (int i = 0; i < 2; i++)
            {
                DataTable harbourTable = new DataTable
                {
                    Columns = { { "Plats", typeof(string) }, { "Båttyp", typeof(string) }, { "Vikt", typeof(string) },
                                { "Nr", typeof(string) }, { "Maxhast", typeof(string) },
                                { "Dagar", typeof(string) }, { "Övrigt", typeof(string) }, }
                };

                if (i == 0)
                {
                    harbourTable.TableName = "LeftHarbour";
                }
                else
                {
                    harbourTable.TableName = "RightHarbour";
                }
                boatInfo.Tables.Add(harbourTable);
            }

            DataTable waitingBoatsTable = new DataTable("WaitingBoats")
            {
                Columns = { { "Båttyp", typeof(string) }, { "Vikt", typeof(string) },
                                { "Nr", typeof(string) }, { "Maxhast", typeof(string) }, { "Övrigt", typeof(string) }, }
            };

            boatInfo.Tables.Add(waitingBoatsTable);
        }

        public static void ListFreeSpots__fix(Harbour harbour)
        {

            DataTable table = boatInfo.Tables[harbour.HarbourName];
            int currentSpot = 1;
            DataRow freeSpot;
            foreach (bool spot in harbour.IsCurrentSpotTaken)
            {
                if (!spot && table.Select($"Plats = '{currentSpot}' AND Båttyp = 'Ledigt'").FirstOrDefault() == null)
                {
                    freeSpot = table.NewRow();
                    freeSpot["Plats"] = currentSpot;
                    freeSpot["Båttyp"] = "Ledigt";
                    table.Rows.Add(freeSpot);

                }
                currentSpot++;
            }
        }
        public static void ListFreeSpots(Harbour harbour)
        {

            DataTable table = boatInfo.Tables[harbour.HarbourName];
            int currentSpot = 1;
            DataRow freeSpot;
            foreach (bool spot in harbour.IsCurrentSpotTaken)
            {
                if (!spot)
                {
                    freeSpot = table.NewRow();
                    freeSpot["Plats"] = currentSpot;
                    freeSpot["Båttyp"] = "Ledigt";
                    table.Rows.Add(freeSpot);

                }
                currentSpot++;
            }
        }

        public static void ClearTable(string tableToClear)
        {
            DataTable table = boatInfo.Tables[tableToClear];
            table.Clear();
        }


        public static void UpdateVisitors(List<Boat> boatsAtSea)
        {
            DataTable boatsWaiting = boatInfo.Tables["WaitingBoats"];
            boatsWaiting.Clear();
            DataRow newWaitingBoat;
            foreach (Boat boat in boatsAtSea)
            {
                try
                {
                    newWaitingBoat = boatsWaiting.NewRow();
                    newWaitingBoat["Båttyp"] = boat.GetBoatType();
                    newWaitingBoat["Vikt"] = boat.Weight + " kg";
                    newWaitingBoat["Nr"] = boat.ModelID;
                    newWaitingBoat["Maxhast"] = boat.TopSpeedKMH;
                    newWaitingBoat["Övrigt"] = boat.GetSpecialProperty();
                    boatsWaiting.Rows.Add(newWaitingBoat);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                }
            }
        }


        public static void RemoveBoat(Boat boat, string tableToModify)
        {
            DataTable table = boatInfo.Tables[tableToModify];
            try
            {
                DataRow currentBoat = table.Select($"Nr = '{boat.ModelID}'").FirstOrDefault();
                table.Rows.Remove(currentBoat);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }



        public static void UpdateHarbour__fix(Harbour harbour, Boat boat)
        {
            DataTable table = boatInfo.Tables[harbour.HarbourName];
            int start = boat.AssignedSpotAtHarbour[0] + 1;
            int end = boat.AssignedSpotAtHarbour.Length < 2 ? boat.AssignedSpotAtHarbour[0] + 1 : boat.AssignedSpotAtHarbour[1] + 1;
            DataRow emptySpot;
            for (int i = start; i <= end; i++)
            {
                emptySpot = table.Select($"Plats = '{i}'").FirstOrDefault();
                if(emptySpot != null)
                {
                    table.Rows.Remove(emptySpot);
                }
                else
                {
                    MessageBox.Show("Error removing Plats " + i);
                }
            }
            AddOrModifyBoat(table, boat);
        }




        public static void UpdateHarbour(Harbour harbour, string tableToUpdate)
        {
            DataTable table = boatInfo.Tables[tableToUpdate];
            foreach (DataRow emptySpot in table.Select("Båttyp = 'Ledigt'"))
            {
                table.Rows.Remove(emptySpot);
            }
            foreach (Boat boat in harbour.Port)
            {
                AddOrModifyBoat(table, boat);
            }

        }


        /// <summary>
        /// lägger till en båt i listan eller uppdateras dess dagar i hamnen
        /// </summary>
        /// <param name="table"></param>
        /// <param name="boat"></param>
        static void AddOrModifyBoat(DataTable table, Boat boat)
        {
            DataRow currentBoat = table.Select($"Nr = '{boat.ModelID}'").FirstOrDefault();
            if (currentBoat != null)
            {
                currentBoat["Dagar"] = $"{boat.DaysSpentAtHarbour}/{boat.MaxDaysAtHarbour}";
            }
            else
            {
                try
                {
                    currentBoat = table.NewRow();
                    currentBoat["Plats"] = boat.GetSpot();
                    currentBoat["Båttyp"] = boat.GetBoatType();
                    currentBoat["Vikt"] = boat.Weight;
                    currentBoat["Nr"] = boat.ModelID;
                    currentBoat["Maxhast"] = boat.TopSpeedKMH;
                    currentBoat["Dagar"] = $"{boat.DaysSpentAtHarbour}/{boat.MaxDaysAtHarbour}";
                    currentBoat["Övrigt"] = boat.GetSpecialProperty();
                    table.Rows.Add(currentBoat);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                }
            }
        }

    }
}
