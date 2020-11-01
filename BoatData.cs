using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;

namespace HamnSimulering
{

    class BoatData
    {

        static DataSet boatInfo = new DataSet("BoatInfo");

        public static void Update(Port port)
        {
            ListFreeSpotsPort(port);
            UpdatePort(port);
            //BoatData.UpdateHarbour(harbour);
            //BoatData.ListFreeSpots(harbour);
        }


        /// <summary>
        /// sorterar raderna i hamnarna per det första talet i "Plats"
        /// </summary>
        /// <param name="table">Vilken tabell som ska visas</param>
        /// <returns></returns>
        public static DataView BoatDataViewer(string table)
        {
            //i brist på bättre lösning
            if (table == "WaitingBoats")
            {
                return boatInfo.Tables[table].DefaultView;
            }
            else
            {
                //splittar hamnplats strängen och sorterar på det första värdet
                Func<string, char, int> splitColumnData =
                    ((data, splitAt) => int.Parse(data.Split(splitAt)[0]));



                var boats = boatInfo.Tables[table].AsEnumerable();
                var q1 = boats.OrderBy(row => splitColumnData(row.Field<string>("Plats"), '-'));
                return q1.AsDataView();
            }
        }

        /// <summary>
        /// Skapar tabeller och kolumner
        /// </summary>
        public static void Port_SetupDataTable(Port port)
        {
                DataTable portTable = new DataTable
                {
                    Columns = { { "Plats", typeof(string) }, { "Båttyp", typeof(string) }, { "Vikt", typeof(string) },
                                { "Nr", typeof(string) }, { "Maxhast", typeof(string) },
                                { "Dagar", typeof(string) }, { "Övrigt", typeof(string) }, }
                };

                    portTable.TableName = port.Name;



            boatInfo.Tables.Add(portTable);
        }

        public static void Waiting_SetupDataTable()
        { 
            DataTable waitingBoatsTable = new DataTable("WaitingBoats")
            {
                Columns = { { "Båttyp", typeof(string) }, { "Vikt", typeof(string) },
                                { "Nr", typeof(string) }, { "Maxhast", typeof(string) }, { "Övrigt", typeof(string) }, }
            };

            boatInfo.Tables.Add(waitingBoatsTable);
        }


