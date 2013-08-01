using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Threading;
using System.Diagnostics;

namespace JGL.Debug
{
	/// <summary>
	/// Trace implementation, static class
	/// </summary>
	public static class Trace
	{
		/// <summary>
		/// Constant trace thread sleep time.
		/// </summary>
		public const int TraceThreadSleepTime = 111;

		/// <summary>
		/// Trace levels, identical to <see cref="TraceEventType"/>
		/// </summary>
		public enum Level
		{
			Critical = TraceEventType.Critical,
			Error = TraceEventType.Error,
			Warning = TraceEventType.Warning,
			Information = TraceEventType.Information,
			Verbose = TraceEventType.Verbose,
			Start = TraceEventType.Start,
			Stop = TraceEventType.Stop,
			Suspend = TraceEventType.Suspend,
			Resume = TraceEventType.Resume,
			Transfer = TraceEventType.Transfer
		}

		/// <summary>
		/// Used for locking when testing/assigning <see cref="AutoTraceSource.Trace"/>
		/// </summary>
		private static readonly object SyncRoot = new object();

		/// <summary>
		/// The event cache.
		/// </summary>
//		public static readonly TraceEventCache EventCache = new TraceEventCache();

		/// <summary>
		/// The _message queue.
		/// </summary>
		private static readonly ConcurrentQueue<LogMessage> _messageQueue = new ConcurrentQueue<LogMessage>();

		/// <summary>
		/// The open listeners.
		/// </summary>
		private static readonly List<AsyncTraceListener> openListeners = new List<AsyncTraceListener>();

		public static readonly ConcurrentBag<TraceListener> Listeners = new ConcurrentBag<TraceListener>();

		/// <summary>
		/// The trace thread.
		/// </summary>
		private static Thread TraceThread;

		/// <summary>
		/// Set this flag to stop <see cref="TraceThread"/>
		/// </summary>
		private static bool _stopThread = false;

		/// <summary>
		/// Indicates if <see cref="TraceThread"/> is still looping
		/// </summary>
		public static bool TraceThreadRunning { get; private set; }

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
			Thread.CurrentThread.Name = "AutoTraceSource.RunTrace";

			Trace.Log(Trace.Level.Verbose, "RunTrace started");

			// Loop while not flagged to stop background logging or while there are still messages or streams to close queued
			while (!_stopThread || _messageQueue.Count > 0)		// || CloseQueue.Count > 0)
			{
				LogMessage message;

				// Dequeue messages until message queue empty
				while (_messageQueue.TryDequeue(out message))
				{
//					TraceSource ts = message.Source;
//					lock ((ts.Listeners as ICollection).SyncRoot)
//					{
						foreach (TraceListener listener in Listeners)
						{
//							message.OutputOptions = listener.TraceOutputOptions;
							if (listener.GetType().IsTypeOf(typeof(AsyncTraceListener)))
							{
								AsyncTraceListener asyncListener = listener as AsyncTraceListener;
								if (!openListeners.Contains(asyncListener))
								{
									openListeners.Add(asyncListener);
									asyncListener.EnsureOpen();
								}
//								asyncListener.TraceData(message.EventCache, ts.Name, message.EventType, message.Id, message);
								asyncListener.Flush();
							}
							else
								listener.TraceData(message.EventCache, message.Frame.GetMethod().DeclaringType.FullName, message.Level, 0, message.Message);// message.MessageAsText);
						}
//					}
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
		/// Ensures the trace thread.
		/// </summary>
		public static void EnsureTraceThread()
		{
			if (!TraceThreadRunning)
			{
				TraceThread = new Thread(RunTrace);
				_stopThread = false;
				TraceThread.Start();
			}
		}

		/// <summary>
		/// Stops all <see cref="AsyncTraceListener"/>s by setting a flag that indicates that <see cref="RunThread"/> should return
		/// </summary>
		public static void StopTraceThread()
		{
			Trace.Log(Trace.Level.Information, "RunTrace thread stopping ({0} open listeners will now be closed)", openListeners.Count);
			_stopThread = true;
			if (TraceThread != null)
				TraceThread.Join(new TimeSpan(0, 0, 8));
			TraceThread = null;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="JGL.Debugging.Trace"/> class.
		/// </summary>
//		static Trace()
//		{
//			lock (SyncRoot)
//			{
//				TraceThreadRunning = false;
//				TraceThread = new Thread(RunTrace);
//				TraceThread.Start();
//			}
//		}

		/// <summary>
		/// Log the specified level, format and args.
		/// </summary>
		/// <param name="level">Level</param>
		/// <param name="message">Message</param>
		public static void Log(Level level, string message)
		{
//			lock (SyncRoot)
//			{
			EnsureTraceThread();
			_messageQueue.Enqueue(new LogMessage(level, message));
//			}
		}

		/// <summary>
		/// Log the specified level, format and args.
		/// </summary>
		/// <param name="level">Level</param>
		/// <param name="format">Format</param>
		/// <param name="args">Arguments</param>
		public static void Log(Level level, string format, params object[] args)
		{
//			lock (SyncRoot)
//			{
			EnsureTraceThread();
			_messageQueue.Enqueue(new LogMessage(level, format, args));
//			}
		}

		/// <summary>
		/// Log the specified level and exception.
		/// </summary>
		/// <param name="level">Level</param>
		/// <param name="exception">Exception to log</param>
		public static void Log(Level level, Exception exception)
		{
			EnsureTraceThread();
			_messageQueue.Enqueue(new LogMessage(level, exception));
		}
	}
}

