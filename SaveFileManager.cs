using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media.Media3D;

namespace HamnSimulering
{
    class SaveFileManager
    {
        public static void Save(List<Boat> boats, string fileName)
        {
            if(!SaveFileExists(fileName))
            {
                CreateSaveFile(fileName);
            }
            File.WriteAllText(fileName, DataToStore(boats));
        }
        public static List<Boat> Load(string fileName)
        {
            if(!SaveFileExists(fileName))
            {
                return new List<Boat>();
            }
            else
            {
                return LoadFromFile(fileName);
            }
        }

        static bool SaveFileExists(string fileName)
        {
            if(File.Exists(fileName))
            {
                return true;
            }
            return false;
        }

        static void CreateSaveFile(string fileName)
        {
            File.Create(fileName);
        }

        static List<Boat> LoadFromFile(string fileName)
        {
            Func<string, int, int, int, int, Boat> addBoat = (id, weight, topSpeed, specialProp, daysAtHarbour) =>
            {

                return (id[0]) switch //första bokstaven i modell id
                {
                    'R' => new Rowboat(id, weight, topSpeed, specialProp, daysAtHarbour),
                    'M' => new Motorboat(id, weight, topSpeed, specialProp, daysAtHarbour),
                    'S' => new Sailboat(id, weight, topSpeed, specialProp, daysAtHarbour),
                    'L' => new Cargoship(id, weight, topSpeed, specialProp, daysAtHarbour),
                    //default
                    _ => new Catamaran(id, weight, topSpeed, specialProp, daysAtHarbour),
                };
            };
            List<Boat> loadedFromFile = new List<Boat>();
            string[] myFile = File.ReadAllLines(fileName);
            foreach (string boatFromFile in myFile)
            {
                string[] saveData = boatFromFile.Split(";");
                string ModelID = saveData[0];
                int topSpeedKnots = Int32.Parse(saveData[1]);
                int weight = Int32.Parse(saveData[2]);
                int daysSpent = Int32.Parse(saveData[3]);
                int specialProperty = Int32.Parse(saveData[4]);
                loadedFromFile.Add(addBoat(ModelID, weight, topSpeedKnots, specialProperty, daysSpent));
            }
            return loadedFromFile;
        }

        static string DataToStore(List<Boat> boats)
        {
            string saveData = null;
            int i = 1;
            int lastIndex = boats.Count;
            foreach(Boat b in boats)
            {
                //generell info om båtarna först, sedan tas varje "special" property ut och läggs till på slutet av strängen
                saveData += $"{b.ModelID};{b.TopSpeedKnots};{b.Weight};{b.DaysSpentAtHarbour};"; 
                if (b is Rowboat row)
                {
                    saveData += $"{row.MaxCapacity}";
                }
                else if(b is Cargoship cargo)
                {
                    saveData += $"{cargo.CurrentCargo}";
                }
                else if( b is Sailboat sail)
                {
                    saveData += $"{sail.BoatLength}";
                }
                else if(b is Motorboat motor)
                {
                    saveData += $"{motor.Horsepowers}";
                }
                else // catamaran
                {
                    saveData += $"{((Catamaran)b).NumberOfBeds}";
                }
                if (i != lastIndex)
                {
                    saveData += "\n";
                }
                i++;
            }
            return saveData;
        }
    }
}
