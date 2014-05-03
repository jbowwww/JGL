using System;

namespace JGL.Heirarchy
{
	public class RenderableObjectProxy : Object, IRenderable
	{
		public IRenderable Renderable;

		public RenderableObjectProxy(IRenderable renderable)
		{
			Renderable = renderable;
		}

		#region IRenderable implementation
		public void Render(JGL.Graphics.RenderArgs renderArgs)
		{
			if (Renderable != null)
				Renderable.Render(renderArgs);
		}
		#endregion
	}
}

