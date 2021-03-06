using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Linq;
using System.IO;
using System.Text;

using OpenTK.Graphics.OpenGL;

using JGL.Graphics;
using JGL.Geometry;
using JGL.Heirarchy.Resources;
using JGL.Debugging;

namespace JGL.Heirarchy
{
	/// <summary>
	/// Represents a 3D mesh made up of a vertex array, and optionally normal and texture coordinate arrays
	/// If using texture coordinates the <see cref="JGL.Heirarchy.Mesh.Texture"/> property should also be set
	/// </summary>
	public class Mesh : Object, IRenderable
	{
		/// <summary>
		/// Tracing
		/// </summary>
//		public static readonly AutoTraceSource Trace = AutoTraceSource.GetOrCreate(AsyncXmlFileTraceListener.GetOrCreate("JGL"));

		#region Geometry fields
		/// <summary>
		/// Vertex (and possibly normal, texture coordinates, and materials) data for the <see cref="Mesh">.
		/// This can be shared with other <see cref="Mesh">es.
		/// </summary>
		public VertexData VertexData;

		/// <summary>
		/// The material.
		/// </summary>
		public Material Material;

		/// <summary>
		/// The triangles.
		/// </summary>
		public IList<TriangleFace> Triangles;

		/// <summary>
		/// Gets the triangles as an array.
		/// </summary>
		/// <remarks>
		///	-	TODO: Look into (think about, test) if using this would speed up looping through triangles when rendering?
		///		-	Any benefit probably limited/negated/overshadowed by having to copy elements to a new array each time
		/// </remarks>
//		private TriangleFace[] TriangleArray {
//			get
//			{
//				TriangleFace[] triangleArray = new TriangleFace[Triangles.Count];
//				this.Triangles.CopyTo(triangleArray, 0);
//				return triangleArray;
//			}
//		}

		/// <summary>
		/// Indicate if mesh faces should be drawn one or two sided
		/// </summary>
		public bool TwoSided = false;
		#endregion

		#region Constructor overloads
		/// <summary>
		/// Initializes a new instance of the <see cref="JGL.Heirarchy.Mesh"/> class.
		/// </summary>
		/// <param name='name'>
		/// Name.
		/// </param>
		public Mesh() { }

