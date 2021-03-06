using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using System.Diagnostics;
using JGL.Debugging;

namespace Dynamic
{
	/// <summary>
	/// Represents a JGL project
	/// </summary>
	public class Project
	{
		public static readonly AutoTraceSource Trace = AutoTraceSource.GetOrCreate(AsyncTextFileTraceListener.GetOrCreate("JGLApp"));

		/// <summary>
		/// Reference path tree node class
		/// </summary>
		[Gtk.TreeNode]
		public class ReferencePathTreeNode : Gtk.TreeNode
		{
			[Gtk.TreeNodeValue(Column=0)]
			public string Path;
		}

		/// <summary>
		/// The default reference paths.
		/// </summary>
		public static string[] DefaultReferencePaths =
			#region new string[] { [Default reference paths] }
			new string[] {
				"System.dll", "JGL.dll",
				"/home/jk/Code/Resources/CS/OpenTK 1.0/Binaries/OpenTK/Release/OpenTK.dll"
			};
			#endregion

		/// <summary>
		/// Project name
		/// </summary>
		[XmlAttribute]
		public string Name;

		/// <summary>
		/// Referenced assembly paths
		/// </summary>
		public List<string> ReferencePaths;

		/// <summary>
		/// The source paths.
		/// </summary>
		public List<string> SourcePaths = new List<string>();

		/// <summary>
		/// Constant default code usings.
		/// </summary>
		public const string DefaultCodeUsings = "using System;\nusing System.Collections;\nusing System.Collections.Generic;\nusing System.Collections.Concurrent;\nusing OpenTK;\nusing JGL;\nusing JGL.Heirarchy;\nusing Dynamic;";

		/// <summary>
		/// The code usings.
		/// </summary>
		public string CodeUsings;

		/// <summary>
		/// The code history.
		/// </summary>
		public List<string> CodeHistory = new List<string>();

		/// <summary>
		/// The compile output.
		/// </summary>
		public string CompileOutput;

		/// <summary>
		/// Initializes a new instance of the <see cref="Dynamic.Project"/> class.
		/// Default constructor used by serialization
		/// </summary>
		protected Project()
		{
			Trace.Log(TraceEventType.Information, "Protected c'tor for deserialization");
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Dynamic.Project"/> class.
		/// </summary>
		/// <param name='sourcePaths'>
		/// Source paths.
		/// </param>
		/// <param name='referencePaths'>
		/// Reference paths.
		/// </param>
		public Project(string[] sourcePaths, string[] referencePaths)
		{
			Trace.Log(TraceEventType.Information, "sourcePaths={0}, referencePaths={1})",
				sourcePaths == null ? "(null)" : sourcePaths.ToString(),										//			          sourcePaths == null ? "(null)" : string.Concat("string[", sourcePaths.Length, "]"),
				referencePaths == null ? "(null)" : referencePaths.ToString());							//			          referencePaths == null ? "(null)" : string.Concat("string[", referencePaths.Length, "]"));

			Name = "New Project";
			SourcePaths = new List<string>(sourcePaths == null ? new string[] { } : sourcePaths);						//(sourcePaths != null ? sourcePaths : new string[] {});
			ReferencePaths = new List<string>(referencePaths == null ? new string[] { } : referencePaths);			//(referencePaths != null ? referencePaths : DefaultReferencePaths);
			CodeHistory = new List<string>();
			CodeUsings = DefaultCodeUsings;
		}

		/// <summary>
		/// Save this instance.
		/// </summary>
		public void Save()
		{
			string filename = string.Format("../../../Data/Projects/{0}.project.xml", Name);
			Trace.Log(TraceEventType.Information, "filename=\"{0}\"", filename);
			XmlSerializer xs = new XmlSerializer(typeof(Project), new XmlAttributeOverrides(), new Type[] { },
					new XmlRootAttribute(){ DataType="project", Namespace="jgl.dynamic" }, "jgl");
			using (Stream s = File.Open(filename, FileMode.Create))
				xs.Serialize(s, this);
		}

		/// <summary>
		/// Save this instance.
		/// </summary>
		public static Project Load(string filename)
		{
			Trace.Log(TraceEventType.Information, "filename=\"{0}\"", filename);
			XmlSerializer xs = new XmlSerializer(typeof(Project), new XmlAttributeOverrides(), new Type[] { },
					new XmlRootAttribute(){ DataType="project", Namespace="jgl.dynamic" }, "jgl");
			using (Stream s = File.Open(filename, FileMode.Open))
				return xs.Deserialize(s) as Project;
		}
	}
}

