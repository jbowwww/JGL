using System;
using System.Collections.Concurrent;
using System.IO;
using System.Threading;
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
		public static readonly AutoTraceSource Trace = new AutoTraceSource(AsyncFileTraceListener.GetOrCreate("JGL"));

		/// <summary>
		/// Constant load thread sleep time.
		/// </summary>
		public const int LoadThreadSleepTime = 144;

		/// <summary>
		/// Dedicated <see cref="Resource"/> loading thread
		/// </summary>
		public static Thread LoadThread = null;

		private static readonly ConcurrentQueue<Resource> _loadQueue = new ConcurrentQueue<Resource>();

		private object SyncRoot = new object();

		/// <summary>
		/// Executed on the dedicated <see cref="Resource"/> loading thread, <see cref="Resource.LoadThread"/>
		/// </summary>
		public static void LoadResources()
		{
			Resource r;
			while (true)
			{
				while (_loadQueue.TryDequeue(out r))
				{
					try
					{
						r.Load();
					}
					catch (Exception ex)
					{
						// TODO: Trace log ex
					}
				}
				Thread.Sleep(LoadThreadSleepTime);
			}
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

		/// <summary>
		/// Gets a value indicating whether this resource is loaded.
		/// </summary>
		public bool IsLoaded { get; private set; }

		/// <summary>
		/// The <see cref="System.Uri"/> of the stored resource
		/// </summary>
		public Uri URI { get; protected set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="JGL.Resource.Resource"/> class with an autogenerated <see cref="Heirarchy.Entity.Name"/>
		/// </summary>
		/// <param name="uri"><see cref="System.Uri"/> of the stored resource</param>
		public Resource(Uri uri)
			: base(null)
		{
			Init(null, uri);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="JGL.Resource.Resource"/> class.
		/// </summary>
		/// <param name="name">Resource name (no requirement to be unique)</param>
		/// <param name="uri"><see cref="System.Uri"/> of the stored resource</param>
		public Resource(string name, Uri uri)
			: base(name)
		{
			Init(name, uri);
		}

		/// <summary>
		/// Init the instance using given name and uri.
		/// </summary>
		/// <param name="name"><see cref="Heirarchy.Entity.Name"/></param>
		/// <param name="uri">Resource URI</param>
		/// <exception cref="FileNotFoundException">Thrown if <paramref name="uri"/> is a file <see cref="Uri"/> and the file does not exist</exception>
		private void Init(string name, Uri uri)
		{
			Name = name;
			IsLoaded = false;
			URI = uri;

			lock (SyncRoot)
			{
				if (LoadThread == null)
				{
					LoadThread = new Thread(LoadResources);
					LoadThread.Start();
				}
			}

			EntityContext.RootContext.Add(this);				// Add all resources as child entities of EntityContext.RootContext
		}
			// think i can remove this, because a FileNotFound exc will be thrown in Uri_Ext.Open() by the File.Open() call if it doesnt exist (correct?)
//			if (uri.IsFile)
//			{
//				string filename = uri.GetLeftPart(UriPartial.Path);
//				if (!File.Exists(uri.GetLeftPart(UriPartial.Path)))
//					throw new FileNotFoundException(string.Format("{0} resource file \"{1}\" not found", GetType().FullName, filename), filename);
//			}

		/// <summary>
		/// Generates the <see cref="Entity.Name"/> base for auto naming
		/// </summary>
		/// <remarks>
		/// Bases names on <see cref="Resource.URI"/> path, includes as a suffix, the URI scheme and, if not a file URI, the URI hostname
		/// </remarks>
		public override string GenerateBaseName()
		{
			return Path.GetFileNameWithoutExtension(URI.GetLeftPart(UriPartial.Path)) + " " + (URI.Scheme) + (URI.IsFile ? "" : URI.Host);
		}

		/// <summary>
		/// Load the <see cref="Resource"/>
		/// </summary>
		/// <returns><c>true</c>, if loaded successfully, otherwise, <c>false</c></returns>
		public abstract void Load();
	}
}

