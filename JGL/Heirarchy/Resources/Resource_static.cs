using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Reflection;
using System.Diagnostics;

using JGL.IO;
using JGL.Debugging;

namespace JGL.Heirarchy.Resources
{
	public partial class Resource
	{
		/// <summary>
		/// The sync root.
		/// </summary>
		private static object SyncRoot = new object();

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
		public static void LoadResources()
		{
			Trace.Log(TraceEventType.Information, "Started");
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
			Trace.Log(TraceEventType.Information, "LoadResources thread stopped");
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
				LoadThread = new Thread(LoadResources);
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
		public static Resource Get(string name)
		{
			if (!EntityContext.Root.Contains(name))
			{
				string error = string.Format("Resource \"{0}\" not found", name);
				Trace.Log(TraceEventType.Error, error);
				throw new ArgumentException(error);
			}
			Resource res = (Resource)EntityContext.Root[name];
			Trace.Log(TraceEventType.Verbose, "Retrieve resource \"{0}\" ({1})", name, res.GetType().FullName);
			return res as Resource;
		}

		/// <summary>
		/// Get the specified <see cref="Resource"/> of type <typeparamref name="TResource"/>, if it already exists,
		/// otherwise, creates the <see cref="Resource"/>, adds it to <see cref="EntityContext.Root"/> and then
		/// returns the newly created <see cref="Resource"/>
		/// </summary>
		/// <param name="name">The <see cref="Entity.Name"/> of the <see cref="Resource"/> to get</param>
		/// <typeparam name="TResource">Type of <see cref="Resource"/> to get or create</typeparam>
		/// <returns>The <see cref="Resource"/> with name <paramref name="name"/>, if it exists, contained in <see cref="EntityContext.RootContext"/></returns>
		public static TResource Get<TResource>(string name)
			where TResource : Resource
		{
			Type TRes = typeof(TResource);
			Resource res;
			if (!EntityContext.Root.Contains(name))
			{
				string msg = string.Format("Resource \"{0}\" not found, constructing ({1})", name, TRes.FullName);
				Trace.Log(TraceEventType.Information, msg);
				ConstructorInfo ci = TRes.GetConstructor(new Type[] { typeof(string) });
				if (ci == null)
				{
					string error = string.Format("Could not get constructor for resource type {0}", TRes.FullName);
					Trace.Log(TraceEventType.Error, error);
					throw new TypeInitializationException(TRes.FullName, new MissingMethodException(TRes.FullName, "c'tor(string)"));
				}
				res = ci.Invoke(new object[] { name }) as TResource;
				EntityContext.Root.Add(res);
				Trace.Log(TraceEventType.Verbose, "Constructed resource \"{0}\" ({1})", name, TRes.FullName);
				return (TResource)res;
			}
			res = (Resource)EntityContext.Root[name];
			Trace.Log(TraceEventType.Verbose, "Retrieve resource \"{0}\" ({1})", name, TRes.FullName);
			return (TResource)res;
		}
	}
}

