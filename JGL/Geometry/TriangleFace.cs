using System;
using JGL.Debugging;

namespace JGL.Geometry
{
	public class TriangleFace
	{
		/// <summary>
		/// Tracing
		/// </summary>
		public static readonly AutoTraceSource Trace = AutoTraceSource.GetOrCreate(AsyncXmlFileTraceListener.GetOrCreate("JGL"));
		public int[] V;
		public int[] N;
		public int[] T;

		public TriangleFace(int[] v, int[] n = null, int[] t = null)
		{
			V = v;
			N = n;
			T = t;
		}
	}
}

