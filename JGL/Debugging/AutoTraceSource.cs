using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using System.Reflection;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;
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
		protected static AutoTraceSource Trace;// = AutoTraceSource.GetOrCreate(AsyncXmlFileTraceListener.GetOrCreate("JGL"));

		/// <summary>
		/// Constant trace thread sleep time.
		/// </summary>
		public const int TraceThreadSleepTime = 111;

		#region Static Members
		#region Private Static Members
		/// <summary>
		/// Used for locking when testing/assigning <see cref="AutoTraceSource.Trace"/>
		/// </summary>
		private static object RootTraceLock = new object();

		/// <summary>
		/// <see cref="AutoTraceSource"/> instances, keyed by <see cref="AutoTraceSource.Name"/>
		/// </summary>
		private static ConcurrentDictionary<string, AutoTraceSource> _namedSources;

		/// <summary>
		/// The _message queue.
		/// </summary>
		private static ConcurrentQueue<LogMessage> _messageQueue;

		/// <summary>
		/// The open listeners.
		/// </summary>
		private static readonly List<AsyncTraceListener> _openListeners = new List<AsyncTraceListener>();

		/// <summary>
		/// The trace thread.
		/// </summary>
		private static Thread TraceThread;

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
		private static void RunTraceThread()
		{
			TraceThreadRunning = true;
			Thread.CurrentThread.Name = "Trace";

			Trace.Log(System.Diagnostics.TraceEventType.Verbose, "Started tracing");

			// Loop while not flagged to stop background logging or while there are still messages or streams to close queued
			while (!_stopThread || _messageQueue.Count > 0)		// || CloseQueue.Count > 0)
			{
				LogMessage message;

				// Dequeue messages until message queue empty
				while (_messageQueue.TryDequeue(out message))
				{
					TraceSource ts = message.Source;
					bool explicitOutputOptions = message.OutputOptions != TraceOptions.None;
					lock ((ts.Listeners as ICollection).SyncRoot)
					{
						foreach (TraceListener listener in ts.Listeners)
						{
							if (!explicitOutputOptions)
								message.OutputOptions = listener.TraceOutputOptions;
							if (listener.GetType().IsTypeOf(typeof(AsyncTraceListener)))
							{
								AsyncTraceListener asyncListener = listener as AsyncTraceListener;
								if (!_openListeners.Contains(asyncListener))
								{
									_openListeners.Add(asyncListener);
									asyncListener.EnsureOpen();
								}
								asyncListener.TraceData(message.EventCache, ts.Name, message.EventType, message.Id, message);
								asyncListener.Flush();
							}
							else
								listener.TraceData(message.EventCache, ts.Name, message.EventType, message.Id, message.Message);// message.MessageAsText);
						}
					}
				}

				// Sleep thread
				if (_messageQueue.Count == 0 && !_stopThread)
					Thread.Sleep(TraceThreadSleepTime);
			}

			TraceThreadRunning = false;

			// Thread exiting, close any open streams
			foreach (AsyncTraceListener listener in _openListeners)
				listener.Close();

			TraceThread = null;
		}
		#endregion

		/// <summary>
		/// The default listeners.
		/// </summary>
		public static readonly List<TraceListener> DefaultListeners = new List<TraceListener>(new TraceListener[] { new MyConsoleTraceListener() });

		/// <summary>
		/// Indicates if <see cref="TraceThread"/> is still looping
		/// </summary>
		public static bool TraceThreadRunning { get; private set; }

		/// <summary>
		/// Stops all <see cref="AsyncTraceListener"/>s by setting a flag that indicates that <see cref="RunThread"/> should return
		/// </summary>
		public static void StopTraceThread()
		{
			if (Trace != null)
				Trace.Log(TraceEventType.Verbose, "RunTrace thread stopping ({0} open listeners will now be closed)", _openListeners.Count);
			_stopThread = true;
			if (TraceThread != null)
				TraceThread.Join(new TimeSpan(0, 0, 8));
		}

		/// <summary>
		/// Gets or creates an <see cref="AutoTraceSource"/> with the specified <paramref name="name"/>
		/// </summary>
		/// <returns>An <see cref="AutoTraceSource"/> reference</returns>
		/// <param name="traceListeners"><see cref="TraceListener"/>s that should be in <see cref="AutoTraceSource.Listeners"/></param>
		/// <remarks>
		///	-	TODO:
		///		-	Create another override for this method that takens no params, and gets its default traceListeners
		///			(or info to create them) from ONE single location, config of some sort, be it file or otherwise. User
		///			can modify default trace listeners, names, filenames??
		/// </remarks>
		public static AutoTraceSource GetOrCreate(params TraceListener[] traceListeners)
		{
			Assembly assembly = Assembly.GetCallingAssembly();
			AssemblyTitleAttribute[] assemblyTitleAttr = (AssemblyTitleAttribute[])assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), true);
			return GetOrCreate(assemblyTitleAttr == null || assemblyTitleAttr.Length == 0 ? assembly.GetName().Name : assemblyTitleAttr[0].Title, true, traceListeners);
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
		public static AutoTraceSource GetOrCreate(string name, bool autoAddDefaultListeners, params TraceListener[] traceListeners)
		{
			// value to be returned
			AutoTraceSource traceSource;

			// Init first trace (has to be for this class) manually
			lock (RootTraceLock)
			{
				// init static private variables
				if (_namedSources == null)
					_namedSources = new ConcurrentDictionary<string, AutoTraceSource>();
				if (_messageQueue == null)
					_messageQueue = new ConcurrentQueue<LogMessage>();
				if (Trace == null)				// TODO: Config class, or file, or something, for trace listener configs & deafults
					Trace = new AutoTraceSource(Assembly.GetAssembly(typeof(AutoTraceSource)).GetName().Name, DefaultListeners.ToArray());
				if (TraceThread == null)
				{
					TraceThread = new Thread(RunTraceThread);
					TraceThread.Start();
				}
			}

			// Find existing trace source or create a new one
			if (_namedSources.ContainsKey(name))
			{
				traceSource = _namedSources[name];
				foreach (TraceListener traceListener in traceListeners)
					if (!traceSource.Listeners.Contains(traceListener))
						traceSource.Listeners.Add(traceListener);
			}
			else
				traceSource = new AutoTraceSource(name, traceListeners);

			// if auto add console listener, ensure one exists in traceListeners, or add a new one
			if (autoAddDefaultListeners)
				foreach (TraceListener traceListener in DefaultListeners)
					if (!traceSource.Listeners.Contains(traceListener))
						traceSource.Listeners.Add(traceListener);

			return traceSource;
		}
		#endregion
		
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
		/// Initializes a new instance of the <see cref="JGL.Debugging.AutoTraceSource"/> class, using the executing module's name
		/// </summary>
		/// <param name="traceListeners">Trace listeners</param>
