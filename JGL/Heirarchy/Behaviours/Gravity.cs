using System;
using JGL.Heirarchy;

namespace JGL.Heirarchy.Behaviours
{
	public class Gravity : NewtonianMovement
	{
		/// <summary>
		/// If an <see cref="JGL.Heirarchy.Object"/>'s Y acceleration component's magnitude is less than this,
		/// this behaviour will give it an initial acceleration using the GravityAcceleration property
		/// </summary>
		public double NeutralAccelerationThreshold = 0.1;

		public double GroundPositionThreshold = -1.2;

		/// <summary>
		/// Acceleration due to gravity is 9.8 m/s (might not necessarily look good though, change if need be)
		/// </summary>
		public double GravityAcceleration = -4.9;

		public override void ApplyTo(EntityCollection<Object>.EntityEventArgs args)
		{
			Object entity = (Object)args.Entity;
//			if (Math.Abs(entity.Acceleration.Y) <= NeutralAccelerationThreshold && entity.Position.Y >= GroundPositionThreshold)
			entity.Acceleration = new OpenTK.Vector3d(entity.Acceleration.X, GravityAcceleration, entity.Acceleration.Z);
		}

		public override void Process(Object entity)
		{
			base.Process(entity);
		}
	}
}

