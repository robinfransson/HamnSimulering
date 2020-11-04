using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows;

namespace HamnSimulering
{

    class BoatData
    {


        /// <summary>
        /// Räknar ut medelvikten av alla båtar i listan.
        /// </summary>
        public static Func<List<Boat>, float> averageWeight = (port) =>
        {
            if (port.Any())
            {
                return port.Sum(boat => boat.Weight) / port.Count;
            }
            else
            {
                return 0;
            }

        };


        /// <summary>
        /// Får ta in antalen som float för att kunna dividera dom direkt.
        /// </summary>
        public static Func<float, float, float> calculatePercentage =(numerator, denominator) => (float)Math.Round(100 - (numerator / (denominator + numerator)) * 100, 1);

        public static Func<List<Boat>, float> averageSpeed = (boats) => (float)Math.Round(boats.Sum(boat => boat.TopSpeedKMH) / boats.Count, 2);


        /// <summary>
        /// Skapar en sträng med statistik om den aktuella listan med båtar.
        /// </summary>
        public static Func<List<Boat>, string> boatStats = (port) => 
            $"Roddbåtar: {port.Count(boat => boat is Rowboat)} \n" +
            $"Motorbåtar: {port.Count(boat => boat is Motorboat)} \n" +
            $"Segelbåtar: {port.Count(boat => boat is Sailboat)} \n" +
            $"Katamaraner: {port.Count(boat => boat is Catamaran)} \n" +
            $"Lastfartyg: {port.Count(boat => boat is Cargoship)} \n\n" +
            $"Snitthastighet: {averageSpeed(port)} km/h\n" +
            $"Totalvikt: {port.Sum(boat => boat.Weight)} kg\n" +
            $"Snittvikt: {averageWeight(port)} kg\n";

        public static string FreeSpotsInPort(Port port)
        {
            Func<Boat, int, bool> rowboatsOnSpot = (boat, current) => boat is Rowboat && boat.AssignedSpot[0] == current;
            string freeSpots = "Lediga platser:\n";
            int currentSpot = 0;
            foreach (bool spotTaken in port.OccupiedSpots)
            {
                if (spotTaken && port.Boats.Count(boat => rowboatsOnSpot(boat, currentSpot)) == 1)
                {
                    freeSpots += $"({currentSpot + 1})\n";
                }
                else if (!spotTaken)
                {
                    freeSpots += (currentSpot + 1) + "\n";
                }
                currentSpot++;
            }
            return freeSpots;
        }





        static DataSet boatInfo = new DataSet("BoatInfo");





        public static void Update(Port port)
        {
            ListAllFreeSpotsInPort(port);
            UpdatePort(port);
        }



        /// <summary>
        /// sorterar raderna i hamnarna per det första talet i "Plats"
        /// </summary>
        /// <param name="table">Vilken tabell som ska visas</param>
        /// <returns></returns>
        public static DataView BoatDataViewer(string tableName)
        {
            if (tableName == "WaitingBoats")
            {
                return boatInfo.Tables[tableName].DefaultView;
            }
            else
            {
                //splittar hamnplats strängen och returnerar det första värdet i strängen
                //i.e en plats 12-14 returnerar 12 och plats 10 returnerar 10
                Func<string, char, int> splitColumnData =
                    (data, splitAt) => int.Parse(data.Split(splitAt)[0]);



                var q1 = boatInfo.Tables[tableName].AsEnumerable() //AsEnumerable så att jag kan använda linq
                                                   .OrderBy(row => splitColumnData(row.Field<string>("Plats"), '-'));
                return q1.AsDataView();
            }
        }

        /// <summary>
        /// Skapar tabeller och kolumner
        /// </summary>
        public static void SetupPortDataTable(string portName)
        {
                DataTable portTable = new DataTable
                {
                    //alla kolumner får datatypen sträng pga att det inte endast är nummer som läggs in
                    Columns = { { "Plats", typeof(string) }, 
                                { "Båttyp", typeof(string) }, 
                                { "Vikt", typeof(string) },
                                { "Nr", typeof(string) }, 
                                { "Maxhast", typeof(string) },
                                { "Dagar", typeof(string) },
                                { "Övrigt", typeof(string) }, }
                };

                    portTable.TableName = portName;



            boatInfo.Tables.Add(portTable);
        }

        public static void SetupWaitingBoatsDataTable()
        { 
            DataTable waitingBoatsTable = new DataTable("WaitingBoats")
            {
                Columns = { { "Båttyp", typeof(string) },
                            { "Vikt", typeof(string) },
                            { "Nr", typeof(string) }, 
                            { "Maxhast", typeof(string) }, 
                            { "Övrigt", typeof(string) }, }
            };

            boatInfo.Tables.Add(waitingBoatsTable);
        }


