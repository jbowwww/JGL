using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Threading;
using System.Diagnostics;
using Gtk;
using JGL.Graphics;
using JGL.Heirarchy;
using JGL.Debugging;
using Mono.CSharp;
using Dynamic;
using JGL;
using Glade;
using System.Configuration;
using Dynamic.Configuration;
using System.Linq.Expressions;

namespace Dynamic.UI
{
	/// <summary>
	/// Code window class
	/// </summary>
	public class CodeWindow
	{
		/// <summary>
		/// The trace.
		/// </summary>
		public static readonly AutoTraceSource Trace = AutoTraceSource.GetOrCreate();//AsyncTextFileTraceListener.GetOrCreate("App"));

		/// <summary>
		/// A <see cref="ConcurrentBag"/> of all <see cref="DynamicCodeTests.CodeWindow"/> instances
		/// </summary>
		public static ConcurrentDictionary<CodeWindow, CodeWindow> CodeWindows = new ConcurrentDictionary<CodeWindow, CodeWindow>();

		/// <summary>
		/// Gets the config.
		/// </summary>
		public System.Configuration.Configuration Config { get; private set; }

		/// <summary>
		/// Gets the code window config.
		/// </summary>
		public CodeWindowConfiguration codeWindowConfig { get; private set; }

		/// <summary>
		/// Get the <see cref="Gtk.Window"/> corresponding to this instance. Gets set during construction
		/// </summary>
		public readonly Window GtkWindow;

		#region Widgets
		/// <summary>
		/// The mi file recent files.
		/// </summary>
		[Glade.Widget]
		protected MenuItem miFileRecentFiles;

		/// <summary>
		/// The mi file recent files.
		/// </summary>
		[Glade.Widget]
		protected MenuItem miFileRecentProjects;

		/// <summary>
		/// <see cref="Gtk.Notebook"/> containing C# source code(s)
		/// </summary>
		[Glade.Widget]
		protected Notebook nbCode;
		
		/// <summary>
		/// The <see cref="Gtk.ScrolledWindow"/> on the left (first) panel of the <see cref="Gtk.HPaned"/>, which after
		/// construction will contain the <see cref="Gtk.NodeView"/> <see cref="DynamicCodeApplication.tvHeirarchy"/>
		/// </summary>
		[Glade.Widget]
		protected Gtk.ScrolledWindow swHeirarchy;

		[Glade.Widget]
		protected Gtk.Label lblCPU;

		[Glade.Widget]
		protected Gtk.Label lblThreadCount;

		/// <summary>
		/// A <see cref="Gtk.NodeView"/> displaying an <see cref="JGL.Heirarchy.Entity"/> heirarchy
		/// </summary>
		protected NodeView nvHeirarchy;
		#endregion

		/// <summary>
		/// The <see cref="Gtk.NodeStore"/> to back <see cref="DynamicCodeApplication.nvWindow"/>
		/// </summary>
		protected NodeStore storeHeirarchy;		// todo: model could store strongly typed values representing entities?

		/// <summary>
		/// The scene window thread.
		/// </summary>
		private Thread SceneWindowThread;
		private SceneWindow sw;
		private int page;
		private Scene newScene;

		/// <summary>
		/// Arguments passed to <see cref="OnFinishExecute"/> handler
		/// </summary>
		public class FinishExecuteArgs : EventArgs
		{
			public Scene Scene;
			public SceneWindow SceneWindow;
			public Thread SceneWindowThread;
		}

		/// <summary>
		/// Gets or sets the project this <see cref="CodeWindow"/> operates on
		/// </summary>
		public Project Project { get; private set; }

		/// <summary>
		/// Recent project names.
		/// </summary>
		protected List<string> RecentProjectPaths = new List<string>();

		/// <summary>
		/// Recent file names.
		/// </summary>
		protected List<string> RecentFilePaths = new List<string>();

