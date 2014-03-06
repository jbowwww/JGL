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
		protected static AutoTraceSource Trace;

		/// <summary>
		/// Global static application reference
		/// </summary>
		public static JGLApp TheApp;
		
		/// <summary>
		/// <see cref="System.Collections.ConcurrentBag"/> of all <see cref="JGL.Heirarchy.Scene"/> instances created by app
		/// </summary>
		public static ConcurrentBag<Scene> Scenes;

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
			if (args.Contains("--keystart"))
				Console.ReadKey();
			Trace = AutoTraceSource.GetOrCreate(AsyncTextFileTraceListener.GetOrCreate("JGLApp"));
			Thread.CurrentThread.Name = "Main";
			Trace.Log(TraceEventType.Information, "Started");
			try
			{
				GLib.ExceptionManager.UnhandledException += UnhandledException;
				Scenes = new ConcurrentBag<Scene>();
				Application.Init("JGLApp", ref args);
				string openProjectFile = null;
				string[] openSourceFiles;
				List<string> openSourceFilesList = new List<string>();
				foreach (string arg in args)
				{
					if (arg.StartsWith("--project=", StringComparison.InvariantCultureIgnoreCase))
						openProjectFile = arg.Substring(10);
					else if (arg.StartsWith("--source=", StringComparison.InvariantCultureIgnoreCase))
						openSourceFilesList.Add(arg.Substring(9));
				}
				openSourceFiles = openSourceFilesList.ToArray();
				JGLApp.TheApp = new JGLApp(openProjectFile, openSourceFiles);			// Set global static application variables
				Engine.Init();
				Application.Run();
			}
			catch (Exception ex)
			{
				Trace.Log(TraceEventType.Error, string.Format("{0}: {1}\n{2}", ex.GetType().Name, ex.Message, ex.StackTrace));
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