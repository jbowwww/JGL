using System;
using System.Collections;
using System.Text;
using System.IO;
using System.CodeDom.Compiler;
using System.CodeDom;
using System.Reflection;
using System.Diagnostics;
using Microsoft.CSharp;
using Mono.CSharp;
using JGL.Debugging;

namespace Dynamic
{
	/// <summary>
	/// Code compilation. Provides implementation via static methods.
	/// </summary>
	public class Compiler
	{
		/// <summary>
		/// Tracing
		/// </summary>
		public readonly static AutoTraceSource Trace = AutoTraceSource.GetOrCreate(AsyncTextFileTraceListener.GetOrCreate("JGLApp"));


		public Project Project { get; protected set; }

		public CompilerSettings Settings { get; protected set; }

		public CompilerContext Context { get; protected set; }

		public Evaluator CSEvaluator { get; protected set; }

		public CompilerResults Results { get; protected set; }

		public StringBuilder CompilerOutput { get; protected set; }

		public Compiler(Project project)
		{
			Project = project;
			Settings = new CompilerSettings()
			{
				OutputFile = "DynamicCodeTests-Scene.dll"				
			};
//			Settings.AssemblyReferences.Remove("system.dll");


		}

		/// <summary>
		/// Executes the code.
		/// </summary>
		/// <returns><c>null</c> if successful, otherwise, a <see cref="string"/></returns>
		/// <param name="code">Code to execute</param>
		/// <exception cref="InvalidProgramException">
		/// Is thrown when a program contains invalid CIL instructions or metadata.
		/// </exception>
		public string ExecuteCode(string code, Project project)
		{
			Trace.Log(TraceEventType.Information, "source=\"(...)\", project.Name=\"{0}\"", project.Name);
			Project.CodeUsings = CSEvaluator.GetUsing();
			Settings.AssemblyReferences = new System.Collections.Generic.List<string>(Project.ReferencePaths);
			CompilerOutput = new StringBuilder();
			Context = new CompilerContext(Settings, new StreamReportPrinter(new StringWriter(CompilerOutput)));
			CSEvaluator = new Evaluator(Context);
			CSEvaluator.Run(project.CodeUsings);
			string r = string.Empty;
			try
			{
				string prefix = "EntityContext RootContext = EntityContext.Root; EntityContext CurrentContext = EntityContext.Current; ICollection<Scene> _scenes = RootContext.OfType<Scene>(); Scene[] Scenes = new Scene[_scenes.Count]; _scenes.CopyTo(Scenes, 0);";
				string source = prefix + code;
				if (code.Contains(";"))
				{
					if (!CSEvaluator.Run(source))
						throw new InvalidProgramException(string.Format("Evaluator.Run() == false (source = \"{0}\")", source.Replace("\n", " ")));
				}
				else
				{
					object result;
					bool result_set;
					string input;
					code += ";";
					if (!CSEvaluator.Run(prefix))
						throw new InvalidProgramException(string.Format("Evaluator.Run() == false (prefix = \"{0}\")", prefix.Replace("\n", " ")));
					input = CSEvaluator.Evaluate(code, out result, out result_set);
					if (input != null)
						throw new InvalidProgramException(string.Format("Evaluator.Evaluate() != null (code = \"{0}\") = \"{1}\"", code.Replace("\n", " "), input.Replace("\n", " ")));
					if (result_set)
						r += result != null ? result.ToString() : "(null)";
				}
			}
			catch (Exception ex)
			{
				Console.Error.WriteLine((char)27 + "[31m" + ex.ToString());
			}
			return r.Length > 0 ? string.Concat(r, "\n") : string.Empty;
		}

		/// <summary>
		/// Compiles the C sharp code.
		/// </summary>
		/// <returns>A <see cref="System.CodeDom.Compiler.CompilerResults"/></returns>
		/// <param name="source">Source code string(s)</param>
		public CompilerResults CompileCode(string[] source, string[] referencePaths)
		{
			Trace.Log(TraceEventType.Information, "CompileCode(source=string[{0}], referencePaths=string[{1}])", source.Length, referencePaths.Length);

			CSharpCodeProvider provider = new CSharpCodeProvider();
			Trace.Log(TraceEventType.Verbose, "CodeProvider: {0}", provider.GetType().Name);
			
			// Build the parameters for source compilation.
			CompilerParameters cp = new CompilerParameters();
			cp.ReferencedAssemblies.AddRange(referencePaths);
			cp.GenerateExecutable = true;
			cp.OutputAssembly = "DynamicCodeTests-Scene.dll";
			cp.GenerateInMemory = false;
			Trace.Log(TraceEventType.Verbose, "Compiler Parameters = {{ {0} }}", ParamsToString(cp));

			// Invoke compilation.
			Results = provider.CompileAssemblyFromSource(cp, source);

			if (Results.Errors.Count > 0)
			{
				// Display compilation errors.
				string errorSummary = string.Format("Compilation errors: {0}", Results.Errors.Count.ToString());
				Trace.Log(TraceEventType.Information, errorSummary);
				foreach (CompilerError ce in Results.Errors)
					Trace.Log(TraceEventType.Error, ce.ToString());
			}
			else
			{
				Trace.Log(TraceEventType.Information, "Source built {0}successfully.", cp.GenerateExecutable ? string.Concat("into ", Results.PathToAssembly, " ") : "");
			}
			return Results;
		}

		/// <summary>
		/// Returns a string detailing compiler parameters
		/// </summary>
		/// <returns>A string representing <paramref name="compileParameters"/></returns>
		/// <param name="cp"><see cref="CompilerParameters"/> to represent in the returned string</param>
		/// <param name="separator">Separator to use in between each array item</param>
		public static string ParamsToString(CompilerParameters compileParameters, string separator = " ")
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendFormat(
				"Generate Executable: {0}, Generate In Memory: {1}, Output Assembly: {2}, Include Debug Info: {3}, Warnings As Errors: {4}, Warning Level: {5}, Compiler Options: {6}, Main Class: {7}, User Token: {8}, Win32 Resource: {9}, Embedded Resources: {10}, Linked Resources: {11}, Referenced Assemblies: {12}, Temp Files: {13}\n}}",
                compileParameters.GenerateExecutable.ToString(), compileParameters.GenerateInMemory.ToString(), compileParameters.OutputAssembly, compileParameters.IncludeDebugInformation.ToString(),
				compileParameters.TreatWarningsAsErrors.ToString(), compileParameters.WarningLevel.ToString(), compileParameters.CompilerOptions, compileParameters.MainClass, compileParameters.UserToken.ToString(),
				compileParameters.Win32Resource, ArrayToString(compileParameters.EmbeddedResources), ArrayToString(compileParameters.LinkedResources), ArrayToString(compileParameters.ReferencedAssemblies), ArrayToString(compileParameters.TempFiles));
			return sb.ToString();
		}

		/// <summary>
		/// Represent an array's items as a string
		/// </summary>
		/// <returns><see cref="string"/> representing the array items</returns>
		/// <param name="array">The array</param>
		/// <param name="separator">Separator to use in between each array item</param>
		public static string ArrayToString(IEnumerable array, string separator = " ")
		{
			StringBuilder sb = new StringBuilder();
			foreach (object o in array)
				sb.Append(string.Concat(o.ToString(), separator));
			return sb.ToString(0, sb.Length > 0 ? sb.Length - separator.Length : 0);
		}
	}
}