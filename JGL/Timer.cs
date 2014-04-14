using System;
using System.Collections.Generic;

namespace JGL
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
		/// Initializes a new instance of the <see cref="JGL.Timer"/> class.
		/// </summary>
		/// <param name="start">If set to <c>true</c> (default), <see cref="JGL.Timer.Update"/> is called</param>
		public Timer(bool start = true)
		{
			if (start)
				Update();
		}

		/// <summary>
		/// A marked time, usually when the event last occurred. To update this call <see cref="Mark"/>.
		/// </summary>
		public DateTime MarkTime { get; private set; }

		/// <summary>
		/// Gets a value indicating whether this instance is initialised.
		/// </summary>
		public bool IsInitialised { get { return MarkTime != default(DateTime); } }

		/// <summary>
		/// Gets the time elapsed since <see cref="MarkTime"/>
		/// </summary>
		public TimeSpan TimeElapsed {
			get { return IsInitialised ? DateTime.Now - MarkTime : TimeSpan.Zero; }
		}

		/// <summary>
		/// The <see cref="TimeSpan"/> between the two most recent consecutive events (ie when <see cref="Mark"/> is called(
		/// </summary>
		public TimeSpan LastElapsed { get; private set; }

		/// <summary>
		/// Stores <see cref="JGL.Timer.TimeElapsed"/> in <see cref="JGTL.Timer.LastElapsed"/>, and updates
		/// <see cref="MarkTime"/> to <c>DateTime.Now</c>
		/// </summary>
		public virtual void Update()
		{
			LastElapsed = TimeElapsed;
			MarkTime = DateTime.Now;
		}
	}
}
