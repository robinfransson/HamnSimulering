using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace HamnSimulering
{
    class DummyBoat : Boat
    {
        public DummyBoat(string id = "Tomt", int weight = 0, int topSpeedKnots=0) : base(id, weight, topSpeedKnots)
        {

        }
    }
}
