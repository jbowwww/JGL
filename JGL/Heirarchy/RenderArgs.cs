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
		: TimedEventArgs
	{
		/// <summary>
		/// Tracing
		/// </summary>
		public static readonly AutoTraceSource Trace = AutoTraceSource.GetOrCreate(AsyncXmlFileTraceListener.GetOrCreate("JGL"));

		/// <summary>
		/// Graphics context being rendered to (which should always be the current one according to GraphicsContext.CurrentContext)
		/// </summary>
		public IGraphicsContext Graphics { get; set; }

		/// <summary>
		/// Window width
		/// </summary>
		public int Width { get; set; }

		/// <summary>
		/// Window height
		/// </summary>
		public int Height { get; set; }

		/// <summary>
		/// The frame count.
		/// </summary>
		public int FrameCount = 0;

		public float FrameRate { get; private set; }

		public DateTime FrameCountMarkTime { get; private set; }

		public TimeSpan FrameCountTimeElapsed {
			get { return DateTime.Now - FrameCountMarkTime; }
		}

		/// <summary>
		/// The number of triangles in the last render cycle
		/// </summary>
		public uint TriangleCount = 0;

		/// <summary>
		/// The number of triangles drawn since <see cref="FrameCountMarkTime"/> was updated.
		/// Used to calculate <see cref="TriangleRate"/>
		/// </summary>
		protected uint TriangleTally = 0;

		/// <summary>
		/// Average number of triangles drawn per second
		/// </summary>
		public float TriangleRate { get; private set; }

		/// <summary>
		/// The current GL light. When <see cref="JGL.Heirarchy.Light.Render"/> is called, the method increments this value.
		/// </summary>
		public LightName CurrentLightName = LightName.Light0;
		
		/// <summary>
		/// Initializes a new instance of the <see cref="JGL.Heirarchy.RenderArgs"/> class.
		/// </summary>
		/// <param name="mark">Whether to call <see cref="TimedEventArgs.Mark"/>. Default is false.</param>
		public RenderArgs(bool mark = false)
			: base(mark)
		{
			FrameCountMarkTime = DateTime.MinValue;
		}

		/// <summary>
		/// Override of <see cref="Update"/> updates frame counters etc, only calculates frame rate once per second at most
		/// </summary>
		public override void Update()
		{
			CurrentLightName = LightName.Light0;
			FrameCount++;
			TriangleTally += TriangleCount;
			TriangleCount = 0;
			if (FrameCountTimeElapsed.TotalSeconds >= 1)
			{
				TimeSpan elapsed = FrameCountTimeElapsed;
				FrameCountMarkTime = DateTime.Now;
				FrameRate = (float)FrameCount * 1000 / (float)elapsed.TotalMilliseconds;
				FrameCount = 0;
				TriangleRate = (float)TriangleTally * 1000 / (float)elapsed.TotalMilliseconds;
				TriangleTally = 0;
			}
			base.Update();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="JGL.Heirarchy.RenderArgs"/> class.
		/// </summary>
		/// <param name='graphicsContext'>Graphics context.</param>
		/// <param name='entityContext'>Entity context.</param>
//		public RenderArgs(IGraphicsContext graphicsContext, EntityContext entityContext, int width, int height)
//		{
//			CreationTime = DateTime.Now;
//			Graphics = graphicsContext;
//			UpdateRoot = entityContext;
////			Entities = new Stack<EntityContext>(new EntityContext[] { entityContext });
//			Width = width;
//			Height = height;
//		}
	}
}

