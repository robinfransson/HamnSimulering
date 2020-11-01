using System;
using System.Collections.Generic;
using System.Text;

namespace HamnSimulering
{
    class Positions
    {
		public int[] Position { get; set; }
		public string Port { get; set; }

		/// <summary>
		/// Positionen i en hamn
		/// </summary>
		/// <param name="pos">Vilken/vilka positioner som ska läggas till</param>
		/// <param name="from">Från vilken kaj positionerna kom</param>
		public Positions(int[] pos, string from)
		{
			Position = pos;
			Port = from;
		}
	}
}

