using System;

using OpenTK;
using OpenTK.Graphics;

using JGL;
using JGL.Heirarchy;

namespace JGL.Graphics
{
	/// <summary>
	/// Scene window
	/// </summary>
	public class SceneWindow : GameWindow
	{
		public Scene Scene {
			get { return RenderArgs.Scene; }
			set { RenderArgs = new RenderArgs(value, this); }
		}

		public RenderArgs RenderArgs { get; private set; }

		public delegate void SceneWindowFrameHandler(object sender, RenderArgs renderArgs);
		public event SceneWindowFrameHandler UpdateHandler;
		public event SceneWindowFrameHandler RenderHandler;

		/// <summary>
		/// Initializes a new instance of the <see cref="DynamicCodeTests.SceneWindow"/> class.
		/// </summary>
		/// <param name="w">Width</param>
		/// <param name="h">Height</param>
		/// <param name="gm">Graphics mode</param>
		/// <param name="title">Window title</param>
		public SceneWindow(int w, int h, GraphicsMode gm, string title = "SceneWindow")
			: base(w, h, gm, title, GameWindowFlags.Default, DisplayDevice.Default)
		{
//			object updateRenderLock = new object();

//			base.UpdateFrame += (object sender, FrameEventArgs e) =>
//			{
////				lock (updateRenderLock)
////				{
//				RenderArgs.Update();
//				if (UpdateHandler != null)
//					UpdateHandler(this, RenderArgs);
////				}
//			};

			base.RenderFrame += (object sender, FrameEventArgs e) =>
			{
//				lock (updateRenderLock)
//				{
				MakeCurrent();
				Configuration.Init(Width, Height);
				RenderArgs.PrepareFrame();
				Scene.ProcessBehaviours(RenderArgs);
				RenderArgs.Camera.Render(RenderArgs);
				SwapBuffers();

				if (RenderHandler != null)
					RenderHandler(this, RenderArgs);
				if (RenderArgs.TimeElapsed.TotalSeconds >= 2)
				{
					RenderArgs.Update();
					if (UpdateHandler != null)
						UpdateHandler(this, RenderArgs);
				}
//				}
			};

			this.KeyPress += (object sender, KeyPressEventArgs e) =>
			{
				switch(e.KeyChar)
				{
					case 'a':
						Scene.DefaultCamera.Rotation += new Vector3d(0, 0.8f, 0);
						break;
					case 'd':
						Scene.DefaultCamera.Rotation -= new Vector3d(0, 0.8f, 0);
						break;
					case 'w':
						Scene.DefaultCamera.Rotation += new Vector3d(0.7f, 0, 0);
						break;
					case 's':
						Scene.DefaultCamera.Rotation -= new Vector3d(0.7f, 0, 0);
						break;
					case '\'':
						Scene.DefaultCamera.MoveForward(0.16);
						break;
					case '/':
						Scene.DefaultCamera.MoveBackward(0.16);
						break;
					default:
						break;
				}
			};
		}
	}
}

