using System;
using System.Reflection;

namespace JGL.Extensions
{
	public static class MemberInfo_Ext
	{
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

