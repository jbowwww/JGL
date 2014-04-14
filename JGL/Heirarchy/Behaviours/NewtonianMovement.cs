using System;

using JGL.Graphics;

namespace JGL.Heirarchy.Behaviours
{
	/// <summary>
	/// Newtonian movement.
	/// </summary>
	public class NewtonianMovement : Behaviour<Object>		//BatchBehaviour<Object>
	{
		/// <summary>
		/// Perform the implemented behaviour by process the supplied <paramref name="entity"/>
		/// </summary>
		/// <param name="entity">The <see cref="JGL.Heirarchy.Entity"/> to process behaviour for</param>
		/// <remarks>JGL.Heirarchy.BatchBehaviour[Object] abstract method implementation</remarks>
		public override void Process(Object entity)
		{
			double timeStep = LastElapsed.TotalSeconds;
			entity.Velocity += entity.Acceleration * timeStep;
			entity.Position += entity.Velocity * timeStep;
			entity.AngularVelocity += entity.AngularAcceleration * timeStep;
			entity.Rotation += entity.AngularVelocity * timeStep;
		}
	}
}

