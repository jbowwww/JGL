using System;
using System.Collections.Generic;
using JGL.Heirarchy;
using JGL.Geometry;
using JGL.Debugging;

namespace JGL.Heirarchy.Library
{
	/// <summary>
	/// A grid mesh
	/// </summary>
	public class Grid : Mesh
	{
		/// <summary>
		/// Tracing
		/// </summary>
		public static readonly AutoTraceSource Trace = AutoTraceSource.GetOrCreate(AsyncXmlFileTraceListener.GetOrCreate("JGL"));

		/// <summary>
		/// Gets or sets the size of the <see cref="Grid"/> in the X dimension
		/// </summary>
		public double XSize { get; private set; }

		/// <summary>
		/// Gets or sets the size of the <see cref="Grid"/> in the Z dimension
		/// </summary>
		public double ZSize { get; private set; }

		/// <summary>
		/// Gets or sets the X resolution (The number of points in the X dimension that the <see cref="Grid"/> contains, minus one)
		/// </summary>
		public int XResolution { get; private set; }

		/// <summary>
		/// Gets or sets the X resolution (The number of points in the Z dimension that the <see cref="Grid"/> contains, minus one)
		/// </summary>
		public int ZResolution { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="JGL.Heirarchy.Library.Grid"/> class.
		/// </summary>
//		public Grid(string name = null)
//			: base(name)
//		{
//			Init(1, 1);
//		}

		/// <summary>
		/// Initializes a new instance of the <see cref="JGL.Heirarchy.Library.Quad"/> class.
		/// </summary>
		/// <param name="name">Entity name</param>
		/// <param name="xSize">X size</param>
		/// <param name="zSize">Z size</param>
		/// <param name="xResolution">X resolution (number of points used in X dimension, minus one)</param>
		/// <param name="zResolution">Z resolution (number of points used in Z dimension, minus one)</param>
		public Grid(string name, double xSize, double zSize, int xResolution, int zResolution)
			: base(name)
		{
			Init(xSize, zSize, xResolution, zResolution);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="JGL.Heirarchy.Library.Quad"/> class.
		/// </summary>
		/// <param name="xSize">X size</param>
		/// <param name="zSize">Z size</param>
		/// <param name="xResolution">X resolution (number of points used in X dimension, minus one)</param>
		/// <param name="zResolution">Z resolution (number of points used in Z dimension, minus one)</param>
		public Grid(double xSize, double zSize, int xResolution, int zResolution)
			: base(null)
		{
			Init(xSize, zSize, xResolution, zResolution);
		}

		/// <summary>
		/// Init the specified xSize and zSize.
		/// </summary>
		/// <param name="xSize">X size</param>
		/// <param name="zSize">Z size</param>
		/// <param name="xResolution">X resolution (number of points used in X dimension, minus one)</param>
		/// <param name="zResolution">Z resolution (number of points used in Z dimension, minus one)</param>
		/// <remarks>
		///	-	TODO: Finish this
		///		-	Need to think about how to construct/render trianglefaces in the grid
		/// </remarks>
		protected void Init(double xSize, double zSize, int xResolution, int zResolution)
		{
			int numVertices = (xResolution + 1) * (zResolution + 1);
			double x = -xSize / 2;
			double z = -zSize / 2;
			int vi;
			Vertex[] v = new Vertex[numVertices];

			for (int zi = 0; zi < zResolution; zi++)
			{
				for (int xi = 0; xi < xResolution; xi++)
				{
					vi = zi * xResolution + xi;
					v[vi] = new Vertex(x, 0, z);
				}
			}
		}
	}
}

