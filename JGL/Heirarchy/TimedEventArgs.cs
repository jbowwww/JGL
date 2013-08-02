using System;
using System.Collections.Generic;

namespace JGL.Heirarchy
{
	/// <summary>
	/// Base class for event arguments <see cref="UpdateArgs"/> and <see cref="RenderArgs"/> that
	/// provides ability to capture timing information between recurring events
	/// Also maintains <see cref="Entities"/> stack which stores the state of the heirarchy traversal,
	/// the top of this stack is the current entity being operated on, and is referred to as <see cref="CurrentEntity"/>
	/// </summary>
	/// <remarks>
	///	-	TODO: This base class should define <see cref="RenderArgs.Graphics"/> and <see cref="UpdateArgs.Graphics"/>
	///		because it is common to both classes. But does that then mean this class is misnamed, or it's intended is not as I thought?
	/// </remarks>
	public class TimedEventArgs
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="JGL.Heirarchy.TimedEventArgs"/> class.
		/// </summary>
		/// <param name="mark">
		/// If set to <c>true</c>, <see cref="Mark"/> is called in the constructor to update <see cref="MarkTime"/>.
		/// Defaults to <c>false</c>.
		/// </param>
		public TimedEventArgs(bool mark = false)
		{
			if (mark)
				Update();
			else
				MarkTime = DateTime.MinValue;
		}

		/// <summary>
		/// The root <see cref="JGL.Heirarchy.EntityContext"/>, which was the first <see cref="Entity"/> operated on
		/// (e.g. rendered with <see cref="RenderArgs"/> or updated with <see cref="UpdateArgs"/>)
		/// </summary>
		public EntityContext UpdateRoot {
			get { return Entities == null || Entities.Count == 0 ? null : Entities.ToArray()[0] as EntityContext; }
			set { Entities = new Stack<Entity>(new Entity[] { value }); }
		}

		/// <summary>
		/// Gets the current <see cref="Entity"/> being operated on (e.g. being rendered in the case of <see cref="RenderArgs"/>
		/// </summary>
		public Entity CurrentEntity {
			get { return Entities == null || Entities.Count == 0 ? null : Entities.Peek(); }
		}

		/// <summary>
		/// The stack of <see cref="JGL.Heirarchy.Context"/>s as rendered so far
		/// </summary>
		public Stack<Entity> Entities { get; private set; }
		
		/// <summary>
		/// Gets a value indicating whether the entity context stack <see cref="JGL.Heirarchy.RenderArgs.Entities"/> is empty
		/// </summary>
		public bool IsEntityContextStackEmpty {
			get { return Entities.Count == 0; }
		}

		/// <summary>
		/// A marked time, usually when the event last occurred. To update this call <see cref="Mark"/>.
		/// </summary>
		public DateTime MarkTime { get; private set; }

		/// <summary>
		/// Gets the time elapsed since <see cref="MarkTime"/>
		/// </summary>
		public TimeSpan TimeElapsed {
			get { return MarkTime == DateTime.MinValue ? TimeSpan.Zero : DateTime.Now - MarkTime; }
		}

		/// <summary>
		/// The <see cref="TimeSpan"/> between the two most recent consecutive events (ie when <see cref="Mark"/> is called(
		/// </summary>
		public TimeSpan LastDuration { get; private set; }
			
		/// <summary>
		/// Updates <see cref="MarkTime"/> to <c>DateTime.Now</c>
		/// </summary>
		public virtual void Update()
		{
			LastDuration = TimeElapsed;
			MarkTime = DateTime.Now;
		}
	}
}
