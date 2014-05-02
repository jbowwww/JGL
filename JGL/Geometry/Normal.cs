using System;
using JGL.Debugging;

namespace JGL.Geometry
{
	public class Normal : Vector3d
	{
		/// <summary>
		/// Tracing
		/// </summary>
//		public static readonly AutoTraceSource Trace = AutoTraceSource.GetOrCreate(AsyncXmlFileTraceListener.GetOrCreate("JGL"));

//		public Normal() {}
		
		public Normal(double x, double y, double z)
			: base (x, y, z) {}
		
		public Normal(double[] components)
			: base (components) {}

		public Normal(Vector3d source)
			: base(source) {}
	}
}

