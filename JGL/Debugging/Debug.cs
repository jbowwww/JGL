using System;

namespace JGL.Debugging
{
	public static class Debug
	{
		public static void Assert(bool condition)
		{
			System.Diagnostics.Debug.Assert(condition);
		}
	}
}

