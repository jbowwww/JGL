using System;
using OpenTK;
//using JGL.Geometry;

namespace JGL.Heirarchy
{
	/// <summary>
	/// I rotatable.
	/// </summary>
	public interface IRotatable
	{
		/// <summary>
		/// Rotation in 3D space (X,Y,Z each describe number of degrees rotation around the X/Y/Z unit vector)
		/// </summary>
		Vector3d Rotation { get; set; }

		/// <summary>
		/// Gets or sets the object orientation (linked to <see cref="Object.Rotation"/>
		/// </summary>
		Vector3d Orientation { get; set; }
	}
}

