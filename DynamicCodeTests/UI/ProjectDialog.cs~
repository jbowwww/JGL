using System;
using System.Collections.Generic;
using System.IO;
using Glade;
using Gtk;
using System.Xml.Serialization;
using Dynamic;

namespace Dynamic.UI
{
	/// <summary>
	/// Project preferences dialog.
	/// </summary>
	public class ProjectDialog : Gtk.Dialog
	{
		/// <summary>
		/// The project preferences to edit
		/// </summary>
		public readonly Project Project;

		/// <summary>
		/// Get the <see cref="Gtk.Window"/> corresponding to this instance. Gets set during construction
		/// </summary>
		public readonly Dialog GtkDialog;

		/// <summary>
		/// Preferences <see cref="Notebook"/>
		/// </summary>
		[Widget]
		public Notebook nbPreferences;

		/// <summary>
		/// The name of the ent project.
		/// </summary>
		[Widget]
		public Entry entName;

		/// <summary>
		/// The tv dynamic code usings.
		/// </summary>
		[Widget]
		public TextView tvDynamicCodeUsings;

		/// <summary>
		/// The text reference paths.
		/// </summary>
		[Widget]
		public ScrolledWindow swReferencePaths;

		/// <summary>
		/// The store reference paths.
		/// </summary>
		public NodeStore storeReferencePaths;

		/// <summary>
		/// The nv reference paths.
		/// </summary>
		public NodeView nvReferencePaths;

		/// <summary>
		/// The updated reference paths.
		/// </summary>
		private List<string> updatedReferencePaths;

		/// <summary>
		/// Initializes a new instance of the <see cref="DynamicCodeTests.UI.ProjectPreferencesDialog"/> class.
		/// </summary>
		public ProjectDialog(Window parent, Project project)
		{
			Project = project;

			XML gxml = new XML("CodeWindow.glade", "ProjectDialog", null);
			gxml.Autoconnect(this);
			GtkDialog = nbPreferences.Toplevel as Dialog;
//			GtkDialog.Reparent(parent as Widget);
			GtkDialog.Modal = GtkDialog.DestroyWithParent = true;

			storeReferencePaths = new NodeStore(typeof(Project.ReferencePathTreeNode));
			nvReferencePaths = new NodeView(storeReferencePaths);
			swReferencePaths.Add(nvReferencePaths);
			nvReferencePaths.AppendColumn("Reference Path", new CellRendererText(), "text", 0);

			Update();
		}

		/// <summary>
		/// Validate project preferences values
		/// </summary>
		protected bool Validate()
		{
			foreach (string usingLine in Project.CodeUsings.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries))
			{
				bool error = false;
				string ul = usingLine.Trim();
				if (ul.StartsWith("using ") && ul.Length > 6)
				{
					foreach (char c in ul.Substring(6))
					{
						if (!((c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || (c == '.')))
						{
//							error = true;
//							break;

				MessageDialog md = new MessageDialog(GtkDialog as Window, DialogFlags.Modal | DialogFlags.DestroyWithParent, MessageType.Error, ButtonsType.Close,
							"Error in dynamic code usings, please correct! Line = \"{0}\"", usingLine);
							md.Run();
							md.Destroy();
							tvDynamicCodeUsings.GrabFocus();
							return false;
						}
					}
				}
			}
			return true;
		}

		/// <summary>
		/// Update (load from or save to <see cref="Preferences"/>) dialog values
		/// </summary>
		/// <param name='save'>Whether this call should update dialog's control's from <see cref="Preferences"/>, or save the values to <see cref="Preferences"/></param>
		public void Update(bool save = false)
		{
			if (save)
			{
				Project.Name = entName.Text;
				Project.CodeUsings = tvDynamicCodeUsings.Buffer.Text;
				Project.ReferencePaths = new List<string>(updatedReferencePaths);
			}
			else
			{
				entName.Text = Project.Name;
				tvDynamicCodeUsings.Buffer.Text = Project.CodeUsings;
				updatedReferencePaths = new List<string>(Project.ReferencePaths);
				RefreshReferencePathsNodeView();
			}
		}

		/// <summary>
		/// Refresh this instance.
		/// </summary>
		public void RefreshReferencePathsNodeView()
		{
			// Clear and repoulate the nodeview
			storeReferencePaths.Clear();
			foreach (string refPath in updatedReferencePaths)
				storeReferencePaths.AddNode(new Project.ReferencePathTreeNode() { Path = refPath });
			nvReferencePaths.ShowAll();
		}

		/// <summary>
		/// Adds the reference.
		/// </summary>
		/// <param name='sender'>
		/// Sender.
		/// </param>
		/// <param name='args'>
		/// Arguments.
		/// </param>
		/// <exception cref='FileNotFoundException'>
		/// Is thrown when a file path argument specifies a file that does not exist.
		/// </exception>
		protected void AddReference(object sender, EventArgs args)
		{
			FileChooserDialog fDlg = new FileChooserDialog("Select Assembly", GtkDialog as Window, FileChooserAction.Open, "Cancel", ResponseType.Cancel, "Open", ResponseType.Accept);
			fDlg.Modal = true;
			fDlg.DestroyWithParent = true;
			fDlg.Close += (s, e) => {
				fDlg.Respond(ResponseType.Cancel); };
			if (fDlg.Run() == (int)ResponseType.Accept)
			{
				string path = fDlg.Filename;
				if (!File.Exists(path))
					throw new FileNotFoundException("Could not open assembly to add to project preferences", path);
				if (!updatedReferencePaths.Contains(path))
					updatedReferencePaths.Add(path);
				RefreshReferencePathsNodeView();
			}
			fDlg.Destroy();
		}

		/// <summary>
		/// Removes the reference.
		/// </summary>
		/// <param name='sender'>
		/// Sender.
		/// </param>
		/// <param name='args'>
		/// Arguments.
		/// </param>
		protected void RemoveReference(object sender, EventArgs args)
		{
			TreeIter ti;
			if (nvReferencePaths.NodeSelection.SelectedNodes.Length > 0)
			{
				foreach (ITreeNode tn in nvReferencePaths.NodeSelection.SelectedNodes)
					updatedReferencePaths.Remove((tn as Project.ReferencePathTreeNode).Path);
				RefreshReferencePathsNodeView();
			}
		}

		protected void OkButtonClicked(object sender, EventArgs args)
		{
			if (Validate())
			{
				Update(true);
				GtkDialog.Respond(ResponseType.Ok);
				GtkDialog.Destroy();
			}
		}

		protected void CancelButtonClicked(object sender, EventArgs args)
		{
			GtkDialog.Respond(ResponseType.Cancel);
			GtkDialog.Destroy();
		}
	}
}

