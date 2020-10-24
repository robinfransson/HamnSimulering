using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows;

namespace HamnSimulering
{
    class BoatData
    {

        static DataSet boatInfo = new DataSet("BoatInfo");



        public static DataView BoatDataViewer(string table)
        {
            Func<string, char, int?> splitColumnData = new Func<string, char, int?>
                ((data, splitAt) => data is null ? 32 : int.Parse(data?.Split(splitAt)[0]));
            var boats = boatInfo.Tables[table].AsEnumerable();
            var q1 = boats
                                .OrderBy(row => splitColumnData(row.Field<string>("Plats"), '-'))
                                .ThenBy(row => row.Field<string>("Dagar vid hamnen") != null)
                                .ThenBy(row => splitColumnData(row.Field<string>("Dagar vid hamnen"), '/'));
            //var q1 = assignedSpot.OrderBy(days => daysAtHarbour);
            EnumerableRowCollection<DataRow> query = from boat in boats
                                                     orderby int.Parse(boat.Field<string>("Plats").Split("-")[0]) ascending
                                                     orderby int.Parse(boat.Field<string>("Dagar vid hamnen").Split("/")[0]) descending
                                                     select boat;
            //return q1.AsDataView();
            //return query.AsDataView();
            return q1.AsDataView();
        }


        public static void SetupDataTables()
        {
            for (int i = 0; i < 2; i++)
            {
                DataTable harbourTable = new DataTable
                {
                    Columns = { { "Plats", typeof(string) }, { "Båttyp", typeof(string) }, { "Vikt", typeof(int) },
                                { "Nr", typeof(string) }, { "Maxhastighet", typeof(string) },
                                { "Dagar vid hamnen", typeof(string) }, { "Övrigt", typeof(string) }, }
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
                Columns = { { "Båttyp", typeof(string) }, { "Vikt", typeof(int) },
                                { "Nr", typeof(string) }, { "Maxhastighet", typeof(string) }, { "Övrigt", typeof(string) }, }
            };

            boatInfo.Tables.Add(waitingBoatsTable);
        }





        public static DataTable Info(string tableName)
        {
            return boatInfo.Tables[tableName];
        }


        public static void ListFreeSpots(bool[] spotsInUse, DataTable table)
        {
            int currentSpot = 1;
            DataRow freeSpot;
            foreach (bool spot in spotsInUse)
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
                    newWaitingBoat["Vikt"] = boat.Weight;
                    newWaitingBoat["Nr"] = boat.ModelID;
                    newWaitingBoat["Maxhastighet"] = boat.TopSpeedKMH;
                    newWaitingBoat["Övrigt"] = boat.SpecialProperty;
                    boatsWaiting.Rows.Add(newWaitingBoat);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.ToString());
                }
            }
        }


        public static void RemoveBoat(DataTable table, Boat boat)
        {
            try
            {
                DataRow currentBoat = table.Select($"Nr = '{boat.ModelID}'").FirstOrDefault(); ;


                table.Rows.Remove(currentBoat);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }






        public static void UpdateHarbour(DataTable table, List<Boat> boats)
        {

            foreach (DataRow emptySpot in table.Select("Båttyp = 'Ledigt'"))
            {
                table.Rows.Remove(emptySpot);
            }
            foreach (Boat boat in boats)
            {
                AddOrModifyBoat(table, boat);
            }

        }

        static void AddOrModifyBoat(DataTable table, Boat boat)
        {
            DataRow currentBoat = table.Select($"Nr = '{boat.ModelID}'").FirstOrDefault();
            if (currentBoat != null)
            {
                currentBoat["Dagar vid hamnen"] = $"{boat.DaysSpentAtHarbour}/{boat.MaxDaysAtHarbour}";
            }
            else
            {
                try
                {
                //    Columns = { // 
                //        { "Båttyp", typeof(string) }, { "Vikt", typeof(string) },
                //                { "Nr", typeof(string) }, { "Maxhastighet", typeof(string) }, { "Övrigt", typeof(string) }, }
                    currentBoat = table.NewRow();
                    currentBoat["Plats"] = boat.GetSpot();
                    currentBoat["Båttyp"] = boat.GetBoatType();
                    currentBoat["Vikt"] = boat.Weight;
                    currentBoat["Nr"] = boat.ModelID;
                    currentBoat["Maxhastighet"] = boat.TopSpeedKMH;
                    currentBoat["Dagar vid hamnen"] = $"{boat.DaysSpentAtHarbour}/{boat.MaxDaysAtHarbour}";
                    currentBoat["Övrigt"] = boat.SpecialProperty;
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
