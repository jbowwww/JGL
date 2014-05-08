using System;
using System.Configuration;

namespace JGL.Configuration
{
	/// <summary>
	/// OpenGL configuration section.
	/// </summary>
	/// <remarks>TODO</remarks>
	public class GLConfiguration : ConfigurationSection
	{
		internal static GLConfiguration Default = new GLConfiguration()
		{
			IsDefault = true
		};

		public GLConfiguration()
		{
		}

		[ConfigurationProperty("IsDefault", IsRequired=false)]
		public bool IsDefault {
			get { return (bool)this["IsDefault"]; }
			set { this["IsDefault"] = value; }
		}


	}
}

