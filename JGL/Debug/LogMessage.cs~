using System;
using System.Collections;
using System.Text;
using System.Reflection;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
using System.Diagnostics;
using JGL.Extensions;

namespace JGL.Debug
{
	/// <summary>
	/// Log message class stores log message parameters
	/// </summary>
	internal class LogMessage
	{
		/// <summary>
		/// Ensures that <paramref name="message"/> is of type <see cref="LogMessage"/>.
		/// </summary>
		/// <remarks>
		///	-	Used internally for <see cref="TraceListener"/>s that override <see cref="TraceListener.TraceData"/>
		/// </remarks>
		/// <param name="message">Instance to check type of</param>
		/// <exception cref="ArgumentNullException">
		/// Is thrown when an argument passed to a method is invalid because it is <see langword="null" /> .
		/// </exception>
		/// <exception cref="ArgumentException">
		/// Is thrown when an argument passed to a method is invalid.
		/// </exception>
//		public static void EnsureType(object message)
//		{
//			if (message == null)
//				throw new ArgumentNullException("data");
//			if (message.GetType().IsTypeOf(typeof(LogMessage)))
//				throw new ArgumentException("Not of type LogMessage", "data");
//		}

		/// <summary>Event cache</summary>
		public System.Diagnostics.TraceEventCache EventCache;

		/// <summary>The <see cref="TraceSource"/> that the message originated from</summary>
//		public TraceSource Source;

		/// <summary><see cref="TraceOptions"/> for output</summary>
//		public TraceOptions OutputOptions;

		/// <summary><see cref="TraceEventType"/> event type</summary>
//		public TraceEventType EventType;

		/// <summary>
		/// The <see cref="JGL.Debug.Trace.Level"/>
		/// </summary>
		public Trace.Level Level;

//		/// <summary>Trace message ID</summary>
//		public int Id;

		/// <summary>Time</summary>
		public DateTime Time {
			get { return EventCache.DateTime; }
		}

		/// <summary>Time stamp</summary>
		public long TimeStamp {
			get { return EventCache.Timestamp; }
		}

		/// <summary>Process Id</summary>
		public int Process {
			get { return EventCache.ProcessId; }
		}

		/// <summary>Thread Id</summary>
		public string Thread {
			get { return EventCache.ThreadId; }
		}

		/// <summary>
		/// The <see cref="StackFrame"/> that called <see cref="AutoTraceSource.Log"/>
		/// </summary>
		public StackFrame Frame {
			get; private set;
		}

		/// <summary>Callstack</summary>
		public string Callstack {
			get { return EventCache.Callstack; }
		}

		/// <summary>Logical operation stack</summary>
		public Stack LogicalOperationStack {
			get { return EventCache.LogicalOperationStack; }
		}

		/// <summary>Format string (or just a plain message string)</summary>
		public string Format;

		/// <summary>Data to go with the </summary>
		public object Data;

		/// <summary>Message text</summary>
		/// <remarks>
		///	-	If <code><see cref="Format"/> == null</code>, returns <code><see cref="Data"/>.ToString()></code>
		///	-	Else if <code><see cref="Data"/> == null</code>, returns <see cref="Format"/>
		///	-	Else returns <code>string.Format(<see cref="Format"/>, returns <see cref="Data"/>)</code>
		/// </remarks>
//		public string Message {
//			get
//			{
//				return string.Concat(Frame.GetMethod().ReflectedType.Name, ".", Frame.GetMethod().Name, " ",
//					Format == null ? (string)Data : Data == null ? Format : string.Format(Format, (object[])Data));
//			}
//		}

		/// <summary>Gets the formatted message string</summary>
//		public string MessageAsText {
//			get
//			{
//				StringBuilder sb = new StringBuilder(256);
//				if (OutputOptions.HasFlag(TraceOptions.DateTime))
//					sb.Append(EventCache.DateTime.ToString("yy-MM-dd HH:mm:ss.ffffff "));
//				if (OutputOptions.HasFlag(TraceOptions.Timestamp))
//					sb.Append(string.Concat(EventCache.Timestamp.ToString(), " "));
//				if (OutputOptions.HasFlag(TraceOptions.ProcessId))
//					sb.Append(string.Concat("P:", EventCache.ProcessId.ToString(), " "));
//				if (OutputOptions.HasFlag(TraceOptions.ThreadId))
//					sb.Append(string.Concat("T:", EventCache.ThreadId, " "));
//				sb.Append(string.Concat(Source.Name, " ", Frame.GetMethod().ReflectedType.Name, ".",
//					Frame.GetMethod().Name, " ", Id.ToString("d3"), " ", EventType.ToString(), " "));
//				if (Format == null)
//				{
//					if (Data != null)
//						sb.Append(Data.ToString());
//				}
//				else if (Data == null)
//						sb.Append(Format);
//					else
//						sb.Append(string.Format(Format, (object[])Data));
//				if (OutputOptions.HasFlag(TraceOptions.Callstack))
//					sb.Append(string.Concat("\nCallstack:\n", EventCache.Callstack));
//				if (OutputOptions.HasFlag(TraceOptions.LogicalOperationStack) && EventCache.LogicalOperationStack.Count > 0)
//				{
//					sb.Append("\nOperation stack:");
//					foreach (object stackEntry in EventCache.LogicalOperationStack)
//						sb.Append(string.Concat("\n", stackEntry));
//				}
////				sb.Append("\n");
//				return sb.ToString();
//			}
//		}

