using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

using OpenTK;
using OpenTK.Graphics.OpenGL;

using JGL.Graphics;
using JGL.Heirarchy.Behaviours;
using JGL.Debugging;

namespace JGL.Heirarchy
{
	/// <summary>
	/// Represents a scene graph, i.e. the top level <see cref="JGL.Heirarchy.Context"/> containing
	/// a heirarchy of <see cref="JGL.Heirarchy.Entity"/>s
	/// </summary>
	public class Scene : EntityContext
	{
		/// <summary>
		/// Tracing
		/// </summary>
		public static readonly AutoTraceSource Trace = AutoTraceSource.GetOrCreate(AsyncXmlFileTraceListener.GetOrCreate("JGL"));

		public Camera DefaultCamera = new Camera("DefaultCamera");

		public readonly IList<Behaviour> Behaviours = new List<Behaviour>();

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
			: base(children)
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

		public void ProcessBehaviours(RenderArgs renderArgs)
		{
			foreach (Behaviour b in Behaviours)
				b.Process(renderArgs);
		}
	}
}

