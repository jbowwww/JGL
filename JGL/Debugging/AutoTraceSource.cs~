using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using System.Reflection;
using System.Threading;
using System.Diagnostics;
using JGL.Extensions;

namespace JGL.Debugging
{
	/// <summary>
	/// Designed to simplify use of <see cref="System.Diagnostics.TraceSource"/>
	/// </summary>
	public class AutoTraceSource : TraceSource
	{
		/// <summary>
		/// Tracing <see cref="AutoTraceSource"/>
		/// </summary>
		public static readonly AutoTraceSource Trace = new AutoTraceSource(typeof(AutoTraceSource).FullName, new ConsoleTraceListener(), AsyncFileTraceListener.GetOrCreate("JGL"));
		                                                                   //new AsyncFileTraceListener("JGL"));
			//AutoTraceSource.GetOrCreate("AutoTraceSource", AsyncFileTraceListener.GetOrCreate("JGL"));
			//new AutoTraceSource(typeof(AutoTraceSource).Name,	new ConsoleTraceListener(), AsyncFileTraceListener.GetOrCreate("JGL"));

		/// <summary>
		/// Log message class stores log message parameters
		/// </summary>
		internal class LogMessage
		{
			/// <summary>Event cache</summary>
			public readonly System.Diagnostics.TraceEventCache EventCache;

			/// <summary>The <see cref="TraceSource"/> that the message originated from</summary>
			public readonly TraceSource Source;

			/// <summary><see cref="TraceEventType"/> event type</summary>
			public readonly TraceEventType EventType;

			/// <summary>Trace message ID</summary>
			public readonly int Id;

			/// <summary>Format string (or just a plain message string)</summary>
			public readonly string Format;

			/// <summary>Data to go with the </summary>
			public readonly object Data;

			/// <summary>Listener the <see cref="LogMessage"/> is intended for</summary>
//			public readonly AsyncTraceListener Listener;

			/// <summary><see cref="TraceOptions"/> for output</summary>
			public TraceOptions OutputOptions;

			/// <summary>Gets the formatted message string</summary>
			public string Message {
				get
				{
					StringBuilder sb = new StringBuilder(256);
					if (OutputOptions.HasFlag(TraceOptions.DateTime))
						sb.Append(EventCache.DateTime.ToString("yy-MM-dd HH:mm:ss.ffffff "));
					if (OutputOptions.HasFlag(TraceOptions.Timestamp))
						sb.Append(string.Concat(EventCache.Timestamp.ToString(), " "));
					if (OutputOptions.HasFlag(TraceOptions.ProcessId))
						sb.Append(string.Concat("P:", EventCache.ProcessId.ToString(), " "));
					if (OutputOptions.HasFlag(TraceOptions.ThreadId))
						sb.Append(string.Concat("T:", EventCache.ThreadId, " "));
					sb.Append(string.Concat(Source, " ", Id.ToString("d3"), " ", EventType.ToString(), " "));
					if (Format == null)
					{
						if (Data != null)
							sb.Append(Data.ToString());
					}
					else if (Data == null)
						sb.Append(Format);
					else
						sb.Append(string.Format(Format, (object[])Data));
					if (OutputOptions.HasFlag(TraceOptions.Callstack))
						sb.Append(string.Concat("\nCallstack:\n", EventCache.Callstack));
					if (OutputOptions.HasFlag(TraceOptions.LogicalOperationStack) && EventCache.LogicalOperationStack.Count > 0)
					{
						sb.Append("\nOperation stack:");
						foreach (object stackEntry in EventCache.LogicalOperationStack)
							sb.Append(string.Concat("\n", stackEntry));
					}
					sb.Append("\n");
					return sb.ToString();
				}
			}

