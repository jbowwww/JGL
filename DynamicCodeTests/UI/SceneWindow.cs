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
		public SceneWindow(int w, int h, OpenTK.Graphics.GraphicsMode gm, Scene scene, string title = "SceneWindow")
			: base(w, h, gm, title, GameWindowFlags.Default, DisplayDevice.Default)
		{
			_scene = scene;
			camera = scene.DefaultCamera;
			frames = 0;
			markStart = DateTime.Now;

			this.RenderFrame += (sender, e) =>
			{
				MakeCurrent();
				RenderArgs ra = new RenderArgs(Context, _scene, Width, Height);
				SceneInfoPanel.UpdateEventArgs uArgs = new SceneInfoPanel.UpdateEventArgs() { Camera = camera, Scene = scene, RenderArgs = ra };
				_scene.Render(ra);
				SwapBuffers();
				markEnd = DateTime.Now;
				duration = markEnd - markStart;
				frames++;
				if (duration.TotalSeconds >= 1)
				{
					uArgs.FrameRate = (float)frames * 1000 / (float)duration.TotalMilliseconds;
					if (InfoPanel != null)
						Gtk.Application.Invoke(this, uArgs, InfoPanel.Update);
					markStart = markEnd;
					frames = 0;
				}
			};
			
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

