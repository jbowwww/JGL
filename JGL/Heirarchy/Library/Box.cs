using System;
using System.Collections.Generic;
using JGL.Heirarchy;
using JGL.Geometry;
using JGL.Debugging;

namespace JGL.Heirarchy.Library
{
	/// <summary>
	/// A mesh representation of a box in 3D space
	/// </summary>
	public class Box : Mesh
	{
		/// <summary>
		/// Tracing
		/// </summary>
		public new static readonly AutoTraceSource Trace = AutoTraceSource.GetOrCreate(AsyncXmlFileTraceListener.GetOrCreate("JGL"));

		#region Box texturise methods
		/// <summary>
		/// Texturise method arguments base class.
		/// </summary>
		public abstract class BoxTexturiseMethod
		{
			/// <summary>
			/// Texturise this instance.
			/// </summary>
			public abstract void Texturise(Box box);
		}

		/// <summary>
		/// Each face of the <see cref="Box"/> displays the <see cref="Texture"/>, repeated <see cref="TexturiseEachFace.URepeat"/>
		/// times in the U dimension and <see cref="TexturiseEachFace.VRepeat"/> in the V direction
		/// </summary>
		public class TexturiseEachFace : BoxTexturiseMethod
		{
			/// <summary>
			/// How many times to repeat the <see cref="Texture"/> in the X (U) direction
			/// </summary>
			int URepeat = 1;

			/// <summary>
			/// How many times to repeat the <see cref="Texture"/> in the Y (V) direction
			/// </summary>
			int VRepeat = 1;

			/// <summary>
			/// Texturise this instance.
			/// </summary>
			public override void Texturise(Box box)
			{
				box.VertexData.TexCoords = new TexCoord[]
				{
					new TexCoord(0, 0), new TexCoord(0, VRepeat),
					new TexCoord(URepeat, VRepeat), new TexCoord(URepeat, 0)
				};
				for (int i = 0; i < 12; i+=2)
				{
					box.Triangles[i].T = new int[] { 2, 1, 0 };
					box.Triangles[i + 1].T = new int[] { 0, 2, 3 };
				}
			}
		}

		/// <summary>
		/// Texturise whole box.
		/// </summary>
		public class TexturiseWholeBox : BoxTexturiseMethod
		{
			/// <summary>
			/// Generates texture coordinates that will wrap a single <see cref="Texture"/> around the whole box
			/// </summary>
			public override void Texturise(Box box)
			{
				box.VertexData.TexCoords = new TexCoord[]
				{
					new TexCoord(0, 0), new TexCoord(0, 0.3333333), new TexCoord(0, 0.6666666), new TexCoord(0, 1),
					new TexCoord(0.25, 0), new TexCoord(0.25, 0.3333333), new TexCoord(0.25, 0.6666666), new TexCoord(0.25, 1),
					new TexCoord(0.5, 0), new TexCoord(0.5, 0.3333333), new TexCoord(0.5, 0.6666666), new TexCoord(0.5, 1),
					new TexCoord(0.75, 0), new TexCoord(0.75, 0.3333333), new TexCoord(0.75, 0.6666666), new TexCoord(0.75, 1),
					new TexCoord(1, 0), new TexCoord(1, 0.3333333), new TexCoord(1, 0.6666666), new TexCoord(1, 1)
				};
				box.Triangles[0].T = new int[] { 6, 2, 1 };
				box.Triangles[1].T = new int[] { 5, 6, 2 };
				box.Triangles[2].T = new int[] { 10, 6, 5 };
				box.Triangles[3].T = new int[] { 9, 10, 6 };
				box.Triangles[4].T = new int[] { 14, 10, 9 };
				box.Triangles[5].T = new int[] { 13, 14, 10 };
				box.Triangles[6].T = new int[] { 18, 14, 1 };
				box.Triangles[7].T = new int[] { 17, 18, 14 };
				box.Triangles[8].T = new int[] { 3, 2, 7 };
				box.Triangles[9].T = new int[] { 2, 6, 7 };
				box.Triangles[10].T = new int[] { 4, 5, 1 };
				box.Triangles[11].T = new int[] { 4, 1, 0 };
			}
		}

		/// <summary>
		/// Texturise each face with default parameters
		/// </summary>
		/// <remarks>
		/// <see cref="TexturiseEachFace.URepeat"/> = 1, <see cref="TexturiseEachFace.VRepeat"/> = 1
		/// </remarks>
		public readonly BoxTexturiseMethod DefaultTexturiseEachFace = new TexturiseEachFace();

		/// <summary>
		/// Texturise whole box with default parameters
		/// </summary>
		public readonly BoxTexturiseMethod DefaultTexturiseWholeBox = new TexturiseWholeBox();
		#endregion

