using System;
using JGL.Debugging;

namespace JGL.Geometry
{
	public class Vertex : Vector3d
	{
		/// <summary>
		/// Tracing
		/// </summary>
		public static readonly AutoTraceSource Trace = new AutoTraceSource(AsyncFileTraceListener.GetOrCreate("JGL"));

		public Vertex() {}
		
		public Vertex(double x, double y, double z)
			: base (x, y, z) {}
		
		public Vertex(double[] components)
			: base (components) {}
	}
}