		/// <summary>
		/// Compiler for dynamic code
		/// </summary>
		public Compiler Compiler { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="DynamicCodeTests.CodeWindow"/> class.
		/// </summary>
		public CodeWindow(string projectFile = null, string[] sourceFiles = null)
		{
			Trace.Log(TraceEventType.Information, "projectFile={0}, sourceFiles=string[{1}]",
				projectFile == null ? "(null)" : projectFile.ToString(), sourceFiles == null ? "(null)" : sourceFiles.Length.ToString());

			CodeWindows.TryAdd(this, this);															// store this CodeWindow instance

			Config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
			if (Config.Sections["codeWindow"] != null)
				codeWindowConfig = (CodeWindowConfiguration)Config.Sections["codeWindow"];
			else
				Config.Sections.Add("codeWindow", codeWindowConfig = new CodeWindowConfiguration());

			Glade.XML gxml = new Glade.XML(/*"/home/jk/Code/JGL/DynamicCodeTests/UI/*/ "CodeWindow.glade", "CodeWindow", null);
			gxml.Autoconnect(this);																				// load & autoconnect glade UI for a CodeWindow
			GtkWindow = nbCode.Toplevel as Window;

			miFileRecentFiles.Submenu = new Menu();
			miFileRecentProjects.Submenu = new Menu();
			foreach (NameValueConfigurationElement element in codeWindowConfig.RecentFilePaths)
				AddRecentFile(element.Value, element.Name);
			foreach (NameValueConfigurationElement element in codeWindowConfig.RecentProjectPaths)
				AddRecentProject(element.Value, element.Name);

			Project = new Project(null, null);

			storeHeirarchy = new NodeStore(typeof(EntityTreeNode));		// create a NodeStore using Entity wrapper class
			nvHeirarchy = new NodeView(storeHeirarchy);									// create a NodeView and add to container ScrolledWindow
			swHeirarchy.Add(nvHeirarchy);
			nvHeirarchy.AppendColumn(new TreeViewColumn("Entity", new CellRendererText(), "text", 0) { Resizable = true });
			nvHeirarchy.AppendColumn("Type", new CellRendererText(), "text", 1);
			nvHeirarchy.Columns[0].Resizable = true;											// Add columns to NodeView and configure
			nvHeirarchy.Selection.Changed +=
			delegate(object o, EventArgs args)
			{
				TreePath[] paths = nvHeirarchy.Selection.GetSelectedRows();
				if (paths.Length > 0)
				{
					Entity entity = (storeHeirarchy.GetNode(paths[0]) as EntityTreeNode).Entity;
					if (entity.IsContext)
						EntityContext.Current = entity as EntityContext;
				}
			};

			PopulateHeirarchy();																					// Populate the NodeView with the Entity heirarchy
			GtkWindow.ShowAll();																				// Show window

			if (projectFile != null)
				OpenProject(projectFile);
			if (sourceFiles != null)
				OpenSourceFiles(sourceFiles);																							// Open initial files (if any)
		}
				
		/// <summary>
		/// Populates the <see cref="CodeWindow.tvHeirarchy"/> <see cref="Gtk.NodeView"/> with
		/// the <see cref="JGL.Heirarchy.Entity"/> heirarchy
		/// </summary>
		public void PopulateHeirarchy()
		{
			Trace.Log(TraceEventType.Verbose);
			storeHeirarchy.Clear();
			if (EntityContext.Root != null)
			{
				foreach (Entity e in EntityContext.Root)
					PopulateHeirarchy(e);
			}
		}
		
		/// <summary>
		/// Populates the heirarchy rooted at <paramref name="ec"/>, as a subtree of node <paramref name="ti"/>
		/// </summary>
		/// <param name='entity'><see cref="JGL.Heirarchy.Entity"> to add.</param>
		/// <param name='tnParent'><see cref="Gtk.TreeNode"/> that is to hold the child tree nodes representing the subheirarchy of <paramref name="ec"/></param>
		private void PopulateHeirarchy(Entity entity, TreeNode tnParent = null)
		{
//			Trace.Log(TraceEventType.Verbose, "entity.Id=\"{0}\", tnParent=\"{1}\"", entity.Id, tnParent == null ? "(null)" : tnParent.ToString());
			TreeNode tnNew = new EntityTreeNode(entity);
			if (tnParent == null)
				storeHeirarchy.AddNode(tnNew);
			else
				tnParent.AddChild(tnNew);
			if (entity.IsContext)
				foreach (Entity e in (entity as EntityContext).Entities)
					PopulateHeirarchy(e, tnNew);
		}

		/// <summary>
		/// Adds the recent project.
		/// </summary>
		/// <param name="filename">Filename.</param>
		private void AddRecentProject(string path, string name = null)
		{
			if (string.IsNullOrWhiteSpace(name))
				name = Path.GetFileName(path);
			if (!RecentProjectPaths.Contains(path))
			{
				RecentProjectPaths.Add(path);
				Menu mnuProjects = ((Menu)miFileRecentProjects.Submenu);
				ImageMenuItem miProject = new ImageMenuItem(name);
				miProject.Activated += (sender, e) => OpenProject(path);
				mnuProjects.Append(miProject);
				mnuProjects.ShowAll();
			}
		}

		/// <summary>
		/// Adds the recent file.
		/// </summary>
		/// <returns>The recent file.</returns>
		/// <param name="filename">Filename.</param>
		private void AddRecentFile(string path, string name = null)
		{
			if (string.IsNullOrWhiteSpace(name))
				name = Path.GetFileName(path);
			if (!RecentFilePaths.Contains(path))
			{
				RecentFilePaths.Add(path);
				Menu mnuRecentFiles = ((Menu)miFileRecentFiles.Submenu);
				ImageMenuItem miRecent = new ImageMenuItem(name);
				miRecent.Activated += (sender, e) => OpenSourceFiles(path);
				mnuRecentFiles.Append(miRecent);
				mnuRecentFiles.ShowAll();
			}
		}

		/// <summary>
		/// Opens the given file(s)
		/// </summary>
		/// <param name='filenames'>Array of filename strings giving paths to files to open</param>
		public int OpenSourceFiles(params string[] filenames)
		{
			Trace.Log(TraceEventType.Information, "filenames=string[{0}]", filenames == null ? "(null)" : filenames.Length.ToString());
			int setPage = nbCode.NPages;
			int newPages = 0;
			foreach (string filename in filenames)
			{
				if (Path.GetExtension(filename) != ".cs")
					Trace.Log(TraceEventType.Warning, "\"{0}\" has an invalid source file extension (must be a .cs file)", filename);
				try
				{
					StaticCodePage scp = (StaticCodePage)GetPagesOfType<StaticCodePage>().FirstOrDefault(
						(cp) => ((StaticCodePage)cp).FileName.CompareTo(filename) == 0);
					if (scp != null)
					{
						nbCode.Page = nbCode.PageNum(scp);
						nbCode.ShowAll();
						if (!Project.SourcePaths.Contains(filename))
						{
							Trace.Log(TraceEventType.Warning, "Project.SourcePaths does not contain file \"{0}\", although it is already open - Adding now", filename);
							Project.SourcePaths.Add(filename);
						}					
					}
					else
					{
						using (FileStream fs = File.OpenRead(filename))
						{
							StaticCodePage cp = new StaticCodePage(fs);
							nbCode.AppendPage(cp, cp.TabLabel);
							newPages++;
							AddRecentFile(filename);
							if (Project.SourcePaths.Contains(filename))
								Trace.Log(TraceEventType.Warning, "Project.SourcePaths already contains file \"{0}\", although it was not open", filename);
							else
								Project.SourcePaths.Add(filename);
						}
					}
				}
				catch (Exception ex)
				{
					string msg = string.Format("{0}: {1}: Could not open file \"{2}\"", ex.GetType().Name, ex.Message, filename);
					Trace.Log(TraceEventType.Error, msg);
					MessageDialog mDlg = new MessageDialog(null, DialogFlags.Modal | DialogFlags.DestroyWithParent, MessageType.Warning, ButtonsType.Close, msg);
					mDlg.Run();
				}
				finally
				{
//					nbCode.Page = setPage + newPages - 1;
					nbCode.ShowAll();
				}
			}
			return newPages;
		}

		/// <summary>
		/// Loads a project from the given file
		/// </summary>
		/// <param name="filename">The project file path. Its extension should be ".project.xml"</param>
		public void OpenProject(string filename)
		{
			Trace.Log(TraceEventType.Information, "filename={0}", filename);
			if (filename == null || filename.Length < 13 || filename.Substring(filename.Length - 12).ToLower() != ".project.xml")
				// TODO: Don't want to throw back to JGLApp.Unhandled exception (i think?) Want to log/display warning?
				throw new ArgumentOutOfRangeException("filename", filename, "Invalid project file extension (must be a .project.xml file)");
			Project = Project.Load(filename);
			AddRecentProject(filename);
			Compiler = new Compiler(Project);
			PopulateHeirarchy();
			OpenSourceFiles(Project.SourcePaths.ToArray());
		}

		/// <summary>
		/// Saves the current project to file. Filename is "[<see cref="Project.Name"/>].project.xml"
		/// </summary>
		protected void SaveProject()
		{
			Trace.Log(TraceEventType.Verbose);
			if (Project == null)
				throw new InvalidOperationException("Project == null");
			Project.Save();
		}

		/// <summary>
		/// Get child pages of <see cref="CodeWindow.nbCode"/> that are of type <typeparamref name="TPage">
		/// </summary>
		/// <returns>An array of <see cref="Gtk.Widget"/>s</returns>
		/// <typeparam name="TPage">The 1st type parameter</typeparam>
		protected Widget[] GetPagesOfType<TPage>()
		{
			Trace.Log(TraceEventType.Verbose, "TPage=typeof({0})", typeof(TPage).FullName);
			List<Widget> pages = new List<Widget>();
			Widget nbPage;
			for (int i = 0; i < nbCode.NPages && ((nbPage = nbCode.GetNthPage(i)) != null); i++)
				if (nbPage.GetType().Equals(typeof(TPage)))
					pages.Add(nbPage);
			return pages.ToArray();
		}

		/// <summary>
		/// Confirms that the user wants to close the <see cref="CodeWindow"/>. If any <see cref="StaticCodePage"/>s are unsaved,
		/// a dialog will prompt the user to save it, discard the unsaved changes, or cancel exiting
		/// </summary>
		/// <returns><c>true</c> as long as the user's response is yes or no. Otherwise (response is cancel), <c>false</c>.</returns>
		protected virtual bool ConfirmExit()
		{
			Trace.Log(TraceEventType.Verbose);
			foreach (StaticCodePage cp in GetPagesOfType<StaticCodePage>())
			{
				if (cp.Unsaved)
				{
					MessageDialog saveDialog = new MessageDialog(
						GtkWindow, DialogFlags.Modal, MessageType.Question, ButtonsType.YesNo | ButtonsType.Cancel,
						"Save modified file \"{0}\" before closing?", cp.FileName);
					switch ((ResponseType)saveDialog.Run())
					{
						case ResponseType.Cancel:
							return false;
						case ResponseType.Yes:
							cp.Save();
							break;
						case ResponseType.No:
						default:
							break;
					}
				}
			}
			return true;
		}

		/// <summary>
		/// Raises the widget delete event event.
		/// </summary>
		public void OnWidgetDeleteEvent(object sender, DeleteEventArgs e)
		{
			Trace.Log(TraceEventType.Verbose, "sender={0}, e={1}", sender.ToString(), e.ToString());
			if (ConfirmExit())
			{
				SaveProject();
				CodeWindow cw;
				if (SceneWindowThread != null)
				{
					sw.Exit();
					SceneWindowThread.Join(new TimeSpan(0, 0, 8));
				}

				codeWindowConfig.RecentProjectPaths.Clear();
				foreach (string projectPath in RecentProjectPaths)
					codeWindowConfig.RecentProjectPaths.Add(new NameValueConfigurationElement(Path.GetFileName(projectPath), projectPath));
				codeWindowConfig.RecentFilePaths.Clear();
				foreach (string filePath in RecentFilePaths)
					codeWindowConfig.RecentFilePaths.Add(new NameValueConfigurationElement(Path.GetFileName(filePath), filePath));

				Config.Save(ConfigurationSaveMode.Full);
				
				CodeWindows.TryRemove(this, out cw);
				if (CodeWindows.Count == 0)
					Gtk.Application.Quit();
			}
		}
		
		/// <summary>
		/// Raises the file new event.
		/// </summary>
		protected void OnFileNew(object sender, EventArgs e)
		{
			Trace.Log(TraceEventType.Verbose, "sender={0}, e={1}", sender.ToString(), e.ToString());
			StaticCodePage cp = new StaticCodePage();
			nbCode.AppendPage(cp, cp.TabLabel);
			nbCode.ShowAll();
		}

		/// <summary>
		/// Raises the file open event.
		/// </summary>
		protected void OnFileOpen(object sender, EventArgs e)
		{
			Trace.Log(TraceEventType.Verbose, "sender={0}, e={1}", sender.ToString(), e.ToString());
			FileChooserDialog fDlg = new FileChooserDialog("Open File", null, FileChooserAction.Open, "Cancel", ResponseType.Cancel, "Open", ResponseType.Accept);
			fDlg.SelectMultiple = true;
			if (fDlg.Run() == (int)ResponseType.Accept)
			{
				List<string> filenames = new List<string>(fDlg.Filenames);
				string[] projectFilenames = filenames.FindAll((string s) => {
					return s.ToLower().EndsWith(".project.xml"); }).ToArray();
				if (projectFilenames.Length > 1)							// filenames include multiple project files
					throw new InvalidOperationException("Cannot open multiple project files. Filenames must include <= 1 project file (*.project.xml)");
				else if (projectFilenames.Length == 1)				// filenames include a project file to open; open the project and then open any selected source files in the project
					{
						OpenProject(projectFilenames[0]);
						filenames.Remove(projectFilenames[0]);
					}
				OpenSourceFiles(filenames.ToArray());
			}
			fDlg.Destroy();
		}

		/// <summary>
		/// Raises the file save event.
		/// </summary>
		protected void OnFileSave(object sender, EventArgs e)
		{
			Trace.Log(TraceEventType.Verbose, "sender={0}, e={1}", sender.ToString(), e.ToString());
			if (nbCode.NPages > 0)
			{
				StaticCodePage cp = (StaticCodePage)nbCode.GetNthPage(nbCode.Page);
				if (cp.FileName.Equals("New File"))
					OnFileSaveAs(sender, e);
				else
				{
//					string labelText = cp.TabLabel.Text;
//					using (FileStream fs = File.OpenWrite(labelText.Substring(0, labelText.Length - 2)))
					using (FileStream fs = File.OpenWrite(cp.FileName))
					{
						cp.Save(fs);
					}
					if (!Project.SourcePaths.Contains(cp.FileName))
					{
						Trace.Log(TraceEventType.Warning, "Project.SourcePaths does not already contain \"{0}\", adding now", cp.FileName);
						Project.SourcePaths.Add(cp.FileName);
					}
				}
			}
		}

		/// <summary>
		/// Raises the file save event.
		/// </summary>
		protected void OnFileSaveAs(object sender, EventArgs e)
		{
			Trace.Log(TraceEventType.Verbose, "sender={0}, e={1}", sender.ToString(), e.ToString());
			if (nbCode.NPages > 0)
			{
				StaticCodePage cp = (StaticCodePage)nbCode.GetNthPage(nbCode.Page);
				FileChooserDialog fDlg = new FileChooserDialog("Save File", null, FileChooserAction.Save, "Cancel", ResponseType.Cancel, "Save", ResponseType.Accept);
				fDlg.SetFilename(cp.FileName);
				if (fDlg.Run() == (int)ResponseType.Accept)
				{
					string oldFilename = cp.FileName;
					using (FileStream fs = File.OpenWrite(fDlg.Filename))
					{
						cp.FileName = fDlg.Filename;
						cp.Save(fs);
					}
					if (!string.IsNullOrWhiteSpace(oldFilename) && Project.SourcePaths.Contains(oldFilename))
						Project.SourcePaths.Remove(oldFilename);
					if (Project.SourcePaths.Contains(cp.FileName))
						Trace.Log(TraceEventType.Warning, "Project.SourcePaths already contains file \"{0}\", no need to add");
					else
						Project.SourcePaths.Add(cp.FileName);
				}
				fDlg.Destroy();
				nbCode.ShowAll();
			}
		}

		/// <summary>
		/// Raises the file save all event.
		/// </summary>
		/// <param name="sender">Sender.</param>
		/// <param name="e">E.</param>
		protected void OnFileSaveAll	(object sender, EventArgs e)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Raises the execute event.
		/// </summary>
		protected void OnProjectCompile(object sender, EventArgs e)
		{
			Trace.Log(TraceEventType.Verbose, "sender={0}, e={1}", sender.ToString(), e.ToString());
			List<string> sources = new List<string>();
			foreach (StaticCodePage cp in GetPagesOfType<StaticCodePage>())
				sources.Add(cp.Source);

			// Compile code
			Compiler.CompileCode(sources.ToArray(), Project.ReferencePaths.ToArray());

			// Compile errors
			if (Compiler.Results.Errors.Count > 0)
			{

				StringBuilder sb = new StringBuilder();
				sb.AppendFormat("{0} errors while compiling {1} sources:\n", Compiler.Results.Errors.Count, sources.Count);
				foreach (System.CodeDom.Compiler.CompilerError error in Compiler.Results.Errors)
					sb.AppendFormat("{0} {1} @ {2}:{3},{4}: {5}\n", error.IsWarning ? "Warning" : "Error",
						 error.ErrorNumber, error.FileName, error.Line, error.Column, error.ErrorText);
			}
		}

		/// <summary>
		/// Raises the execute event.
		/// </summary>
		protected void OnProjectExecute(object sender, EventArgs e)
		{
			Trace.Log(TraceEventType.Verbose, "sender={0}, e={1}", sender.ToString(), e.ToString());
			if (SceneWindowThread == null)
			{
				List<string> sources = new List<string>();
				foreach (StaticCodePage cp in GetPagesOfType<StaticCodePage>())
					sources.Add(cp.Source);

				// Compile code
				Compiler.CompileCode(sources.ToArray(), Project.ReferencePaths.ToArray());
	
				// Compile errors
				if (Compiler.Results.Errors.Count > 0)
				{
	
					StringBuilder sb = new StringBuilder();
					sb.AppendFormat("{0} errors while compiling {1} sources:\n", Compiler.Results.Errors.Count, sources.Count);
					foreach (System.CodeDom.Compiler.CompilerError error in Compiler.Results.Errors)
						sb.AppendFormat("{0} {1} @ {2}:{3},{4}: {5}\n", error.IsWarning ? "Warning" : "Error", error.ErrorNumber, error.FileName, error.Line, error.Column, error.ErrorText);
//					MessageDialog mDlg = new MessageDialog(null, DialogFlags.Modal, MessageType.Error, ButtonsType.Ok, sb.ToString());
//					mDlg.Response += delegate(object o, ResponseArgs args) {
//						if (args.ResponseId == ResponseType.Ok || args.ResponseId == ResponseType.Close)
//							mDlg.Destroy();
//					};
//					mDlg.Show();
				}
	
				// Compilation success
				else
				{
					// Find scene types in compiled assembly
					Type[] assemblyTypes = Compiler.Results.CompiledAssembly.GetTypes();
					List<Type> sceneTypes = new List<Type>();
					foreach (Type t in assemblyTypes)
						if (t.IsSubclassOf(typeof(Scene)))
							sceneTypes.Add(t);
		
					// Error: no scene types found
					if (sceneTypes.Count == 0)
					{
						MessageDialog mDlg = new MessageDialog(null, DialogFlags.Modal | DialogFlags.DestroyWithParent,
							MessageType.Error, ButtonsType.Ok, "Assembly \"{0}\" does not contain any Scene types", Compiler.Results.CompiledAssembly.FullName);
						mDlg.Show();
					}
	
					// Run assembly
					else
					{
						try
						{
							Type sceneType = sceneTypes[0];			// select first scene type by default
							if (sceneTypes.Count > 1)							// if more than one found, display
								sceneType = new SceneTypeDialog(sceneTypes.ToArray(), this).Run();
							Trace.Assert(sceneType != null);
		
//							newScene = sceneType.GetConstructor(new Type[] { typeof(string) }).Invoke(new object[] { null }) as Scene;
							newScene = sceneType.GetConstructor(new Type[] { }).Invoke(new object[] { }) as Scene;
							EntityContext.Root.Add(newScene);

							DynamicCodePage dcp = new DynamicCodePage(Project, this);
//							dcp.Position = dcp.HeightRequest - 44;
							page = nbCode.AppendPage(dcp, new Label("[Immediate]"));

							SceneInfoPanel sip = new SceneInfoPanel();
							nbCode.AppendPage(sip, new Label("[Scene Info]"));

							SceneWindowThread = new Thread(() =>
							{
								try
								{
									sw = new SceneWindow(800, 600, null, "bullshit") { Scene = newScene };
									sw.UpdateHandler += (object swSender, RenderArgs renderArgs) =>
									{
										Gtk.Application.Invoke(sw, renderArgs, sip.Update);	// RenderArgs = ra }
									};
									sw.Run(1, 30);
								}
								catch (Exception ex)
								{
									Trace.Log(TraceEventType.Error, ex.Message + "\nStacktrace:\n" + ex.StackTrace);
								}
								finally
								{
									if (sw != null)
									{
										sw.Close();		//.Dispose();
										sw = null;
									}
									Application.Invoke(this, new FinishExecuteArgs() { Scene = newScene, SceneWindow = sw, SceneWindowThread = SceneWindowThread }, OnFinishExecute);
								}
								}) { Name = "SceneWindow" };
//							Process.GetCurrentProcess();PerformanceCounter pc;pc.CounterType = 
							GLib.Timeout.Add(1366, () =>
								{
//									barCPU.Fraction = Engine.CPUTime.NextValue();
//									lblThreadCount.Text = "Threads:\n" + Engine.ThreadCount.RawValue;
									if (Engine.CPUTime != null)
									{
										CounterSample sample = Engine.CPUTime.NextSample();
//										barCPU.Fraction = ((float)sample.RawValue * 100f / (float)sample.TimeStamp);
										                  ////(float)Engine.CPUTime..RawValue / 100f;
										float cpu = ((float)sample.RawValue * 100f / (float)sample.TimeStamp);
										lblCPU.Text = "CPU:\n" + cpu.ToString("0.1f");
									}
									if (Engine.ThreadCount != null)
										lblThreadCount.Text = "Threads:\n" + Engine.ThreadCount.RawValue;
									return true;
								});
							SceneWindowThread.Start();
							Trace.Log(TraceEventType.Information, "Started SceneWindow thread for scene \"{0}\" .. ", newScene.Id);
						}
						catch (Exception ex)
						{
							Trace.Log(TraceEventType.Error, ex);
						}
						PopulateHeirarchy();
					}
				}
			}
		}

		/// <summary>
		/// Handler Invoked by the thread created in <see cref="OnExecute"/> to run a <see cref="SceneWindow"/>,
		/// after the window has been destroyed, right before the thread exits
		/// </summary>
		protected void OnFinishExecute(object sender, EventArgs args)
		{
			Trace.Log(TraceEventType.Verbose, "sender={0}, args={1}", sender.ToString(), args.ToString());
			FinishExecuteArgs _args = args as FinishExecuteArgs;
			Trace.Log(TraceEventType.Verbose, "Ended SceneWindow thread for scene \"{0}\"", _args.Scene.Id);
			nbCode.RemovePage(page);
			nbCode.RemovePage(page);
			EntityContext.Root.Remove(_args.Scene);
			SceneWindowThread = null;
			SaveProject();
		}

		/// <summary>
		/// Raises the refresh event.
		/// </summary>
		protected void OnRefresh(object sender, EventArgs args)
		{
			Trace.Log(TraceEventType.Verbose, "sender={0}, args={1}", sender.ToString(), args.ToString());
			PopulateHeirarchy();
		}

		/// <summary>
		/// Raises the edit preferences event.
		/// </summary>
		protected void OnProjectPreferences(object sender, EventArgs args)
		{
			Trace.Log(TraceEventType.Verbose, "sender={0}, args={1}", sender.ToString(), args.ToString());
			ProjectDialog ppDlg = new ProjectDialog(GtkWindow as Gtk.Window, Project);
			int r = ppDlg.GtkDialog.Run();
			if (r == (int)ResponseType.Ok)
			{
				Compiler = new Compiler(Project);
				SaveProject();
				PopulateHeirarchy();
			}
		}
	}
}