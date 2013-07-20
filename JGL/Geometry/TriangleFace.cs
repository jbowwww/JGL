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

		/// <summary>
		/// Initializes a new instance of the <see cref="JGL.Geometry.TriangleFace"/> class, by cloning another and then
		/// adding an adjustment to all of the vertex, nromal and texture coord indices. Used by <see cref="Object.MergeChildMeshes"/>
		/// </summary>
		/// <param name='source'>
		/// Source.
		/// </param>
		/// <param name='viAdjust'>
		/// Vi adjust.
		/// </param>
		/// <param name='niAdjust'>
		/// Ni adjust.
		/// </param>
		/// <param name='tiAdjust'>
		/// Ti adjust.
		/// </param>
		internal TriangleFace(TriangleFace source, int viAdjust = 0, int niAdjust = 0, int tiAdjust = 0)
		{
			if (source.V != null)
			{
				V = (int[])source.V.Clone();
				V[0]++; V[1]++; V[2]++;
			}
			if (source.N != null)
			{
				N = (int[])source.N.Clone();
				N[0]++;
				N[1]++;
				N[2]++;
			}
			if (source.T != null)
			{
				T = (int[])source.T.Clone();
				T[0]++;
				T[1]++;
				T[2]++;
			}
		}
	}
}

