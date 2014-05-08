using System;

namespace JGL.Configuration
{
	public class TraceListenerConfigurationCollectionCollection : ConfigurationElementCollection
	{
		public TraceListenerConfigurationCollection this[int index]
		{
			get { return (TraceListenerConfigurationCollection)BaseGet(index); }
			set
			{
				if (BaseGet(index) != null)
				{
					BaseRemoveAt(index);
					BaseAdd(index, value);
				}
			}
		}

		public new TraceListenerConfigurationCollection this[string name]
		{
			get { return (TraceListenerConfigurationCollection)BaseGet(name); }
			set
			{
				if (BaseGet(name) != null)
				{
					BaseRemove(name);
					BaseAdd(value);
				}
			}
		}

		public void Add(TraceListenerConfigurationCollection config)
		{
			BaseAdd(config);
		}

		public void Remove(TraceListenerConfigurationCollection config)
		{
			BaseRemove(config);
		}

		public void RemoveAt(int index)
		{
			BaseRemoveAt(index);
		}

		public void Clear()
		{
			BaseClear();
		}

		#region implemented abstract members of ConfigurationElementCollection

		protected override ConfigurationElement CreateNewElement()
		{
			throw new NotImplementedException();
		}

		protected override ConfigurationElement CreateNewElement(string elementName)
		{
			return new TraceListenerConfigurationCollection() { AssemblyName = elementName };
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((TraceListenerConfiguration)element).Name;
		}

		protected override void BaseAdd(ConfigurationElement element)
		{
			base.BaseAdd(element);
		}

		#endregion
	}
}

