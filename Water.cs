using System;
using System.Collections.Generic;
using System.Text;

namespace HamnSimulering
{
    class Water
    {
        public static List<Boat> Waiting = new List<Boat>();

        public void MoreBoats(int ammount)
        {
            for(int i = 1; i <= ammount; i++)
            {

            }
        }

        public static void AddBoat(Boat b)
        {
            Waiting.Add(b);
        }
    }
}
