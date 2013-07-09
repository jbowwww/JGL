using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using System.Reflection;
using System.Threading;
using System.Diagnostics;

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
		public static readonly AutoTraceSource Trace = AutoTraceSource.GetOrCreate("AutoTraceSource", AsyncFileTraceListener.GetOrCreate("JGL"));
			//new AutoTraceSource(typeof(AutoTraceSource).Name,	new ConsoleTraceListener(), AsyncFileTraceListener.GetOrCreate("JGL"));

		/// <summary>
		/// <see cref="AutoTraceSource"/> instances, keyed by <see cref="AutoTraceSource.Name"/>
		/// </summary>
		private static ConcurrentDictionary<string, AutoTraceSource> _namedSources;

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
			return GetOrCreate(name, true, traceListeners);
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
			if (_namedSources == null)
				_namedSources = new ConcurrentDictionary<string, AutoTraceSource>();
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
				TraceListener[] listeners;
				if (autoAddConsoleListener)
				{
					listeners = new TraceListener[traceListeners.Length + 1];
					listeners[0] = new ConsoleTraceListener();
					Array.Copy(traceListeners, 0, listeners, 1, traceListeners.Length);
				}
				else
					listeners = traceListeners;
				traceSource = _namedSources[name] = new AutoTraceSource(name, listeners);
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
		/// <param name="traceListeners">Trace listeners to add to the <see cref="AutoTraceSource"/></param>
		public AutoTraceSource(params TraceListener[] traceListeners)
			: base(string.Concat("Tracer:", ((Int16)DateTime.Now.Ticks).ToString("x")))
		{
			Switch.Level = SourceLevels.All;
			Listeners.Clear();
			Listeners.AddRange(traceListeners);
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="AutoTraceSource"/> class.
		/// </summary>
		/// <param name="name">Name for the new <see cref="AutoTraceSource"/></param>
		/// <param name="traceListeners">Trace listeners to add to the <see cref="AutoTraceSource"/></param>
		public AutoTraceSource(string name, params TraceListener[] traceListeners)
			: base(name, SourceLevels.All)
		{
			Switch.Level = SourceLevels.All;
			Listeners.Clear();
			Listeners.AddRange(traceListeners);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="AutoTraceSource"/> class.
		/// </summary>
		/// <param name="type"><see cref="Type"/> that this <see cref="AutoTraceSource"/> is intended to trace for</param>
		/// <param name="traceListeners">Trace listeners to add to the <see cref="AutoTraceSource"/></param>
		public AutoTraceSource(Type type, params TraceListener[] traceListeners)
			: base(type.Name, SourceLevels.All)
		{
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
//			TraceData(type, TraceId, data);
			lock ((Listeners as ICollection).SyncRoot)
			{
				int id = TraceId;
				foreach (TraceListener listener in this.Listeners)
					listener.TraceData(new TraceEventCache(), Name, type, id, data);
			}
		}
		
		/// <summary>
		/// Log specified <paramref name="message"/> using the specified <see cref="TraceEventType"/>
		/// </summary>
		/// <param name="type">The <see cref="TraceEventType"/> to use</param>
		/// <param name="message">Message to log</param>
		public void Log(TraceEventType type, string message)
		{
//			TraceEvent(type, TraceId, message);
			lock ((Listeners as ICollection).SyncRoot)
			{
				int id = TraceId;
				foreach (TraceListener listener in this.Listeners)
					listener.TraceEvent(new TraceEventCache(), Name, type, id, message);
			}
		}
		
		/// <summary>
		/// Log specified <paramref name="data"/> using the specified <see cref="TraceEventType"/>
		/// </summary>
		/// <param name="type">The <see cref="TraceEventType"/> to use</param>
		/// <param name="format">Message/Format string to log</param>
		/// <param name="data">Data to log</param>
		public void Log(TraceEventType type, string format, params object[] data)
		{
//			TraceEvent(type, TraceId, format, data);
			lock ((Listeners as ICollection).SyncRoot)
			{
				int id = TraceId;
				foreach (TraceListener listener in this.Listeners)
					listener.TraceEvent(new TraceEventCache(), Name, type, id, format, data);
			}
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
			lock ((Listeners as ICollection).SyncRoot)
			{
				int id = TraceId;
				foreach (TraceListener listener in this.Listeners)
					listener.TraceEvent(new TraceEventCache(), Name, type, id, message);
			}
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

