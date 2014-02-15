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
	public class Timer
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="JGL.Heirarchy.Timer"/> class.
		/// </summary>
		/// <param name="mark">
		/// If set to <c>true</c>, <see cref="Mark"/> is called in the constructor to update <see cref="MarkTime"/>.
		/// Defaults to <c>false</c>.
		/// </param>
		public Timer()
		{
			Update();
		}

		/// <summary>
		/// The <see cref="TimeSpan"/> between the two most recent consecutive events (ie when <see cref="Mark"/> is called(
		/// </summary>
		public TimeSpan LastDuration { get; private set; }

		/// <summary>
		/// A marked time, usually when the event last occurred. To update this call <see cref="Mark"/>.
		/// </summary>
		public DateTime MarkTime { get; private set; }

		/// <summary>
		/// Gets the time elapsed since <see cref="MarkTime"/>
		/// </summary>
		public TimeSpan TimeElapsed {
			get { return MarkTime == default(DateTime) ? TimeSpan.Zero : DateTime.Now - MarkTime; }
		}

		/// <summary>
		/// Updates <see cref="MarkTime"/> to <c>DateTime.Now</c>
		/// </summary>
		public virtual void Update()
		{
			Mark();
		}

		protected void Mark()
		{
			LastDuration = TimeElapsed;
			MarkTime = DateTime.Now;
		}
	}
}
