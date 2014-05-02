using System;
using System.IO;
using System.Diagnostics;

namespace JGL.Debugging
{
	/// <summary>
	/// Asynchronous trace listener that writes to a file
	/// </summary>
	public class AsyncTextFileTraceListener : AsyncTraceListener
	{
		/// <summary>
		/// Tracing
		/// </summary>
//		public static readonly AutoTraceSource Trace = AutoTraceSource.GetOrCreate(AsyncXmlFileTraceListener.GetOrCreate("JGL"));

		/// <summary>
		/// Gets (if exists) or creates a <see cref="AsyncFileTraceListener"/> with the specified <paramref name="name"/>
		/// </summary>
		/// <returns>An <see cref="AsyncFileTraceListener"/> reference</returns>
		/// <param name="name">Name of the <see cref="AsyncFileTraceListener"/> to get or create</param>
		public static AsyncTextFileTraceListener GetOrCreate(string name)
		{
			return (AsyncTextFileTraceListener)AsyncTraceListener.GetOrCreate(name, typeof(AsyncTextFileTraceListener));
		}

		/// <summary>
		/// The path prefix.
		/// </summary>
		public virtual string PathPrefix { get { return "../../Logs/"; } }

		/// <summary>
		/// The path suffix.
		/// </summary>
		public virtual string PathSuffix { get { return ".Trace.Log"; } }

		/// <summary>
		/// Path of the file that this trace listener will open
		/// </summary>
		public string Path {
			get { return string.Concat(PathPrefix, Name, PathSuffix); }
		}

		/// <summary>
		/// Whether to truncate the file before opening it for writing
		/// </summary>
		public bool TruncateFile = true;

		/// <summary>
		/// Initializes a new instance of the <see cref="JGL.Debugging.AsyncTextFileTraceListener"/> class.
		/// </summary>
		/// <param name="name">Name of the new trace listener</param>
		internal AsyncTextFileTraceListener(string name)
			: base(name)
		{
		}

		/// <summary>
		/// Opens the <see cref="Stream"/> the trace listener will write to
		/// </summary>
		/// <returns>The newly opened <see cref="Stream"/></returns>
		/// <remarks>
		/// Implementation of abstract member of JGL.Debugging.AsyncTraceListener
		/// </remarks>
		internal override Stream OpenStream()
		{
			string dir = System.IO.Path.GetDirectoryName(Path);
			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);
			if (File.Exists(Path) && TruncateFile)
				File.Delete(Path);
			Stream stream = File.Open(Path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
			Trace.Log(TraceEventType.Information, "OpenStream (this.Name = \"{0}\", this.Path = \"{1}\")", Name, Path);
			return stream;
		}

		/// <summary>
		/// Called by <see cref="System.Diagnostics.TraceSource"/> methods to specify a message to log
		/// </summary>
		/// <param name="eventCache">Event cache</param>
		/// <param name="source">Source</param>
		/// <param name="eventType">Event type</param>
		/// <param name="id">Identifier</param>
		/// <param name="data">Data</param>
		public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
		{
//			LogMessage.EnsureType(data);
			LogMessage msg = (LogMessage)data;
			byte[] buf = FormatMessage(string.Concat(msg.MessageAsText, "\n"));
			Stream.Write(buf, 0, buf.Length);													// Write message buffer
			Stream.Flush();
		}
	}
}