        /// <summary>
        /// Lägger in en rad på varje plats som är ledig
        /// </summary>
        /// <param name="harbour"></param>
        public static void ListFreeSpots(Port harbour)
        {

            DataTable table = boatInfo.Tables[harbour.Name];
            int currentSpot = 1;
            DataRow freeSpot;
            foreach (bool spot in harbour.SpotIsTaken)
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

        /// <summary>
        /// Rensar tabellen
        /// </summary>
        /// <param name="tableToClear"></param>
        public static void ClearTable(string tableToClear)
        {
            DataTable table = boatInfo.Tables[tableToClear];
            table.Clear();
        }


        /// <summary>
        /// Uppdaterar tabellen för båtarna som ska parkeras
        /// </summary>
        /// <param name="boatsAtSea"></param>
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
                    MessageBox.Show(e.ToString() + $"{boat.GetSpot} {boat.ModelID} {boat.GetBoatType()}");
                }
            }
        }

        /// <summary>
        /// Väljer raden med båtens nr, och tar bort den från listan som tillhör
        /// hamnen den kom ifrån
        /// </summary>
        /// <param name="boat"></param>
        /// <param name="tableToModify"></param>
        public static void RemoveBoat(Boat boat, string tableToModify)
        {
            DataTable table = boatInfo.Tables[tableToModify];
            try
            {
                //tar med vikten för att vara extra säker att rätt rad tas bort
                DataRow currentBoat = table.Select($"Nr = '{boat.ModelID}' AND Vikt = '{boat.Weight}'").FirstOrDefault();
                table.Rows.Remove(currentBoat);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + $"{boat.GetSpot} {boat.ModelID} {boat.GetBoatType()}");
            }
        }




        public static void FillEmpty(Port port)
        {
            DataTable table = boatInfo.Tables[port.Name];

            DataRow freeSpot;
            for (int i = 1; i <= 32; i++)
            {
                freeSpot = table.NewRow();
                freeSpot["Plats"] = i;
                freeSpot["Båttyp"] = "Ledigt";
                freeSpot["Dagar"] = "";
                table.Rows.Add(freeSpot);
            }
        }


        public static void ListFreeSpotsPort(Port port)
        {

            DataTable table = boatInfo.Tables[port.Name];
            int currentSpot = 1;
            DataRow freeSpot;
            foreach (bool spot in port.SpotIsTaken)
            {
                if (!spot && table.Select($"Plats = '{currentSpot}' AND Båttyp = 'Ledigt'").FirstOrDefault() == null)
                {
                    freeSpot = table.NewRow();
                    freeSpot["Plats"] = currentSpot;
                    freeSpot["Båttyp"] = "Ledigt";
                    freeSpot["Dagar"] = "";
                    table.Rows.Add(freeSpot);

                }
                currentSpot++;
            }

        }
        public static void UpdatePort(Port port, Boat boat = null)
        {
            DataTable table = boatInfo.Tables[port.Name];
            if (boat == null) // om boat är null har jag bestämt att det är då den laddas från fil
            {
                foreach (Boat b in port.Boats)
                {
                    UpdateSingleBoat(table, b);
                    //int start = boat.AssignedSpot[0] + 1;
                    //int end = boat.AssignedSpot.Length < 2 ? boat.AssignedSpot[0] + 1 : boat.AssignedSpot[1] + 1;
                    //DataRow emptySpot;
                    //for (int i = start; i <= end; i++)
                    //{
                    //    emptySpot = table.Select($"Plats = '{i}' AND Båttyp = 'Ledigt'").FirstOrDefault();
                    //    if (emptySpot != null)
                    //    {
                    //        table.Rows.Remove(emptySpot);
                    //    }
                    //}
                    //AddOrModifyBoat(table, boat);
                }
            }
            else
            {
                UpdateSingleBoat(table, boat);
            }

        }

        static void UpdateSingleBoat(DataTable table, Boat boat)
        {
            int start = boat.AssignedSpot[0] + 1;
            int end = boat.AssignedSpot.Length < 2 ? boat.AssignedSpot[0] + 1 : boat.AssignedSpot[1] + 1;
            DataRow emptySpot;
            for (int i = start; i <= end; i++)
            {
                emptySpot = table.Select($"Plats = '{i}' AND Båttyp = 'Ledigt'").FirstOrDefault();
                if (emptySpot != null)
                {
                    table.Rows.Remove(emptySpot);
                }
            }
            AddOrModifyBoat(table, boat);
        }

        public static void AddTimeToBoat(Boat boat, Port port)
        {
            DataTable table = boatInfo.Tables[port.Name];
            DataRow currentBoat = table.Select($"Nr = '{boat.ModelID}' AND Vikt = '{boat.Weight}'").FirstOrDefault();
            if (currentBoat != null)
            {
                currentBoat["Dagar"] = $"{boat.DaysSpentAtHarbour}/{boat.MaxDaysAtHarbour}";
            }
            else
            {
                MessageBox.Show("Error updating time on " + boat.ModelID);
            }
        }

        /// <summary>
        /// lägger till en båt i listan eller uppdateras dess dagar i hamnen
        /// </summary>
        /// <param name="table"></param>
        /// <param name="boat"></param>
        static void AddOrModifyBoat(DataTable table, Boat boat)
        {
            DataRow currentBoat = table.Select($"Nr = '{boat.ModelID}' AND Vikt = '{boat.Weight}'").FirstOrDefault();
            if (currentBoat != null)
            {
                currentBoat["Dagar"] = $"{boat.DaysSpentAtHarbour}/{boat.MaxDaysAtHarbour}";
            }
            else
            {
                try
                {
                    currentBoat = table.NewRow();
                    currentBoat["Plats"] = boat.GetSpot;
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



























//public static void UpdateHarbour(Harbour harbour)
//{
//    DataTable table = boatInfo.Tables[harbour.Name];
//    //tar bort alla rader som båttyp = Ledigt
//    foreach (DataRow emptySpot in table.Select("Båttyp = 'Ledigt'"))
//    {
//        table.Rows.Remove(emptySpot);
//    }
//    foreach (Boat boat in harbour.Port)
//    {
//        AddOrModifyBoat(table, boat);
//    }

//}