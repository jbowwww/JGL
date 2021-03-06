using System;
using System.Reflection;
using JGL.Debugging;

namespace JGL.Extensions
{
	public static class MemberInfo_Ext
	{
		/// <summary>
		/// Tracing
		/// </summary>
		public static readonly AutoTraceSource Trace = AutoTraceSource.GetOrCreate(AsyncXmlFileTraceListener.GetOrCreate("JGL"));

		public static bool HasAttribute<TAttribute>(this MemberInfo member)
			where TAttribute : Attribute
		{
			object[] attrs = member.GetCustomAttributes(typeof(TAttribute), false);
			return attrs.Length > 0;
		}

		public static TAttribute GetAttribute<TAttribute>(this MemberInfo member)
			where TAttribute : Attribute
		{
			object[] attrs = member.GetCustomAttributes(typeof(TAttribute), false);
			return attrs.Length > 0 ? attrs[0] as TAttribute : null;
		}
	}
}

