using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Security.Cryptography.Xml;
using System.Text;
using System.Windows.Controls;
using System.Windows.Data;

namespace HamnSimulering
{
    static class ExtensionMethods
    {
        public static void SetColumnNameAndWidth(this DataGrid grid, Dictionary<string, int> pairs)
        {

            foreach(var columnData in pairs)
            {
                DataGridTextColumn column = new DataGridTextColumn
                {
                    Header = columnData.Key,
                    Binding = new Binding(columnData.Key),
                    Width = columnData.Value
                };
                grid.Columns.Add(column);
            }
        }
    }
}
