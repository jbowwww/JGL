using System;
using System.Xml;
using System.Xml.Serialization;

using JGL.Graphics;
using JGL.Debugging;

namespace JGL.Heirarchy
{
	[Serializable]
	public class SimpleObject : Object, IRenderable
	{
		/// <summary>
		/// Tracing
		/// </summary>
		public static readonly AutoTraceSource Trace = AutoTraceSource.GetOrCreate(AsyncXmlFileTraceListener.GetOrCreate("JGL"));

		public delegate void RenderFunc(RenderArgs renderArgs);
		
		private RenderFunc _renderFunc = null;
		
		/// <summary>
		/// Position in 3D space
		/// </summary>
		/// <remarks>IPositionable implementation</remarks>
//		[XmlElement("Position", typeof(OpenTK.Vector3d))]
//		public OpenTK.Vector3d Position { get; set; }
		
		/// <summary>
		/// Rotation in 3D space (X,Y,Z each describe number of degrees rotation around the X/Y/Z unit vector)
		/// </summary>
		/// <remarks>IRotatable implementation</remarks>
//		[XmlElement("Rotation", typeof(OpenTK.Vector3d))]
//		public OpenTK.Vector3d Rotation { get; set; }

		#region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="JGL.Heirarchy.SimpleObject"/> class.
		/// </summary>
		public SimpleObject()
			: base()
		{

		}

		/// <summary>
		/// Initializes a new instance of the <see cref="JGL.Heirarchy.SimpleObject"/> class.
		/// </summary>
		/// <param name='name'>Object name</param>
//		public SimpleObject(string name = null)
//			: base(name)
//		{
//
//		}
				
		/// <summary>
		/// Initializes a new instance of the <see cref="JGL.Heirarchy.SimpleObject"/> class.
		/// </summary>
		/// <param name='name'>Object name</param>
//		public SimpleObject(string name, RenderFunc renderFunc)
//			: base(name)
//		{
//			_renderFunc = renderFunc;
//		}
				
		/// <summary>
		/// Initializes a new instance of the <see cref="JGL.Heirarchy.SimpleObject"/> class.
		/// </summary>
		/// <param name='name'>Object name</param>
		public SimpleObject(RenderFunc renderFunc)
		{
			_renderFunc = renderFunc;
		}
		#endregion

		#region IRenderable implementation
		public void Render(RenderArgs renderArgs)
		{
			if (_renderFunc != null)
				_renderFunc(renderArgs);
		}
		#endregion
	}
}
