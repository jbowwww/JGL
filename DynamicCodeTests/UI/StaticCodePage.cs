using System;
using System.Text;
using System.IO;
using System.Runtime;
using System.Diagnostics;
using Gtk;
using JGL.Debugging;

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
		/// <summary>
		/// The trace.
		/// </summary>
		public static readonly AutoTraceSource Trace = AutoTraceSource.GetOrCreate(AsyncTextFileTraceListener.GetOrCreate("JGLApp"));

		private TextView _tv;

		/// <summary>
		/// Gets or sets the source.
		/// </summary>
		public string Source {
			get { return _tv.Buffer.Text; }
			set { _tv.Buffer.Text = value; }
		}

		private bool _unsaved = false;

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="Dynamic.UI.StaticCodePage"/> is unsaved.
		/// </summary>
		public bool Unsaved {
			get { return _unsaved; }
			private set
			{
				if (TabLabel != null)
				{
					if (_unsaved == false && value == true)
						TabLabel.Text += " *";
					else if (_unsaved == true && value == false)
						TabLabel.Text = TabLabel.Text.Substring(0, TabLabel.Text.Length - 2);
				}
				_unsaved = value;
			}
		}

		/// <summary>
		/// Gets or sets the tab label.
		/// </summary>
		public Gtk.Label TabLabel { get; private set; }
		
		private string _fn;

		/// <summary>
		/// Gets or sets the name of the file.
		/// </summary>
		public string FileName {
			get { return _fn; }
			set
			{
				_fn = value;
				if (TabLabel != null)
					TabLabel.Text = System.IO.Path.GetFileName(value);
			}
		}

		/// <summary>
		/// Texts the buffer changed.
		/// </summary>
		private void TextBufferChanged(object sender, EventArgs e)
		{
			Trace.Log(TraceEventType.Verbose, "TextBufferChanged");
			if (!Unsaved)
				Unsaved = true;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Dynamic.UI.StaticCodePage"/> class.
		/// </summary>
		public StaticCodePage()
		{
			Init();
			_tv.Buffer.Changed += TextBufferChanged;
			ShowAll();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Dynamic.UI.StaticCodePage"/> class.
		/// </summary>
		/// <param name="fs"><see cref="FileStream"/> to load source from</param>
		public StaticCodePage(FileStream fs)
		{
			Init(fs.Name);
			byte[] buf = new byte[fs.Length];
			fs.Read(buf, 0, (int)fs.Length);
			Source = Encoding.ASCII.GetString(buf);
			_tv.Buffer.Changed += TextBufferChanged;
			ShowAll();
		}

		/// <summary>
		/// Init the specified filename.
		/// </summary>
		/// <param name="filename">File name</param>
		private void Init(string filename = "New File")
		{
			Trace.Log(TraceEventType.Information, "Init(filename=\"{0}\")", filename);
			_tv = new TextView();
			ScrolledWindow sw = new ScrolledWindow();
			sw.Add(_tv);
			AddWithViewport(sw);
			TabLabel = new Gtk.Label();
			FileName = filename;
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
		/// <param name="stream">The stream to write to</param>
		public void Save(Stream stream)
		{
			Trace.Log(TraceEventType.Information, "Save(stream=\"{0}\")", stream.ToString());
			byte[] buf = Encoding.ASCII.GetBytes(Source);
			stream.Write(buf, 0, buf.Length);
			Unsaved = false;
		}
	}
}

