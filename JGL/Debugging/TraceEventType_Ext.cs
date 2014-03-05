using System;

namespace JGL.Debugging
{
	public static class TraceEventType_Ext
	{
		public static string ToString(this System.Diagnostics.TraceEventType eventType)
		{
			return Enum.GetName(typeof(System.Diagnostics.TraceEventType), eventType).Substring(0, 3);
		}
	}
}