        /// <summary>
        /// Lägger in en rad på varje plats som är ledig
        /// </summary>
        /// <param name="harbour"></param>
        public static void ListFreeSpots(Port port)
        {

            DataTable table = boatInfo.Tables[port.PortName];
            int currentSpot = 1;
            DataRow freeSpot;
            foreach (bool spotTaken in port.OccupiedSpots)
            {
                if (!spotTaken)
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
                    newWaitingBoat["Maxhast"] = boat.Speed;
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
                DataRow currentBoat = table.Select($"Nr = '{boat.ModelID}' AND Vikt = '{boat.Weight} kg'").FirstOrDefault();
                table.Rows.Remove(currentBoat);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message + $"{boat.GetSpot} {boat.ModelID} {boat.GetBoatType()}");
            }
        }
        /// <summary>
        /// Listar båten som ska tas borts platser som lediga 
        /// </summary>
        /// <param name="port"></param>
        /// <param name="boat"></param>
        public static void UpdatePartOfPort(Port port, Boat boat)
        {

            DataTable table = boatInfo.Tables[port.PortName];
            DataRow freeSpot;
            int startRow = boat.AssignedSpot[0] + 1;
            int endRow = boat.AssignedSpot.Length < 2 ? boat.AssignedSpot[0] : boat.AssignedSpot[1];

            for(int currentRow = startRow; currentRow <= endRow; currentRow++)
            {
                bool spotTaken = port.OccupiedSpots[currentRow];
                //kollar om det redan finns en rad listad som ledig
                if (!spotTaken && table.Select($"Plats = '{currentRow}' AND Båttyp = 'Ledigt'").FirstOrDefault() == null)
                {
                    freeSpot = table.NewRow();
                    freeSpot["Plats"] = currentRow;
                    freeSpot["Båttyp"] = "Ledigt";
                    table.Rows.Add(freeSpot);

                }
            }
        }



        public static void ListAllFreeSpotsInPort(Port port)
        {

            DataTable table = boatInfo.Tables[port.PortName];
            DataRow freeSpot;
            int currentSpot = 1;


            foreach (bool spotTaken in port.OccupiedSpots)
            {
                //om det inte står någon båt på platsen och platsen inte redan är listad som ledig
                if (!spotTaken && table.Select($"Plats = '{currentSpot}' AND Båttyp = 'Ledigt'").FirstOrDefault() == null)
                {
                    freeSpot = table.NewRow();
                    freeSpot["Plats"] = currentSpot;
                    freeSpot["Båttyp"] = "Ledigt";
                    table.Rows.Add(freeSpot);

                }

                currentSpot++;
            }
        }


        public static void UpdatePort(Port port, Boat boat = null)
        {
            DataTable table = boatInfo.Tables[port.PortName];
            if (boat == null) // om boat är null har jag bestämt att det är då den laddas från fil
            {
                foreach (Boat b in port.Boats)
                {
                    UpdateSingleBoat(table, b);
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
                //tar bort raderna som är listade som lediga
                emptySpot = table.Select($"Plats = '{i}' AND Båttyp = 'Ledigt'").FirstOrDefault();
                if (emptySpot != null)
                {
                    table.Rows.Remove(emptySpot);
                }
            }
            //lägger till båten i tabellen
            AddBoatToTable(table, boat);
        }


        /// <summary>
        /// Uppdaterar kolumnen Dagar på båten 
        /// </summary>
        /// <param name="boat"></param>
        /// <param name="port"></param>
        public static void AddTimeToBoat(Boat boat, string tableName)
        {
            DataTable table = boatInfo.Tables[tableName];
            //väljer båtens rad i tabellen och ändrar tiden vid hamnen
            DataRow currentBoat = table.Select($"Nr = '{boat.ModelID}' AND Vikt = '{boat.Weight} kg'").FirstOrDefault();
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
        static void AddBoatToTable(DataTable table, Boat boat)
        {

            try
            {
                DataRow currentBoat = table.NewRow();
                currentBoat["Plats"] = boat.GetSpot;
                currentBoat["Båttyp"] = boat.GetBoatType();
                currentBoat["Vikt"] = boat.Weight + " kg";
                currentBoat["Nr"] = boat.ModelID;
                currentBoat["Maxhast"] = boat.Speed;
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




















//public static void FillEmpty(Port port)
//{
//    DataTable table = boatInfo.Tables[port.PortName];

//    DataRow freeSpot;
//    for (int i = 1; i <= port.Spots; i++)
//    {
//        freeSpot = table.NewRow();
//        freeSpot["Plats"] = i;
//        freeSpot["Båttyp"] = "Ledigt";
//        freeSpot["Dagar"] = "";
//        table.Rows.Add(freeSpot);
//    }
//}






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