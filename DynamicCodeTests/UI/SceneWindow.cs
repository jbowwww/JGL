using System;
using OpenTK;
using JGL.Heirarchy;

namespace Dynamic.UI
{
	/// <summary>
	/// Scene window
	/// </summary>
	public class SceneWindow : GameWindow
	{
		private Scene _scene = null;

		public SceneInfoPanel InfoPanel;
		
		Camera camera;

		private int frames;
		private DateTime markStart;
		private DateTime markEnd;
		private TimeSpan duration;
		private DateTime markUpdateStart;
		private DateTime markUpdateEnd;
		private TimeSpan durationUpdate;

		private UpdateArgs _updateArgs;
		private RenderArgs _renderArgs;

		/// <summary>
		/// Initializes a new instance of the <see cref="DynamicCodeTests.SceneWindow"/> class.
		/// </summary>
		/// <param name='w'>
		/// W.
		/// </param>
		/// <param name='h'>
		/// H.
		/// </param>
		/// <param name='gm'>
		/// Gm.
		/// </param>
		/// <param name='scene'>
		/// Scene.
		/// </param>
		/// <param name='title'>
		/// Title.
		/// </param>
		/// <remarks>
		///	-	TODO: Could <see cref="UpdateArgs"/> and <see cref="RenderArgs"/> be combined into one class and one instance
		///		to use with both <see cref="UpdateFrame"/> and <see cref="RenderFrame"/>
		///			- Maybe not, if you want <see cref="RenderArgs"/> to update framerate only once a second or some other arbitrary slow interval
		/// </remarks>
		public SceneWindow(int w, int h, OpenTK.Graphics.GraphicsMode gm, Scene scene, string title = "SceneWindow")
			: base(w, h, gm, title, GameWindowFlags.Default, DisplayDevice.Default)
		{
			_scene = scene;
			camera = scene.DefaultCamera;
//			frames = 0;
//			markStart = markUpdateStart = DateTime.Now;

			_updateArgs = new UpdateArgs();
			_renderArgs = new RenderArgs(true) { Graphics = Context, Width = Width, Height = Height };

			this.UpdateFrame += (sender, e) =>
			{
				_updateArgs.UpdateRoot = _scene;
				_updateArgs.Graphics = Context;
				_updateArgs.Update();
				if (InfoPanel != null)
				{
					SceneInfoPanel.UpdateEventArgs uArgs = new SceneInfoPanel.UpdateEventArgs()
						{ Camera = camera, Scene = scene, RenderArgs = _renderArgs, FrameRate = _renderArgs.FrameRate };
					Gtk.Application.Invoke(this, uArgs, InfoPanel.Update);
				}

			};

			this.RenderFrame += (sender, e) =>
			{
				//RenderArgs ra = new RenderArgs(Context, _scene, Width, Height);
				_renderArgs.UpdateRoot = _scene;
				_renderArgs.Graphics = Context;
				_renderArgs.Width = Width;
				_renderArgs.Height = Height;
				_renderArgs.Update();
				MakeCurrent();
				_scene.Render(_renderArgs);
				SwapBuffers();
			};
//				frames++;
//				_renderArgs.FrameCount++;
//				if (_renderArgs.TimeElapsed.TotalSeconds >= 1)
//				{
//					_renderArgs.Mark();
//					uArgs.FrameRate = (float)_renderArgs.FrameCount * 1000 / (float)_renderArgs.LastDuration.TotalMilliseconds;
//					if (InfoPanel != null)
//						Gtk.Application.Invoke(this, uArgs, InfoPanel.Update);
//					_renderArgs.FrameCount = 0;
////					markStart = markEnd;
////					frames = 0;
//				}

			this.KeyPress += (object sender, KeyPressEventArgs e) =>
			{
				switch(e.KeyChar)
				{
					case 'a':
						scene.DefaultCamera.Rotation += new Vector3d(0, 0.8f, 0);
						break;
					case 'd':
						scene.DefaultCamera.Rotation -= new Vector3d(0, 0.8f, 0);
						break;
					case 'w':
						scene.DefaultCamera.Rotation += new Vector3d(0.7f, 0, 0);
						break;
					case 's':
						scene.DefaultCamera.Rotation -= new Vector3d(0.7f, 0, 0);
						break;
					case '\'':
						scene.DefaultCamera.MoveForward(0.16);
						break;
					case '/':
						scene.DefaultCamera.MoveBackward(0.16);
						break;
					default:
						break;
				}
			};
		}
	}
}

