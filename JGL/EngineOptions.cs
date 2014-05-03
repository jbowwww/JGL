using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace JGL
{
	/// <summary>
	/// Engine.
	/// </summary>
	public class EngineOptions
	{
		#region Internal options
		/// <summary>
		/// The concurrent collection operation retry limit
		/// </summary>
		/// <remarks>
		/// Currently set to 1, for debugging purposes. Think about appropriate default value otherwise - 2, 4?
		/// </remarks>
		internal int ConcurrentCollectionOperationRetryLimit = 1;

		/// <summary>
		/// The concurrent collection operation retry delay in milliseconds
		/// </summary>
//		internal int ConcurrentCollectionOperationRetryDelayMs = 4;

		/// <summary>
		/// The concurrent collection operation retry delay - number of iterations to <see cref="System.Threading.Thread.SpinWait"/> for
		/// </summary>
		internal int ConcurrentCollectionOperationRetryDelayCycles = 4;
		#endregion

		#region Resources
		/// <summary>
		/// The resource search paths, indexed by type
		/// </summary>
		public ConcurrentDictionary<Type, string[]> ResourceSearchPaths { get; private set; }
		#endregion

		/// <summary>
		/// Initializes a new instance of the <see cref="JGL.EngineOptions"/> class.
		/// </summary>
		public EngineOptions()
		{
			ResourceSearchPaths = new ConcurrentDictionary<Type, string[]>(new KeyValuePair<Type, string[]>[]
			{
				new KeyValuePair<Type, string[]>(typeof(Heirarchy.Resources.Texture), new string[] { "../../../Data/Textures/" })
			// TODO: More as necessary
			});
		}
	}
}

