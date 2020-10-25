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



       

        public static DataView BoatDataViewer(string table)
        {
            if(table == "WaitingBoats")
            {
                return boatInfo.Tables[table].DefaultView;
            }
            else
            {
                Func<string, char, int> splitColumnData = new Func<string, char, int>
                    ((data, splitAt) => int.Parse(data.Split(splitAt)[0]));



                var boats = boatInfo.Tables[table].AsEnumerable();
                var q1 = boats.OrderBy(row => splitColumnData(row.Field<string>("Plats"), '-'));
                return q1.AsDataView();
            }
        }


        public static void SetupDataTables()
        {
            for (int i = 0; i < 2; i++)
            {
                DataTable harbourTable = new DataTable
                {
                    Columns = { { "Plats", typeof(string) }, { "Båttyp", typeof(string) }, { "Vikt", typeof(int) },
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
                Columns = { { "Båttyp", typeof(string) }, { "Vikt", typeof(int) },
                                { "Nr", typeof(string) }, { "Maxhast", typeof(string) }, { "Övrigt", typeof(string) }, }
            };

            boatInfo.Tables.Add(waitingBoatsTable);
        }


        public static void ListFreeSpots(bool[] spotsInUse, string tableToModify)
        {
            DataTable table = boatInfo.Tables[tableToModify];
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
                    newWaitingBoat["Vikt"] = boat.Weight;
                    newWaitingBoat["Nr"] = boat.ModelID;
                    newWaitingBoat["Maxhast"] = boat.TopSpeedKMH;
                    newWaitingBoat["Övrigt"] = boat.SpecialProperty;
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






        public static void UpdateHarbour(List<Boat> boats, string tableToUpdate)
        {
            DataTable table = boatInfo.Tables[tableToUpdate];
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
