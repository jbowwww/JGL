using System;
using System.Collections.Generic;
using JGL.Heirarchy;
using JGL.Geometry;

namespace JGL.Heirarchy.Library
{
	/// <summary>
	/// A mesh representation of a box in 3D space
	/// </summary>
	public class Box : Mesh
	{
		/// <summary>
		/// Box texturise mode
		/// </summary>
		/// <remarks>Passed to texturise methods to indicate how texture coordinates are generated</remarks>
		public enum BoxTexturiseMode
		{
			/// <summary>
			/// Each face of the box displays the whole texture from (0,0) to (1,1)
			/// </summary>
			TextureEachFace,

			/// <summary>
			/// Texture is mapped around the box
			/// </summary>
			/// <remarks>
			///	- Texture aspect ratio should be 4:3
			///		- Left 1/4 of texture contains top, front and bottom sides
			///		- Vertically middle 1/3 of texture contains front, right, back and left sides
			///		- Other half of texture coordinate space not used
			/// </remarks>
			TextureWholeBox
		};

		/// <summary>
		/// Initializes a new instance of the <see cref="JGL.Heirarchy.Library.Box"/> class.
		/// </summary>
		/// <param name="name">Name</param>
		public Box(BoxTexturiseMode mode = BoxTexturiseMode.TextureEachFace)
			: base(null)
		{
			Init(1, 1, 1, mode);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="JGL.Heirarchy.Library.Box"/> class.
		/// </summary>
		/// <param name="name">Name</param>
		public Box(string name, BoxTexturiseMode mode = BoxTexturiseMode.TextureEachFace)
			: base(name)
		{
			Init(1, 1, 1, mode);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="JGL.Heirarchy.Library.Box"/> class.
		/// </summary>
		/// <param name="name">Name</param>
		/// <param name="xSize">X size</param>
		/// <param name="ySize">Y size</param>
		/// <param name="zSize">Z size</param>
		public Box(string name, double xSize, double ySize, double zSize, BoxTexturiseMode mode = BoxTexturiseMode.TextureEachFace)
			: base(name)
		{
			Init(xSize, ySize, zSize, mode);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="JGL.Heirarchy.Library.Box"/> class.
		/// </summary>
		/// <param name="xSize">X size</param>
		/// <param name="ySize">Y size</param>
		/// <param name="zSize">Z size</param>
		public Box(double xSize, double ySize, double zSize, BoxTexturiseMode mode = BoxTexturiseMode.TextureEachFace)
			: base(null)
		{
			Init(xSize, ySize, zSize, mode);
		}

		/// <summary>
		/// Init the specified xSize, ySize, zSize and material.
		/// </summary>
		/// <param name="xSize">X size</param>
		/// <param name="ySize">Y size</param>
		/// <param name="zSize">Z size</param>
		/// <param name="mode">Box texturing mode</param>
		/// <remarks>
		///	-	Worth rewriting this and below methods to get texcoord array from another method, selected using BoxTexturingMode?
		/// </remarks>
		protected void Init(double xSize, double ySize, double zSize, BoxTexturiseMode mode)
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
			Texturise(mode);
		}

		/// <summary>
		/// Generates texture coordinates for the box using the specified mode
		/// </summary>
		/// <param name="mode">Mode of texture coordinate generation</param>
		/// <remarks>
		///	-	Should this assume all vertices have been generated, insert new texcoords directly into Triangles array??
		///		-	See remarks for <see cref="Init"/>
		/// </remarks>
		public void Texturise(BoxTexturiseMode mode)
		{
			switch (mode)
			{
				case BoxTexturiseMode.TextureEachFace:
					VertexData.TexCoords = new TexCoord[]
					{
						new TexCoord(0, 0), new TexCoord(0, 1),
						new TexCoord(1, 1), new TexCoord(1, 0)
					};
					for (int i = 0; i < 12; i+=2)
					{
						Triangles[i].T = new int[] { 2, 1, 0 };
						Triangles[i + 1].T = new int[] { 0, 2, 3 };
					}
					break;

				case BoxTexturiseMode.TextureWholeBox:
					VertexData.TexCoords = new TexCoord[]
					{
						new TexCoord(0, 0), new TexCoord(0, 0.3333333), new TexCoord(0, 0.6666666), new TexCoord(0, 1),
						new TexCoord(0.25, 0), new TexCoord(0.25, 0.3333333), new TexCoord(0.25, 0.6666666), new TexCoord(0.25, 1),
						new TexCoord(0.5, 0), new TexCoord(0.5, 0.3333333), new TexCoord(0.5, 0.6666666), new TexCoord(0.5, 1),
						new TexCoord(0.75, 0), new TexCoord(0.75, 0.3333333), new TexCoord(0.75, 0.6666666), new TexCoord(0.75, 1),
						new TexCoord(1, 0), new TexCoord(1, 0.3333333), new TexCoord(1, 0.6666666), new TexCoord(1, 1)
					};

					Triangles[0].T = new int[] { 6, 2, 1 };
					Triangles[1].T = new int[] { 5, 6, 2 };
					Triangles[2].T = new int[] { 10, 6, 5 };
					Triangles[3].T = new int[] { 9, 10, 6 };
					Triangles[0].T = new int[] { 14, 10, 9 };
					Triangles[1].T = new int[] { 13, 14, 10 };
					Triangles[0].T = new int[] { 18, 14, 1 };
					Triangles[1].T = new int[] { 17, 18, 14 };
					Triangles[0].T = new int[] { 3, 2, 7 };
					Triangles[1].T = new int[] { 2, 6, 7 };
					Triangles[0].T = new int[] { 4, 5, 1 };
					Triangles[1].T = new int[] { 4, 1, 0 };

					break;

				default:
					throw new Exception("Just shouldn't happen!! mode=" + mode.ToString());
					break;
			}
		}
	}
}

