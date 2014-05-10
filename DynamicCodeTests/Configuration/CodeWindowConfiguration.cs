using System;
using System.Configuration;
using GLib;
using System.Collections.Generic;
using Dynamic.UI;

namespace Dynamic.Configuration
{
	public class CodeWindowConfiguration : ConfigurationSection
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="Dynamic.Configuration.CodeWindowConfiguration"/> class.
		/// </summary>
		/// <param name="recentProjectPaths">Recent project paths.</param>
		/// <param name="recentFilePaths">Recent file paths.</param>
		public CodeWindowConfiguration(string[] recentProjectPaths = null, string[] recentFilePaths = null)
		{
//			Init();
//			RecentProjectPaths = recentProjectPaths == null ? new string[] { } : recentProjectPaths;
//			RecentFilePaths = recentFilePaths == null ? new string[] { } : recentFilePaths;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Dynamic.Configuration.CodeWindowConfiguration"/> class.
		/// </summary>
		public CodeWindowConfiguration()
		{
//			Init();
		}

		/// <summary>
		/// The recent project paths.
		/// </summary>
		[ConfigurationCollection(typeof(string))]
		[ConfigurationProperty("RecentProjectPaths", IsRequired = false)]
		public NameValueConfigurationCollection RecentProjectPaths {
			get { return (NameValueConfigurationCollection)this["RecentProjectPaths"]; }
			set { this["RecentProjectPaths"] = value; }
		}

		/// <summary>
		/// The recent file paths.
		/// </summary>
		[ConfigurationCollection(typeof(string))]
		[ConfigurationProperty("RecentFilePaths", IsRequired = false)]
		public NameValueConfigurationCollection RecentFilePaths {
			get { return (NameValueConfigurationCollection)this["RecentFilePaths"]; }
			set { this["RecentFilePaths"] = value; }
		}
	}
}

