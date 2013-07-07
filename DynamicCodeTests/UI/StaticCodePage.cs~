using System;
using System.Text;
using System.IO;
using System.Runtime;
using Gtk;
//using ICSharpCode.AvalonEdit;

namespace Dynamic.UI
{
	/// <summary>
	/// Custom widget displays a source file's contents inside a <see cref="Gtk.Notebook"/>
	/// </summary>
	/// <remarks>
	/// Eventually I'm hoping <see cref="MonoDevelop.SourceEditor"/> can be used to provide code formatting,
	/// code completion and syntax highlighting
	/// </remarks>
	public class StaticCodePage : ScrolledWindow
	{
		private TextView _tv;
//		MonoDevelop.SourceEditor.SourceEditorView _se;
//		ICSharpCode.AvalonEdit.TextEditor _te;
//		TextEditor _te;
		public string Source {
			get { return _tv.Buffer.Text; }
			set { _tv.Buffer.Text = value; }
//			get { return _se.Text; }
//			set {  _seText = value; }
//			get { return _te.Text; }
//			set { _te.Text = value; }
		}
		
		public bool Unsaved { get; private set; }
		
		public Gtk.Label TabLabel { get; private set; }
		
		private string _fn;
		public string FileName
		{
			get { return _fn; }
			set { _fn = value; if (TabLabel != null) TabLabel.Text = System.IO.Path.GetFileName(value); }
		}
		
		private void TextBufferChanged(object sender, EventArgs e)
		{
			if (!Unsaved)
			{
				Unsaved = true;
				if (TabLabel != null)
				{
					JGL.Debugging.Debug.Assert(!TabLabel.Text.EndsWith(" *"));
					TabLabel.Text += " *";
				}
			}
		}
			
		public StaticCodePage()
		{
			_tv = new TextView();
//			_se = new SourceEditorView();
			ScrolledWindow sw = new ScrolledWindow();
			sw.Add(_tv);
			AddWithViewport(sw);//_se););
//			Add((Widget)(_te = new TextEditor()));
			TabLabel = new Gtk.Label();
			FileName = "New File";
			_tv.Buffer.Changed += TextBufferChanged;
//			_se.HierarchyChanged += TextBufferChanged;
//			TextBufferChanged(this, EventArgs.Empty);
			ShowAll();
		}
		
		public StaticCodePage(FileStream fs)
		{
			_tv = new TextView();
//			_se = new SourceEditorView();
			ScrolledWindow sw = new ScrolledWindow();
			sw.Add(_tv);
			AddWithViewport(sw);//_se););
//			Add((Widget)(_te = new TextEditor()));
			TabLabel = new Gtk.Label();
			FileName = fs.Name;
			byte[] buf = new byte[fs.Length];
			fs.Read(buf, 0, (int)fs.Length);
			Source = Encoding.ASCII.GetString(buf);
			_tv.Buffer.Changed += TextBufferChanged;
//			_se.TextChanged += TextBufferChanged;
			ShowAll();
		}

		/// <summary>
		/// Save the source code to the file referred to by <see cref="StaticCodePage.FileName"/>
		/// </summary>
		public void Save()
		{
			using (Stream s = File.Open(FileName, FileMode.Create))
				Save(s);
		}

		/// <summary>
		/// Save the source code to the specified <see cref="System.IO.Stream"/>
		/// </summary>
		/// <param name="s">The stream to write to</param>
		public void Save(Stream s)
		{
//			byte[] buf = Convert.FromBase64String(Source);
			byte[] buf = Encoding.ASCII.GetBytes(Source);
			s.Write(buf, 0, buf.Length);
			Unsaved = false;
//			if (TabLabel != null)
//			{
//				FileName = fs.Name;
//				if (TabLabel.Text.EndsWith(" *"))
//					TabLabel.Text = TabLabel.Text.Substring(0, TabLabel.Text.Length - 2);
//			}
		}
	}
}

