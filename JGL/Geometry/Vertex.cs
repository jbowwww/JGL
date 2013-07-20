using System;
using JGL.Debugging;

namespace JGL.Geometry
{
	public class Vertex : Vector3d
	{
		/// <summary>
		/// Tracing
		/// </summary>
		public static readonly AutoTraceSource Trace = AutoTraceSource.GetOrCreate(AsyncXmlFileTraceListener.GetOrCreate("JGL"));

		public Vertex() {}
		
		public Vertex(double x, double y, double z)
			: base (x, y, z) {}
		
		public Vertex(double[] components)
			: base (components) {}
	}
}

