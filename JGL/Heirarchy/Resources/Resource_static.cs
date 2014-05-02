using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Reflection;
using System.Diagnostics;

using JGL.IO;
using JGL.Debugging;
using System.Collections.Generic;

namespace JGL.Heirarchy.Resources
{
	public partial class Resource
	{
		/// <summary>
		/// Constant load thread sleep time.
		/// </summary>
		public const int LoadThreadSleepTime = 132;

		/// <summary>
		/// Dedicated <see cref="Resource"/> loading thread
		/// </summary>
		public static Thread LoadThread = null;

		/// <summary>
		/// The _load queue.
		/// </summary>
		private static readonly ConcurrentQueue<Resource> _loadQueue = new ConcurrentQueue<Resource>();

		/// <summary>
		/// The _stop load thread.
		/// </summary>
		private static bool _stopLoadThread = false;

		/// <summary>
		/// Executed on the dedicated <see cref="Resource"/> loading thread, <see cref="Resource.LoadThread"/>
		/// </summary>
		public static void RunLoadThread()
		{
			Trace.Log(TraceEventType.Information, "Thread started");
			Resource r;
			while (!_stopLoadThread || !_loadQueue.IsEmpty)
			{
				while (_loadQueue.TryDequeue(out r))
				{
					try
					{
						using (Stream sRes = Filesystem.Open(r.Path, FileMode.Open, JGL.Engine.Options.ResourceSearchPaths[r.GetType()]))
							r.Load(sRes);
						r.IsLoaded = true;
					}
					catch (Exception ex)
					{
						Trace.Log(TraceEventType.Error, ex);
					}
				}
				if (!_stopLoadThread && _loadQueue.IsEmpty)
					Thread.Sleep(LoadThreadSleepTime);
			}
			Trace.Log(TraceEventType.Information, "Thread exiting");
		}

		/// <summary>
		/// Starts the load thread, if not already running
		/// </summary>
		public static void StartLoadThread()
		{
			if (LoadThread != null && LoadThread.IsAlive)
				Trace.Log(TraceEventType.Information, "Thread already started");
			else 			//if (LoadThread == null)
			{
				Trace.Log(TraceEventType.Information, "Starting thread");
				LoadThread = new Thread(RunLoadThread);
				LoadThread.Name = "Resource";
				LoadThread.Start();
			}
		}

		/// <summary>
		/// Stops the load thread.
		/// </summary>
		public static void StopLoadThread()
		{
			if (LoadThread == null || !LoadThread.IsAlive || _stopLoadThread)
				Trace.Log(TraceEventType.Information, "Thread not running");
			else 			//if (LoadThread == null)
			{
				Trace.Log(TraceEventType.Information, "Stopping thread");
				_stopLoadThread = true;
				if (LoadThread != null)
					LoadThread.Join(new TimeSpan(0, 0, 8));
			}
		}

		/// <summary>
		/// Get the specified <see cref="Resource"/>
		/// </summary>
		/// <param name="name">The <see cref="Entity.Name"/> of the <see cref="Resource"/> to get</param>
		/// <returns>The <see cref="Resource"/> with name <paramref name="name"/>, if it exists, contained in <see cref="EntityContext.RootContext"/></returns>
//		public static Resource Get(string name)
//		{
//			if (!EntityContext.Root.Contains(name))
//			{
//				Trace.Log(TraceEventType.Error, "Resource \"{0}\" not found", name);
//				throw new KeyNotFoundException(name);
//			}
//			Resource res = (Resource)EntityContext.Root[name];
//			Trace.Log(TraceEventType.Verbose, name, res.GetType().FullName);
//			return res;
//		}

		/// <summary>
		/// Get the specified <see cref="Resource"/> of type <typeparamref name="TResource"/>, if it already exists,
		/// otherwise, creates the <see cref="Resource"/>, adds it to <see cref="EntityContext.Root"/> and then
		/// returns the newly created <see cref="Resource"/>
		/// </summary>
		/// <param name="name">The <see cref="Entity.Name"/> of the <see cref="Resource"/> to get</param>
		/// <typeparam name="TResource">Type of <see cref="Resource"/> to get or create</typeparam>
		/// <returns>The <see cref="Resource"/> with name <paramref name="name"/>, if it exists, contained in <see cref="EntityContext.RootContext"/></returns>
		public static TResource Get<TResource>(string name) where TResource : Resource
		{
			Type TRes = typeof(TResource);
			Resource res;
			if (!EntityContext.Root.Contains(name))
			{
				Trace.Log(TraceEventType.Information, "Constructing new {0} resource \"{1}\"", TRes.FullName, name);
				ConstructorInfo ci = TRes.GetConstructor(new Type[] { typeof(string) });
				if (ci == null)
					throw new TypeInitializationException(TRes.FullName, new MissingMethodException(TRes.FullName, "c'tor(string)"));
				res = (Resource)ci.Invoke(new object[] { name });
				EntityContext.Root.Add(res);
				return (TResource)res;
			}
			res = (Resource)EntityContext.Root[name];
			Trace.Log(TraceEventType.Verbose, "Retrieved existing {0} resource \"{1}\"", TRes.FullName, name);
			return (TResource)res;
		}
	}
}

