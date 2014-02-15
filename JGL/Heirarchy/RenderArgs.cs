using System;
using System.Collections.Generic;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using JGL.Heirarchy;
using JGL.Debugging;

namespace JGL.Graphics
{
	/// <summary>
	/// Contains info relevant to classes' implementations of <see cref="JGL.Heirarchy.IRenderable.Render"/>
	/// </summary>
	/// <remarks>Inherits from <see cref="System.EventArgs"/> so that it can be passed to event handlers</remarks>
	public class RenderArgs : EventArgs
	{
		/// <summary>
		/// Tracing
		/// </summary>
		public static readonly AutoTraceSource Trace = AutoTraceSource.GetOrCreate(AsyncXmlFileTraceListener.GetOrCreate("JGL"));

		public int Width, Height;

		public SceneWindow SceneWindow { get; private set; }

		/// <summary>
		/// The <see cref="JGL.Heirarchy.Scene"/> for this <see cref="SceneWindow"/> to render
		/// </summary>
		public Scene Scene { get; private set; }

		/// <summary>
		/// The <see cref="JGL.Heirarchy.Scene"/>'s currently active <see cref="JGL.Heirarchy.Camera"/> to use for rendering
		/// </summary>
		public Camera Camera { get { return Scene.DefaultCamera; } }

		/// <summary>
		/// The current GL light. When <see cref="JGL.Heirarchy.Light.Render"/> is called, the method increments this value.
		/// </summary>
		public LightName CurrentLightName = LightName.Light0;

		/// <summary>
		/// Gets the current <see cref="Entity"/> being operated on (e.g. being rendered in the case of <see cref="RenderArgs"/>
		/// </summary>
		public Entity CurrentEntity {
			get { return Entities == null || Entities.Count == 0 ? null : Entities.Peek(); }
		}

		/// <summary>
		/// The stack of <see cref="JGL.Heirarchy.Context"/>s as rendered so far
		/// </summary>
		public readonly Stack<Entity> Entities = new Stack<Entity>();

		/// <summary>
		/// Number of triangles rendered in the previous frame - public field to be set by rendering algorithm
		/// </summary>
		public uint FrameTriangleCount { get; set; }

		/// <summary>
		/// The number of triangles in the last render cycle
		/// </summary>
		public uint TriangleCount { get; private set; }

		/// <summary>
		/// The frame count.
		/// </summary>
		public uint FrameCount { get; private set; }

		/// <summary>
		/// Average number of triangles drawn per second
		/// </summary>
		public float TrianglesPerSecond { get; private set; }

		/// <summary>
		/// Average number of triangles per frame
		/// </summary>
		public float TrianglesPerFrame { get; private set; }

		/// <summary>
		/// Average frames per second
		public float FramesPerSecond { get; private set; }

		/// <summary>
		/// A marked time, usually when the event last occurred. To update this call <see cref="Mark"/>.
		/// </summary>
		public DateTime MarkTime { get; private set; }

		/// <summary>
		/// Gets the time elapsed since <see cref="MarkTime"/>
		/// </summary>
		public TimeSpan TimeElapsed {
			get { return MarkTime == default(DateTime) ? TimeSpan.Zero : DateTime.Now - MarkTime; }
		}

		/// <summary>
		/// The <see cref="TimeSpan"/> between the two most recent consecutive events (ie when <see cref="Reset"/> is called(
		/// </summary>
		public TimeSpan LastDuration { get; private set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="JGL.Heirarchy.RenderArgs"/> class.
		/// </summary>
		/// <param name="scene">The <see cref="JGL.Hierarchy.Scene"/> to be rendered in this window</param>
		public RenderArgs(Scene scene, SceneWindow sceneWindow)
		{
			this.Scene = scene;
			this.SceneWindow = sceneWindow;
		}

		/// <summary>
		/// Override of <see cref="Update"/> updates frame counters etc, only calculates frame rate once per second at most
		/// </summary>
		public void Update()
		{
			LastDuration = TimeElapsed;
			MarkTime = DateTime.Now;
			FramesPerSecond = (float)FrameCount * 1000 / (float)LastDuration.TotalMilliseconds;
			TrianglesPerSecond = (float)TriangleCount * 1000 / (float)LastDuration.TotalMilliseconds;
			TrianglesPerFrame = (float)TriangleCount * 1000 / (float)FrameCount;
			TriangleCount = 0;
			FrameCount = 0;
		}

		/// <summary>
		/// Reset the triangle count, record the timespan since the last mark time and mark it again
		/// </summary>s
		/// <remarks>This should get called before every rendering</remarks>
		public void PrepareFrame()
		{
			if (MarkTime == default(DateTime))
				MarkTime = DateTime.Now;
			FrameCount++;
			TriangleCount += FrameTriangleCount;
			FrameTriangleCount = 0;
			CurrentLightName = LightName.Light0;

		}
	}
}

