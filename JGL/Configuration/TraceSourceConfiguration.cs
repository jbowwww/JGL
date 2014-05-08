using System;
using System.Configuration;
using System.Diagnostics;
using OpenTK.Graphics.ES10;
using System.Runtime.CompilerServices;

namespace JGL.Configuration
{
	public class TraceSourceConfiguration : ConfigurationElement
	{
		[ConfigurationProperty("name", IsKey = true, DefaultValue = "_default_")]		/*IsRequired = true*/
		public string Name {
			get { return (string)base["Name"]; }
			set { base["Name"] = value; }
		}

		[ConfigurationProperty("sourceLevels", IsRequired = false, DefaultValue = SourceLevels.All)]
		public SourceLevels SourceLevels {
			get { return (SourceLevels)this["SourceLevels"]; }
			set { this["SourceLevels"] = value; }
		}

		[ConfigurationProperty("addDefaultListeners", IsRequired = false, DefaultValue = true)]
		public bool AddDefaultListeners {
			get { return (bool)base["AddDefaultListeners"]; }
			set { base["AddDefaultListeners"] = value; }
		}

		[ConfigurationProperty("listeners", IsRequired = false)]
		public TraceListenerConfigurationCollection Listeners {
			get { return (TraceListenerConfigurationCollection)this["Listeners"]; }
			set { this["Listeners"] = value; }
		}
	}
}

