using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace JGL
{
	/// <summary>
	/// Engine.
	/// </summary>
	public static class Engine
	{
		/// <summary>
		/// The resource search paths, indexed by type
		/// </summary>
		public static ConcurrentDictionary<Type, string[]> ResourceSearchPaths { get; private set; }

		/// <summary>
		/// Init this instance.
		/// </summary>
		public static void Init()
		{
			ResourceSearchPaths = new ConcurrentDictionary<Type, string[]>(new KeyValuePair<Type, string[]>[]
			{
				new KeyValuePair<Type, string[]>(typeof(Heirarchy.Resources.Texture), new string[] { "../../../Data/Textures/" })
				// TODO: More as necessary
			});
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