		/// <summary>
		/// Initializes a new instance of the <see cref="JGL.Heirarchy.Library.Box"/> class.
		/// </summary>
		/// <param name="texturiseMethod">A <see cref="BoxTexturiseMethod"/> instance, or null to use <see cref="DefaultTexturiseEachFace"/></param>
		public Box(BoxTexturiseMethod texturiseMethod = null)
			: base(null)
		{
			Init(1, 1, 1, texturiseMethod);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="JGL.Heirarchy.Library.Box"/> class.
		/// </summary>
		/// <param name="name">Name</param>
		/// <param name="texturiseMethod">A <see cref="BoxTexturiseMethod"/> instance, or null to use <see cref="DefaultTexturiseEachFace"/></param>
		public Box(string name, BoxTexturiseMethod texturiseMethod = null)
			: base(name)
		{
			Init(1, 1, 1, texturiseMethod);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="JGL.Heirarchy.Library.Box"/> class.
		/// </summary>
		/// <param name="name">Name</param>
		/// <param name="xSize">X size</param>
		/// <param name="ySize">Y size</param>
		/// <param name="zSize">Z size</param>
		/// <param name="texturiseMethod">A <see cref="BoxTexturiseMethod"/> instance, or null to use <see cref="DefaultTexturiseEachFace"/></param>
		public Box(string name, double xSize, double ySize, double zSize, BoxTexturiseMethod texturiseMethod = null)
			: base(name)
		{
			Init(xSize, ySize, zSize, texturiseMethod);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="JGL.Heirarchy.Library.Box"/> class.
		/// </summary>
		/// <param name="xSize">X size</param>
		/// <param name="ySize">Y size</param>
		/// <param name="zSize">Z size</param>
		/// <param name="texturiseMethod">A <see cref="BoxTexturiseMethod"/> instance, or null to use <see cref="DefaultTexturiseEachFace"/></param>
		public Box(double xSize, double ySize, double zSize, BoxTexturiseMethod texturiseMethod = null)
			: base(null)
		{
			Init(xSize, ySize, zSize, texturiseMethod);
		}

		/// <summary>
		/// Init the specified xSize, ySize, zSize and material.
		/// </summary>
		/// <param name="xSize">X size</param>
		/// <param name="ySize">Y size</param>
		/// <param name="zSize">Z size</param>
		/// <param name="texturiseMethod">A <see cref="BoxTexturiseMethod"/> instance, or null to use <see cref="DefaultTexturiseEachFace"/></param>
		protected void Init(double xSize, double ySize, double zSize, BoxTexturiseMethod texturiseMethod)
		{
			double x = xSize / 2;
			double y = ySize / 2;
			double z = zSize / 2;
			Vertex[] v = new Vertex[]
			{
				new Vertex(-x, -y, +z), new Vertex(-x, +y, +z), new Vertex(+x, +y, +z), new Vertex(+x, -y, +z),
				new Vertex(-x, -y, -z), new Vertex(-x, +y, -z), new Vertex(+x, +y, -z), new Vertex(+x, -y, -z)
			};
			Normal[] n = new Normal[]
			{
				new Normal(0, 0, +1), new Normal(+1, 0, 0), new Normal(0, 0, -1),
				new Normal(-1, 0, 0), new Normal(0, +1, 0), new Normal(0, -1, 0)
			};
			VertexData = new VertexData() { Vertices = v, Normals = n }; 	//, TexCoords = t };
			Triangles = new List<TriangleFace>(new TriangleFace[]
			{
				new TriangleFace(new int[] { 2, 1, 0 }, new int[] { 0, 0, 0 }),//, new int[] { 2, 1, 0 }),
				new TriangleFace(new int[] { 3, 2, 0 }, new int[] { 0, 0, 0 }),//, new int[] { 3, 2, 0 }),
				new TriangleFace(new int[] { 6, 2, 3 }, new int[] { 1, 1, 1 }),//, new int[] { 2, 1, 0 }),
				new TriangleFace(new int[] { 7, 6, 3 }, new int[] { 1, 1, 1 }),//, new int[] { 3, 2, 0 }),

				new TriangleFace(new int[] { 5, 6, 7 }, new int[] { 2, 2, 2 }),//, new int[] { 2, 1, 0 }),
				new TriangleFace(new int[] { 4, 5, 7 }, new int[] { 2, 2, 2 }),//, new int[] { 3, 2, 0 }),
				new TriangleFace(new int[] { 1, 5, 4 }, new int[] { 3, 3, 3 }),//, new int[] { 2, 1, 0 }),
				new TriangleFace(new int[] { 0, 1, 4 }, new int[] { 3, 3, 3 }),//, new int[] { 3, 2, 0 }),

				new TriangleFace(new int[] { 1, 5, 6 }, new int[] { 4, 4, 4 }),//, new int[] { 2, 1, 0 }),
				new TriangleFace(new int[] { 5, 6, 2 }, new int[] { 4, 4, 4 }),//, new int[] { 3, 2, 0 }),
				new TriangleFace(new int[] { 3, 7, 4 }, new int[] { 5, 5, 5 }),//, new int[] { 2, 1, 0 }),
				new TriangleFace(new int[] { 3, 4, 0 }, new int[] { 5, 5, 5 }),//, new int[] { 3, 2, 0 }),
			});
			if (texturiseMethod == null)
				texturiseMethod = DefaultTexturiseEachFace;
			texturiseMethod.Texturise(this);
		}
	}
}

