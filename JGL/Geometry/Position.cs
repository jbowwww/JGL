using System;

namespace JGL.Geometry
{
	/// <summary>
	/// Represents a position in 3D space
	/// </summary>
	public class Position : Vector3d
	{
		/// <summary>
		/// A vector in the direction considered "forward"
		/// </summary>
		public Vector3d Orientation;

		/// <summary>
		/// The rotations, in degrees, around the X, Y and Z axes
		/// </summary>
		public Vector3d Rotation;

		/// <summary>
		/// Move Position vector a distance in the given direction
		/// </summary>
		/// <param name="direction">Orientation</param>
		/// <param name="distance">Distance</param>
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
		/// <param name="distance">Distance to move, default 1.0</param>
		public void MoveForward(double distance = 1.0)
		{
			Position += Orientation * distance;
		}

		/// <summary>Moves <see cref="Entity"/> backward</summary>
		/// <param name="distance">Distance to move, default 1.0</param>
		public void MoveBackward(double distance = 1.0)
		{
			Position -= Orientation * distance;
		}
	}
}

//		private double[] _v = { 0, 0, 0 };
//
//		/// <summary>
//		/// Gets or sets the X component
//		/// </summary>
//		public double X { get { return _v[0]; } set { _v[0] = value; } }
//
//		/// <summary>
//		/// Gets or sets the Y component
//		/// </summary>
//		public double Y { get { return _v[1]; } set { _v[1] = value; } }
//
//		/// <summary>
//		/// Gets or sets the Z component
//		/// </summary>
//		public double Z { get { return _v[2]; } set { _v[2] = value; } }
//
//		/// <summary>
//		/// Initializes a new instance of the <see cref="JGL.Geometry.Position"/> class.
//		/// </summary>
//		/// <param name="v">Position vector components</param>
//		private Position(double[] v)
//		{
//			_v = v;
//		}
//
//		/// <summary>
//		/// Initializes a new instance of the <see cref="JGL.Geometry.Position"/> class.
//		/// </summary>
//		/// <param name="x">X</param>
//		/// <param name="y">Y</param>
//		/// <param name="z">Z</param>
//		public Position(double x, double y, double z)
//		{
//			_v = new double[] { x, y, z};
//		}
//
//		/// <summary>
//		/// Initializes a new instance of the <see cref="JGL.Geometry.Position"/> class.
//		/// </summary>
//		public Position()
//		{
//			_v = new double[] { 0, 0, 0 };
//		}
//
//		/// <summary>Convert a <see cref="OpenTK.Vector3d"/> to a <see cref="Position"/></summary>
//		/// <param name="v3d"><see cref="OpenTK.Vector3d"/> to convert</param>
//		public implicit operator Position(OpenTK.Vector3d v3d)
//		{
//			return new Position(v3d.X, v3d.Y, v3d.Z);
//		}
//
//		/// <summary>Convert a <see cref="Position"/> to a <see cref="OpenTK.Vector3d"/></summary>
//		/// <param name="position"><see cref="Position"/> to convert</param>
//		public implicit operator OpenTK.Vector3d(Position position)
//		{
//			return new OpenTK.Vector3d(position.X, position.Y, position.Z);
//		}
//	}
//}

