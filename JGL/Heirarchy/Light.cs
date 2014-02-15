using System;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using JGL.Graphics;
using JGL.Debugging;

namespace JGL.Heirarchy
{
	public class Light : Entity, IPositionable, IRotatable, IRenderable
	{
		/// <summary>
		/// Tracing
		/// </summary>
		public static readonly AutoTraceSource Trace = AutoTraceSource.GetOrCreate(AsyncXmlFileTraceListener.GetOrCreate("JGL"));

		/// <summary>
		/// Enable/disable the light
		/// </summary>
		public bool Enabled = true;
		
		/// <summary>
		/// Light's position
		/// </summary>
		/// <remarks><see cref="JGL.Heirarchy.IPositionable"/> implementation</remarks>
		public Vector3d Position { get; set; }
		
		/// <summary>
		/// Light's rotation
		/// </summary>
		/// <remarks><see cref="JGL.Heirarchy.IRotatable"/> implementation</remarks>
		public Vector3d Rotation { get; set; }

		/// <summary>
		/// Gets or sets the object orientation (linked to <see cref="Object.Rotation"/>
		/// </summary>
		public Vector3d Orientation {
			get
			{
				Vector3d orientation = Vector3d.Transform(-Vector3d.UnitZ,
				Matrix4d.CreateRotationX(MathHelper.DegreesToRadians((float)Rotation.X)));
				orientation = Vector3d.Transform(orientation, Matrix4d.CreateRotationY(MathHelper.DegreesToRadians((float)Rotation.Y)));
				orientation = Vector3d.Transform(orientation, Matrix4d.CreateRotationZ(MathHelper.DegreesToRadians((float)Rotation.Z)));
				return orientation;
			}
			set
			{
				throw new NotImplementedException("TODO");
			}
		}

		/// <summary>
		/// Ambient light color.
		/// </summary>
		public Color4 Ambient = new Color4(0.1f, 0.1f, 0.1f, 1f);

		/// <summary>
		/// Ambient light color.
		/// </summary>
		public Color4 Diffuse = new Color4(0.4f, 0.4f, 0.4f, 1f);

		/// <summary>
		/// Ambient light color.
		/// </summary>
		public Color4 Specular = new Color4(0.8f, 0.8f, 0.8f, 1f);
		
		public float ConstantAttenuation = 1f;
		
		public float LinearAttenuation = 0f;
		
		public float QuadraticAttenuation = 0f;
		
		public float SpotCutoff = 180f;
		
		public float SpotExponent = 0;
		
		public Vector4 SpotDirection = new Vector4(0, 0, 1, 1);
		
		/// <summary>
		/// Initializes a new instance of the <see cref="JGL.Heirarchy.Light"/> class.
		/// </summary>
		public Light()
		{
			
		}

		/// <summary>
		/// Renders the light
		/// </summary>
		/// <param name="renderArgs">Render arguments.</param>
		/// <remarks><see cref="JGL.Heirarchy.IRenderable"/> implementation</remarks>
		public void Render(RenderArgs renderArgs)
		{
			LightName lightName = renderArgs.CurrentLightName++;
//			GL.Light(lightName, LightParameter.Position, Position);
			GL.Light(lightName, LightParameter.Position, Vector4.UnitW);	// Because this implements IPositionable, the modelview matrix should already locate the light correctly, so specify 0,0,0,1 (confirm this theory)
			GL.Light(lightName, LightParameter.Ambient, Ambient);
			GL.Light(lightName, LightParameter.Diffuse, Diffuse);
			GL.Light(lightName, LightParameter.Specular, Specular);
			GL.Light(lightName, LightParameter.ConstantAttenuation, ConstantAttenuation);
			GL.Light(lightName, LightParameter.LinearAttenuation, LinearAttenuation);
			GL.Light(lightName, LightParameter.QuadraticAttenuation, QuadraticAttenuation);
			GL.Light(lightName, LightParameter.SpotExponent, SpotExponent);
			if (SpotExponent > 0)
			{
				GL.Light(lightName, LightParameter.SpotCutoff, SpotCutoff);
				GL.Light(lightName, LightParameter.SpotDirection, SpotDirection);
			}
			GL.Enable(EnableCap.Light0 + (lightName - LightName.Light0));			
		}
	}
}

