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
                case "MOTORBOAT":
                    prefix = "M-";
                    ID = GenerateName(prefix);
                    topSpeedKnots = rand.Next(60); //upp till 60 knop
                    weight = rand.Next(200, 3000 + 1); // exclusive max därav +1
                    specialProperty = rand.Next(10, 1000 + 1); //hästkrafter i det här fallet
                    return new Motorboat(ID, weight, topSpeedKnots, specialProperty);
                case "ROWBOAT":
                    prefix = "M-";
                    ID = GenerateName(prefix);
                    topSpeedKnots = rand.Next(3); //upp till 3 knop
                    weight = rand.Next(100, 300 + 1); // exclusive max därav +1
                    specialProperty = rand.Next(1, 6 + 1); //antal passagerare i det här fallet
                    return new Rowboat(ID, weight, topSpeedKnots, specialProperty);
                case "SAILBOAT":
                    prefix = "S-";
                    ID = GenerateName(prefix);
                    topSpeedKnots = rand.Next(60); //upp till 60 knop
                    weight = rand.Next(200, 3000 + 1); // exclusive max därav +1
                    specialProperty = rand.Next(10, 1000 + 1); //hästkrafter i det här fallet
                    return new Sailboat(ID, weight, topSpeedKnots, specialProperty);
                case "CARGOSHIP":
                    prefix = "L-";
                    ID = GenerateName(prefix);
                    topSpeedKnots = rand.Next(60); //upp till 60 knop
                    weight = rand.Next(200, 3000 + 1); // exclusive max därav +1
                    specialProperty = rand.Next(10, 1000 + 1); //hästkrafter i det här fallet
                    return new Cargoship(ID, weight, topSpeedKnots, specialProperty);
                default: // catamaran
                    prefix = "K-";
                    ID = GenerateName(prefix);
                    topSpeedKnots = rand.Next(60); //upp till 60 knop
                    weight = rand.Next(200, 3000 + 1); // exclusive max därav +1
                    specialProperty = rand.Next(10, 1000 + 1); //hästkrafter i det här fallet
                    return new Catamaran(ID, weight, topSpeedKnots, specialProperty);
            }
        }

        static string GenerateName(string prefix)
        {
            char[] allowedLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            for (int i = 0; i < 3; i++)
            {
                int index = rand.Next(allowedLetters.GetUpperBound(0));
                prefix += allowedLetters[index];
            }
            return prefix;
        }
    }
}

