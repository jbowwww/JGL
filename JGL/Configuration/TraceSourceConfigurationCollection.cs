using System;
using System.Configuration;
using System.Linq;

namespace JGL.Configuration
{
	[ConfigurationCollection(typeof(TraceSourceConfiguration),
		CollectionType = ConfigurationElementCollectionType.AddRemoveClearMap)]
	public class TraceSourceConfigurationCollection : ConfigurationElementCollection
	{
//		public const string TraceSourceConfigElementName = "traceSource";

		protected override bool ThrowOnDuplicate { get { return true; } }

		#region implemented abstract members of ConfigurationElementCollection
		private Type _TSource;

		/// <summary>
		/// Gets the type of the listener. Has to derive from TraceListener
		/// </summary>
		/// <returns>The listener type.</returns>
		/// <param name="listenerTypeName">Listener short type name.</param>
		public Type GetSourceType(string sourceTypeName)
		{
			return _TSource ?? (_TSource =
				typeof(System.Diagnostics.TraceListener).Assembly.GetTypes().First((T)
					=> T.Name.ToLower().Equals(sourceTypeName, StringComparison.CurrentCultureIgnoreCase)
					&& typeof(System.Diagnostics.TraceListener).IsAssignableFrom(T)));
		}

		protected override string ElementName {
			get { return string.Empty; }		// TraceSourceConfigElementName; }
		}

		protected override bool IsElementName(string elementName)
		{
			return GetSourceType(elementName) != null;
		}

		protected override object GetElementKey(ConfigurationElement element)
		{
			return ((TraceSourceConfiguration)element).Name;
		}

		protected override ConfigurationElement CreateNewElement()
		{
			return new TraceSourceConfiguration();
		}

		protected override ConfigurationElement CreateNewElement(string elementName)
		{
			return new TraceSourceConfiguration() { Name = elementName };
		}
		#endregion
	}
}

