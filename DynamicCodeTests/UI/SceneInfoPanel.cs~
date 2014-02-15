using System;
using Gtk;
using JGL.Graphics;
using JGL.Heirarchy;

namespace Dynamic.UI
{
	public class SceneInfoPanel : VBox
	{
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

		public void Update(object sender, EventArgs eArgs)		// Scene scene, Camera camera)
		{
			RenderArgs renderArgs = (RenderArgs)eArgs;
			lblPosition.Text = renderArgs.Camera.Position.ToString();
			lblRotation.Text = renderArgs.Camera.Rotation.ToString();
			lblChildren.Text = renderArgs.Scene.Count.ToString();
			lblRender.Text = string.Format(
				"Frame Rate: {0}\nLast Frame: {1} triangles\nAverage Frame: {2} triangles\nTriangle Rate: {3} triangles/second",
				renderArgs.FramesPerSecond, renderArgs.FrameTriangleCount, renderArgs.TrianglesPerFrame, renderArgs.TrianglesPerSecond);
		}
	}
}

//		public void Update(object sender, RenderArgs renderArgs)// UpdateArgs updateArgs)		// Scene scene, Camera camera)
//		{
//			lblPosition.Text = renderArgs.OwnerScene.DefaultCamera.Position.ToString();
//			lblRotation.Text = renderArgs.OwnerScene.DefaultCamera.Rotation.ToString();
//			lblChildren.Text = renderArgs.OwnerScene.Count.ToString();
//			lblRender.Text = string.Format("Frame Rate: {0}\nLast Frame: {1} triangles", renderArgs.FrameRate, renderArgs.TriangleCount);
//		}

//			lblPosition.Text = updateArgs.OwnerScene.DefaultCameraFrame: {1} triangles", updateArgs.FrameRate, uArgs.RenderArgs.TriangleCount);
//.Position.ToString();
//			lblRotation.Text = updateArgs.OwnerScene.DefaultCamera.Rotation.ToString();
//			lblChildren.Text = updateArgs.OwnerScene.Count.ToString();
//
//			lblRender.Text = string.Format("Frame Rate: {0}\nLast
