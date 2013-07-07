using System;
using System.Xml;
using System.Xml.Linq;
using JGL.Data;
using JGL.Extensions;

namespace SerializationTests.Extensions
{
	public static class XElement_Ext
	{
		public static implicit operator XElement (this Instance data)
		{
			XElement r = new XElement(data.ReflectedType.Name, data.Members.Nodes());
			return r;
			
		}
		
		public static implicit operator XElement (InstanceMember data)
		{
			XElement r = new XElement(data.Name, data.ReflectedType.IsSimple() ? data.Value.ToString () : data.Instance);
			return r;
			
		}
		
		
	}
}

