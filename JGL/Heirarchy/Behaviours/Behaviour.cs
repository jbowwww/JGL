using System;
using JGL.Graphics;

namespace JGL.Heirarchy.Behaviours
{
	public abstract class Behaviour : Behaviour<Entity>
	{

	}

	/// <summary>
	/// Provides functions that are called by an active <see cref="JGL.Heirarchy.Scene"/> to perform
	/// some implement some kind of temporal behaviour
	/// </summary>
	/// <remarks>
	/// Not sure yet whether I'm going to be implementing this in object/entity's or implementing it
	/// separately in some kind of processor/controller class. Maybe both
	/// </remarks>
	public abstract class Behaviour<TEntity> : Timer, IBehaviour
		where TEntity : Entity
	{
		/// <summary>
		/// The <see cref="JGL.Heirarchy.Entity"/>(s) this <see cref="Behaviour"/> is currently applied to
		/// </summary>
		public readonly EntityCollection<TEntity> Entities = new EntityCollection<TEntity>();

		/// <summary>
		/// The target process time period, the ideal maximum amount of time between calls to <see cref="Behaviour.Process"/>
		/// Defaults to zero, which means it gets called every frame
		/// </summary>
		public int TargetProcessRate {
			get { return _targetProcessRate; }
			set { TargetPeriod = 1000d / (_targetProcessRate = value); }
		}

		private int _targetProcessRate = 0;

		/// <summary>
		/// Get the target period, in milliseconds, calculated from <see cref="TargetProcessRate"/>
		/// </summary>
		public double TargetPeriod { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="JGL.Heirarchy.Behaviours.Behaviour`1"/> class.
		/// </summary>
		public Behaviour() : base(false)
		{
//			((EntityCollection)Entities).Clear();
			Entities.Added += (args) =>
			{
				ApplyTo(args);
			};
		}

		/// <summary>
		/// Apply this <see cref="JGL.Heirarchy.Behaviours.Behaviour`1"/> to <paramref name="entity"/>
		/// </summary>
		/// <param name="args">The <see cref="JGL.Heirarchy.EntityCollection.EntityEventArgs"/>  describing the event</param>
		public virtual void ApplyTo(EntityCollection<TEntity>.EntityEventArgs args)
		{
		}

		/// <summary>
		/// Called by the <see cref="JGL.Heirarchy.Scene"/> to process each <see cref="JGL.Heirarchy.Entity"/>'s <see cref="Behaviour"/>
		/// </summary>
		public void Process()
		{
			if (!IsInitialised)
				Update();
			else if (LastElapsed.TotalMilliseconds >= TargetPeriod)
				{
					Update();
					foreach (TEntity e in Entities)
						Process(e);
				}
		}

		/// <summary>
		/// Perform the implemented behaviour by process the supplied <paramref name="entity"/>
		/// </summary>
		/// <param name="entity">The <see cref="JGL.Heirarchy.Entity"/> to process behaviour for</param>
		/// <remarks>
		/// This class does not store an Entity reference, as one behaviour instance can be applied to many entities if need be.
		/// Not sure exactly how I'm going to implement it yet but I'm thinking maybe Entity needs a member method called
		/// ApplyBehaviour(Behaviour): This passes the behaviour instance and its own reference to an internal method
		/// using the instance of the entity's owner Scene - that method then somehow stores the two together for easy processing
		/// </remarks>
		public abstract void Process(TEntity entity);
	}
}
