using System;
using Gtk;
using JGL.Debugging;

namespace DynamicCodeTests
{
	/// <summary>
	/// Code completion window.
	/// </summary>
	/// <remarks>
	/// Only need to construct the window once for any given parent window. After doing this, call Show and Hide
	/// methods to show and hide the window as desired.
	/// </remarks>
	public class CodeCompletionWindow : Window
	{
		/// <summary>
		/// The code completions combo box widget
		/// </summary>
		protected readonly ComboBox cbCompletions;

		/// <summary>
		/// The selected completion.
		/// </summary>
		public string SelectedCompletion;

		public delegate void CompletionSelectedEvent();
		public event CompletionSelectedEvent CompletionSelected;

		public CodeCompletionWindow(Widget parent)
			: base(WindowType.Popup)
		{
			DestroyWithParent = true;
			Parent = parent;
			ScrolledWindow sw = new ScrolledWindow();
			cbCompletions = new ComboBox();
			sw.Add(cbCompletions);
			Add(sw);
		}

		public void Show(int x, int y, string[] entries)
		{
			Move(x, y);
			cbCompletions.Clear();
			foreach (string entry in entries)
				cbCompletions.AppendText(entry);
			Show();
		}

		public void OnKeyPress(object sender, KeyPressEventArgs args)
		{
			if (args.Event.Key.Equals(Gdk.Key.Return))
			{
				Debug.Assert(Visible);
				Debug.Assert(!string.IsNullOrWhiteSpace(cbCompletions.ActiveText));
				SelectedCompletion = cbCompletions.ActiveText;
				CompletionSelected();
			}
		}
	}
}

