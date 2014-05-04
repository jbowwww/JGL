using System;
using System.Configuration;

namespace JGL.Configuration
{
	/// <summary>
	/// Scene window configuration section.
	/// </summary>
	/// <remarks>TODO</remarks>
	public class SceneWindowConfigurationSection : ConfigurationSection
	{
		public SceneWindowConfigurationSection()
		{
		}

		[ConfigurationProperty(Name="Graphics", IsRequired=true)]
		public string Name {
			get;
			private set;
		}

		[ConfigurationProperty(Name="Graphics", IsRequired=false)]
		public bool IsDefault {
			get;
			private set;
		}

		[ConfigurationProperty(Name="Graphics", IsRequired=false)]
		public GraphicsConfigurationSection Graphics {
			get;
			private set;
		}


	}
}

