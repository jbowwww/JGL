using System;
using System.Configuration;

namespace JGL.Configuration
{
	/// <summary>
	/// Graphics configuration section.
	/// </summary>
	/// <remarks>TODO</remarks>
	public class GraphicsConfigurationSection : ConfigurationSection
	{
		public GraphicsConfigurationSection()
		{
		}

		[ConfigurationProperty(Name="GL", IsRequired=false)]
		GLConfigurationSection GL {
			get;
			private set;
		}
	}
}

