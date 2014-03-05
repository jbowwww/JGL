using System;
using System.Collections.Generic;
using JGL.Geometry;

using JGL.Graphics;
using JGL.Heirarchy;
using JGL.Debugging;

namespace JGL.Geometry
{
	/// <summary>
	/// Describes a collection vertices, and optionally normals, texture coordinates and materials, used by <see cref="Mesh"/>
	/// </summary>
	public class VertexData
	{
		/// <summary>
		/// Tracing
		/// </summary>
		public static readonly AutoTraceSource Trace = AutoTraceSource.GetOrCreate(AsyncXmlFileTraceListener.GetOrCreate("JGL"));

		/// <summary>
		/// The vertices.
		/// </summary>
		public IList<Vertex> Vertices { get; set; }

		/// <summary>
		/// The normals.
		/// </summary>
		public IList<Normal> Normals { get; set; }

		/// <summary>
		/// The tex coords.
		/// </summary>
		public IList<TexCoord> TexCoords { get; set; }

		/// <summary>
		/// The texture.
		/// </summary>
		public MaterialLibrary Materials { get; set; }

		/// <summary>
		/// Clear this instance.
		/// </summary>
		public void Clear()
		{
			Vertices = new List<Vertex>();
			Normals = new List<Normal>();
			TexCoords = new List<TexCoord>();
			Materials = null;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="JGL.Geometry.MeshVertexData"/> class.
		/// </summary>
		public VertexData()
		{
			Clear();
		}
	}
}

