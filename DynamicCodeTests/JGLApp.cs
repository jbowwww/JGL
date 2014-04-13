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
using TraceService;
using System.ServiceModel;

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
//		protected static AutoTraceSource Trace;

		public static readonly Source Trace = Source.GetOrCreate("JGLApp", true, new ConsoleListener());

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
			Thread.CurrentThread.Name = "Main";
			if (args.Contains("--keystart"))
				Console.ReadKey();
//			Trace = AutoTraceSource.GetOrCreate(AsyncTextFileTraceListener.GetOrCreate("JGLApp"));

			Process traceProcess = Process.Start("TraceService.exe");		//"../../../../JDBG/bin/Debug/TraceService.exe");
			Thread.Sleep(1200);
			TraceProxy traceProxy = new TraceProxy(new NetTcpBinding(), new EndpointAddress("net.tcp://localhost:7777/ITraceService/"));
			TraceProxyListener traceProxyListener = new TraceProxyListener(traceProxy);
			Trace.Listeners.Add(traceProxyListener);

			try
			{
//				Trace.Log(TraceEventType.Information, "Started");
				Trace.Log(MessageLevel.Information, "Execution", "Started", args);

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
				Trace.Log(MessageLevel.Error, "Execution", ex.GetType().FullName, ex);		//string.Format("{0}: {1}\n{2}", ex.GetType().Name, ex.Message, ex.StackTrace));
			}
			finally
			{
				Trace.Log(MessageLevel.Information, "Execution", "Exiting..");
				Engine.Quit();

				Source.StopAll();
				Source.WaitAll();

				if (traceProxyListener != null)
					traceProxyListener.Dispose();
				if (traceProxy != null)
					traceProxy.Dispose();
				if (traceProcess != null)
					traceProcess.Dispose();
			}
		}

		/// <summary>
		/// Called by GLib on an unhandled exception
		/// </summary>
		protected static void UnhandledException(GLib.UnhandledExceptionArgs args)
		{
			Trace.Log(MessageLevel.Verbose, "Execution", ((Exception)args.ExceptionObject).GetType().FullName, args.ExceptionObject, args.IsTerminating, args.ExitApplication);
//			Trace.Log(MessageLevel.Error, args.ExceptionObject as Exception);			//IsTerminating={0}, ExitApplication={1})", args.IsTerminating, args.ExitApplication);
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