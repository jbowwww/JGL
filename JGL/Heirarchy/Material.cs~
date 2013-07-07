using System;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using JGL.Resource;

namespace JGL.Heirarchy
{
	public class Material
	{
		#region Fields
		
		public readonly string Name;
		
		public Color4 Ambient = Color4.White;
		
		public Color4 Diffuse = Color4.White;
		
		public Color4 Specular = Color4.Black;
		
		public int SpecularExponent = 1;
		
		public double ReflectionSharpness = 60;
		
		public Color4 TransparencyFilter = Color4.White;
		
		public float Transparency {
			get { return TransparencyFilter.A;}
			set { TransparencyFilter.A = value; }
		}
		
		public bool TransparencyHalo = false;
		
		public double OpticalDensity = 1;
		
		public Texture TextureAmbient = null;
		
		public Texture TextureDiffuse = null;
		
		public Texture TextureSpecular = null;

		public bool HasTexture {
			get { return TextureAmbient != null || TextureDiffuse != null || TextureSpecular != null; }
		}

		#endregion
		
		public Material(string name)
		{
			Name = name;
		}
		
		/// <summary>
		/// Sets the material parameters using OpenGL
		/// </summary>
		/// <remarks>
		/// - May need a parameter in future (RenderArgs?) for making state information available
		/// </remarks>
		public void glSet()
		{
//			GL.
			//GL.Color4(Color4.White);
			GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Ambient, Ambient);
			GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Diffuse, Diffuse);
			GL.Material(MaterialFace.FrontAndBack, MaterialParameter.Specular, Specular);
//			GL.Disable(EnableCap.Texture2D);
//			GL.BindTexture(TextureTarget.Texture2D, 0);
//			GL.Color4()
		}
	}
}

