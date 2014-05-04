using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using System.IO;
using System.Threading;
using System.Diagnostics;
using Gtk;
using JGL;
using JGL.Heirarchy;
using JGL.Extensions;
using JGL.Debugging;
using Dynamic.UI;

namespace Dynamic
{
	/// <summary>
	/// JGL app.
	/// </summary>
	public class JGLApp
	{
		/// <summary>
		/// <see cref="JGL.Debugging.AutoTraceSource"/> for <see cref="DynamicCodeTests.JGLApp"/>
		/// </summary>
		public static AutoTraceSource Trace { get; private set; }

		/// <summary>
		/// Global static application reference
		/// </summary>
		public static JGLApp TheApp;
		
		/// <summary>
		/// <see cref="System.Collections.ConcurrentBag"/> of all <see cref="JGL.Heirarchy.Scene"/> instances created by app
		/// </summary>
		public static ConcurrentBag<Scene> Scenes;

		#region Private helper methods
		/// <summary>
		/// Gets the arguments open files.
		/// </summary>
		/// <returns>The arguments open files.</returns>
		/// <param name="args">Arguments.</param>
		private static List<string> GetArgsOpenFiles(string[] args)
		{
			List<string> openFiles = new List<string>(new string[] { null });
			foreach (string arg in args)
			{
				if (arg.StartsWith("--project=", StringComparison.InvariantCultureIgnoreCase))
					openFiles[0] = arg.Substring(10);
				else if (arg.StartsWith("--source=", StringComparison.InvariantCultureIgnoreCase))
					openFiles.Add(arg.Substring(9));
			}
			return openFiles;		//.ToArray();
		}
		#endregion

		/// <summary>
		/// The entry point of the program, where the program control starts and ends.
		/// </summary>
		/// <param name='args'>Command-line arguments</param>
		/// <remarks>
		/// Sets up trace listeners, <see cref="DynamicCodeApplication.Scenes"/> bag, and allocates a new
		/// <see cref="DynamicCodeApplication"/> instance which in turn creates a <see cref="DynamicCodeApplication.CodeWindow"/>
		/// </remarks>
		public static void Main(string[] args)
		{
			Thread.CurrentThread.Name = "Main";
			if (args.Contains("--keystart"))
				Console.ReadKey();

			Trace = AutoTraceSource.GetOrCreate(AsyncTextFileTraceListener.GetOrCreate("App"));
			Trace.Log(TraceEventType.Information, "Started");

			try
			{
				GLib.ExceptionManager.UnhandledException += UnhandledException;
				Application.Init("JGLApp", ref args);
				Engine.Init();
				Scenes = new ConcurrentBag<Scene>();
				List<string> openFiles = GetArgsOpenFiles(args);
				JGLApp.TheApp = new JGLApp(openFiles[0], openFiles.Length == 1 ? new string[] { } : openFiles.GetRange(1, openFiles.Count - 1));
				Application.Run();
			}
			catch (Exception ex)
			{
				Trace.Log(TraceEventType.Error, "{0}: {1}\n{2}", ex.GetType().Name, ex.Message, ex.StackTrace);
			}
			finally
			{
				Trace.Log(TraceEventType.Information, "Finished, exiting ..");
				Engine.Quit();
			}
		}

		/// <summary>
		/// Called by GLib on an unhandled exception
		/// </summary>
		protected static void UnhandledException(GLib.UnhandledExceptionArgs args)
		{
			Trace.Log(TraceEventType.Verbose, "IsTerminating={0}, ExitApplication={1})", args.IsTerminating, args.ExitApplication);
			Trace.Log(TraceEventType.Error, args.ExceptionObject as Exception);
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="DynamicCodeTests.JGLApp"/> class.
		/// Creates a new <see cref="DynamicCodeApplication.CodeWindow"/>
		/// </summary>
		public JGLApp(string projectFile, string[] sourceFiles)
		{
			// Start a new code window
			new CodeWindow(projectFile, sourceFiles);
		}
	}
}