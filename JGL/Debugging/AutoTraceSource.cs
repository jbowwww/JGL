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
		public static readonly AutoTraceSource Trace = new AutoTraceSource(typeof(AutoTraceSource).Name,
			new ConsoleTraceListener(), new 
		/// <summary>
		/// Assert the specified condition.
		/// </summary>
		/// <param name="condition">Condition</param>
		public void Assert(bool condition)
		{
			Debug.Assert(condition);
		}
		
		private static int _traceId = 0;
		
		public static int TraceId { get { System.Threading.Interlocked.Increment(ref _traceId); return _traceId; } }

		public static string Timestamp {
			get {
				DateTime now = DateTime.Now;
				return string.Concat(now.ToShortDateString(), " ", now.ToShortTimeString(), ": ");
			}
		}

		public AutoTraceSource(params TraceListener[] traceListeners)
			: base(string.Concat("Tracer:", ((Int16)DateTime.Now.Ticks).ToString("x")))
		{
			Switch.Level = SourceLevels.All;
			Listeners.Clear();
			Listeners.AddRange(traceListeners);
		}
		
		public AutoTraceSource(string name, params TraceListener[] traceListeners)
			: base(name, SourceLevels.All)
		{
			Switch.Level = SourceLevels.All;
			Listeners.Clear();
			Listeners.AddRange(traceListeners);
		}
		
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

