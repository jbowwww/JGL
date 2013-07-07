using System;
using System.IO;
using System.Diagnostics;

namespace JGL.Debugging
{
	/// <summary>
	/// Asynchronous trace listener that writes to a file
	/// </summary>
	public class AsyncFileTraceListener : AsyncTraceListener
	{
		/// <summary>
		/// The path prefix.
		/// </summary>
		public string PathPrefix = "../../Logs/";

		/// <summary>
		/// The path suffix.
		/// </summary>
		public string PathSuffix = ".Trace.Log";

		/// <summary>
		/// Path of the file that this trace listener will open
		/// </summary>
		public string Path {
			get { return string.Concat(PathPrefix, Name, PathSuffix); }
		}

		/// <summary>
		/// Whether to truncate the file before opening it for writing
		/// </summary>
		public bool TruncateFile = true;

		/// <summary>
		/// Initializes a new instance of the <see cref="JGL.Debugging.AsyncFileTraceListener"/> class.
		/// </summary>
		/// <param name="name">Name of the new trace listener</param>
		public AsyncFileTraceListener(string name)
			: base(name)
		{
		}

		/// <summary>
		/// Opens the <see cref="Stream"/> the trace listener will write to
		/// </summary>
		/// <returns>The newly opened <see cref="Stream"/></returns>
		/// <remarks>
		/// Implementation of abstract member of JGL.Debugging.AsyncTraceListener
		/// </remarks>
		public override System.IO.Stream OpenStream()
		{
			string path = Path;
			string dir = System.IO.Path.GetDirectoryName(path);
			string file = System.IO.Path.GetFileName(path);
			if (!Directory.Exists(dir))
				Directory.CreateDirectory(dir);
			if (File.Exists(path) && TruncateFile)
				File.Delete(path);
			Stream stream = File.Open(string.Concat(PathPrefix, Name, PathSuffix), FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read);
			return stream;
		}
	}
}

