using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using JGL.Debugging;
using OpenTK;
using OpenTK.Graphics.OpenGL;
//using Glade;

namespace JGL.Heirarchy
{
	/// <summary>
	/// Represents a scene graph, i.e. the top level <see cref="JGL.Heirarchy.Context"/> containing
	/// a heirarchy of <see cref="JGL.Heirarchy.Entity"/>s
	/// </summary>
	public class Scene : EntityContext, IRenderable
	{
		/// <summary>
		/// Tracing
		/// </summary>
		public static readonly AutoTraceSource Trace = AutoTraceSource.GetOrCreate(AsyncXmlFileTraceListener.GetOrCreate("JGL"));

		public readonly Camera DefaultCamera = new Camera("DefaultCamera");
		
		/// <summary>
		/// Construct a new <see cref="JGL.Heirarchy.Scene"/>
		/// </summary>
		/// <param name='name'><see cref="JGL.Heirarchy.Entity.Name"/> of the new scene</param>
//		public Scene (string name = "NewScene")
//			: base(null, name)
//		{
//			
//		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="JGL.Heirarchy.Scene"/> class.
		/// </summary>
		/// <param name='children'>Zero or more child entities to add to this <see cref="JGL.Heirarchy.Context"/></param>
		public Scene (params Entity[] children)
			: base("Scene", children)
		{		
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="JGL.Heirarchy.Scene"/> class.
		/// </summary>
		/// <param name='children'>Zero or more child entities to add to this <see cref="JGL.Heirarchy.Context"/></param>
		/// <param name="name">Entity name</param>
		public Scene (string name, params Entity[] children)
			: base(name, children)
		{
			
		}

		/// <summary>
		/// Renders the scene by recursing through the heirarchy, inspecting each <see cref="JGL.Heirarchy.Entity"/>
		/// to see which interfaces it implements and performing the appropriate operations.
		/// </summary>
		/// <param name='renderArgs'>Render context.</param>
		/// <remarks>IRenderable implementation</remarks>
		public void Render(RenderArgs renderArgs)
		{
			GL.LoadIdentity();
			DefaultCamera.Render(renderArgs);
		}
	}
}

