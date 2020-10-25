using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Text;

namespace HamnSimulering
{
    class Generate
    {
        static Random rand = new Random();
        enum BoatType
        {
            MOTORBOAT,
            ROWBOAT,
            SAILBOAT,
            CARGOSHIP,
            CATAMARAN
        }

        public static Boat RandomBoat()
        {
            int topSpeedKnots;
            int weight;
            string ID;
            string prefix;
            int specialProperty;
            string randomBoatType = Enum.GetName(typeof(BoatType), rand.Next(Enum.GetNames(typeof(BoatType)).Length));
            switch (randomBoatType)
            {


                case "ROWBOAT":
                    prefix = "R-";
                    ID = GenerateName(prefix);
                    weight = rand.Next(100, 300 + 1);
                    topSpeedKnots = rand.Next(0, 3 + 1); //upp till 3 knop, exclusive max därav +1
                    specialProperty = rand.Next(1, 6 + 1); //antal passagerare i det här fallet
                    return new Rowboat(ID, weight, topSpeedKnots, specialProperty);


                case "MOTORBOAT":
                    prefix = "M-";
                    ID = GenerateName(prefix);
                    weight = rand.Next(200, 3000 + 1);
                    topSpeedKnots = rand.Next(0, 60 + 1); //upp till 60 knop
                    specialProperty = rand.Next(10, 1000 + 1); //hästkrafter i det här fallet
                    return new Motorboat(ID, weight, topSpeedKnots, specialProperty);


                case "SAILBOAT":
                    prefix = "S-";
                    ID = GenerateName(prefix);
                    weight = rand.Next(800, 6000 + 1);
                    topSpeedKnots = rand.Next(0, 12 + 1); //upp till 12 knop
                    specialProperty = rand.Next(10, 60 + 1); //fot i det här fallet
                    return new Sailboat(ID, weight, topSpeedKnots, specialProperty);
                
                
                case "CARGOSHIP":
                    prefix = "L-";
                    ID = GenerateName(prefix);
                    weight = rand.Next(3000, 20000 + 1);
                    topSpeedKnots = rand.Next(0, 20 + 1); //upp till 20 knop
                    specialProperty = rand.Next(0, 500 + 1); //hästkrafter i det här fallet
                    return new Cargoship(ID, weight, topSpeedKnots, specialProperty);


                default: // catamaran
                    prefix = "K-";
                    ID = GenerateName(prefix);
                    topSpeedKnots = rand.Next(0, 12 + 1); //upp till 12 knop
                    weight = rand.Next(1200, 8000 + 1); // exclusive max därav +1
                    specialProperty = rand.Next(10, 1000 + 1); //hästkrafter i det här fallet
                    return new Catamaran(ID, weight, topSpeedKnots, specialProperty);
            }
        }

        static string GenerateName(string prefix)
        {
            char[] allowedLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            for (int i = 0; i < 3; i++)
            {
                int index = rand.Next(0, allowedLetters.GetUpperBound(0)+1);
                prefix += allowedLetters[index];
            }
            return prefix;
        }
    }
}

