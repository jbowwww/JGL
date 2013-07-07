using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using JGL.Extensions;
using JGL.OpenGL;
using OpenTK.Graphics.OpenGL;

namespace JGL.Resource
{
	/// <summary>
	/// Texture <see cref="Resource"/>
	/// </summary>
	public class Texture : Resource
	{
		/// <summary>
		/// Gets or sets the <see cref="Texture"/> image
		/// </summary>
		public Image Image { get; protected set; }

		/// <summary>
		/// Gets the width.
		/// </summary>
		public int Width { get { return Image.Width; } }

		/// <summary>
		/// Gets the height.
		/// </summary>
		public int Height { get { return Image.Height; } }

		/// <summary>
		/// The OpenGL texture id, stored for each GL context it has been used on
		/// </summary>
		public GLContextualData<Texture, int> TextureId { get; protected set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="JGL.Resource.Texture"/> class.
		/// </summary>
		/// <param name="uri"><see cref="Texture"/> <see cref="Uri"/></param>
		public Texture(Uri uri)
			: base(uri)
		{
			Init(null, uri);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="JGL.Resource.Texture"/> class.
		/// </summary>
		/// <param name="name"><see cref="Texture"/> <see cref="JGL.Heirarchy.Entity.Name"/></param>
		/// <param name="uri"><see cref="Texture"/> <see cref="Uri"/></param>
		public Texture(string name, Uri uri)
			: base(name, uri)
		{
			Init(name, uri);
		}

		/// <summary>
		///  Init the instance using given name and uri.
		/// </summary>
		/// <param name="name"><see cref="Texture"/> <see cref="JGL.Heirarchy.Entity.Name"/></param>
		/// <param name="uri"><see cref="Texture"/> <see cref="Uri"/></param>
		private void Init(string name, Uri uri)
		{

		}

		#region Implemented abstract members of JGL.Resource.Resource
		/// <summary>
		/// Loads a <see cref="Texture"/> <see cref="Resource"/>
		/// </summary>
		/// <returns><c>true</c> if successful, otherwise, <c>false</c></returns>
		/// <remarks>
		///	- Executed on <see cref="Resource.LoadThread"/>
		/// </remarks>
		public override void Load()
		{
			Stream s = null;
			try
			{
				using (s = URI.Open())
				{
					Image = Image.FromStream(s);
					TextureId = new GLContextualData<Texture, int>(CreateGLTexture, this);
				}
			}
			catch (Exception ex)
			{
				throw new Exception(string.Format("Failed loading texture \"{0}\" from stream \"{1}\"", Name, s == null ? "(null)" : s.ToString()), ex);
			}
		}

		/// <summary>
		/// Creates the GL texture - called once for each GL graphics context the texture is used on (tex ids are different per context - correct?)
		/// </summary>
		/// <returns>The GL texture id</returns>
		/// <param name="texture"><see cref="Texture"/></param>
		private static int CreateGLTexture(Texture texture)
		{
			int glid = GL.GenTexture();
			// how to get pixel data out of this.Image??
//			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, Width, Height, 0, Image.PixelFormat, PixelType.Bitmap, new BitmapData());
			return glid;
		}
		#endregion
	}
}