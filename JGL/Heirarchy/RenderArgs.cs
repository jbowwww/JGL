using System;
using System.Collections.Generic;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using JGL.Debugging;

namespace JGL.Heirarchy
{
	/// <summary>
	/// Contains info relevant to classes' implementations of <see cref="JGL.Heirarchy.IRenderable.Render"/>
	/// </summary>
	public class RenderArgs
	{
		/// <summary>
		/// Tracing
		/// </summary>
		public static readonly AutoTraceSource Trace = AutoTraceSource.GetOrCreate(AsyncFileTraceListener.GetOrCreate("JGL"));

		/// <summary>
		/// Graphics context being rendered to (which should always be the current one according to GraphicsContext.CurrentContext)
		/// </summary>
		public readonly IGraphicsContext Graphics;

		/// <summary>
		/// Window width
		/// </summary>
		public readonly int Width;

		/// <summary>
		/// Window height
		/// </summary>
		public readonly int Height;

		public readonly DateTime CreationTime;

		public uint TriangleCount = 0;

		public DateTime StartTime { get; protected set; }

		public DateTime StopTime { get; protected set; }

		public TimeSpan Duration { get; protected set; }

		/// <summary>
		/// The current <see cref="JGL.Heirarchy.Context"/> being rendered
		/// </summary>
		public EntityContext Entity {
			get
			{
				Debug.Assert(Entities.Count >= 1);
				return Entities.Peek();
			}
		}

		/// <summary>
		/// The stack of <see cref="JGL.Heirarchy.Context"/>s as rendered so far
		/// </summary>
		public readonly Stack<EntityContext> Entities;
		
		/// <summary>
		/// Gets a value indicating whether the entity context stack <see cref="JGL.Heirarchy.RenderArgs.Entities"/> is empty
		/// </summary>
		public bool IsEntityContextStackEmpty {
			get { return Entities.Count == 0; }
		}
		
		/// <summary>
		/// The current GL light. When <see cref="JGL.Heirarchy.Light.Render"/> is called, the method increments this value.
		/// </summary>
		public LightName CurrentLightName = LightName.Light0;
		
		// TODO: Will need to add more members as you go
		
		/// <summary>
		/// Initializes a new instance of the <see cref="JGL.Heirarchy.RenderArgs"/> class.
		/// </summary>
		/// <param name='graphicsContext'>Graphics context.</param>
		/// <param name='entityContext'>Entity context.</param>
		public RenderArgs(IGraphicsContext graphicsContext, EntityContext entityContext, int width, int height)
		{
			CreationTime = DateTime.Now;
			Graphics = graphicsContext;
			Entities = new Stack<EntityContext>(new EntityContext[] { entityContext });
			Width = width;
			Height = height;
		}

		/// <summary>
		/// Marks the start time of a render frame
		/// </summary>
		public void Start()
		{
			StartTime = DateTime.Now;
		}

		/// <summary>
		/// Marks the stop time of a render frame
		/// </summary>
		public void Stop()
		{
			StopTime = DateTime.Now;
			Duration = StartTime - StopTime;
		}
	}
}

