using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using System.Diagnostics;

namespace JGL.Extensions
{
	public static class Type_Ext
	{
		/// <summary>
		/// Determines whether the given type is simple (i.e. a primitive, a string, datetime or timespan)
		/// </summary>
		/// <param name='T'>The type to test</param>
		public static bool IsSimple (this Type T)
		{
			return T.IsPrimitive ||
			    T == typeof(System.String) ||
			    T == typeof(System.DateTime) ||
			    T == typeof(System.TimeSpan);			
		}
		
		/// <summary>
		/// Gets a type's heirarchy, i.e. an array of <see cref="System.Type"/>s, starting with basest type
		/// (<see cref="System.Object"/> or <see cref="System.ValueType"/>), and ending with <paramref name="T"/>
		/// </summary>
		/// <returns>
		/// An <see cref="System.Collections.Generic.IEnumerable<System.Type>"/> representing the type <paramref name="T"/>'s heirarchy
		/// </returns>
		/// <param name='T'>The <see cref="System.Type"/> to get the type heirarchy for</param>
		public static IEnumerable<Type> GetBaseTypes(this Type T, Func<Type, bool> filter = null)
		{
			List<Type> heirarchy = new List<Type>();
			for (Type type = T; type.BaseType != null; type = type.BaseType)
				if (filter == null || filter.Invoke(type))
					heirarchy.Add(type);		//heirarchy += type;
			heirarchy.Reverse();
			return heirarchy;
		}
		
		/// <summary>
		/// Determines whether <paramref name="T"/> has an element value type
		/// </summary>
		/// <returns><c>true</c> if <paramref name="T"/> has an element value type; otherwise, <c>false</c>.</returns>
		/// <param name='T'>The <see cref="System.Type"/> to test for an element value type.</param>
		/// <remarks>
		/// A type has an element value type if it implements <see cref="System.ICollection<T>"/>. This includes arrays, dictionaries, lists (+ what else..?)
		/// </remarks>
		public static bool HasElementValueType(this Type T)
		{
			return T.GetInterface ("ICollection`1") != null;
		}
		
		/// <summary>
		/// If T implements <see cref="System.Collections.ICollection"/> (or the generic version) or <see cref="System.Collections.IDictionary"/>
		/// (or the generic version), returns the type of the element. For dictionary types, it returns the element's value type. If the non-generic
		/// interfaces only are implemented, the element type is returned as <see cref="System.Object"/>. <paramref name="isDictionary"/>
		/// receives a boolean value that indicates if the type is a dictionary type
		/// </summary>
		/// <returns>The element type</returns>
		/// <param name='T'>A type that implements either <see cref="System.Collections.ICollection"/> and/or <see cref="System.Collections.IDictionary"/></param>
		/// <param name='isDictionary'>An 
		public static Type GetElementValueType(this Type T, out bool isDictionary)
		{
			Debug.Assert (T.HasElementValueType());
			Type elementDeclaredType;
			isDictionary = false;
			if (T.GetInterface ("ICollection`1") != null)
			{
				elementDeclaredType = T.GetInterface ("ICollection`1").GetGenericArguments ()[0];
				if (elementDeclaredType.ContainsGenericParameters && elementDeclaredType.GetGenericTypeDefinition()
					    == (typeof(System.Collections.Generic.KeyValuePair<object, object>).GetGenericTypeDefinition())
				    || elementDeclaredType == typeof(System.Collections.DictionaryEntry))
				{
					isDictionary = true;
					elementDeclaredType = elementDeclaredType.ContainsGenericParameters ?
						elementDeclaredType.GetGenericArguments ()[1] : typeof(System.Object);
				}
			}
			else
				elementDeclaredType = typeof(System.Object);
			return elementDeclaredType;
		}	
	}
}