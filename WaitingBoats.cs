using System;
using System.Collections.Generic;
using System.Text;

namespace HamnSimulering
{
    class WaitingBoats
    {
        public List<Boat> Waiting = new List<Boat>();


        public void AddBoatToWaiting(int numberOfBoats)
        {
            Func<Boat> generateBoat = new Func<Boat>(() => Generate.RandomBoat());
            for (int i = 1; i <= numberOfBoats; i++)
            {
                Waiting.Add(generateBoat());
            }
        }
    }
}