		/// <summary>
		/// Initializes a new instance of the <see cref="JGL.Debugging.AsyncTraceListener.LogMessage"/> class.
		/// </summary>
		/// <param name="listener">The listener the message is for</param>
		/// <param name="eventCache">Event cache</param>
		/// <param name="source">Source</param>
		/// <param name="eventType">Event type</param>
		/// <param name="id">Identifier</param>
		/// <param name="data">Data</param>
		public LogMessage(Trace.Level level, object data)
		{
			Init(level, null, data);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="JGL.Debugging.AsyncTraceListener.LogMessage"/> class.
		/// </summary>
		/// <param name="listener">The listener the message is for</param>
		/// <param name="eventCache">Event cache</param>
		/// <param name="source">Source</param>
		/// <param name="eventType">Event type</param>
		/// <param name="id">Identifier</param>
		/// <param name="message">Message</param>
		public LogMessage(Trace.Level level, string message)
		{
			Init(level, message, null);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="JGL.Debugging.AsyncTraceListener.LogMessage"/> class.
		/// </summary>
		/// <param name="listener">The listener the message is for</param>
		/// <param name="eventCache">Event cache</param>
		/// <param name="source">Source</param>
		/// <param name="eventType">Event type</param>
		/// <param name="id">Identifier</param>
		/// <param name="format">Message</param>
		public LogMessage(Trace.Level level, string format, params object[] data)
		{
			Init(level, format, data);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="JGL.Debugging.LogMessage"/> class.
		/// </summary>
		/// <param name="listener">The listener the message is for</param>
		/// <param name="eventCache">Event cache</param>
		/// <param name="source">Source</param>
		/// <param name="eventType">Event type</param>
		/// <param name="id">Identifier</param>
		/// <param name="exception">Exception to log</param>
		public LogMessage(Exception exception)
		{
			StringBuilder sb = new StringBuilder();
			string message, indent = string.Empty;
			for (Exception _ex = exception; _ex != null; _ex = _ex.InnerException)
			{
				sb.AppendFormat("{0}{1}: {2}\n{0}Stacktrace:\n{0}    {3}\n", indent, _ex.GetType().Name, _ex.Message, _ex.StackTrace.Replace("\n", "\n    " + indent));
				if (_ex.InnerException != null)
					sb.AppendFormat("{0}InnerException:\n    ", indent);
				indent += "    ";
			}
			message = sb.ToString();
			Init(Trace.Level.Error, message, null);
		}

		/// <summary>
		/// Init the specified eventCache, source, eventType, id, format and data.
		/// </summary>
		/// <param name="level">Trace message level</param>
		/// <param name="format">Message</param>
		/// <param name="data">Data</param>
		private void Init(Trace.Level level, string format, object data)
		{
//			EventCache = eventCache;
//			Source = source;
//			EventType = eventType;
//			Id = id;
			EventCache = new TraceEventCache();
			Level = level;
			Format = format;
			Data = data;
			Frame = new StackFrame(3, true);
		}

		/// <summary>
		/// Returns a <see cref="System.String"/> that represents the current <see cref="JGL.Debugging.LogMessage"/>.
		/// </summary>
		/// <remarks>
		///	-	<see cref="System.Diagnostics.ConsoleTraceListener"/> relies on this
		/// </remarks>
//		public override string ToString()
//		{
//			return MessageAsText;
//		}

		/// <summary>Writes the xml</summary>
		/// <param name="writer">The <see cref="XmlWriter"/> to write to</param>
		/// <remarks>IXmlSerializable implementation</remarks>
//		public void WriteXml(XmlWriter xw)
//		{
//			xw.WriteStartElement(EventType.ToString());
//			xw.WriteAttributeString("Id", Id.ToString());
//			if (OutputOptions.HasFlag(TraceOptions.DateTime))
//				xw.WriteAttributeString("Time", Time.ToString("yy-MM-dd HH:mm:ss.ffffff "));
//			if (OutputOptions.HasFlag(TraceOptions.Timestamp))
//				xw.WriteAttributeString("Timestamp", TimeStamp.ToString());
//			if (OutputOptions.HasFlag(TraceOptions.ProcessId))
//				xw.WriteAttributeString("Process", Process.ToString());
//			if (OutputOptions.HasFlag(TraceOptions.ThreadId))
//				xw.WriteAttributeString("Thread", Thread);
//
//			xw.WriteAttributeString("Source", Source.Name);
//			xw.WriteAttributeString("Method", string.Concat(Frame.GetMethod().ReflectedType.Name, ".", Frame.GetMethod().Name));
//			xw.WriteElementString("Message", Message);
//
//			if (OutputOptions.HasFlag(TraceOptions.Callstack))
//			{
//				xw.WriteStartElement("Callstack");
//				foreach (string call in Callstack.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries))
//					xw.WriteElementString("Call", call);		// TODO: split 'call' string into components like file,line,column,type,method,etc/??
//				xw.WriteEndElement();
//			}
//			if (OutputOptions.HasFlag(TraceOptions.LogicalOperationStack) && LogicalOperationStack.Count > 0)
//			{
//				xw.WriteStartElement("Opstack");
//				foreach (object op in LogicalOperationStack)
//					xw.WriteElementString("Op", op.ToString());
//				xw.WriteEndElement();
//			}
//			xw.WriteEndElement();
//			xw.WriteWhitespace("\n");
//		}

		#region Unused
		/// <summary>Gets the schema</summary>
		/// <exception cref="NotImplementedException">Is thrown when a requested operation is not implemented for a given type</exception>
		/// <remarks>IXmlSerializable implementation</remarks>
//		public System.Xml.Schema.XmlSchema GetSchema()
//		{
//			throw new NotImplementedException();
//		}

		/// <summary>Reads the xml</summary>
		/// <exception cref="NotImplementedException">Is thrown when a requested operation is not implemented for a given type</exception>
		/// <remarks>IXmlSerializable implementation</remarks>
//		public void ReadXml(XmlReader xr)
//		{
//			throw new NotImplementedException();
//		}
		#endregion
	}
}

