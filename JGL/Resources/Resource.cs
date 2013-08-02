using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
using System.Diagnostics;
using JGL.Heirarchy;
using JGL.Debugging;

namespace JGL.Resource
{
	/// <summary>
	/// Abstract resource base class
	/// </summary>
	public abstract class Resource : Entity
	{
		/// <summary>
		/// Tracing
		/// </summary>
		public static readonly AutoTraceSource Trace = AutoTraceSource.GetOrCreate(AsyncXmlFileTraceListener.GetOrCreate("JGL"));

		/// <summary>
		/// Constant load thread sleep time.
		/// </summary>
		public const int LoadThreadSleepTime = 132;

		#region Static members
		/// <summary>
		/// The sync root.
		/// </summary>
		private static object SyncRoot = new object();

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
			Trace.Log(TraceEventType.Information, "LoadResources thread started");
			Resource r;
			while (!_stopLoadThread || !_loadQueue.IsEmpty)
			{
				while (_loadQueue.TryDequeue(out r))
				{
					try
					{
						r.Load();
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
			lock (SyncRoot)
			{
				if (LoadThread == null)
				{
					LoadThread = new Thread(LoadResources);
					LoadThread.Name = "Resource.LoadResources";
					LoadThread.Start();
				}
			}
		}

		/// <summary>
		/// Stops the load thread.
		/// </summary>
		public static void StopLoadThread()
		{
			_stopLoadThread = true;
			if (LoadThread != null)
				LoadThread.Join(new TimeSpan(0, 0, 8));
		}

		/// <summary>
		/// Get the specified <see cref="Resource"/>
		/// </summary>
		/// <param name="name">The <see cref="Entity.Name"/> of the <see cref="Resource"/> to get</param>
		/// <returns>The <see cref="Resource"/> with name <paramref name="name"/>, if it exists, contained in <see cref="EntityContext.RootContext"/></returns>
		public static Resource Get(string name)
		{
			if (!EntityContext.RootContext.Contains(name))
				throw new ArgumentException(string.Format("Resource \"{0}\" not found", name), "name");
			return EntityContext.RootContext[name] as Resource;
		}
		#endregion
		
		/// <summary>
		/// Gets a value indicating whether this resource is loaded.
		/// </summary>
		public bool IsLoaded { get; private set; }

		/// <summary>
		/// Path to the resource file
		/// </summary>
		public string Path { get; protected set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="JGL.Resource.Resource"/> class with an autogenerated <see cref="Heirarchy.Entity.Name"/>
		/// </summary>
		/// <param name="uri"><see cref="System.Uri"/> of the stored resource</param>
		public Resource(string path)
			: base(null)
		{
			Init(base.Name, path);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="JGL.Resource.Resource"/> class.
		/// </summary>
		/// <param name="name">Resource name (no requirement to be unique)</param>
		/// <param name="path">Path to the stored resource</param>
		public Resource(string name, string path)
			: base(name)
		{
			Init(name, path);
		}

		/// <summary>
		/// Init the instance using given name and uri.
		/// </summary>
		/// <param name="name"><see cref="Heirarchy.Entity.Name"/></param>
		/// <param name="path">Resource Path</param>
		private void Init(string name, string path)
		{
			Trace.Log(TraceEventType.Information, "Init(name=\"{0}\", path=\"{1}\")", name, path);

			Name = name;
			IsLoaded = false;
			Path = path;

			StartLoadThread();

			EntityContext.RootContext.Add(this);				// Add all resources as child entities of EntityContext.RootContext
			_loadQueue.Enqueue(this);
		}

		/// <summary>
		/// Generates the <see cref="Entity.Name"/> base for auto naming
		/// </summary>
		public override string GenerateBaseName()
		{
			return System.IO.Path.GetFileNameWithoutExtension(Path);
		}

		/// <summary>
		/// Load the <see cref="Resource"/>
		/// </summary>
		/// <returns><c>true</c>, if loaded successfully, otherwise, <c>false</c></returns>
		public abstract void Load();
	}
}
