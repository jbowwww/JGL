using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Reflection;
using JGL.Extensions;

namespace JGL.Debugging
{
	/// <summary>
	/// A trace listener that queues data it receives into a static (shared among all <see cref="AsyncTraceListener"/>s)
	/// queue until the background thread (see <see cref="RunThread"/>) formats it for output and writes it to the <see cref="Stream"/>
	/// associated with the <see cref="AsyncTraceListener"/> the message belongs too
	/// </summary>
	public abstract class AsyncTraceListener : TraceListener
	{
		/// <summary>
		/// Tracing
		/// </summary>
		public static readonly AutoTraceSource Trace = AutoTraceSource.GetOrCreate(AsyncXmlFileTraceListener.GetOrCreate("JGL"));

		#region Static members
		/// <summary>
		/// <see cref="AsyncTraceListener"/>s, keyed by name
		/// </summary>
		private static ConcurrentDictionary<string, AsyncTraceListener> _namedListeners;

		/// <summary>
		/// Gets or create a <see cref="AsyncTraceListener"/>
		/// </summary>
		/// <returns>
		/// The retrieved or newly created <see cref="AsyncTraceListener"/>
		/// </returns>
		/// <param name="name">Name of listener to get/create</param>
		/// <param name="type">Type of listener to create</param>
		public static AsyncTraceListener GetOrCreate(string name, Type type)
		{
			if (_namedListeners == null)
				_namedListeners = new ConcurrentDictionary<string, AsyncTraceListener>();
			if (!_namedListeners.ContainsKey(name))
				_namedListeners[name] = (AsyncTraceListener)type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null,
					new Type[] { typeof(string) }, new ParameterModifier[] {}).Invoke(new object[] { name });
			return _namedListeners[name];
		}
		#endregion

		/// <summary>
		/// Time (in milliseconds) for <see cref="AsyncTraceListener.RunThread"/> to sleep for,
		/// after writing all <see cref="LogMessage"/>s in queue
		/// </summary>
		private const int ThreadWaitTime = 141;

		/// <summary>
		/// The stream that this <see cref="AsyncTraceListener"/> writes to
		/// </summary>
		protected Stream Stream = null;

		/// <summary>
		/// Initializes a new instance of the <see cref="JGL.Debugging.AsyncTraceListener"/> class.
		/// </summary>
		/// <param name="name">Name</param>
		protected AsyncTraceListener(string name) : base(name)
		{
			TraceOutputOptions = TraceOptions.DateTime | TraceOptions.ProcessId | TraceOptions.ThreadId;
		}

		/// <summary>
		/// Format message delegate method. Formats a <see cref="LogMessage"/> instance into <see cref="byte[]"/> data to be
		/// written to the output <see cref="Stream"/>. Override to customise behaviour.
		/// </summary>
		internal virtual byte[] FormatMessage(string message)
		{
			return Encoding.ASCII.GetBytes(message);
		}

		/// <summary>
		/// Dispose of this <see cref="AsyncTraceListener"/> (calls <see cref="Stop"/>)
		/// </summary>
		/// <param name="disposing">Disposing</param>
		protected override void Dispose(bool disposing = true)
		{
			if (disposing)
			{
				Flush();
				if (Stream != null)
				{
					Stream.Close();
					Stream = null;
				}
			}
		}

		/// <summary>
		/// Ensures <see cref="Stream"/> is a valid open stream
		/// </summary>
		internal virtual void EnsureOpen()
		{
			if (Stream == null)
				Stream = OpenStream();
		}

		/// <summary>
		/// The open stream method opens the output stream when required by the background thread. Override to customise behaviour.
		/// </summary>
		internal abstract Stream OpenStream();

