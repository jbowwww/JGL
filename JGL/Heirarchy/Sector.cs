using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK.Graphics.OpenGL;
using JGL.Geometry;
using JGL.Debugging;

namespace JGL.Heirarchy
{
	/// <summary>
	/// Represents a subspace of a <see cref="JGL.Heirarchy.Scene"/>, and contains child <see cref="JGL.Heirarchy.Entity"/>s
	/// (such as <see cref="JGL.Heirarchy.MeshObject"/>s)
	/// </summary>
	public class Sector : EntityContext, IPositionable, IRotatable
	{
		/// <summary>
		/// Tracing
		/// </summary>
//		public static readonly AutoTraceSource Trace = AutoTraceSource.GetOrCreate(AsyncXmlFileTraceListener.GetOrCreate("JGL"));

		/// <summary>
		/// Initializes a new <see cref="JGL.Heirarchy.Sector"/> instance, optionally containing supplied
		 /// child <see cref="JGL.Heirarchy.Entity"/>s.
		/// </summary>
		/// <param name='children'>Child entities to add to this <see cref="JGL.Heirarchy.Context"/></param>
		public Sector(params Entity[] children)
			: base (children)
		{
			
		}
		
		/// <summary>
		/// Gets child objects
		/// </summary>
		public IEnumerable<Object> Objects {
			get { return Entities.OfType<Object>(); }
		}

		/// <summary>
		/// Position in 3D space
		/// </summary>
		/// <remarks>IPositionable implementation</remarks>
		public OpenTK.Vector3d Position { get; set; }
		
		/// <summary>
		/// Rotation in 3D space (X,Y,Z each describe number of degrees rotation around the X/Y/Z unit vector)
		/// </summary>
		/// <remarks>IRotatable implementation</remarks>
		public OpenTK.Vector3d Rotation { get; set; }

		public OpenTK.Vector3d Orientation {
			get {
				throw new NotImplementedException ();
			}
			set {
				throw new NotImplementedException ();
			}
		}
	}
}

