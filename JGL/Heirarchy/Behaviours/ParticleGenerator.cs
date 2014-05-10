using System;
using System.Linq;
using OpenTK;

namespace JGL.Heirarchy.Behaviours
{
	/// <summary>
	/// Particle generator.
	/// </summary>
	/// <remarks>
	///  !! TODO : This is the first very basic implementation that basically works, but obviously it'll be much better than this
	/// 		- Firstly : Particles need to have a finite lifespan, some condition that sooner or later allows them to be destroyed
	/// 		- Then: Could quite probably render the trinangles much more efficiently - each one is a whole mesh object
	/// 			(that way I could use the position, accel etc). Even if you can't avoid THAT (using a mesh/object), maybe you could
	/// 			write some streamlined IRenderable.Render that renders all the particles slickly??
	/// </remarks>
	public class ParticleGenerator : Behaviour<Object>
	{
		public IRenderable Particle;
		public Behaviour<Object> Gravity;

		public double ParticleRate = 10;
		public Vector3d AverageInitialVelocity = new Vector3d(0, 12, 0);
		public Vector3d InitialVelocityRange = new Vector3d(2.5, 4.2, 2.5);

		public override void ApplyTo(EntityCollection<Object>.EntityEventArgs args)
		{
			base.ApplyTo(args);
		}

		/// <summary>
		/// Process the specified renderArgs and entity.
		/// </summary>
		/// <param name='entity'>
		/// Entity.
		/// </param>
		/// <remarks>JGL.Heirarchy.Behaviours.Behaviour[Object] abstract member implementation</remarks>
		public override void Process(Object entity)
		{
			RenderableObjectProxy ro = new RenderableObjectProxy(Particle)
			{
				Name = "Particle" + Guid.NewGuid().ToString().Substring(0, 8),
				Velocity = MakeVelocity(AverageInitialVelocity, InitialVelocityRange)
			};
			entity.Add(ro);
			if (Gravity != null)
				Gravity.Entities.Add(ro);
		}

		/// <summary>
		/// Calculates a velocity for a new particle
		/// </summary>
		/// <returns>
		/// The velocity.
		/// </returns>
		/// <param name='averageInitial'>
		/// Average initial.
		/// </param>
		/// <param name='range'>
		/// Range.
		/// </param>
		internal Vector3d MakeVelocity(Vector3d averageInitial, Vector3d range)
		{
				Random r = new Random();
				double[] v = new double[3];
				for (int i = 0; i < 3; i ++)
					v[i] = (r.NextDouble() - 0.5) * 2;
			Vector3d Velocity = averageInitial + new Vector3d(range.X * v[0], range.Y * v[1], range.Z * v[2]);
			return Velocity;// + new Random().NextDouble
		}
	}
}

