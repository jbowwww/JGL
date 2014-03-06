using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace JGL.Extensions
{
	public static class Array_Ext
	{
		public static string ToString(this object[] items)
		{
//			StringBuilder sb = new StringBuilder();
			return string.Format("object[{0}]", items.Length);
		}

		public static string ToString<T>(this T[] items)
		{
			return string.Format(typeof(T).Name, items.Length);
		}

		public static bool Contains(this IEnumerable items, object item)
		{
			foreach (object i in items)
				if (i.Equals(item))
					return true;
			return false;
		}

		public static bool Contains<T>(this IEnumerable<T> items, T item)
		{
			foreach (T i in items)
				if (i.Equals(item))
					return true;
			return false;
		}
	}
}

