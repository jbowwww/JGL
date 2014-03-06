using System;
using System.Diagnostics;
using OpenTK.Graphics.ES10;
using System.Threading;
using System.Collections.Generic;

namespace JGL
{
	/// <summary>
	/// Engine.
	/// </summary>
	public static class Engine
	{
		/// <summary>
		/// Options specified by an <see cref="EngineOptions"/> instance
		/// </summary>
		public static EngineOptions Options { get; private set; }

		public static Process RunningProcess { get; private set; }

		public static ProcessThreadCollection ProcessThreads { get { return RunningProcess == null ? null : RunningProcess.Threads; } }

		public static PerformanceCounterCategory[] Categories { get; private set; }

		public static PerformanceCounter[] PerformanceCounters { get; private set; }

		public static PerformanceCounter CPUTime { get; private set; }

		public static PerformanceCounter ThreadCount { get; private set; }

		/// <summary>
		/// Init this instance.
		/// </summary>
		public static void Init()
		{
			Options = new EngineOptions();

			RunningProcess = Process.GetCurrentProcess();
			List<PerformanceCounter> perfCounters = new List<PerformanceCounter>();
			PerformanceCounterCategory[] processCategories = PerformanceCounterCategory.GetCategories();
			foreach (PerformanceCounterCategory category in processCategories)
			{
				if (category.CategoryName.Equals("Processor", StringComparison.InvariantCultureIgnoreCase))
				{
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
				}
				else if (category.CategoryName.Equals("Process", StringComparison.InvariantCultureIgnoreCase))
				{
					string[] instanceNames = category.GetInstanceNames();
					foreach (string instanceName in instanceNames)
					{
						if (instanceName.StartsWith(string.Concat(RunningProcess.Id.ToString(), "/"), StringComparison.InvariantCultureIgnoreCase))			// && instanceName.EndsWith(Process.GetCurrentProcess().ProcessName))
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
				}
			}

			return;
		}

//			string[] instanceName;// = RunningProcess.Id.ToString();
//			string machineName = RunningProcess.MachineName;
//			PerformanceCounter[] counters;
//			Categories = PerformanceCounterCategory.GetCategories();
//			foreach (PerformanceCounterCategory category in Categories)
//			{
//
//			instanceName = category.GetInstanceNames();
//				foreach (string instName in instanceName)
//					counters = category.GetCounters(instName);
//			}
//
////						CPUTime = new PerformanceCounter("Processor", "% Processor Time", instanceName, machineName);
////			CPUTime.NextValue();
//			ThreadCount = new PerformanceCounter("Process", "Thread Count", instanceName, machineName);

		/// <summary>
		/// Quit this instance.
		/// </summary>
		public static void Quit()
		{
			JGL.Heirarchy.Resources.Resource.StopLoadThread();
			JGL.Debugging.AutoTraceSource.StopTraceThread();
		}
	}
}

