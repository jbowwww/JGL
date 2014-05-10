using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Text;
using System.IO;
using System.Configuration;
using System.Threading;
using System.Diagnostics;
using Gtk;
using JGL;
using JGL.Heirarchy;
using JGL.Extensions;
using JGL.Debugging;
using Dynamic.UI;
using JGL.Configuration;
using Gdk;

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
		public static ConcurrentBag<Scene> Scenes = new ConcurrentBag<Scene>();

		/// <summary>
		/// The entry point of the program, where the program control starts and ends.
		/// </summary>
		/// <param name='args'>Command-line arguments</param>
		/// <remarks>
		/// Sets up trace listeners, <see cref="DynamicCodeApplication.Scenes"/> bag, and allocates a new
		/// <see cref="DynamicCodeApplication"/> instance which in turn creates a <see cref="DynamicCodeApplication.CodeWindow"/>
		/// </remarks>
		public static void Main(string[] argv)
		{
//			Console.WindowLeft = 0;
//			Console.WindowTop = 0;
//			Console.WindowHeight = Console.LargestWindowHeight;
//			Console.WindowWidth = Console.LargestWindowWidth;
			Console.Title = "JGLApp Console";
			Thread.CurrentThread.Name = "Main";
			if (argv.Contains("--keystart"))
				Console.ReadKey();

//			Gdk.Window window = (Gdk.Window)Gdk.Window.GetObject(Process.GetCurrentProcess().MainWindowHandle);
//			window.Maximize();

//			Console.SetWindowPosition(0, 0);
//			Console.SetWindowSize(Console.LargestWindowWidth, Console.LargestWindowHeight);

//			JGLConfigurationSection config = new JGLConfigurationSection();
//			config.SectionInformation.SetRawXml(File.ReadAllText("JGL.dll.config"));
//				//new ConfigurationFileMap("JGL.dll.config");
////						 = /*typeof(AutoTraceSource)*/ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None).EvaluationContext.HostingContext
////			JGLConfigurationSection jglConfig = (JGLConfigurationSection)config.SectionInformation.GetSection("JGL");
////			config.SectionInformation.RevertToParent();
//			Console.WriteLine("\n{0}\n", config.SectionInformation.ConfigSource);
//			Console.ReadLine();

			Trace = AutoTraceSource.GetOrCreate(AsyncTextFileTraceListener.GetOrCreate("App"));
			Trace.Log(TraceEventType.Information, "Started");

			try
			{
				GLib.ExceptionManager.UnhandledException += UnhandledException;
				Gtk.Init.Check(ref argv);
				Gtk.Application.Init("JGLApp", ref argv);

//				foreach (Gdk.Window window in DisplayManager.Get().DefaultDisplay.DefaultScreen.ToplevelWindows)
//				{
//					Trace.Log(TraceEventType.Information, "Gtk.Window:\n {0}", window);//.GetType().FullName);
//				}

//				foreach (Gdk.Window window in DisplayManager	)	//; Gdk.Window.Toplevels)
//				{d
//					window.Maximize();
//					Trace.Log(TraceEventType.Information, "Gtk.Window: {0}", window.GetType().FullName);
//					window.Title = "JGLApp";
//				}

				string openProjectFile = null;
				string[] openSourceFiles;
				List<string> openSourceFilesList = new List<string>();
				foreach (string arg in argv)
				{
					if (arg.StartsWith("--project=", StringComparison.InvariantCultureIgnoreCase))
						openProjectFile = arg.Substring(10);
					else if (arg.StartsWith("--source=", StringComparison.InvariantCultureIgnoreCase))
						openSourceFilesList.Add(arg.Substring(9));
				}
				openSourceFiles = openSourceFilesList.ToArray();

				JGLApp.TheApp = new JGLApp(openProjectFile, openSourceFiles);			// Set global static application variables
				JGL.Engine.Init();

				Gtk.Application.Run();
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