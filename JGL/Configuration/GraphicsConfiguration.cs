using System;
using System.Configuration;

namespace JGL.Configuration
{
	/// <summary>
	/// Graphics configuration section.
	/// </summary>
	/// <remarks>TODO</remarks>
	public class GraphicsConfiguration : ConfigurationElement
	{
		internal static GraphicsConfiguration Default = new GraphicsConfiguration()
		{
			IsDefault = true,
			GL = GLConfiguration.Default
		};

		[ConfigurationProperty("IsDefault", IsRequired=false)]
		public bool IsDefault {
			get { return (bool)this["IsDefault"]; }
			set { this["IsDefault"] = value; }
		}

		[ConfigurationProperty("GL", IsRequired=false)]
		public GLConfiguration GL {
			get { return (GLConfiguration)this["GL"]; }
			set { this["GL"] = value; }
		}
	}
}