//		internal AutoTraceSource(params TraceListener[] traceListeners)
//			: base(new StackFrame(1).GetMethod().Module.Name, SourceLevels.All)
//		{
//			Init(Name, traceListeners);
//		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AutoTraceSource"/> class.
		/// </summary>
		/// <param name="name">Name for the new <see cref="AutoTraceSource"/></param>
		/// <param name="traceListeners">Trace listeners to add to the <see cref="AutoTraceSource"/></param>
		internal AutoTraceSource(string name, params TraceListener[] traceListeners)
			: base(name, SourceLevels.All)
		{
			Init(name, traceListeners);
		}

		/// <summary>
		/// Init the specified name and traceListeners.
		/// </summary>
		/// <param name="name">Source name</param>
		/// <param name="traceListeners">Trace listeners</param>
		internal void Init(string name, TraceListener[] traceListeners)
		{
			_namedSources[name] = this;
			Switch.Level = SourceLevels.All;
			Listeners.Clear();
			Listeners.AddRange(traceListeners);
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
				sb.AppendFormat("{0}{1}: {2}\n", indent, _ex.GetType().Name, _ex.Message);
				if (ex.Data.Count > 0)
				{
					sb.AppendFormat("{0}Data:\n");
					foreach (DictionaryEntry exData in ex.Data)
						sb.AppendFormat("{0}    {1}={2}\n", indent, exData.Key.ToString(), exData.Value.ToString());
				}
				sb.AppendFormat("{0}Stacktrace:\n{0}    {1}\n", indent, _ex.StackTrace.Replace("\n", "\n    " + indent));
				if (_ex.InnerException != null)
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
//			if (condition)
//				Trace.Log(TraceEventType.Verbose, "Assert OK at {0}+{1} (in file {2}:{3},{4}", sf.GetMethod().Name,
//					sf.GetILOffset(), sf.GetFileName(), sf.GetFileLineNumber(), sf.GetFileColumnNumber());
//			else
			if (!condition)
				Trace.Log(TraceEventType.Warning, "Assert FAILED at {0}+{1} (in file {2}:{3},{4})", sf.GetMethod().Name,
					sf.GetILOffset(), sf.GetFileName(), sf.GetFileLineNumber(), sf.GetFileColumnNumber());
		}
	}
}

