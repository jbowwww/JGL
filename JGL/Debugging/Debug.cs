using System;

namespace JGL.Debugging
{
	/// <summary>
	/// Wrapper class for <see cref="System.Diagnostics.Debug"/>.
	/// </summary>
	/// <remarks>
	///	-	TODO: Check all references to this class and it's members, and see if those references
	///		can be changed to use <see cref="AutoTraceSource"/> members, for example, <see cref="AutoTraceSource.Debug"/>
	///		-	This class might then be obsolete
	/// </remarks>
	public static class Debug
	{
		/// <summary>
		/// Assert the specified condition.
		/// </summary>
		/// <param name="condition">Condition</param>
		public static void Assert(bool condition)
		{
			System.Diagnostics.Debug.Assert(condition);
		}
	}
}

