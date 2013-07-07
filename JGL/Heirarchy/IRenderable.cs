using System;
using System.Collections.Generic;

namespace JGL.Heirarchy
{
	/// <summary>
	/// Implemented by renderable instances in a <see cref="JGL.Heirarchy.Scene"/>
	/// </summary>
	public interface IRenderable
	{
		/// <summary>
		/// Render the specified currentlyUnused.
		/// </summary>
		/// <param name='currentlyUnused'>Will probably need to pass some sort of state and/or context</param>
		void Render(RenderArgs renderArgs);
	}
}
