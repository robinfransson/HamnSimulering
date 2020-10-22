using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace HamnSimulering
{
    class BoatData
    {

        static DataSet boatInfo = new DataSet("BoatInfo");
        public static void Init()
        {
            SetupDataTables();
        }
        private static void SetupDataTables()
        {
            for (int i = 0; i < 2; i++)
            {
                DataTable harbourTable = new DataTable
                {
                    Columns = { { "Plats", typeof(string) }, { "Båttyp", typeof(string) }, { "Vikt", typeof(int) },
                                { "Nr", typeof(string) }, { "Maxhastighet", typeof(string) },
                                { "Dagar vid hamnen", typeof(int) }, { "Övrigt", typeof(string) }, }
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
                Columns = { { "Plats", typeof(string) }, { "Båttyp", typeof(string) }, { "Vikt", typeof(int) },
                                { "Nr", typeof(string) }, { "Maxhastighet", typeof(string) }, { "Övrigt", typeof(string) }, }
            };

            boatInfo.Tables.Add(waitingBoatsTable);
        }
        public static DataSet BoatDataSet()
        {
            return boatInfo;
        }
        public static DataTable Info(string tableName)
        {
            return boatInfo.Tables[tableName];
        }

        public static void UpdateTable(string table, string query)
        {
            DataTable currentTable = boatInfo.Tables[table];
            using (currentTable)
            {
            }
        }

        public static bool HasChanges()
        {
            return boatInfo.HasChanges();
        }

    }
}
