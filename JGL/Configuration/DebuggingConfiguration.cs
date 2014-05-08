using System;
using System.Configuration;
using System.CodeDom;

namespace JGL.Configuration
{
	public class DebuggingConfiguration : ConfigurationElement
	{
		[ConfigurationProperty("defaultSourceConfig", IsRequired = false, DefaultValue = default(TraceSourceConfiguration))]
		public TraceSourceConfiguration DefaultSourceConfig {
			get { return (TraceSourceConfiguration)this["DefaultSourceConfig"]; }
			set { this["DefaultSourceConfig"] = value; }
//			{
//				TraceSourceConfiguration config =
//					TraceSources[DefaultSourceName] ??
//					TraceSources[DefaultSourceName] = new TraceSourceConfiguration();
//			}
		}

		[ConfigurationProperty("", IsDefaultCollection = true)]
		public TraceSourceConfigurationCollection TraceSources {
			get { return (TraceSourceConfigurationCollection)this["TraceSources"]; }
			set { this["TraceSources"] = value; }
		}		
	}
}

