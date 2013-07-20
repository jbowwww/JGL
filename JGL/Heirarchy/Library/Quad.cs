using System;
using System.Collections.Generic;
using JGL.Heirarchy;
using JGL.Geometry;
using JGL.Debugging;

namespace JGL.Heirarchy.Library
{
	/// <summary>
	/// A quad mesh - 4 points, two triangles
	/// </summary>
	public class Quad : Mesh
	{
		/// <summary>
		/// Tracing
		/// </summary>
		public static readonly AutoTraceSource Trace = AutoTraceSource.GetOrCreate(AsyncXmlFileTraceListener.GetOrCreate("JGL"));

		/// <summary>
		/// Initializes a new instance of the <see cref="JGL.Heirarchy.Library.Quad"/> class.
		/// </summary>
		/// <param name='name'>Entity name</param>
		public Quad(string name = null)
			: base(name)
		{
			Init(1, 1);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="JGL.Heirarchy.Library.Quad"/> class.
		/// </summary>
		/// <param name='name'>Entity name</param>
		/// <param name='xSize'>X size</param>
		/// <param name='zSize'>Z size</param>
		public Quad(string name, double xSize, double zSize)
			: base(name)
		{
			Init(xSize, zSize);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="JGL.Heirarchy.Library.Quad"/> class.
		/// </summary>
		/// <param name='xSize'>X size</param>
		/// <param name='zSize'>Z size</param>
		public Quad(double xSize, double zSize)
			: base(null)
		{
			Init(xSize, zSize);
		}

		/// <summary>
		/// Init the specified xSize and zSize.
		/// </summary>
		/// <param name='xSize'>X size</param>
		/// <param name='zSize'>Z size</param>
		protected void Init(double xSize, double zSize)
		{
			double x = xSize / 2;
			double z = zSize / 2;
			Vertex[] v = new Vertex[] { new Vertex(-x, 0, +z), new Vertex(+x, 0, +z), new Vertex(+x, 0, -z), new Vertex(-x, 0, -z) };
			Normal[] n = new Normal[] { new Normal(0, +1, 0) };
			TexCoord[] t = new TexCoord[] { new TexCoord(0, 0), new TexCoord(1, 0), new TexCoord(1, 1), new TexCoord(0, 1) };
			VertexData = new VertexData() { Vertices = v, Normals = n, TexCoords = t };
			Triangles = new List<TriangleFace>(new TriangleFace[]
			{
				new TriangleFace(new int[] { 0, 1, 2 }, new int[] { 0, 0, 0 }, new int[] { 0, 1, 2 }),
				new TriangleFace(new int[] { 0, 2, 3 }, new int[] { 0, 0, 0 }, new int[] { 0, 2, 3 }),
			});
		}
	}
}
