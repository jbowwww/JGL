using System;
using System.Collections.Generic;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using JGL.Debugging;

namespace JGL.Heirarchy
{
	/// <summary>
	/// Info regarding an openTK update cycle
	/// </summary>
	public class UpdateArgs
		: TimedEventArgs
	{
		/// <summary>
		/// Tracing
		/// </summary>
		public static readonly AutoTraceSource Trace = AutoTraceSource.GetOrCreate(AsyncXmlFileTraceListener.GetOrCreate("JGL"));

		/// <summary>
		/// Graphics context being rendered to (which should always be the current one according to GraphicsContext.CurrentContext)
		/// </summary>
		public IGraphicsContext Graphics;

		/// <summary>
		/// Initializes a new instance of the <see cref="JGL.Heirarchy.UpdateArgs"/> class.
		/// </summary>
		/// <param name='context'>
		/// Context.
		/// </param>
		/// <param name='updateRoot'>
		/// Update root.
		/// </param>
		public UpdateArgs()
		{

		}
	}
}

