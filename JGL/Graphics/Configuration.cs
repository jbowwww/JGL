using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using JGL.Heirarchy;

namespace JGL.Graphics
{
	public static class Configuration
	{
		public static void Init(int width, int height)
		{
			GL.Enable(EnableCap.LineSmooth);
			GL.ShadeModel(ShadingModel.Smooth);

			GL.Enable(EnableCap.DepthTest);
			GL.DepthFunc(DepthFunction.Lequal);

			GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.DstAlpha);
			GL.BlendEquation(BlendEquationMode.FuncAdd);

			GL.CullFace(CullFaceMode.Back);

			GL.Enable(EnableCap.Lighting);
			GL.LightModel(LightModelParameter.LightModelAmbient, new float[] { 0.22f, 0.22f, 0.22f, 1f });
			GL.LightModel(LightModelParameter.LightModelLocalViewer, 0.5f);
//			for (int lightIndex = 0; lightIndex < 8; lightIndex++)		// disable all lights by default before each frame rendering,
//				GL.Disable(EnableCap.Light0 + lightIndex);					// let the JoGL.Heirarchy.Light entity re-enable as necessary
			
			GL.Viewport(0, 0, width, height);
			GL.MatrixMode(MatrixMode.Projection);
			Matrix4d m = Matrix4d.CreatePerspectiveFieldOfView(MathHelper.PiOver4, width / height, 0.1d, 500d);
			GL.LoadMatrix(ref m);
			GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);
			
			GL.MatrixMode(MatrixMode.Modelview);
			GL.LoadIdentity();
			GL.Clear(ClearBufferMask.AccumBufferBit | ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			
		}
	}
}