			/// <summary>
			/// Initializes a new instance of the <see cref="JGL.Debugging.AsyncTraceListener.LogMessage"/> class.
			/// </summary>
			/// <param name="listener">The listener the message is for</param>
			/// <param name="eventCache">Event cache</param>
			/// <param name="source">Source</param>
			/// <param name="eventType">Event type</param>
			/// <param name="id">Identifier</param>
			/// <param name="data">Data</param>
			public LogMessage(TraceEventCache eventCache, TraceSource source, System.Diagnostics.TraceEventType eventType, int id, object data)
			{
				EventCache = eventCache;
				Source = source;
				EventType = eventType;
				Id = id;
				Format = null;
				Data = data;
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
			public LogMessage(TraceEventCache eventCache, TraceSource source, System.Diagnostics.TraceEventType eventType, int id, string message)
			{
				EventCache = eventCache;
				Source = source;
				EventType = eventType;
				Id = id;
				Format = message;
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
			public LogMessage(TraceEventCache eventCache, TraceSource source, System.Diagnostics.TraceEventType eventType, int id, string format, params object[] data)
			{
				EventCache = eventCache;
				Source = source;
				EventType = eventType;
				Id = id;
				Format = format;
				Data = data;
			}
		}

		/// <summary>
		/// Constant trace thread sleep time.
		/// </summary>
		public const int TraceThreadSleepTime = 111;

		/// <summary>
		/// <see cref="AutoTraceSource"/> instances, keyed by <see cref="AutoTraceSource.Name"/>
		/// </summary>
		private static ConcurrentDictionary<string, AutoTraceSource> _namedSources;

		/// <summary>
		/// The _message queue.
		/// </summary>
		private static ConcurrentQueue<LogMessage> _messageQueue;

		private static List<AsyncTraceListener> openListeners = new List<AsyncTraceListener>();

		public static readonly ConcurrentQueue<AsyncTraceListener> CloseQueue = new ConcurrentQueue<AsyncTraceListener>();

		/// <summary>
		/// The trace thread.
		/// </summary>
		private static Thread TraceThread;

		/// <summary>
		/// Indicates if <see cref="TraceThread"/> is still looping
		/// </summary>
		public static bool TraceThreadRunning { get; private set; }

		/// <summary>
		/// Set this flag to stop <see cref="TraceThread"/>
		/// </summary>
		private static bool _stopThread = false;

		/// <summary>
		/// Thread entry point
		/// </summary>
		/// <remarks>
		/// TODO: Try out a few different approaches to this e.g.
		///		- 31/5 This current approach is the 1-thread running in the background, with a single queue,
		///		handling stream writing for all listeners and their streams
		///			+ just noticed _messageQueue.TryDequeue should be in a while() conditional expression, not an if
		///				(with if thread Sleeps() inbetween writing every message - should loop until queue empty without delay?
		///				maybe small delay? don't want this thread randomly hammering a more important thread (e.g. update/render scenes)
		///				- Fixed 8/6
		/// 		- Would 1 thread per listener/stream work better?
		/// 		- Both above approaches with varying Thread.Sleep() times
		/// 		- Use Async File operations? How to get an async-capable file handle?
		/// 			+ Thread doesn't wait for each Stream.Write(), instead collects IAsyncResults from a BeginWrite for every
		/// 				message found in the queue for this iteration. Also builds list of the listeners that were involved.
		/// 				At end of thread, calls all the Stream.EndRead() methods and then flushes each listener once
		/// 					- This approach may or may not work - depends on semantics of Stream's Async methods
		/// 					  Definitely worth looking into, the Async approach is probably the best of these above alternatives to try first (?)
		/// </remarks>
		private static void RunTrace()
		{
			TraceThreadRunning = true;
			Trace.Log(System.Diagnostics.TraceEventType.Verbose, "RunTrace started");

			// Loop while not flagged to stop background logging or while there are still messages or streams to close queued
			while (!_stopThread || _messageQueue.Count > 0)		// || CloseQueue.Count > 0)
			{
				LogMessage message;

				// Dequeue messages until message queue empty
				while (_messageQueue.TryDequeue(out message))
				{
					TraceSource ts = message.Source;
					lock ((ts.Listeners as ICollection).SyncRoot)
					{
						foreach (TraceListener listener in ts.Listeners)
						{
							message.OutputOptions = listener.TraceOutputOptions;
							if (listener.GetType().IsTypeOf(typeof(AsyncTraceListener)) && !openListeners.Contains((AsyncTraceListener)listener))
								openListeners.Add(listener as AsyncTraceListener);
							listener.TraceData(message.EventCache, ts.Name, message.EventType, message.Id,
								message.Format == null ? message.Data : message.Data == null ? message.Format : string.Format(message.Format, (object[])message.Data));
						}
					}
				}

				// Sleep thread
				if (_messageQueue.Count == 0 && !_stopThread)
					Thread.Sleep(TraceThreadSleepTime);
			}

			TraceThreadRunning = false;

			// Thread exiting, close any open streams
			foreach (AsyncTraceListener listener in openListeners)
				listener.Close();

			TraceThread = null;
		}

		/// <summary>
		/// Stops all <see cref="AsyncTraceListener"/>s by setting a flag that indicates that <see cref="RunThread"/> should return
		/// </summary>
		public static void StopTraceThread()
		{
			Trace.Log(TraceEventType.Information, "RunTrace thread stopping ({0} open listeners will now be closed)", openListeners.Count);
			_stopThread = true;
			if (TraceThread != null)
				TraceThread.Join(new TimeSpan(0, 0, 8));
		}

		/// <summary>
		/// Gets or creates an <see cref="AutoTraceSource"/> with the specified <paramref name="name"/>
		/// </summary>
		/// <returns>An <see cref="AutoTraceSource"/> reference</returns>
		/// <param name="traceListeners"><see cref="TraceListener"/>s that should be in <see cref="AutoTraceSource.Listeners"/></param>
		public static AutoTraceSource GetOrCreate(params TraceListener[] traceListeners)
		{
			StackFrame sf = new StackFrame(1);
			return GetOrCreate(sf.GetMethod().DeclaringType.FullName, true, traceListeners);
		}

		/// <summary>
		/// Gets or creates an <see cref="AutoTraceSource"/> with the specified <paramref name="name"/>
		/// </summary>
		/// <returns>An <see cref="AutoTraceSource"/> reference</returns>
		/// <param name="name">Name for the <see cref="AutoTraceSource"/> to get or create</param>
		/// <param name="traceListeners"><see cref="TraceListener"/>s that should be in <see cref="AutoTraceSource.Listeners"/></param>
		public static AutoTraceSource GetOrCreate(string name, params TraceListener[] traceListeners)
		{
			return GetOrCreate(name,  true, traceListeners);
		}

		/// <summary>
		/// Gets or creates an <see cref="AutoTraceSource"/> with the specified <paramref name="name"/>
		/// </summary>
		/// <returns>An <see cref="AutoTraceSource"/> reference</returns>
		/// <param name="name">Name for the <see cref="AutoTraceSource"/> to get or create</param>
		/// <param name="autoAddConsoleListener">Whether to add a new <see cref="ConsoleTraceListener"/> to the <see cref="AutoTraceSource"/> if creating a new instance</param>
		/// <param name="traceListeners"><see cref="TraceListener"/>s that should be in <see cref="AutoTraceSource.Listeners"/></param>
		public static AutoTraceSource GetOrCreate(string name, bool autoAddConsoleListener, params TraceListener[] traceListeners)
		{
			AutoTraceSource traceSource;
			if (_namedSources.ContainsKey(name))
			{
				traceSource = _namedSources[name];
				foreach (TraceListener traceListener in traceListeners)
					if (!traceSource.Listeners.Contains(traceListener))
						traceSource.Listeners.Add(traceListener);
			}
			else
			{
				traceSource = new AutoTraceSource(name, traceListeners);
			}
			if (autoAddConsoleListener)
			{
				foreach (TraceListener traceListener in traceSource.Listeners)
				{
					if (traceListener.GetType().IsTypeOf(typeof(ConsoleTraceListener)))
					{
						autoAddConsoleListener = false;
						break;
					}
				}
				if (autoAddConsoleListener)
					traceSource.Listeners.Add(new ConsoleTraceListener());
			}
			return traceSource;
		}

		/// <summary>
		/// Current trace id
		/// </summary>
		private int _traceId = 0;

		/// <summary>
		/// Gets the current trace identifier (incremented for each message logged)
		/// </summary>
		public virtual int TraceId {
			get { System.Threading.Interlocked.Increment(ref _traceId);
				return _traceId; }
		}

		/// <summary>
		/// Formats a timestamp for use with trace messages
		/// </summary>
		public virtual string Timestamp {
			get
			{
				DateTime now = DateTime.Now;
				return string.Concat(now.ToShortDateString(), " ", now.ToShortTimeString(), ": ");
			}
		}

		/// <summary>
		/// The <see cref="TraceSource.Switch.Level"/>
		/// </summary>
		public SourceLevels SourceLevels {
			get { return Switch.Level; }
			set { Switch.Level = value; }
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AutoTraceSource"/> class.
		/// </summary>
		/// <param name="name">Name for the new <see cref="AutoTraceSource"/></param>
		/// <param name="traceListeners">Trace listeners to add to the <see cref="AutoTraceSource"/></param>
		protected AutoTraceSource(string name, params TraceListener[] traceListeners)
			: base(name, SourceLevels.All)
		{
			if (_namedSources == null)
				_namedSources = new ConcurrentDictionary<string, AutoTraceSource>();
			_namedSources[base.Name] = this;
			if (_messageQueue == null)
				_messageQueue = new ConcurrentQueue<LogMessage>();
			Switch.Level = SourceLevels.All;
			Listeners.Clear();
			Listeners.AddRange(traceListeners);
			if (TraceThread == null)
			{
				TraceThread = new Thread(RunTrace);
				TraceThread.Name = "AutoTraceSource.RunTrace";
				TraceThread.Start();
			}
		}

		/// <summary>
		/// Log specified <paramref name="data"/> using the specified <see cref="TraceEventType"/>
		/// </summary>
		/// <param name="type">The <see cref="TraceEventType"/> to use</param>
		/// <param name="data">Data to log</param>
		public void Log(TraceEventType type, params object[] data)
		{
			_messageQueue.Enqueue(new LogMessage(new TraceEventCache(), this, type, TraceId, (object)data));
		}
		
		/// <summary>
		/// Log specified <paramref name="message"/> using the specified <see cref="TraceEventType"/>
		/// </summary>
		/// <param name="type">The <see cref="TraceEventType"/> to use</param>
		/// <param name="message">Message to log</param>
		public void Log(TraceEventType type, string message)
		{
			_messageQueue.Enqueue(new LogMessage(new TraceEventCache(), this, type, TraceId, message));
		}
		
		/// <summary>
		/// Log specified <paramref name="data"/> using the specified <see cref="TraceEventType"/>
		/// </summary>
		/// <param name="type">The <see cref="TraceEventType"/> to use</param>
		/// <param name="format">Message/Format string to log</param>
		/// <param name="data">Data to log</param>
		public void Log(TraceEventType type, string format, params object[] data)
		{
			_messageQueue.Enqueue(new LogMessage(new TraceEventCache(), this, type, TraceId, format, data));
		}

		/// <summary>
		/// Log the specified <see cref="Exception"/> using the specified <see cref="TraceEventType"/>
		/// </summary>
		/// <param name="type">The <see cref="TraceEventType"/> to use</param>
		/// <param name="ex">The <see cref="Exception"/> to log</param>
		public void Log(TraceEventType type, Exception ex)
		{
			StringBuilder sb = new StringBuilder();
			string message, indent = string.Empty;
			for (Exception _ex = ex; _ex != null; _ex = _ex.InnerException)
			{
				sb.AppendFormat("{0}{1}: {2}\n{0}Stacktrace:\n{0}    {3}", indent, _ex.GetType().Name, _ex.Message, _ex.StackTrace.Replace("\n", "\n    " + indent));
				if (ex.InnerException != null)
					sb.AppendFormat("{0}InnerException:\n    ", indent);
				indent += "    ";
			}
			message = sb.ToString();
			_messageQueue.Enqueue(new LogMessage(new TraceEventCache(), this, type, TraceId, message));
		}

		/// <summary>
		/// Assert the specified condition, logging result to <see cref="AutoTraceSource.Trace"/>
		/// </summary>
		/// <param name="condition">Condition</param>
		public void Assert(bool condition)
		{
			//Debug.Assert(condition);
			StackFrame sf = new StackFrame(1);
			if (condition)
				Trace.Log(TraceEventType.Verbose, "Assert OK at {0}+{1} (in file {2}:{3},{4}", sf.GetMethod().Name,
					sf.GetILOffset(), sf.GetFileName(), sf.GetFileLineNumber(), sf.GetFileColumnNumber());
			else
				Trace.Log(TraceEventType.Information, "Assert FAILED at {0}+{1} (in file {2}:{3},{4})", sf.GetMethod().Name,
					sf.GetILOffset(), sf.GetFileName(), sf.GetFileLineNumber(), sf.GetFileColumnNumber());
		}
	}
}

