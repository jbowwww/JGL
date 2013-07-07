using System;
using System.Collections.Concurrent;
using System.Text;
using System.IO;
using System.Threading;
using System.Diagnostics;
using Gtk;
using JGL.Heirarchy;
using JGL.Debugging;
using OpenTK.Graphics.OpenGL;
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
		public readonly static AutoTraceSource Trace = new AutoTraceSource(typeof(JGLApp).Name,
			new ConsoleTraceListener(),
			AsyncTraceListener.GetOrCreate( "Dynamic_JGLApp", /*typeof(JGLApp).Name*/ typeof(AsyncFileTraceListener)));

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
			Trace.Log(TraceEventType.Information, "JGLApp.Main() started");
			try
			{
				GLib.ExceptionManager.UnhandledException += UnhandledException;
				Scenes = new ConcurrentBag<Scene>();
				Application.Init("JGLApp", ref args);
				JGLApp.TheApp = new JGLApp(args);			// Set global static application variables
				Application.Run();
			}
			catch (Exception ex)
			{
				Trace.Log(TraceEventType.Error, string.Format("{0}: {1}\n{2}", ex.GetType().Name, ex.Message, ex.StackTrace));
			}
			finally
			{
				Trace.Log(TraceEventType.Verbose, "JGLApp.Main() finishing");
				AsyncTraceListener.StopAll();
			}
		}

		/// <summary>
		/// Called by GLib on an unhandled exception
		/// </summary>
		protected static void UnhandledException(GLib.UnhandledExceptionArgs args)
		{
			Trace.Log(TraceEventType.Information, "JGLApp.UnhandledException() (ExceptionObject={0}, IsTerminating={1}, ExitApplication={2})", args.ExceptionObject, args.IsTerminating, args.ExitApplication);
			StringBuilder sb = new StringBuilder();
			string indent = string.Empty;
			for (Exception ex = args.ExceptionObject as Exception; ex != null; ex = ex.InnerException)
			{
				sb.AppendFormat("{0}{1}: {2}\n{0}Stacktrace:\n{0}    {3}\n{0}InnerException:\n    ", indent, ex.GetType().Name, ex.Message, ex.StackTrace.Replace("\n", "\n    " + indent));
				indent += "    ";
			}
			Trace.Log(TraceEventType.Error, sb.ToString());


				// trace logs should flush/close without needing these lines (finally clause in JGLApp.Main())
				//				if (args.IsTerminating || args.ExitApplication)
//				{
//					Trace.Log(TraceEventType.Information, "{0}JGLApp:UnhandledException(): quitting (IsTerminating={1}, ExitApplication={2})", indent, args.IsTerminating, args.ExitApplication);
//					Trace.Flush();
//					Trace.Close();
//					AsyncTraceListener.StopAll();
//				}
//				else
//				{
//					Trace.Log(TraceEventType.Information, "{0}JGLApp:UnhandledException(): continuing (IsTerminating={1}, ExitApplication={2})", indent, args.IsTerminating, args.ExitApplication);
//				}
//				indent += "    ";
//			}
//			Trace.Log(TraceEventType.Error,
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="DynamicCodeTests.JGLApp"/> class.
		/// Creates a new <see cref="DynamicCodeApplication.CodeWindow"/>
		/// </summary>
		public JGLApp(params string[] initialFiles)
		{
			// Create default windows
			Trace.Log(TraceEventType.Information, "Starting JGLApp:ctor");

			// Start a new code window
			new CodeWindow(initialFiles) { Trace = JGLApp.Trace };
//			new ProjectWindow();
		}
	}
}