using System;
using System.Collections.Generic;
using System.Linq;
using OpenTK;
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
		public static readonly AutoTraceSource Trace = new AutoTraceSource(AsyncFileTraceListener.GetOrCreate("JGL"));

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
		/// Gets child objects
		/// </summary>
		public IEnumerable<Object> SubObjects {
			get { return Entities.OfType<Object>(); }
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
	}
}