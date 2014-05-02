using System;
using System.Collections.Generic;

using OpenTK.Graphics.OpenGL;

using JGL.Graphics;
using JGL.Geometry;
using JGL.Debugging;

namespace JGL.Heirarchy
{
	/// <summary>
	/// Camera entity.
	/// </summary>
	public class Camera : Object, IRenderable, IPositionable, IRotatable
	{
//		public new OpenTK.Vector3d Position { get { return -base.Position; } set { base.Position = value; } }
//		
//		public new OpenTK.Vector3d Rotation { get { return - base.Rotation; } set { base.Rotation = value; } }

//		public static AutoTraceSource Trace = AutoTraceSource.GetOrCreate(AsyncXmlFileTraceListener.GetOrCreate("JGL"));

		/// <summary>
		/// Initializes a new instance of the <see cref="JGL.Heirarchy.Camera"/> class.
		/// </summary>
		/// <param name='name'>Camera's entity name</param>
//		public Camera(string name)
//			: base(name)
//		{
//		}
		
		/// <summary>
		/// Render owner <see cref="JGL.Heirarchy.Scene"/> using the perspective of this <see cref="JGL.Heirarchy.Camera"/>,
		/// </summary>
		/// <param name='renderArgs'>Render arguments.</param>
		/// <remarks>IRenderable implementation</remarks>
		public void Render(RenderArgs renderArgs)
		{
			Stack<Entity> entityStack = renderArgs.Entities;
			if (entityStack.Count > 0)
				throw new InvalidOperationException("RenderArgs.Entities stack should be empty when calling Camera.Render");
			entityStack.Push(renderArgs.Scene);

			GL.Rotate(-Rotation.X, 1, 0, 0);
			GL.Rotate(-Rotation.Y, 0, 1, 0);
			GL.Rotate(-Rotation.Z, 0, 0, 1);
			GL.Translate(-Position);
			
			bool[] eFlags = new bool[4];
			while (entityStack.Count > 0)
			{
				Entity entity = entityStack.Pop();
				if (entity == null)									// check for null values in the stack; they are markers that indicate that an EntityContext's
					GL.PopMatrix();						// child Entities have just finished rendering, so the context's position/rotation changes can be reversed
				else
				{
					eFlags[0] = entity is IRenderable && entity != this && entity.GetType() != typeof(Camera) && entity.GetType() != typeof(Scene) && !entity.GetType().IsSubclassOf(typeof(Scene));		//!entity.GetType().IsSubclassOf(typeof(Camera)); 		// if e == this, this is the current execution of e as Irenderable.Render so don't want to call itself again
					eFlags[1] = entity is EntityContext;
//					if (eFlags[0] || eFlags[1])					// if entity is not renderable and does not contains child entities, no point translating right?
//					{
					eFlags[2] = entity is IPositionable && entity.GetType() != typeof(Camera);		//!entity.GetType().IsSubclassOf(typeof(Camera));
					eFlags[3] = entity is IRotatable && entity.GetType() != typeof(Camera);		//!entity.GetType().IsSubclassOf(typeof(Camera));
					if (eFlags[2] || eFlags[3])
					{
						GL.PushMatrix();
						if (eFlags[2])							// (e is IPositionable)
							GL.Translate((entity as IPositionable).Position);
						if (eFlags[3])							// (e is IRotatable)
						{
							IRotatable ir = entity as IRotatable;
							GL.Rotate(ir.Rotation.X, 1, 0, 0);
							GL.Rotate(ir.Rotation.Y, 0, 1, 0);
							GL.Rotate(ir.Rotation.Z, 0, 0, 1);
						}
					}
						
					if (eFlags[0])								// (e is IRenderable)
						(entity as IRenderable).Render(renderArgs);
					if (eFlags[1])								// (e is EntityContext)
					{
						if (eFlags[2] || eFlags[3])			// only need to worry about popping the GL matrix stack if something has been pushed on it (ie this EntityContext must be IPositionable or IRotatable)
							entityStack.Push(null);				// marks location in the stack where the GL modelview matrix should be popped (after rendering a EntityContext's child Entities which were pushed immediately before this marker)
						foreach (Entity _e in (entity as EntityContext))
							entityStack.Push(_e);				// Push this EntityContext's child Entities (if any) onto the stack, so they will be next to be rendered (while the modelview matrix has been set by the containg EntityContext)
					}
					else if (eFlags[2] || eFlags[3])		// Entity is not an EntityContext (so can't have child Entities), but it is IPositionable and/or IRotatable so the modelview matrix has been pushed. Because no children, can pop it immediately
							GL.PopMatrix();
//					}
				}
			}
		}
	}
}

