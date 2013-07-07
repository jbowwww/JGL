using System;
using System.Collections;
using System.Threading;
using Gtk;
using JGL.Heirarchy;

namespace Dynamic.UI
{
	/// <summary>
	/// Scene type selection window displays types derived from <see cref="JGL.Heirarchy.Scene"/> in the currently
	/// compiled assembly, allowing creation of one or more instances of the desired types
	/// </summary>
	public class SceneTypeDialog
	{
		//private CodeWindow _codeWindow;
		
		/// <summary>
		/// Get the <see cref="Gtk.Window"/> corresponding to this instance. Gets set during construction
		/// </summary>
		public Widget GtkWindow { get; private set; }
		
		/// <summary>
		/// Combo box with listing types derived from <see cref="JGL.Heirarchy.Scene"/>
		/// </summary>
		[Glade.Widget]
		ComboBox cmbSceneTypes;

		/// <summary>
		/// The return <see cref="System.Type"/>
		/// </summary>
		public Type Return = null;

		private bool closeClicked = false;

		/// <summary>
		/// Initializes a new instance of the <see cref="DynamicCodeTests.SceneTypeDialog"/> class.
		/// </summary>
		/// <param name='sceneTypes'><see cref="JGL.Heirarchy.Scene"/>-derived types</param>
		/// <exception cref='ArgumentException'>
		/// Is thrown when an argument passed to a method is invalid.
		/// </exception>
		public SceneTypeDialog(Type[] sceneTypes, CodeWindow codeWindow)// : base(WindowType.Toplevel)
		{
			//_codeWindow = codeWindow;
			Glade.XML gxml = new Glade.XML(/*"/home/jk/Code/JGL/DynamicCodeTests/UI/*/ "UI/CodeWindow.glade", "SceneTypeDialog", null);
			gxml.Autoconnect(this);																			// load & autoconnect glade UI for a CodeWindow
			
			CellRenderer r = new CellRendererText();
			cmbSceneTypes.PackStart(r, true);
			cmbSceneTypes.SetAttributes(r, "text", 0);
			ListStore store = new ListStore(typeof(string));
				for (int i = 0; i < sceneTypes.Length; i++)
			{
				Type sType = sceneTypes[i];
				if (!sType.IsSubclassOf(typeof(Scene)))
					throw new ArgumentException(string.Format("{0} is not a scene type", sType.FullName), string.Format("sceneTypes[{0}]", i));
				else
					store.AppendValues(sType.FullName);
			}
			cmbSceneTypes.Model = store;
	
			cmbSceneTypes.ShowAll();

			cmbSceneTypes.Toplevel.ShowAll();
		}

		/// <summary>
		/// Run this modal dialog
		/// </summary>
		/// <returns>
		/// A <see cref="System.Type"/> derived from <see cref="JGL.Heirarchy.Scene"/>, if the create button was
		/// clicked, otherwise, returns null.
		/// </returns>
		public Type Run()
		{
			cmbSceneTypes.RootWindow.Show();
			while (!closeClicked)
				Thread.Sleep(220);
			cmbSceneTypes.RootWindow.Destroy();
			return Return;
		}

		/// <summary>
		/// Handles the create button click event
		/// </summary>
		protected void OnCreateScene(object sender, EventArgs e)
		{
			Return = Type.GetType(cmbSceneTypes.ActiveText);
			closeClicked = true;
		}

		/// <summary>
		/// Raises the close event.
		/// </summary>
		protected void OnClose(object sender, EventArgs e)
		{
		//	cmbSceneTypes.Toplevel.Destroy();
			closeClicked = true;
		}
	}
}

