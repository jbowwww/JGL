using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Threading;
using System.Diagnostics;
using OpenTK.Graphics.OpenGL;
using JGL.Extensions;
using JGL.OpenGL;
using JGL.Debugging;

namespace JGL.Heirarchy.Resources
{
	/// <summary>
	/// Texture <see cref="Resource"/>
	/// </summary>
	public class Texture : Resource
	{
		/// <summary>
		/// Tracing
		/// </summary>
		public static readonly AutoTraceSource Trace = AutoTraceSource.GetOrCreate(AsyncXmlFileTraceListener.GetOrCreate("JGL"));

		/// <summary>
		/// Gets or sets the <see cref="Texture"/> image
		/// </summary>
		public Bitmap Image { get; protected set; }

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
		/// <param name="path">Path to <see cref="Texture"/></param>
		public Texture(string path) : base(path)
		{
			Init(base.Name, path);
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="JGL.Resource.Texture"/> class.
		/// </summary>
		/// <param name="name"><see cref="Texture"/> <see cref="JGL.Heirarchy.Entity.Name"/></param>
		/// <param name="path"><see cref="Texture"/> path</param>
		public Texture(string name, string path) : base(name, path)
		{
			Init(name, path);
		}

		/// <summary>
		///  Init the instance using given name and uri.
		/// </summary>
		/// <param name="name"><see cref="Texture"/> <see cref="JGL.Heirarchy.Entity.Name"/></param>
		/// <param name="path"><see cref="Texture"/> path</param>
		private void Init(string name, string path)
		{
			Trace.Log(TraceEventType.Information, "Init(name=\"{0}\", path=\"{1}\")", name, path);
		}

		#region Implemented abstract members of JGL.Resource.Resource
		/// <summary>
		/// Loads a <see cref="Texture"/> <see cref="Resource"/>
		/// </summary>
		/// <returns><c>true</c> if successful, otherwise, <c>false</c></returns>
		/// <remarks>
		///	- Executed on <see cref="Resource.LoadThread"/>
		/// </remarks>
		public override void Load(Stream stream)
		{
			try
			{
//				using (stream = File.Open(string.IsNullOrEmpty(System.IO.Path.GetDirectoryName(Path)) ?
//				"../../../Data/Textures/" + Path : Path, FileMode.Open, FileAccess.Read, FileShare.Read))
//				{
				Image = new Bitmap(stream);
				//Bitmap.FromStream(s);
				Trace.Log(TraceEventType.Information, "Load (this.Name=\"{0}\", this.Path=\"{1}\") loaded {2}x{3} image, {4}",
					Name, Path, Image.Width, Image.Height, Image.PixelFormat);
//				}
			}
			catch (Exception ex)
			{
				throw new Exception(string.Format("Failed loading texture \"{0}\" from stream \"{1}\"",
					Name, stream == null ? "(null)" : stream.ToString()), ex);
			}
			finally
			{
				TextureId = new GLContextualData<Texture, int>(CreateGLTexture, this);
			}
		}

		/// <summary>
		/// Creates the GL texture - called once for each GL graphics context the texture is used on (tex ids are different per context - correct?)
		/// </summary>
		/// <returns>The GL texture id</returns>
		/// <param name="texture"><see cref="Texture"/></param>
		private static int CreateGLTexture(Texture texture)
		{
			if (!texture.IsLoaded)
				throw new InvalidDataException(string.Format(
					"Texture resource \"{0}\" failed to load and nothing noticed until it tried to use it!!", texture.Name));

			Bitmap img = texture.Image;
			int glid = GL.GenTexture();
			GL.BindTexture(TextureTarget.Texture2D, glid);

			BitmapData bd = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
			GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgb, bd.Width, bd.Height, 0, OpenTK.Graphics.OpenGL.PixelFormat.Bgr, PixelType.UnsignedByte, bd.Scan0);
			img.UnlockBits(bd);
 
			// We haven't uploaded mipmaps, so disable mipmapping (otherwise the texture will not appear).
			// On newer video cards, we can use GL.GenerateMipmaps() or GL.Ext.GenerateMipmaps() to create
			// mipmaps automatically. In that case, use TextureMinFilter.LinearMipmapLinear to enable them.
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
			GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);

			Trace.Log(TraceEventType.Information, "CreateGLTexture(texture=\"{0}\") generated GL texture ID {1}", texture.Id, glid);
			return glid;
		}
		#endregion
	}
}