using System;

namespace JGL
{
	public static class Engine
	{
		/// <summary>
		/// Options specified by an <see cref="EngineOptions"/> instance
		/// </summary>
		public static EngineOptions Options { get; private set; }

		/// <summary>
		/// Init this instance.
		/// </summary>
		public static void Init()
		{
			Options = new EngineOptions();
		}

		/// <summary>
		/// Quit this instance.
		/// </summary>
		public static void Quit()
		{
			JGL.Heirarchy.Resources.Resource.StopLoadThread();
			JGL.Debugging.AutoTraceSource.StopTraceThread();
		}
	}
}

