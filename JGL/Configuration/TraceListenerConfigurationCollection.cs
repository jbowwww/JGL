using System;
using System.Configuration;
using System.Linq;
using JGL.Graphics;
using System.Xml;

namespace JGL.Configuration
{
	[ConfigurationCollection(typeof(TraceListenerConfiguration),
		CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap)]
	public class TraceListenerConfigurationCollection : ConfigurationElementCollection
	{
//		public TraceListenerConfiguration this[int index]
//		{
//			get { return (TraceListenerConfiguration)BaseGet(index); }
//			set
//			{
//				if (BaseGet(index) != null)
//				{
//					BaseRemoveAt(index);
//					BaseAdd(index, value);
//				}
//			}
//		}
//
//		public new TraceListenerConfiguration this[string name]
//		{
//			get { return (TraceListenerConfiguration)BaseGet(name); }
//			set
//			{
//				if (BaseGet(name) != null)
//				{
//					BaseRemove(name);
//					BaseAdd(value);
//				}
//			}
//		}

//		public void Add(TraceListenerConfiguration config)
//		{
//			BaseAdd(config);
//		}
//
//		public void Remove(TraceListenerConfiguration config)
//		{
//			BaseRemove(config);
//		}
//
//		public void RemoveAt(int index)
//		{
//			BaseRemoveAt(index);
//		}
//
//		public void Clear()
//		{
//			BaseClear();
//		}

		#region implemented abstract members of ConfigurationElementCollection
		private Type _TListener;

		/// <summary>
		/// Gets the type of the listener. Has to derive from TraceListener
		/// </summary>
		/// <returns>The listener type.</returns>
		/// <param name="listenerTypeName">Listener short type name.</param>
		public Type GetListenerType(string listenerTypeName)
		{
			return _TListener ?? (_TListener =
				typeof(System.Diagnostics.TraceListener).Assembly.GetTypes().First((T)
					=> T.Name.ToLower().Equals(listenerTypeName, StringComparison.CurrentCultureIgnoreCase)
					&& typeof(System.Diagnostics.TraceListener).IsAssignableFrom(T)));
		}

		protected override string ElementName {
			get { return string.Empty; }		// TraceSourceConfigElementName; }
		}

		protected override bool IsElementName(string elementName)
		{
			return GetListenerType(elementName) != null;
		}

		protected override ConfigurationElement CreateNewElement()
		{
			return new TraceListenerConfiguration();
		}

		protected override ConfigurationElement CreateNewElement(string elementName)
		{
			return new TraceListenerConfiguration() { Name = elementName };
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((TraceListenerConfiguration)element).Name;
		}

//		protected override void BaseAdd(ConfigurationElement element)
//		{
//			base.BaseAdd(element);
//		}

		#endregion
	}
}

