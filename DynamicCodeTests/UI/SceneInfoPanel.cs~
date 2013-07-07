using System;
using Gtk;
using JGL.Heirarchy;

namespace Dynamic.UI
{
	public class SceneInfoPanel : VBox
	{
		internal class UpdateEventArgs : EventArgs
		{
			public Scene Scene;
			public Camera Camera;
			public RenderArgs RenderArgs;
			public float FrameRate;
		}

		/// <summary>
		/// The lbl position.
		/// </summary>
		protected Label lblPosition;

		protected Label lblRotation;

		protected Label lblChildren;

		protected Label lblRender;

		public Widget Widget { get { return lblPosition.Parent; } }
		public SceneInfoPanel()
		{
			PackStart(lblPosition = new Label());
			PackStart(lblRotation = new Label());
			PackStart(lblChildren = new Label());
			PackStart(lblRender = new Label());

			ShowAll();
		}

		public void Update(object sender, EventArgs args)		// Scene scene, Camera camera)
		{
			UpdateEventArgs uArgs = (UpdateEventArgs)args;
			lblPosition.Text = uArgs.Camera.Position.ToString();
			lblRotation.Text = uArgs.Camera.Rotation.ToString();
			lblChildren.Text = uArgs.Scene.Count.ToString();

			lblRender.Text = string.Format("Frame Rate: {0}\nLast Frame: {1} triangles", uArgs.FrameRate, uArgs.RenderArgs.TriangleCount);
		}
	}
}

