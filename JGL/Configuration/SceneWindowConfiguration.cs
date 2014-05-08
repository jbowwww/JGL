using System;
using System.Configuration;

namespace JGL.Configuration
{
	/// <summary>
	/// Scene window configuration section.
	/// </summary>
	/// <remarks>TODO</remarks>
	public class SceneWindowConfiguration : ConfigurationElement
	{
		internal static SceneWindowConfiguration Default = new JGL.Configuration.SceneWindowConfiguration()
		{
			Name = "Default",
			IsDefault = true,
			Graphics = GraphicsConfiguration.Default
		};

		internal SceneWindowConfiguration()
		{
		}

		[ConfigurationProperty("Name", IsRequired=true)]
		public string Name {
			get { return (string)this["Name"]; }
			set { this["Name"] = value; }
		}

		[ConfigurationProperty("IsDefault", IsRequired=false)]
		public bool IsDefault {
			get { return (bool)this["IsDefault"]; }
			set { this["IsDefault"] = value; }
		}

		[ConfigurationProperty("Graphics", IsRequired=false)]
		public GraphicsConfiguration Graphics {
			get { return (GraphicsConfiguration)this["Graphics"]; }
			set { this["Graphics"] = value; }
		}


	}
}