		/// <summary>
		/// Initializes a new instance of the <see cref="JGL.Heirarchy.Mesh"/> class.
		/// </summary>
		/// <param name='vertexData'>
		/// VertexData.
		/// </param>
		public Mesh(string name, VertexData vertexData, IList<TriangleFace> triangles = null)// : base(name)
		{
			base.Name = name;
			VertexData = vertexData;
			Triangles = triangles != null ? triangles : new List<TriangleFace>();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="JGL.Heirarchy.Mesh"/> class.
		/// </summary>
		/// <param name='name'>
		/// Name.
		/// </param>
		/// <param name='vertexData'>
		/// VertexData.
		/// </param>
		/// <param name='triangles'>
		/// Triangles.
		/// </param>
		public Mesh(string name, VertexData vertexData, IEnumerable<TriangleFace> triangles)// : base(name)
		{
			base.Name = name;
			VertexData = vertexData;
			Triangles = new List<TriangleFace>(triangles);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="JGL.Heirarchy.Mesh"/> class from an .OBJ (lightwave) file
		/// </summary>
		/// <param name="filename">Filename</param>
		/// <param name="dummy">Dummy</param>
		public Mesh(string filename, bool dummy)
		{
			Mesh[] meshes;
			using (Stream s = File.Open(filename, FileMode.Open))
				meshes = LoadObjFile(s);
			if (meshes.Length == 1)
			{
				this.VertexData = meshes[0].VertexData;
				this.Material = meshes[0].Material;
				this.Triangles = meshes[0].Triangles;
			}
			else
				AddRange(meshes);
		}
		#endregion

	/// <summary>
	/// An abstract representation of a "Texturise" method. Generates texture coordinates and
	/// applies them to a specific type of mesh.
	/// </summary>
	/// <typeparam name="TMesh">A <see cref="Mesh"/>-derived type that this texturise method operates on</typeparam>
	/// <remarks>
	/// Currently only used by classes in the <see cref="JGL.Heirarchy.MeshLibrary"/> namespace
	/// (e.g. <see cref="Box"/>, <see cref="Quad"/>)
	/// TODO: I can't make up my mind whether to keep it inside this mesh class, or make it it's own, and/or whether to put it in the
	/// mesh library namespace as that's where its currently being used. BUT, maybe eventually I might want to use a texturisemethod
	/// on a generic mesh, say to skin a model?? I dunno doesnt matter right now
	/// </remarks>
	public abstract class TexturiseMethod<TMesh> where TMesh : Mesh
	{
		/// <summary>
		/// Texturise the specified mesh.
		/// </summary>
		/// <param name="mesh">A <see cref="Mesh"/>-derived instance to be texturised</param>
		public abstract void Texturise(TMesh mesh);
	}

		/// <summary>
		/// Texturise the specified texturiseMethod.
		/// </summary>
		/// <param name="texturiseMethod">Texturise method</param>
		public virtual void Texturise<TMesh>(TexturiseMethod<TMesh> texturiseMethod)
			where TMesh : Mesh
		{
			texturiseMethod.Texturise(this as TMesh);
			Trace.Log(System.Diagnostics.TraceEventType.Verbose, "Textured {0} mesh \"{1}\" using {2}, {3} texture coordinates",
				typeof(TMesh).FullName, this.Id, texturiseMethod.ToString(), VertexData.TexCoords.Count);
		}

		/// <summary>
		/// Render <see cref="Mesh"/>
		/// </summary>
		/// <param name="args">Render arguments</param>
		/// <remarks>IRenderable implementation</remarks>
		public void Render(RenderArgs args)
		{
			if (Triangles != null && VertexData != null)
			{
				// Set one/two sided
				// TODO: Test this works since changing cullface call in Configuration.Init
				if (TwoSided)
					GL.Disable(EnableCap.CullFace);
				else
					GL.Enable(EnableCap.CullFace);

				// No material set, render as wireframe or solid
				// TODO: Add additional checks for new wireframe/solid rendering override options if (Material == null)
				if (Material == null)
				{
					GL.Disable(EnableCap.ColorMaterial);
					GL.Color4(1.0, 1.0, 1.0, 1.0);
					GL.Begin(BeginMode.Triangles);
					RenderFaces();
					GL.End();
					GL.Enable(EnableCap.ColorMaterial);
				}

				// Render using material
				else 			// if (Material != null)
				{
					// Set material
					GL.ColorMaterial(MaterialFace.FrontAndBack, ColorMaterialParameter.AmbientAndDiffuse);
					Material.glSet();

					GL.Begin(BeginMode.Triangles);
					if (Material.HasTexture)			// TODO: (Eventually) Add additional check for a wireframe/disabled texture option state
						RenderFacesTextured();		// Textured
					else
						RenderFaces();						// Non-textureMeshd
					GL.End();
				}

				args.FrameTriangleCount += (uint)Triangles.Count;		// Update triangle count
			}
		}

		/// <summary>
		/// Renders triangles, not using a texture
		/// </summary>
		/// <remarks>
		///	-	Assumes <see cref="Mesh.Triangles"/> != <c>null</c>
		/// </remarks>
		private void RenderFaces()
		{
			foreach (TriangleFace T in Triangles)
			{
				GL.Normal3(VertexData.Normals[T.N[0]].Components);
				GL.Vertex3(VertexData.Vertices[T.V[0]].Components);
				GL.Normal3(VertexData.Normals[T.N[1]].Components);
				GL.Vertex3(VertexData.Vertices[T.V[1]].Components);
				GL.Normal3(VertexData.Normals[T.N[2]].Components);
				GL.Vertex3(VertexData.Vertices[T.V[2]].Components);
			}
		}

		/// <summary>
		/// Renders triangles, using <see cref="Mesh.Material.Texture"/>
		/// </summary>
		/// <remarks>
		///	-	Assumes <see cref="Mesh.Triangles"/> != <c>null</c> and in each <see cref="Triangle"/>, assumes
		///		<see cref="Triangle.V"/> != <c>null</c>, <see cref="Triangle.N"/> != <c>null</c> and <see cref="Triangle.T"/> != <c>null</c>
		/// </remarks>
		private void RenderFacesTextured()
		{
			foreach (TriangleFace T in Triangles)
			{
				GL.Normal3(VertexData.Normals[T.N[0]].Components);
				GL.TexCoord2(VertexData.TexCoords[T.T[0]].GetComponents());
				GL.Vertex3(VertexData.Vertices[T.V[0]].Components);
				GL.Normal3(VertexData.Normals[T.N[1]].Components);
				GL.TexCoord2(VertexData.TexCoords[T.T[1]].GetComponents());
				GL.Vertex3(VertexData.Vertices[T.V[1]].Components);
				GL.Normal3(VertexData.Normals[T.N[2]].Components);
				GL.TexCoord2(VertexData.TexCoords[T.T[2]].GetComponents());
				GL.Vertex3(VertexData.Vertices[T.V[2]].Components);
			}
		}

		/// <summary>
		/// Loads the object file.
		/// </summary>
		/// <param name="fs">Stream containing lightwave .OBJ format data</param>
		public static Mesh[] LoadObjFile(Stream fs)
		{
			List<Mesh> meshes = new List<Mesh>();
			VertexData vertexData = new VertexData();
			using (StreamReader sr = new StreamReader(fs))
			{
				string line;
				string[] tokens;
				Mesh mesh = null;
				while (!sr.EndOfStream)		//(fs.Position < (fs.Length - 1))
				{
					line = sr.ReadLine();
					tokens = line.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
					if (tokens.Length > 0)
					{
						switch (tokens[0].ToLower())
						{
							case "mtllib":
								Debug.Assert(tokens.Length == 2);
								vertexData.Materials = new MaterialLibrary(tokens[1]);
								break;
							case "v":
								Debug.Assert(tokens.Length == 4);
								vertexData.Vertices.Add(new Vertex(double.Parse(tokens[1]), double.Parse(tokens[2]), double.Parse(tokens[3])));
								break;
							case "vn":
								Debug.Assert(tokens.Length == 4);
								vertexData.Normals.Add(new Normal(double.Parse(tokens[1]), double.Parse(tokens[2]), double.Parse(tokens[3])));
								break;
							case "vt":
								Debug.Assert(tokens.Length == 3);
								vertexData.TexCoords.Add(new TexCoord(double.Parse(tokens[1]), double.Parse(tokens[2])));
								break;
							case "g":
								mesh = new Mesh(tokens.Length > 1 ? tokens[1] : null, vertexData);
								meshes.Add(mesh);
								break;
							case "usemtl":
								Debug.Assert(tokens.Length == 2 && mesh != null && vertexData.Materials != null);
								mesh.Material = vertexData.Materials[tokens[1]];
								break;
							case "f":
								Debug.Assert(tokens.Length == 4 && mesh != null);
								Debug.Assert(tokens[1].Split(new char[] {'/'}, StringSplitOptions.RemoveEmptyEntries).Length == 3);
								int[] v = new int[3];
								int[] n = new int[3];
								int[] t = new int[3];
								for (int i = 1; i < 4; i++)
								{
									string[] tvparts = tokens[i].Split(new char[] {'/'}, StringSplitOptions.RemoveEmptyEntries);
									v[i - 1] = int.Parse(tvparts[0]) - 1;
									n[i - 1] = int.Parse(tvparts[1]) - 1;
									t[i - 1] = int.Parse(tvparts[2]) - 1;
								}
								mesh.Triangles.Add(new TriangleFace(v, n, t));
								break;
						}
					}
				}				
			}
			return meshes.ToArray();
		}
	}
}

