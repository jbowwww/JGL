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
		/// Gets or sets the size of the <see cref="Quad"/> in the X dimension
		/// </summary>
		public double XSize { get; private set; }

		/// <summary>
		/// Gets or sets the size of the <see cref="Quad"/> in the Z dimension
		/// </summary>
		public double ZSize { get; private set; }

		/// <summary>
		/// Simple texturise method
		/// </summary>
		/// <remarks>
		///	-	UV coords for vertices are (0,0), (1, 0), (1, 1), (0, 1) respectively
		/// </remarks>
		public class SimpleTexturise
			: TexturiseMethod<Quad>
		{
			/// <summary>
			/// The U repeat.
			/// </summary>
			public double URepeat = 1;

			/// <summary>
			/// The V repeat.
			/// </summary>
			public double VRepeat  = 1;

			/// <summary>
			/// Texturise the specified mesh. UV coords for vertices are (0,0), (1, 0), (1, 1), (0, 1) respectively.
			/// </summary>
			/// <param name="quad">Mesh</param>
			/// <remarks>Implemented abstract member of JGL.Heirarchy.TexturiseMethod[Quad]</remarks>
			public override void Texturise(Quad quad)
			{
				quad.VertexData.TexCoords = new TexCoord[]
				{
					new TexCoord(0, 0),
					new TexCoord(URepeat, 0),
					new TexCoord(URepeat, VRepeat),
					new TexCoord(0, VRepeat)
				};
				quad.Triangles[0].T = new int[] { 0, 1, 2 };
				quad.Triangles[1].T = new int[] { 0, 2, 3 };
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="JGL.Heirarchy.Library.Quad"/> class.
		/// </summary>
		/// <param name='name'>Entity name</param>
		public Quad(string name = null)
//			: base(name)
		{
			base.Name = name;
			Init(1, 1);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="JGL.Heirarchy.Library.Quad"/> class.
		/// </summary>
		/// <param name='name'>Entity name</param>
		/// <param name='xSize'>X size</param>
		/// <param name='zSize'>Z size</param>
		public Quad(string name, double xSize, double zSize)
//			: base(name)
		{
			base.Name = name;
			Init(xSize, zSize);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="JGL.Heirarchy.Library.Quad"/> class.
		/// </summary>
		/// <param name='xSize'>X size</param>
		/// <param name='zSize'>Z size</param>
		public Quad(double xSize, double zSize)
//			: base(null)
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
			XSize = xSize;
			ZSize = zSize;
			double x = xSize / 2;
			double z = zSize / 2;
			Vertex[] v = new Vertex[] { new Vertex(-x, 0, +z), new Vertex(+x, 0, +z), new Vertex(+x, 0, -z), new Vertex(-x, 0, -z) };
			Normal[] n = new Normal[] { new Normal(0, +1, 0) };
			VertexData = new VertexData() { Vertices = v, Normals = n };
			Triangles = new List<TriangleFace>(new TriangleFace[]
			{
				new TriangleFace(new int[] { 0, 1, 2 }, new int[] { 0, 0, 0 }),		// new int[] { 0, 1, 2 }),
				new TriangleFace(new int[] { 0, 2, 3 }, new int[] { 0, 0, 0 }),		// new int[] { 0, 2, 3 }),
			});
			Texturise<Quad>(new SimpleTexturise());
		}
	}
}
