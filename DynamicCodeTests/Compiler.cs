using System;
using System.Collections;
using System.Text;
using System.IO;
using System.CodeDom.Compiler;
using System.CodeDom;
using System.Reflection;
using System.Diagnostics;
using Microsoft.CSharp;
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
		public readonly static AutoTraceSource Trace = AutoTraceSource.GetOrCreate(AsyncFileTraceListener.GetOrCreate( "JGLApp"));
		
		/// <summary>
		/// Compiles the C sharp code.
		/// </summary>
		/// <returns>A <see cref="System.CodeDom.Compiler.CompilerResults"/></returns>
		/// <param name="source">Source code string(s)</param>
		public static CompilerResults CompileCSharpCode(string[] source, string[] referencePaths)
		{
			CompilerResults _cr;
			CSharpCodeProvider provider = new CSharpCodeProvider();
			Trace.Log(TraceEventType.Verbose, "CodeProvider: {0}", provider.GetType().Name);
			
			// Build the parameters for source compilation.
			CompilerParameters cp = new CompilerParameters();
//			cp.ReferencedAssemblies.Add("System.dll");			    // Add assembly references
//			cp.ReferencedAssemblies.Add("/usr/lib/monodevelop/AddIns/MonoDevelop.GtkCore/MonoDevelop.GtkCore.dll");
//			cp.ReferencedAssemblies.Add("/home/jk/Code/Resources/OpenTK 1.0/Binaries/OpenTK/Release/OpenTK.dll");
//			cp.ReferencedAssemblies.Add("/home/jk/Code/JGL/JGL/bin/Debug/JGL.dll");
			cp.ReferencedAssemblies.AddRange(referencePaths);
			cp.GenerateExecutable = true;//false;
			cp.OutputAssembly = "DynamicCodeTests-Scene.dll";
			cp.GenerateInMemory = false; //true;
//			cp.MainClass = ""
			Trace.Log(TraceEventType.Verbose, ParamsToString(cp));
		    
			// Invoke compilation.
			_cr = provider.CompileAssemblyFromSource(cp, source);

			if (_cr.Errors.Count > 0)
			{
				// Display compilation errors.
				string errorSummary = string.Format("Compilation errors: {0}", _cr.Errors.Count.ToString());
				Trace.Log(TraceEventType.Information, errorSummary);
				foreach (CompilerError ce in _cr.Errors)
					Trace.Log(TraceEventType.Error, ce.ToString());
//				throw new InvalidProgramException(errorSummary);
			}
			else
			{
				Trace.Log(TraceEventType.Information, "Source built {0}successfully.", cp.GenerateExecutable ? string.Concat("into ", _cr.PathToAssembly, " ") : "");
			}
			return _cr;
		}

		/// <summary>
		/// Returns a string detailing compiler parameters
		/// </summary>
		/// <returns>A string representing <paramref name="cp"/></returns>
		/// <param name="cp"><see cref="CompilerParameters"/> to represent in the returned string</param>
		public static string ParamsToString(CompilerParameters cp)
		{
			StringBuilder sb = new StringBuilder();
			sb.AppendFormat(
				"Compiler Parameters =\n{{\n\tGenerate Executable: {0}\n\tGenerate In Memory: {1}\n\tOutput Assembly: {2}\n\tInclude Debug Info: {3}\n\tWarnings As Errors: {4}\n\tWarning Level: {5}\n\tCompiler Options: {6}\n\tMain Class: {7}\n\tUser Token: {8}\n\tWin32 Resource: {9}\n\tEmbedded Resources: {10}\n\tLinked Resources: {11}\n\tReferenced Assemblies: {12}\n\tTemp Files: {13}\n}}",
                cp.GenerateExecutable.ToString(), cp.GenerateInMemory.ToString(), cp.OutputAssembly, cp.IncludeDebugInformation.ToString(),
				cp.TreatWarningsAsErrors.ToString(), cp.WarningLevel.ToString(), cp.CompilerOptions, cp.MainClass, cp.UserToken.ToString(),
				cp.Win32Resource, ArrayToString(cp.EmbeddedResources), ArrayToString(cp.LinkedResources), ArrayToString(cp.ReferencedAssemblies), ArrayToString(cp.TempFiles));
			return sb.ToString();
		}

		/// <summary>
		/// Arraies to string.
		/// </summary>
		/// <returns>
		/// The to string.
		/// </returns>
		/// <param name='ar'>
		/// Ar.
		/// </param>
		public static string ArrayToString(IEnumerable ar, string separator = " ")
		{
			StringBuilder sb = new StringBuilder();
			foreach (object o in ar)
				sb.Append(string.Concat(o.ToString(), separator));
			return sb.ToString(0, sb.Length > 0 ? sb.Length - separator.Length : 0);
		}
	}
}