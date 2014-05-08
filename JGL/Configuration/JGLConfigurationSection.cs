using System;
using System.Configuration;

namespace JGL.Configuration
{
	/// <summary>
	/// JGL configuration section.
	/// </summary>
	public class JGLConfigurationSection : ConfigurationSection
	{
		/// <summary>
		/// Gets the debug config section
		/// </summary>
		[ConfigurationProperty("debug", IsRequired = false)]
		public DebuggingConfiguration Debug {
			get { return (DebuggingConfiguration)this["Debug"]; }
			set { this["Debug"] = value; }
		}

		/// <summary>
		/// Gets the scene window profiles config section
		/// </summary>
		[ConfigurationCollection(typeof(SceneWindowConfiguration))]
		[ConfigurationProperty("sceneWindowProfiles")]
		public SceneWindowConfigurationCollection SceneWindowProfiles {
			get { return (SceneWindowConfigurationCollection)this["SceneWindowProfiles"]; }
			set { this["SceneWindowProfiles"] = value; }
		}

		/// <summary>
		/// Gets the scene window default config section
		/// </summary>
		public SceneWindowConfiguration DefaultSceneWindowProfile {
			get { return (SceneWindowConfiguration)this["DefaultSceneWindowProfile"]; }
			set { this["DefaultSceneWindowProfile"] = value; }
		}

	}
}

