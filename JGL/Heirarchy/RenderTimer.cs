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
		public static readonly AutoTraceSource Trace = AutoTraceSource.GetOrCreate(AsyncXmlFileTraceListener.GetOrCreate("JGL"));

		/// <summary>
		/// The number of triangles drawn since <see cref="FrameCountMarkTime"/> was updated.
		/// Used to calculate <see cref="TriangleRate"/>
		/// </summary>
		protected uint TriangleTally = 0;

		/// <summary>
		/// Override of <see cref="Update"/> updates frame counters etc, only calculates frame rate once per second at most
		/// </summary>
		public override void Update()
		{
			CurrentLightName = LightName.Light0;
			FrameCount++;
			TriangleTally += TriangleCount;
			LastFrameTriangleCount = TriangleCount;
			TriangleCount = 0;
			if (TimeElapsed.TotalSeconds >= 1)
			{
				base.Update();
				FramesPerSecond = (float)FrameCount * 1000 / (float)LastDuration.TotalMilliseconds;
				TrianglesPerSecond = (float)TriangleTally * 1000 / (float)LastDuration.TotalMilliseconds;
				TrianglesPerFrame = (float)TriangleTally * 1000 / (float)FrameCount;
				TriangleTally = 0;
				FrameCount = 0;
			}
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

