using System;
using OpenTK;

namespace JGL.Heirarchy
{
	public interface INewtonian : IPositionable, IRotatable
	{
		Vector3d Velocity { get; set; }
		Vector3d Acceleration { get; set; }
		Vector3d AngularVelocity { get; set; }
		Vector3d AngularAcceleration { get; set; }
	}
}

