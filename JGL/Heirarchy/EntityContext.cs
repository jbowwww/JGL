using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using System.Runtime;
using System.Reflection;
using JGL.Debugging;
using OpenTK.Graphics.OpenGL;

namespace JGL.Heirarchy
{
	/// <summary>
	/// An <see cref="JGL.Heirarchy.Entity"/>-derived class that contains 0+ child entities
	/// </summary>
	/// <remarks>
	/// 		- TODO: Would be nice if this could implement IEnumerable, (i think) this allows LINQ queries?
	/// 			- Could use for stuff like foreach (Entity e from RootContext.Descendants
	/// 																			where e.GetType().Equals(typeof(JGL.Heirarchy.Object)) and e.Id != "excludedId")
	/// </remarks>
	public class EntityContext : Entity, ICollection<Entity>
	{
		/// <summary>
		/// Tracing
		/// </summary>
//		public static readonly AutoTraceSource Trace = AutoTraceSource.GetOrCreate(AsyncXmlFileTraceListener.GetOrCreate("JGL"));

		#region Static members
		/// <summary>
		/// The root <see cref="JGL.Heirarchy.EntityContext"/>
		/// </summary>
		public static readonly EntityRootContext Root = new EntityRootContext();

		/// <summary>
		/// The current context.
		/// </summary>
		public static EntityContext Current = Root;
		#endregion

		#region Properties, indexers & read-only fields
		/// <summary>
		/// Return a <see cref="System.Collections.Generic.ICollection[JGL.Heirarchy.Entity]"/>
		/// representing the current direct child entities of this <see cref="JGL.Heirarchy.EntityContext"/>
		/// </summary>
		public readonly EntityCollection Entities;

		/// <summary>
		/// Gets the descendant <see cref="JGL.Heirarchy.Entity"/>s of this <see cref="JGL.Heirarchy.EntityContext"/>
		/// </summary>
		public IEnumerable<Entity> Descendants {
			get
			{
				List<Entity> descendants = new List<Entity>();
				foreach (Entity e in this)
				{
					descendants.Add(e);
					if (e.IsContext)
						descendants.AddRange((e as EntityContext).Descendants);
				}
				return descendants;
			}
		}

		/// <summary>
		/// Get the <see cref="JGL.Heirarchy.Entity"/> with the specified <paramref name="relativeId"/>
		/// </summary>
		/// <param name="relativeId">ID, relative to this <see cref="JGL.Heirarchy.EntityContext"/>, of the entity to get</param>
		public Entity this[string relativeId] {
			get { return Get(relativeId); }
		}
		
		/// <summary>
		/// Gets the <see cref="JGL.Heirarchy.Entity"/> at the specified index <paramref name="entityIndex"/>
		/// </summary>
		/// <param name="entityIndex">Index of the <see cref="JGL.Heirarchy.Entity"/> to get</param>
		public Entity this [int entityIndex] {
			get { return Entities[entityIndex]; }
		}

