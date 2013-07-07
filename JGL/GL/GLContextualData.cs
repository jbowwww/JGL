using System;
using System.Collections;
using System.Collections.Generic;

using OpenTK.Graphics;

using JGL.Debugging;

namespace JGL.OpenGL
{
	/// <summary>
	/// GL contextual.
	/// </summary>
	public class GLContextualData<TContainer, TData>
	{
		public delegate TData GLContextualDataCreationHandler(TContainer container);
		
		private GLContextualDataCreationHandler _creationHandler;
		
		private TContainer _container;
		
		private readonly Dictionary<IGraphicsContext, TData> _contextData = new Dictionary<IGraphicsContext, TData>();

		public TData Data {
			get
			{
				IGraphicsContext gc = GraphicsContext.CurrentContext;
				return _contextData.ContainsKey(gc) ?
					_contextData[gc] : _contextData[gc] = _creationHandler(_container);
			}
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="JGL.GL.GLContextualData"/> class.
		/// </summary>
		public GLContextualData(GLContextualDataCreationHandler creationHandler, TContainer container)
		{
			Debug.Assert(creationHandler != null && container != null);
			_creationHandler = creationHandler;
			_container = container;
		}
		
		public static implicit operator TData(GLContextualData<TContainer, TData> ct)
		{
			return ct.Data;
		}
	}
}

