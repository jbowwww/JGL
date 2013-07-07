using System;
using OpenTK.Graphics.OpenGL;
using JGL.Heirarchy;

namespace Dynamic
{
	public class Origin : SimpleObject
	{
		public Origin()
			: base("Origin", (renderArgs) =>
				{
//					GL.Disable(EnableCap.Texture2D);
		GL.Disable(EnableCap.Lighting);
					//GL.Disable(EnableCap.DepthTest);
					GL.Disable(EnableCap.ClipPlane0);
					GL.Disable(EnableCap.CullFace);
					//GL.Disable(EnableCap.DepthClamp);
			GL.Enable(EnableCap.ColorMaterial);
					GL.LineWidth(1.8f);
					GL.Begin(BeginMode.Lines);
					GL.Color4(1.0, 0.8, 0.8, 0.7);
					GL.Vertex3(0, 0, 0);
					GL.Color4(1.0, 0.25, 0.25, 1);
					GL.Vertex3(1, 0, 0);
					GL.Color4(0.8, 1.0, 0.8, 0.7);
					GL.Vertex3(0, 0, 0);
					GL.Color4(0.25, 1.0, 0.25, 1);
					GL.Vertex3(0, 1, 0);
					GL.Color4(0.8, 0.8, 1.0, 0.7);
					GL.Vertex3(0, 0, 0);
					GL.Color4(0.25, 0.25, 1.0, 1);
					GL.Vertex3(0, 0, 1);
					GL.End();
					GL.Disable(EnableCap.ColorMaterial);
			GL.Enable(EnableCap.ClipPlane0);
//					GL.Enable(EnableCap.DepthClamp);
					GL.Enable(EnableCap.CullFace);
//					GL.Enable(EnableCap.DepthTest);
			GL.Enable(EnableCap.Lighting);
//			GL.Enable(EnableCap.Texture2D);
				})
		{
		}
	}
}

