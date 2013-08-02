using System;
using System.Collections;
using System.Collections.Generic;
using JGL.Heirarchy.Resources;
using JGL.Debugging;

namespace JGL.Heirarchy
{
	/// <summary>
	/// A special subclass of <see cref="JGL.Heirarchy.Context"/> designed 
	/// ... ??
	/// to hold all <see cref="JGL.Heirarchy.Scene"/>s ??
	/// other top level entities??
	/// </summary>
	public class EntityRootContext : EntityContext
	{
		/// <summary>
		/// Tracing
		/// </summary>
		public static readonly AutoTraceSource Trace = AutoTraceSource.GetOrCreate(AsyncXmlFileTraceListener.GetOrCreate("JGL"));

		#region Properties and indexers
		public ICollection<Resource> Resources {
			get { return OfType<Resource>(); }
		}
		#endregion

		/// <summary>
		/// Initializes a new instance of the <see cref="JGL.Heirarchy.EntityRootContext"/> class.
		/// </summary>
		/// <param name="entities">Optional parameter array of child <see cref="JGL.Heirarchy.Entity"/> instances</param>
		public EntityRootContext(params Entity[] entities)
			: base(null, entities)
		{
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="JGL.Heirarchy.EntityRootContext"/> class.
		/// </summary>
		/// <param name="name">Name for the new <see cref="JGL.Heirarchy.Context"/></param>
		/// <param name="entities">Optional parameter array of child <see cref="JGL.Heirarchy.Entity"/> instances</param>
		public EntityRootContext(string name, params Entity[] entities)
			: base(name, entities)
		{
		}
	}
}

