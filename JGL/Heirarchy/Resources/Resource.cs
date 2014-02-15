using System;
using System.IO;
using System.Diagnostics;
using JGL.IO;
using JGL.Heirarchy;
using JGL.Debugging;

namespace JGL.Heirarchy.Resources
{
	/// <summary>
	/// Abstract resource base class
	/// </summary>
	public abstract partial class Resource : Entity
	{
		/// <summary>
		/// Tracing
		/// </summary>
		public static readonly AutoTraceSource Trace = AutoTraceSource.GetOrCreate(AsyncXmlFileTraceListener.GetOrCreate("JGL"));

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
		protected Resource(string path)
			: base(path)
		{
			Init(path, path);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="JGL.Resource.Resource"/> class.
		/// </summary>
		/// <param name="name">Resource name (no requirement to be unique)</param>
		/// <param name="path">Path to the stored resource</param>
		protected Resource(string name, string path)
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

			IsLoaded = false;
			Path = path;
			Name = name;

			StartLoadThread();

//			EntityContext.Root.Add(this);				// Add all resources as child entities of EntityContext.RootContext
			_loadQueue.Enqueue(this);
		}

		/// <summary>
		/// Generates the <see cref="Entity.Name"/> base for auto naming
		/// </summary>
		protected override string GenerateAutoName()
		{
			return System.IO.Path.GetFileName(Path);
		}

		/// <summary>
		/// Load the <see cref="Resource"/>
		/// </summary>
		/// <returns><c>true</c>, if loaded successfully, otherwise, <c>false</c></returns>
		public abstract void Load(Stream stream);
	}
}

