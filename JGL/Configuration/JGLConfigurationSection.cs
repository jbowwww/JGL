using System;
using System.Configuration;

namespace JGL.Configuration
{
	/// <summary>
	/// JGL configuration section.
	/// </summary>
	public class JGLConfigurationSection : ConfigurationSection
	{
		public JGLConfigurationSection()
		{
		}

		[ConfigurationProperty(Name = "Debug", IsRequired = false)]
		public DebuggingConfigurationElement Debug {
			get;
			private set;
		}

		public SceneWindowConfigurationSection SceneWindowDefault {
			get;
			private set;
		}

		[ConfigurationCollection(
			ItemType = typeof(SceneWindowConfigurationSection),
			AddItemName = "add",
			ClearItemsName = "clear",
			RemoveItemName = "remove")]
		public ConfigurationSectionCollection SceneWindowProfiles {
			get;
			private set;
		}
	}
}

