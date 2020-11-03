using System;
using System.Collections.Generic;
using System.Text;

namespace HamnSimulering
{
    class Position
    {
		public int[] Spot { get; set; }
		public string Port { get; set; }

		/// <summary>
		/// Positionen i en hamn
		/// </summary>
		/// <param name="spot">Vilken/vilka positioner som ska läggas till</param>
		/// <param name="from">Från vilken kaj positionerna kom</param>
		public Position(int[] spot, string from)
		{
			Spot = spot;
			Port = from;
		}
	}
}

