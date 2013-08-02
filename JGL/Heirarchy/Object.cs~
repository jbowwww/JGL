using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
//using JGL.Geometry;
using JGL.Extensions;
using JGL.Debugging;

namespace JGL.Heirarchy
{
	/// <summary>
	/// An <see cref="Entity"/> in 3D space, ie it implements <see cref="IPositionable"/> and <see cref="IRotatable"/>
	/// </summary>
	public class Object : EntityContext, IPositionable, IRotatable
	{
		/// <summary>
		/// Tracing
		/// </summary>
		public static readonly AutoTraceSource Trace = AutoTraceSource.GetOrCreate(AsyncXmlFileTraceListener.GetOrCreate("JGL"));

		/// <summary>
		/// Position in 3D space
		/// </summary>
		/// <remarks>IPositionable implementation</remarks>
		public Vector3d Position { get; set; }
		
		/// <summary>
		/// Rotation in 3D space (X,Y,Z each describe number of degrees rotation around the X/Y/Z unit vector)
		/// </summary>
		/// <remarks>IRotatable implementation</remarks>
		public Vector3d Rotation { get; set; }

		/// <summary>
		/// Gets or sets the object orientation (linked to <see cref="Object.Rotation"/>
		/// </summary>
		public Vector3d Orientation {
			get
			{
				Vector3d orientation = Vector3d.Transform(-Vector3d.UnitZ,
				Matrix4d.CreateRotationX(MathHelper.DegreesToRadians((float)Rotation.X)));
				orientation = Vector3d.Transform(orientation, Matrix4d.CreateRotationY(MathHelper.DegreesToRadians((float)Rotation.Y)));
				orientation = Vector3d.Transform(orientation, Matrix4d.CreateRotationZ(MathHelper.DegreesToRadians((float)Rotation.Z)));
				return orientation;
			}
			set
			{
				throw new NotImplementedException("TODO");
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="JGL.Heirarchy.Object"/> class.
		/// </summary>
		public Object(params Entity[] children)
			: base(children)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="JGL.Heirarchy.Object"/> class.
		/// </summary>
		public Object(string name, params Entity[] children)
			: base(name, children)
		{
		}

		/// <summary>
		/// Move Position vector a distance in the given direction
		/// </summary>
		/// <param name='direction'>
		/// Orientation.
		/// </param>
		/// <param name='distance'>
		/// Distance.
		/// </param>
		/// <remarks>
		/// TODO:
		/// Implement/find your own Vector3d class, use that, move this method into that class. Could leave it here as well and
		/// call the Vector3d member. V3d class will need explicit (and implicit if possible?) conversion operator to OpenTK.Vector3d
		/// </remarks>
		public void Move(Vector3d direction, double distance = 1.0)
		{
			Position += direction * distance;
		}

		/// <summary>Moves <see cref="Entity"/> forward</summary>
		/// <param name='distance'>Distance to move, default 1.0</param>
		public void MoveForward(double distance = 1.0)
		{
			Position += Orientation * distance;
		}
		
		/// <summary>Moves <see cref="Entity"/> backward</summary>
		/// <param name='distance'>Distance to move, default 1.0</param>
		public void MoveBackward(double distance = 1.0)
		{
			Position -= Orientation * distance;
		}

		public delegate bool MeshInclusionTest(Mesh mesh);

		/// <summary>
		/// Default <see cref="MergeChildMeshes"/> inclusion test includes all.
		/// </summary>
		/// <returns><c>true</c>, always</returns>
		/// <param name="mesh">The <see cref="Mesh"/> that will be included</param>
		private static bool IncludeAllMeshes(Mesh mesh) { return true; }

		/// <summary>
		/// Merges selected (by <paramref name="inclusionTest"/>) child <see cref="Mesh"/> <see cref="Entity"/>s'
		/// <see cref="Mesh.VertexData"/>s into a single instance, and referenced only by a single <see cref="Mesh"/>
		/// <see cref="Entity"/> that has had all the children's <see cref="Mesh.Triangles"/> copied to it. If this
		/// <see cref="Object"/> instance is not a <see cref="Mesh"/>, a new one is created and returned, after replacing
		/// <c>this</c> instance in the heirarchy (hence unless it is referenced elsewhere, <c>this</c> instance should be
		/// marked for garbage collection.
		/// </summary>
		/// <returns>
		/// Unless there are no child M<see cref="Mesh"/>es (whether they are selected for merging or not), returns a <see cref="Mesh"/>
		/// instance, that is either <c>this</c>, or a newly created instance. If there are no child <see cref="Mesh"/>es, returns <c>this</c>
		/// </returns>
		/// <param name="inclusionTest">Inclusion test, default includes all</param>
		public Object MergeChildMeshes(MeshInclusionTest inclusionTest = null)
		{
			IEnumerable<Mesh> childMeshes = Objects.OfType<Mesh>();

			if (childMeshes.Count() > 0)
			{
				if (inclusionTest == null)
					inclusionTest = IncludeAllMeshes;
				bool isThisMesh = this.GetType().IsTypeOf(typeof(Mesh));

				JGL.Geometry.VertexData vertexData = isThisMesh ? (this as Mesh).VertexData : new JGL.Geometry.VertexData();
				IList<JGL.Geometry.TriangleFace> triangles = isThisMesh ? (this as Mesh).Triangles : new List<JGL.Geometry.TriangleFace>();

				int vc = 0, nc = 0, tc = 0;
				foreach (Mesh childMesh in childMeshes)
				{
					if (childMesh.Triangles.Count > 0 && inclusionTest(childMesh))
					{
						vc += childMesh.VertexData.Vertices.Count;
						nc += childMesh.VertexData.Normals.Count;
						tc += childMesh.VertexData.TexCoords.Count;
						vertexData.Vertices.Concat(childMesh.VertexData.Vertices);
						vertexData.Normals.Concat(childMesh.VertexData.Normals);
						vertexData.TexCoords.Concat(childMesh.VertexData.TexCoords);
						foreach (JGL.Geometry.TriangleFace tf in childMesh.Triangles)
							triangles.Add(new JGL.Geometry.TriangleFace(tf, vc, nc, tc));
					}
				}

				Mesh mesh = isThisMesh ? this as Mesh : new Mesh(null);
				mesh.VertexData = vertexData;
				mesh.Triangles = triangles;

				if (!isThisMesh)
				{
					// this is a newly created Mesh instance that needs to replace this non-Mesh Entity in the Heirarchy
					if (Parent != null)
					{
						Parent.Remove(this);
						Parent.Add(mesh);
					}
				}

				return mesh;
			}
			return this;
		}

		/// <summary>
		/// Merges selected (by <paramref name="inclusionTest"/>) child <see cref="Mesh"/> <see cref="Entity"/>s'
		/// <see cref="Mesh.VertexData"/>s into a single instance, which is then referenced by all the child <see cref="Mesh"/>
		/// <see cref="Entity"/>s that were selected for combining.
		/// </summary>
		/// <param name="inclusionTest">Inclusion test</param>
		public void CombineChildMeshes(MeshInclusionTest inclusionTest = null)
		{
			IEnumerable<Mesh> childMeshes = Objects.OfType<Mesh>();

			if (childMeshes.Count() > 0)
			{
				if (inclusionTest == null)
					inclusionTest = IncludeAllMeshes;
			}
		}
	}
}