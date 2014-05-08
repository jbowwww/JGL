using System;
using System.Configuration;
using JGL.Graphics;

namespace JGL.Configuration
{
	public class SceneWindowConfigurationCollection : ConfigurationElementCollection
	{
		public SceneWindowConfigurationCollection()
		{
		}

		public SceneWindowConfiguration this[int index]
		{
			get { return (SceneWindowConfiguration)BaseGet(index); }
			set
			{
				 if (BaseGet(index) != null)
				{
					BaseRemoveAt(index);
					BaseAdd(index, value);
				}
			}
		}

		public new SceneWindowConfiguration this[string name]
		{
			get { return (SceneWindowConfiguration)BaseGet(name); }
			set
			{
				if (BaseGet(name) != null)
				{
					BaseRemove(name);
					BaseAdd(value);
				}
			}
		}

		public void Add(SceneWindowConfiguration config)
		{
			BaseAdd(config);
		}

		public void Remove(SceneWindowConfiguration config)
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
			return new SceneWindowConfiguration();
		}

		protected override ConfigurationElement CreateNewElement(string elementName)
		{
			return new SceneWindowConfiguration() { Name = elementName };
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((SceneWindowConfiguration)element).Name;
		}

		protected override void BaseAdd(ConfigurationElement element)
		{
			base.BaseAdd(element);
		}

		#endregion
	}
}

