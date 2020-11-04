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

            //BoatType randomBoatType = (BoatType)rand.Next(Enum.GetNames(typeof(BoatType)).Length);
            Func<int, int, int> propertyRange = (min, max) => rand.Next(min, max + 1);
            Func<BoatType> selectRandomType = () => (BoatType)rand.Next(Enum.GetNames(typeof(BoatType)).Length);


            BoatType randomBoatType = selectRandomType();
            int topSpeedKnots;
            int weight;
            string ID;
            string prefix;
            int specialProperty;


            switch (randomBoatType)
            {


                case BoatType.ROWBOAT:
                    prefix = "R-";
                    ID = GenerateName(prefix);
                    weight = propertyRange(100, 300);
                    topSpeedKnots = propertyRange(0, 3); //upp till 3 knop
                    specialProperty = propertyRange(1, 6); //antal passagerare i det här fallet
                    return new Rowboat(ID, weight, topSpeedKnots, specialProperty);


                case BoatType.MOTORBOAT:
                    prefix = "M-";
                    ID = GenerateName(prefix);
                    weight = propertyRange(200, 3000);
                    topSpeedKnots = propertyRange(0, 60); //upp till 60 knop
                    specialProperty = propertyRange(10, 1000); //hästkrafter i det här fallet
                    return new Motorboat(ID, weight, topSpeedKnots, specialProperty);


                case BoatType.SAILBOAT:
                    prefix = "S-";
                    ID = GenerateName(prefix);
                    weight = propertyRange(800, 6000);
                    topSpeedKnots = propertyRange(0, 12);
                    specialProperty = propertyRange(10, 60); //fot i det här fallet
                    return new Sailboat(ID, weight, topSpeedKnots, specialProperty);


                case BoatType.CARGOSHIP:
                    prefix = "L-";
                    ID = GenerateName(prefix);
                    weight = propertyRange(3000, 20000);
                    topSpeedKnots = propertyRange(0, 20);
                    specialProperty = propertyRange(0, 500); //hästkrafter i det här fallet
                    return new Cargoship(ID, weight, topSpeedKnots, specialProperty);



                case BoatType.CATAMARAN:
                    prefix = "K-";
                    ID = GenerateName(prefix);
                    topSpeedKnots = propertyRange(0, 12); //upp till 12 knop
                    weight = propertyRange(1200, 8000); // exclusive max därav +1
                    specialProperty = propertyRange(1, 4); //sängplatser i det här fallet
                    return new Catamaran(ID, weight, topSpeedKnots, specialProperty);

                default:
                    throw new NotImplementedException("Not yet implemented!\n " + randomBoatType.ToString());
            }
        }

        static string GenerateName(string prefix)
        {
            char[] allowedLetters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();
            for (int i = 0; i < 3; i++)
            {
                int index = rand.Next(0, allowedLetters.Length);
                prefix += allowedLetters[index];
            }
            return prefix;
        }
    }
}