		/// <summary>
		/// Gets the number of <see cref="JGL.Heirarchy.Entity"/>s in this <see cref="JGL.Heirarchy.EntityContext"/>
		/// </summary>
		/// <remarks>ICollection[Entity] implementation</remarks>
		public int Count {
			get { return Entities.Count; }
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="JGL.Heirarchy.EntityContext"/> instance is read only.
		/// </summary>
		/// <returns><c>False</c></returns>
		/// <remarks>ICollection[Entity] implementation</remarks>
		public bool IsReadOnly {
			get { return Entities.IsReadOnly; }
		}
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a new <see cref="JGL.Heirarchy.EntityContext"/> with zero initial child entities
		/// </summary>
		/// <param name="entities">Optional parameter array of child <see cref="JGL.Heirarchy.Entity"/> instances</param>
		public EntityContext(params Entity[] entities)// : base(null)
		{
			Entities = new EntityCollection();
			if (entities != null)
				AddRange(entities);
		}

		/// <summary>
		/// Constructs a new <see cref="JGL.Heirarchy.EntityContext"/> with zero initial child entities
		/// </summary>
		/// <param name="name">Name for the new <see cref="JGL.Heirarchy.EntityContext"/></param>
		/// <param name="entities">Optional parameter array of child <see cref="JGL.Heirarchy.Entity"/> instances</param>
//		public EntityContext (string name, params Entity[] entities) : base (name)
//		{
//			Entities = new EntityCollection(this);
//			Add(entities);
//		}
		#endregion

		#region Methods
		/// <summary>
		/// Test if the given <see cref="JGL.Heirarchy.Entity"/> exists in this <see cref="JGL.Heirarchy.EntityContext"/>
		/// </summary>
		/// <param name="e">The <see cref="JGL.Heirarchy.Entity"/> to check for existence of</param>
		/// <returns><c>True</c> if found, otherwise <c>false</c></returns>
		/// <remarks>ICollection implementation</remarks>
		public bool Contains(Entity e)
		{
			return Entities.Contains(e.Name);
		}

		/// <summary>
		/// Test if the given <see cref="JGL.Heirarchy.Entity"/> exists in this <see cref="JGL.Heirarchy.EntityContext"/>
		/// </summary>
		/// <param name="entityName">The entity name to check for existence of</param>
		/// <returns><c>True</c> if found, otherwise <c>false</c></returns>
		public bool Contains(string entityName)
		{
			return Entities.Contains(entityName);
		}

		/// <summary>
		/// Get the <see cref="JGL.Heirarchy.Entity"/> using an ID relative to this <see cref="JGL.Heirarchy.EntityContext"/>
		/// </summary>
		/// <param name="relativeId">An Id string relative to this <see cref="JGL.Heirarchy.EntityContext"/></param>
		/// <returns>The <see cref="JGL.Heirarchy.Entity"/> specified by the relative ID</returns>
		/// <remarks>
		/// If resources are named as their filename including the extension, the period before the extension causes a problem here
		/// Hack for now/Possibly longer term: Resources can be specified using "res://" prefix, which causes this method to
		/// look for an id exactly matching the remainder of the string specified for relative id. EG "res://test1.png" will get child entity
		/// with the id "test1.png"
		/// 	-	! Now that I have move this out of EntityCollection and back in here, I may not have to worry about that issue with Resources:
		/// 		Now I'm using EntityCollection in Behaviour's too, so they should not be aware of the heirarchy (thats why I moved this method back here)
		/// 		Which means I could use an EntityCollection in the RootContext and/or Scene instances, for storing Resource entities.
		/// 		The indexer properties in EntityCollection (taking string and int respectively) will now not be aware of any heirarchy, so they
		/// 		can be used to retrieve resources with '.' in the name, and you can still use '.' for Entity heirarchy ID separation, if you want~!
		/// </remarks>
		public Entity Get(string relativeId)
		{
			Debug.Assert(!string.IsNullOrEmpty(relativeId));
			Entity e = null;
			string[] splitRelativeId = relativeId.Split(new string[] { Entity.EntityIdSeparator }, StringSplitOptions.None);
			foreach (string partId in splitRelativeId)
				e = e == null ? Entities[partId] : (e as EntityContext).Entities[partId];
			return e;
		}

		/// <summary>
		/// Add an <see cref="JGL.Heirarchy.Entity"/> to this <see cref="JGL.Heirarchy.EntityContext"/>
		/// </summary>
		/// <param name="e">The <see cref="JGL.Heirarchy.Entity"/> to add</param>
		/// <remarks>ICollection implementation</remarks>
		public void Add(Entity e)
		{
			if (Entities.Contains(e.Name))
				throw new ArgumentException(string.Concat("Entity name \"", e.Name, "\" already exists in context \"", Id, "\""));
			Entities.Add(e);
		}

		/// <summary>
		/// Add one or more <see cref="JGL.Heirarchy.Entity"/> instances to this <see cref="JGL.Heirarchy.EntityContext"/>
		/// </summary>
		/// <param name="entities"><see cref="JGL.Heirarchy.Entity"/> instance(s) to add</param>
		public void AddRange(params Entity[] entities)
		{
//			if (entities != null)	// && entities.GetType().IsArray)
				foreach (Entity child in entities)
					Entities.Add(child);
		}

		/// <summary>
		/// Attempt to remove the given <see cref="JGL.Heirarchy.Entity"/> from this <see cref="JGL.Heirarchy.EntityContext"/>
		/// </summary>
		/// <param name="e">The <see cref="JGL.Heirarchy.Entity"/> to attempt to remove</param>
		/// <returns><c>true</c> if found and removed, otherwise, <c>false</c></returns>
		/// <remarks>ICollection[Entity] implementation</remarks>
		public bool Remove(Entity e)
		{
			if (!Entities.Contains(e.Name))
				return false;
			return Entities.Remove(e);
		}
		
		/// <summary>
		/// Attempt to remove the given <see cref="JGL.Heirarchy.Entity"/> from this <see cref="JGL.Heirarchy.EntityContext"/>
		/// </summary>
		/// <param name="entityName">The name of the <see cref="JGL.Heirarchy.Entity"/> to attempt to remove</param>
		/// <returns><c>true</c> if found and removed, otherwise, <c>false</c></returns>
		public bool Remove(string entityName)
		{
			if (!Entities.Contains(entityName))
				return false;
			return Entities.Remove(entityName);
		}

		/// <summary>
		/// Clear all <see cref="JGL.Heirarchy.Entity"/>s from this <see cref="JGL.Heirarchy.EntityContext"/>
		/// </summary>
		/// <remarks>ICollection[Entity] implementation</remarks>
		public void Clear()
		{
			foreach (Entity e in Entities)
				e.Parent = null;
			Entities.Clear();
		}
		
		/// <summary>
		/// Copies all <see cref="JGL.Heirarchy.Entity"/>s in this <see cref="JGL.Heirarchy.EntityContext"/>
		/// to <paramref name="array"/>, starting at index <paramref name="arrayIndex"/>
		/// </summary>
		/// <param name="array">The array to copy the <see cref="JGL.Heirarchy.Entity"/>s to</param>
		/// <param name="arrayIndex">The base index into the array to start copying to</param>
		/// <remarks>ICollection[Entity] implementation</remarks>
		public void CopyTo(Entity[] array, int arrayIndex)
		{
			Entities.CopyTo(array, arrayIndex);
		}

		/// <summary>
		/// Gets a generic <see cref="JGL.Heirarchy.Entity"/> enumerator.
		/// </summary>
		/// <returns>An <see cref="System.Collections.Generic.IEnumerator<JGL.Heirarchy.Entity>"/> enumerator</returns>
		/// <remarks>IEnumerable[Entity] implementation</remarks>
		public IEnumerator<Entity> GetEnumerator ()
		{
			return Entities.GetEnumerator ();
		}
		
		/// <summary>
		/// Gets a non-generic enumerator
		/// </summary>
		/// <returns>An <see cref="System.Collections.IEnumerator"/> enumerator</returns>
		/// <remarks>IEnumerable implementation</remarks>
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator ();
		}
		#endregion
	}
}

