using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using JGL.Debugging;

namespace JGL.IO
{
	/// <summary>
	/// Static class with helper methods for dealing with files and directories
	/// </summary>
	public static class Filesystem
	{
		private static AutoTraceSource Trace = AutoTraceSource.GetOrCreate(AsyncXmlFileTraceListener.GetOrCreate("JGL"));

		public const string DefaultPathSeparator = "/";

		public static readonly char[] ValidPathSeparators = new char[] { '/', '\\' };

		public static readonly string[] DefaultSearchPaths = new string[] { "." };

		/// <summary>
		/// Open a file given a path and <see cref="FileMode"/>
		/// </summary>
		/// <param name="path">Path to file. May be relative or absolute. If relative, searches configured base directories for file</param>
		/// <param name="mode"><see cref="FileMode"/> to open file with</param>
		/// <param name="searchPaths">
		/// Optionally specifies base directory search paths to attempt to find the file in.
		/// Ignored if <paramref name="path"/> is an absolute path.
		/// </param>
		/// <returns>A <see cref="FileStream"/> representing the opened file</returns>
		public static FileStream Open(string path, FileMode mode, string[] searchPaths = null)
		{
			if (Path.IsPathRooted(path) || searchPaths == null || searchPaths.Length == 0)
				return File.Open(path, mode);

			foreach (string baseDir in searchPaths)
			{
				baseDir.Trim(ValidPathSeparators);
				string tryPath = string.Concat(baseDir, DefaultPathSeparator, path);
				string tryDir = Path.GetDirectoryName(tryPath);
				string tryName = Path.GetFileName(tryPath);
				if (Directory.Exists(tryDir))
				{
					if (File.Exists(tryPath))
						return File.Open(tryPath, mode);
				}
				else
					Trace.Log(System.Diagnostics.TraceEventType.Warning, "Directory \"{0}\" does not exist", tryDir);
			}

			throw new FileNotFoundException(string.Format(
				"Searched {0} base paths for file with path \"{1}\", which could not be found", searchPaths.Length, path), path);
		}
	}
}

