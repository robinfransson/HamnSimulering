﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Media3D;

namespace HamnSimulering
{
    class SaveFileManager
    {

        public static void Save(List<Boat> boats, string fileName)
        {
            File.WriteAllLines(fileName, DataToStore(boats));
        }

        public static void DeleteSaves()
        {
            string[] saveFiles = new string[2] { "stats.txt", "boats.txt"};
            foreach (string file in saveFiles)
            {
                File.Delete(file);
            }
        }



        public static List<Boat> Load(string fileName)
        {
            if (!File.Exists(fileName))
            {
                return new List<Boat>();
            }
            else
            {
                try
                {
                    return LoadFromFile(fileName);
                }
                catch(Exception e)
                {
                    MessageBox.Show(e.Message + "\n" + fileName);
                    return new List<Boat>();
                }
            }
        }




        public static void LoadStatistics(string fileName)
        {
            int daysPassed = 0;
            int boatsRejected = 0;
            int boatsAccepted = 0;
            int boatsPerDay = 5; // 5 ska vara default
            if (File.Exists(fileName))
            {
                foreach (string line in File.ReadAllLines(fileName))
                {
                    string[] saveInfo = line.Split("=");
                    if (saveInfo[0] == "days passed")
                    {
                        daysPassed = Int32.Parse(saveInfo[1]);
                    }
                    else if (saveInfo[0] == "boats rejected")
                    {
                        boatsRejected = Int32.Parse(saveInfo[1]);
                    }
                    else if (saveInfo[0] == "boats accepted")
                    {
                        boatsAccepted = Int32.Parse(saveInfo[1]);
                    }
                    else if (saveInfo[0] == "boats per day")
                    {
                        boatsPerDay = Int32.Parse(saveInfo[1]);
                    }
                }
            }

            Simulate.BoatsPerDay = boatsPerDay;
            Simulate.BoatsRejected = boatsRejected;
            Simulate.BoatsAccepted = boatsAccepted;
            Simulate.DaysPassed = daysPassed;
        }

        static Boat AddBoatFromFile(string id, int weight, int topSpeed, int specialProp, int daysAtHarbour, int[] spotsTaken, string port)
        {
            char boatModel = id[0];//första bokstaven i modell id
            return boatModel switch
            {
                'R' => new Rowboat(id, weight, topSpeed, specialProp, daysAtHarbour, spotsTaken, port),
                'M' => new Motorboat(id, weight, topSpeed, specialProp, daysAtHarbour, spotsTaken, port),
                'S' => new Sailboat(id, weight, topSpeed, specialProp, daysAtHarbour, spotsTaken, port),
                'L' => new Cargoship(id, weight, topSpeed, specialProp, daysAtHarbour, spotsTaken, port),
                'K' => new Catamaran(id, weight, topSpeed, specialProp, daysAtHarbour, spotsTaken, port),
                _ => throw new NotImplementedException("Unsupported boattype! Model: " + id),
            };
        }

        static List<Boat> LoadFromFile(string fileName)
        {
            
            List<Boat> loadedFromFile = new List<Boat>();

            //skippar första raden för att det är en rad som visar 
            //vilket format datat är sparat i
            string[] myFile = File.ReadAllLines(fileName).Skip(1).ToArray();


            foreach (string dataLine in myFile)
            {
                if (dataLine == string.Empty )
                {
                    break;
                }
                string[] saveData = dataLine.Split(";");
                string ModelID = saveData[0];
                int topSpeedKnots = Int32.Parse(saveData[1]);
                int weight = Int32.Parse(saveData[2]);
                int daysSpent = Int32.Parse(saveData[3]);
                int specialProperty = Int32.Parse(saveData[4]);
                int[] spots = null;
                string port = saveData[6];

                //om det finns platser reserverade
                if (saveData[5] != "?")
                {
                    string[] spotsTaken = saveData[5].Split("-");
                    spots = spotsTaken.GetUpperBound(0) < 1 ? new int[1] { Int32.Parse(spotsTaken[0]) } :
                                                            new int[2] { Int32.Parse(spotsTaken[0]), Int32.Parse(spotsTaken[1]) };
                }
                

                loadedFromFile.Add(AddBoatFromFile(ModelID, weight, topSpeedKnots, specialProperty, daysSpent, spots, port));
            }
            return loadedFromFile;
        }

        static string[] DataToStore(List<Boat> boats)
        {
            //antalet båtar som ska sparas
            int saveDataLength = boats.Count;
            //plussar på 1 för att jag ska spara en rad med formatet så det är lättare att förstå  
            //hur datat sparas
            string[] saveData = new string[saveDataLength+1];
            char dataSeparator = ';';
            saveData[0] = "[ID;Maxfart(KNOP);Vikt;Dagar vid hamnen;Båtens special property;(plats vid hamnen, ? = not set)];(vilken hamn, ?=not set)";
            int i = 1;
            foreach (Boat boat in boats.OrderBy(b => b.IsInPort))
            {
                string boatData = "";
                int? numberOfSpots = boat.AssignedSpot?.Length;
                 
                //generell info om båtarna först, sedan tas varje "special" property ut och läggs till på slutet av strängen
                boatData += $"{boat.ModelID}" + dataSeparator;
                boatData += $"{boat.TopSpeedKnots}" + dataSeparator;
                boatData += $"{boat.Weight}" + dataSeparator;
                boatData += $"{boat.DaysSpentAtHarbour}" + dataSeparator;

                switch(boat)
                {
                    case Rowboat row:
                        boatData += $"{row.MaxCapacity}";
                        break;
                    case Cargoship cargo:
                        boatData += $"{cargo.Containers}";
                        break;
                    case Sailboat sail:
                        boatData += $"{sail.BoatLength}";
                        break;
                    case Motorboat motor:
                        boatData += $"{motor.Horsepowers}";
                        break;
                    case Catamaran cata:
                        boatData += $"{cata.NumberOfBeds}";
                        break;
                    default:
                        throw new NotImplementedException("Unsupported boat type: " + boat.GetType());
                }


                boatData += dataSeparator;
                switch (numberOfSpots) //längden av arrayen där platserna sparas
                {
                    case 1:
                        boatData += $"{boat.AssignedSpot[0]}";
                        break;
                    case 2:
                        boatData += $"{boat.AssignedSpot[0]}-{boat.AssignedSpot[1]}";
                        break;
                    default:
                        boatData += "?";
                        break;
                }
                boatData += dataSeparator;

                switch(boat.IsInPort)
                {
                    case null:
                        boatData += "?";
                        break;
                    default:
                        boatData += boat.IsInPort;
                        break;
                }
                saveData[i] = boatData;
                i++;
            }
            return saveData;
        }

        public static void SaveStatistics(string fileName)
        {
            string[] statsToSave = { $"days passed={Simulate.DaysPassed}", 
                                     $"boats rejected={Simulate.BoatsRejected}", 
                                     $"boats accepted={Simulate.BoatsAccepted}", 
                                     $"boats per day={Simulate.BoatsPerDay}"};
            File.WriteAllLines(fileName, statsToSave);
        }
    }
}
