using System;
using System.Diagnostics;
using OpenTK.Graphics.ES10;
using System.Threading;
using System.Collections.Generic;
using JGL.Debugging;
using System.Configuration;
using System.Runtime.Hosting;
using System.Runtime.Remoting;
using System.Reflection;

namespace JGL
{
	/// <summary>
	/// Engine.
	/// </summary>
	public static class Engine
	{
		public static readonly AutoTraceSource Trace = AutoTraceSource.GetOrCreate(AsyncXmlFileTraceListener.GetOrCreate("JGL"));

		#region Properties
		/// <summary>
		/// Options specified by an <see cref="EngineOptions"/> instance
		/// </summary>
//		public static Configuration Config { get; private set; }

//		public static DebuggingConfigurationSection DebugggingConfiguration { get; private set; }

		public static EngineOptions Options { get; private set; }

		public static Process RunningProcess { get; private set; }

		public static ProcessThreadCollection ProcessThreads { get { return RunningProcess == null ? null : RunningProcess.Threads; } }

		public static PerformanceCounterCategory[] Categories { get; private set; }

		public static PerformanceCounter[] PerformanceCounters { get; private set; }

		public static PerformanceCounter CPUTime { get; private set; }

		public static PerformanceCounter ThreadCount { get; private set; }
		#endregion

		#region Public Methods
		/// <summary>
		/// Init this instance.
		/// </summary>
		public static void Init()
		{
			Trace.Log(TraceEventType.Information);

			Options = new EngineOptions();
			RunningProcess = Process.GetCurrentProcess();
			InitCounters();
//			Config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
//			DebugggingConfiguration = Config.GetSection("JGL.Debugging");
			

		}

		/// <summary>
		/// Quit this instance.
		/// </summary>
		public static void Quit()
		{
			Trace.Log(TraceEventType.Information);
			JGL.Heirarchy.Resources.Resource.StopLoadThread();
			JGL.Debugging.AutoTraceSource.StopTraceThread();
		}

		/// <summary>
		/// Inits the counters.
		/// </summary>
		private static void InitCounters()
		{
			Trace.Log(TraceEventType.Information);
//			List<PerformanceCounter> perfCounters = new List<PerformanceCounter>();
			PerformanceCounterCategory[] processCategories = PerformanceCounterCategory.GetCategories();
			foreach (PerformanceCounterCategory category in processCategories)
			{
				switch (category.CategoryName.ToLower())
				{
					case "processor":
						foreach (string instanceName in category.GetInstanceNames())
						{
							if (instanceName.Equals("_Total", StringComparison.InvariantCultureIgnoreCase))
								foreach (PerformanceCounter counter in category.GetCounters(instanceName))
								{
									if (counter.CounterName.Equals("% Processor Time"))
									{
										CPUTime = counter;
										break;
									}
								}
							if (CPUTime != null)
								break;
						}
						break;
					case "process":
						string[] instanceNames = category.GetInstanceNames();
						foreach (string instanceName in instanceNames)
						{
							if (instanceName.StartsWith(string.Concat(RunningProcess.Id.ToString(), "/"), StringComparison.InvariantCultureIgnoreCase))// && instanceName.EndsWith(Process.GetCurrentProcess().ProcessName))
							{
								foreach (PerformanceCounter counter in category.GetCounters(instanceName))
								{
									if (counter.CounterName == "Thread Count")
									{
										ThreadCount = counter;
										break;
									}
								}
								if (ThreadCount != null)
									break;
							}
						}
						break;
					default:
						break;
				}
			}
		}
		#endregion
	}
}

