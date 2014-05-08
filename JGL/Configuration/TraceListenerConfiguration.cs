using System;
using System.Configuration;
using OpenTK;
using System.Diagnostics;

namespace JGL.Configuration
{
	/// <summary>
	/// Debugging configuration element.
	/// </summary>
	/// <remarks>
	/// TODO: Devise and implement a mechanism for supplying parameters to TraceListener-derived types
	/// </remarks>
	public class TraceListenerConfiguration : ConfigurationElement
	{
		internal TraceListenerConfiguration()
		{
		}

		[ConfigurationProperty("name", IsRequired=true)]
		public string Name {
			get { return (string)this["Name"]; }
			set { this["Name"] = value; }
		}

//		[ConfigurationProperty("Type", IsRequired=true)]
//		public Type Type {
//			get { return (Type)Type.GetType((string)this["Type"]); }
//			set { this["Type"] = value.FullName; }
//		}

		[ConfigurationProperty("level", IsRequired=true)]
		public SourceLevels Level {
			get { return (SourceLevels)Enum.Parse(typeof(SourceLevels), (string)this["Level"]); }
			set { this["Name"] = value.ToString(); }
		}
	}
}

