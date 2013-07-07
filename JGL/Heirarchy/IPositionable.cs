using System;
using OpenTK;
//using JGL.Geometry;

namespace JGL.Heirarchy
{
	/// <summary>
	/// I positionable.
	/// </summary>
	public interface IPositionable
	{
		/// <summary>
		/// Position in 3D space
		/// </summary>
		Vector3d Position { get; set; }
	}
}

