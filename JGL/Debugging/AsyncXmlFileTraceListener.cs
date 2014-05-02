using System;
using System.Text;
using System.Xml;
using JGL.Extensions;
using System.Diagnostics;

namespace JGL.Debugging
{
	/// <summary>
	/// Async xml trace listener.
	/// </summary>
	public class AsyncXmlFileTraceListener
		: AsyncTextFileTraceListener
	{
//		private static AutoTraceSource Trace = AutoTraceSource.GetOrCreate(AsyncXmlFileTraceListener.GetOrCreate("JGL"));

		public static new AsyncTraceListener GetOrCreate(string name)
		{
			return AsyncTraceListener.GetOrCreate(name, typeof(AsyncXmlFileTraceListener));
		}

		public override string PathSuffix { get { return ".Trace.Log.Xml"; } }

		/// <summary>
		/// Initializes a new instance of the <see cref="JGL.Debugging.AsyncXmlFileTraceListener"/> class.
		/// </summary>
		internal AsyncXmlFileTraceListener(string name) : base(name)
		{
			TraceOutputOptions |= System.Diagnostics.TraceOptions.LogicalOperationStack;
		}

		/// <summary>
		/// Traces the data.
		/// </summary>
		/// <param name="eventCache">Event cache</param>
		/// <param name="source">Source</param>
		/// <param name="eventType">Event type</param>
		/// <param name="id">Identifier</param>
		/// <param name="data">Data - should be a <see cref="LogMessage"/> instance</param>
		public override void TraceData(System.Diagnostics.TraceEventCache eventCache, string source, System.Diagnostics.TraceEventType eventType, int id, object data)
		{
//			LogMessage.EnsureType(data);
			LogMessage msg = (LogMessage)data;
			XmlWriterSettings xws = new XmlWriterSettings()
			{
				CheckCharacters = true,
				CloseOutput = false,
				ConformanceLevel = ConformanceLevel.Fragment,
				Encoding = ASCIIEncoding.ASCII,
				Indent = true, IndentChars = "\t",
				NamespaceHandling = NamespaceHandling.OmitDuplicates,
//				NewLineChars = "\n",
//				NewLineHandling = NewLineHandling.Entitize,
				NewLineOnAttributes = false,
				OmitXmlDeclaration = true
			};
			using (XmlWriter xw = XmlWriter.Create(Stream, xws))
				msg.WriteXml(xw);
		}
	}
}