		/// <summary>
		/// Called by <see cref="System.Diagnostics.TraceSource"/> methods to specify a message to log
		/// </summary>
		/// <param name="eventCache">Event cache</param>
		/// <param name="source">Source</param>
		/// <param name="eventType">Event type</param>
		/// <param name="id">Identifier</param>
		/// <param name="data">Data</param>
//		public override void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data)
//		{
//			if (this.Filter == null || this.Filter.ShouldTrace(eventCache, source, eventType, id, null, null, data, null))
//				if (Stream == null)
//					Stream = OpenStream();
//
//			StringBuilder sb = new StringBuilder(256);
//			if (TraceOutputOptions.HasFlag(TraceOptions.DateTime))
//				sb.Append(eventCache.DateTime.ToString("yy-MM-dd HH:mm:ss.ffffff "));
//			if (TraceOutputOptions.HasFlag(TraceOptions.Timestamp))
//				sb.Append(string.Concat(eventCache.Timestamp.ToString(), " "));
//			if (TraceOutputOptions.HasFlag(TraceOptions.ProcessId))
//				sb.Append(string.Concat("P:", eventCache.ProcessId.ToString(), " "));
//			if (TraceOutputOptions.HasFlag(TraceOptions.ThreadId))
//				sb.Append(string.Concat("T:", eventCache.ThreadId, " "));
//			sb.Append(string.Concat(source, " ", id.ToString("d3"), " ", eventType.ToString(), " ", data.ToString()));
//			if (TraceOutputOptions.HasFlag(TraceOptions.Callstack))
//				sb.Append(string.Concat("\nCallstack:\n", eventCache.Callstack));
//			if (TraceOutputOptions.HasFlag(TraceOptions.LogicalOperationStack) && eventCache.LogicalOperationStack.Count > 0)
//			{
//				sb.Append("\nOperation stack:");
//				foreach (object stackEntry in eventCache.LogicalOperationStack)
//					sb.Append(string.Concat("\n", stackEntry));
//			}
//			sb.Append("\n");
//			byte[] buf = FormatMessage(sb.ToString());
//			Stream.Write(buf, 0, buf.Length);													// Write message buffer
//			Stream.Flush();
//		}

		/// <summary>
		/// Base implementation throws exception if <paramref name="data"/> is <c>null</c> or not of type <see cref="LogMessage"/>
		/// </summary>
		/// <param name="eventCache"></param>
		/// <param name="source"></param>
		/// <param name="eventType"></param>
		/// <param name="id"></param>
		/// <param name="data">A <see cref="LogMessage"/> instance</param>
//		public abstract void TraceData(TraceEventCache eventCache, string source, TraceEventType eventType, int id, object data);
//		{
//			LogMessage.EnsureType(data);
//		}

		/// <summary>
		/// Flush this instance.
		/// </summary>
		/// <exception cref="InvalidOperationException">
		/// Is thrown when an operation cannot be performed.
		/// </exception>
		public override void Flush()
		{
			if (Stream == null)
				throw new InvalidOperationException("Stream is null");
			Stream.Flush();
		}

		/// <summary>
		/// Write the specified message.
		/// </summary>
		/// <param name='message'>
		/// Message.
		/// </param>
		/// <exception cref='InvalidOperationException'>
		/// Is thrown when an operation cannot be performed.
		/// </exception>
		/// <remarks>Only implemented because base class marks it as abstract - due to my <see cref="TraceData"/> override this should never execute</remarks>
		public override void Write(string message)
		{
			throw new InvalidOperationException(string.Format("AsyncTraceListener(\"{0}\").Write(message=\"{1}\"): Should not be inside this method, TraceData override should avoid that!?!", Name, message));
		}

		/// <summary>
		/// Write the specified message.
		/// </summary>
		/// <param name='message'>
		/// Message.
		/// </param>
		/// <exception cref='InvalidOperationException'>
		/// Is thrown when an operation cannot be performed.
		/// </exception>
		/// <remarks>Only implemented because base class marks it as abstract - due to my <see cref="TraceData"/> override this should never execute</remarks>
		public override void WriteLine(string message)
		{
			throw new InvalidOperationException(string.Format("AsyncTraceListener(\"{0}\").Write(message=\"{1}\"): Should not be inside this method, TraceData override should avoid that!?!", Name, message));
		}
	}
}