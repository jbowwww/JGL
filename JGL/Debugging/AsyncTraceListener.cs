using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using System.IO;
using System.Threading;
using System.Diagnostics;
using System.Reflection;

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
		public static readonly AutoTraceSource Trace = AutoTraceSource.GetOrCreate("AsyncTraceListener", AsyncFileTraceListener.GetOrCreate("JGL"));
			//new AutoTraceSource(typeof(AsyncTraceListener).Name, new ConsoleTraceListener(),AsyncTraceListener.GetOrCreate("JGL", /*typeof(AsyncTraceListener).Name,*/ typeof(AsyncFileTraceListener)));

		/// <summary>
		/// Log message class stores log message parameters
		/// </summary>
		internal class LogMessage
		{
			/// <summary>Event cache</summary>
			public readonly System.Diagnostics.TraceEventCache EventCache;

			/// <summary>The <see cref="TraceSource"/> that the message originated from</summary>
			public readonly string Source;

			/// <summary><see cref="TraceEventType"/> event type</summary>
			public readonly TraceEventType EventType;

			/// <summary>Trace message ID</summary>
			public readonly int Id;

			/// <summary>Data to go with the </summary>
			public readonly object Data;

			/// <summary>Listener the <see cref="LogMessage"/> is intended for</summary>
			public readonly AsyncTraceListener Listener;

			/// <summary><see cref="TraceOptions"/> for output</summary>
			public readonly TraceOptions OutputOptions;

			/// <summary>Gets the formatted message string</summary>
			public string Message {
				get
				{
					StringBuilder sb = new StringBuilder(string.Concat(Id.ToString("d3"), " "), 256);
					if (OutputOptions.HasFlag(System.Diagnostics.TraceOptions.DateTime))
						sb.Append(EventCache.DateTime.ToString("yy-MM-dd HH:mm:ss.ffffff "));
					if (OutputOptions.HasFlag(System.Diagnostics.TraceOptions.Timestamp))
						sb.Append(string.Concat(EventCache.Timestamp.ToString(), " "));
					if (OutputOptions.HasFlag(System.Diagnostics.TraceOptions.ProcessId))
						sb.Append(string.Concat("P:", EventCache.ProcessId.ToString(), " "));
					if (OutputOptions.HasFlag(System.Diagnostics.TraceOptions.ThreadId))
						sb.Append(string.Concat("T:", EventCache.ThreadId, " "));
					sb.Append(string.Concat(Source, " ", EventType.ToString(), " ", Data));
					if (OutputOptions.HasFlag(System.Diagnostics.TraceOptions.Callstack))
						sb.Append(string.Concat("\nCallstack:\n", EventCache.Callstack));
					if (OutputOptions.HasFlag(System.Diagnostics.TraceOptions.LogicalOperationStack) && EventCache.LogicalOperationStack.Count > 0)
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
			public LogMessage(AsyncTraceListener listener, System.Diagnostics.TraceEventCache eventCache, string source, System.Diagnostics.TraceEventType eventType, int id, object data)
			{
				EventCache = eventCache;
				Source = source;
				EventType = eventType;
				Id = id;
				Data = data;
				Listener = listener;
				OutputOptions = listener.TraceOutputOptions;
			}
		}

		#region Static members
		#region Private fields
		private static bool _threadLooping = true;
		private static Thread _fileIoThread;
		private static bool _stopAll = false;
		private static ConcurrentQueue<LogMessage> _messageQueue = new ConcurrentQueue<LogMessage>();
		private static ConcurrentQueue<AsyncTraceListener> _closeQueue = new ConcurrentQueue<AsyncTraceListener>();
		private static ConcurrentDictionary<string, AsyncTraceListener> _namedListeners;
		#endregion

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
			//Thread.BeginCriticalRegion();
			if (_namedListeners == null)
				_namedListeners = new ConcurrentDictionary<string, AsyncTraceListener>();
			//Thread.EndCriticalRegion();
			if (!_namedListeners.ContainsKey(name))
				_namedListeners[name] = (AsyncTraceListener)type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null,
					new Type[] { typeof(string) }, new ParameterModifier[] {}).Invoke(new object[] { name });
			return _namedListeners[name];
		}

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
		private static void RunThread()
		{
			// Lists of listeners that are open, that need flushing
			List<AsyncTraceListener> openListeners = new List<AsyncTraceListener>();
			List<AsyncTraceListener> flushListeners = new List<AsyncTraceListener>();

			try
			{
//				Trace.Log(System.Diagnostics.TraceEventType.Verbose, "RunThread() started");

				// Loop while not flagged to stop background logging or while there are still messages or streams to close queued
				while (!_stopAll || _closeQueue.Count > 0 || _messageQueue.Count > 0)
				{
					LogMessage message;
					AsyncTraceListener listener;
	
					// Dequeue messages until message queue empty
					while (_messageQueue.TryDequeue(out message))
					{
						listener = message.Listener;
						if (listener.Stream == null)																					// If listener does not have an open stream, open one and add to our list of listeners with open streams
						{
							listener.Stream = listener.OpenStream();			//	File.Open(listener.Path, FileMode.Create, FileAccess.Write, FileShare.Read);
							openListeners.Add(listener);
//							Trace.Log(TraceEventType.Verbose, "OpenStream(this=AsyncTraceListener[Name=\"{0}\"]) = {2}", listener.Name, listener.Stream.ToString());
						}

						byte[] buf = listener.FormatMessage(message);			//Encoding.ASCII.GetBytes(message.Message);					// Get and encode message
						listener.Stream.Write(buf, 0, buf.Length);													// Write message buffer
						listener.Stream.Flush();
//						if (!flushListeners.Contains(listener))															// If listener not already queued for flushing, queue for flushing
//							flushListeners.Add(listener);
					}

					// Flush any listeners queued to be flushed (had one or more messages written to them this iteration)
//					foreach (AsyncTraceListener flushListener in flushListeners)
//						flushListener.Flush();
//					flushListeners.Clear();

					// Close any listeners queued for closing
					while (_closeQueue.TryDequeue(out listener))
					{
						Debug.Assert(listener.Stream != null);
						listener.Stream.Close();
//						listener.Close();
						listener.Stream = null;
						openListeners.Remove(listener);
					}

					// Yield thread
					//Thread.Yield();

					// Sleep thread
					if (_messageQueue.Count == 0 && !_stopAll)
						Thread.Sleep(ThreadWaitTime);
//					else
//						Thread.Yield();
				}
				_threadLooping = false;
			}
			catch (Exception ex)
			{
				;
			}
			finally
			{

				// Thread exiting, close any open streams
				foreach (AsyncTraceListener listener in openListeners)
				{
					listener.Flush();
					if (listener.Stream != null)
					{
						listener.Stream.Close();
						listener.Stream = null;
					}
				}
				_fileIoThread = null;
			}
		}

		/// <summary>
		/// Stops all <see cref="AsyncTraceListener"/>s by setting a flag that indicates that <see cref="RunThread"/> should return
		/// </summary>
		public static void StopAll()
		{
			Trace.Log(System.Diagnostics.TraceEventType.Information, "StopAll() ({0} named listeners)", _namedListeners.Count);
			_stopAll = true;
		}
		#endregion

		#region Private members
		/// <summary>
		/// Time (in milliseconds) for <see cref="AsyncTraceListener.RunThread"/> to sleep for,
		/// after writing all <see cref="LogMessage"/>s in queue
		/// </summary>
		private const int ThreadWaitTime = 141;

		/// <summary>
		/// Indicates that this <see cref="AsyncTraceListener"/> should be stopped
		/// </summary>
		private bool _stop = false;

		/// <summary>
		/// The stream that this <see cref="AsyncTraceListener"/> writes to
		/// </summary>
		private Stream Stream = null;
		#endregion
		
		/// <summary>
		/// Initializes a new instance of the <see cref="JGL.Debugging.AsyncTraceListener"/> class.
		/// </summary>
		/// <param name="name">Name</param>
		protected AsyncTraceListener(string name) : base(name)
		{
			TraceOutputOptions = System.Diagnostics.TraceOptions.DateTime | System.Diagnostics.TraceOptions.ProcessId | System.Diagnostics.TraceOptions.ThreadId;
			Thread.BeginCriticalRegion();
//			if (Thread.VolatileRead(ref (System.Object)AsyncTraceListener._fileIoThread) == null)
			if (_fileIoThread == null)
			{
//				Thread.VolatileWrite(ref _fileIoThread, (Thread)1);
//				Thread.VolatileWrite(ref fThread, new Thread(RunThread) { Priority = ThreadPriority.BelowNormal });
				_fileIoThread = new Thread(RunThread) { Priority = ThreadPriority.BelowNormal };
				_fileIoThread.Start();
			}
			Thread.EndCriticalRegion();
		}

		/// <summary>
		/// Format message delegate method. Formats a <see cref="LogMessage"/> instance into <see cref="byte[]"/> data to be
		/// written to the output <see cref="Stream"/>. Override to customise behaviour.
		/// </summary>
		internal virtual byte[] FormatMessage(LogMessage message)
		{
			return Encoding.ASCII.GetBytes(message.Message);
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
				if (_threadLooping)
					_closeQueue.Enqueue(this);
				else if (Stream != null)
				{
					Stream.Close();
					Stream = null;
				}
			}
		}

		/// <summary>
		/// Called by <see cref="System.Diagnostics.TraceSource"/> methods to specify a message to log
		/// </summary>
		/// <param name="eventCache">Event cache</param>
		/// <param name="source">Source</param>
		/// <param name="eventType">Event type</param>
		/// <param name="id">Identifier</param>
		/// <param name="data">Data</param>
		public override void TraceData(System.Diagnostics.TraceEventCache eventCache, string source, System.Diagnostics.TraceEventType eventType, int id, object data)
		{
			if (this.Filter == null || this.Filter.ShouldTrace(eventCache, source, eventType, id, null, null, data, null))
				_messageQueue.Enqueue(new LogMessage(this, eventCache, source, eventType, id, data));
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

		/// <summary>
		/// The open stream method opens the output stream when required by the background thread. Override to customise behaviour.
		/// </summary>
		protected abstract Stream OpenStream();
	}
}