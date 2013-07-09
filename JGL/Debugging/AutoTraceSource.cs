using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Reflection;
using System.Diagnostics;
using System.Threading;

namespace JGL.Debugging
{
	/// <summary>
	/// Designed to simplify use of <see cref="System.Diagnostics.TraceSource"/>
	/// </summary>
	public class AutoTraceSource : TraceSource
	{
		/// <summary>
		/// <see cref="AutoTraceSource"/> for trace messages
		/// </summary>
		public static readonly AutoTraceSource Trace = new AutoTraceSource(typeof(AutoTraceSource).Name,
			new ConsoleTraceListener(), AsyncFileTraceListener.GetOrCreate("JGL"));

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
				Trace.Log(TraceEventType.Information, "Assert FAILED at {0}+{1} (in file {2}:{3},{4}", sf.GetMethod().Name,
					sf.GetILOffset(), sf.GetFileName(), sf.GetFileLineNumber(), sf.GetFileColumnNumber());
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
		/// Log specified <paramref name="data"/> in a <see cref="LogMessage"/> using the specified <see cref="TraceEventType"/>
		/// </summary>
		/// <param name="type">The <see cref="TraceEventType"/> for this <see cref="LogMessage"/></param>
		/// <param name="data">Data to log in <see cref="LogMessage"/></param>
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
		/// Log specified <paramref name="data"/> in a <see cref="LogMessage"/> using the specified <see cref="TraceEventType"/>
		/// </summary>
		/// <param name="type">The <see cref="TraceEventType"/> for this <see cref="LogMessage"/></param>
		/// <param name="message">Message to log in <see cref="LogMessage"/></param>
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
		/// Log specified <paramref name="data"/> in a <see cref="LogMessage"/> using the specified <see cref="TraceEventType"/>
		/// </summary>
		/// <param name="type">The <see cref="TraceEventType"/> for this <see cref="LogMessage"/></param>
		/// <param name="format">Message/Format string to log in <see cref="LogMessage"/></param>
		/// <param name="data">Data to log in <see cref="LogMessage"/></param>
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
	}
}

