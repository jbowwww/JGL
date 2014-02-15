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
		public static readonly AutoTraceSource Trace = AutoTraceSource.GetOrCreate(AsyncXmlFileTraceListener.GetOrCreate("JGL"));

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

		#region Properties and indexers
		/// <summary>
		/// Return a <see cref="System.Collections.Generic.ICollection[JGL.Heirarchy.Entity]"/>
		/// representing the current direct child entities of this <see cref="JGL.Heirarchy.EntityContext"/>
		/// </summary>
		public EntityCollection Entities { get; private set; }

		/// <summary>
		/// Return a <see cref="System.Collections.Generic.ICollection[JGL.Heirarchy.Object]"/>
		/// representing the current direct child entities of this <see cref="JGL.Heirarchy.EntityContext"/>
		/// which are of type <see cref="JGL.Heirarchy.Object"/>
		/// </summary>
		/// <remarks>
		///	-	TODO: Test this. Might need to change to return <c>this.OfType`1[Object]()</c>
		/// </remarks>
		public IEnumerable<Object> Objects {
//			get { return (this as ICollection<Object>); }// _entities.Values; }
			get { return this.OfType<Object>(); }
		}

		/// <summary>
		/// Get <see cref="Entity"/>s contained in this <see cref="EntityContext"/> which are of type <typeparamref name="TEntity"/>
		/// </summary>
		/// <returns><see cref="Entity"/>s of type <typeparamref name="TEntity"/></returns>
		/// <typeparam name="TEntity">The type of <see cref="Entity"/>s to return</typeparam>
//		public ICollection<TEntity> OfType<TEntity>()
//		{
//			return (this as ICollection<Entity>).OfType<TEntity>().ToList();
//		}

		/// <summary>
		/// Gets the descendant <see cref="JGL.Heirarchy.Entity"/>s of this <see cref="JGL.Heirarchy.EntityContext"/>
		/// </summary>
		/// <remarks>
		/// 	- Uses <c>yield</c>
		/// 	- TODO: Would be nice if this was an ICollection, because that would mean it has a Count member? (and prob other things)
		/// 		- NicER would be IEnumerable, (i think) this allows LINQ queries?
		/// 			- Could use for stuff like foreach (Entity e from RootContext.Descendants
		/// 																			where e.GetType().Equals(typeof(JGL.Heirarchy.Object)) and e.Id != "excludedId")
		/// </remarks>
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
//					yield return e;
//					if (e.IsContext)
//						yield return (e as EntityContext).Descendants;
//					if (e is EntityContext)
//						foreach (Entity de in (e as EntityContext).Descendants)
//							yield return de;

		/// <summary>
		/// Get the <see cref="JGL.Heirarchy.Entity"/> with the specified <paramref name="entityName"/>
		/// </summary>
		/// <param name="entityName">Name of entity to get</param>
		public Entity this[string entityName] {
			get { return Entities.Get(entityName); }
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

		#region Events
		/// <summary>
		/// Entity event arguments.
		/// </summary>
		public class EntityEventArgs : EventArgs
		{
			/// <summary>
			/// The <see cref="EntityContext.EntityEventHandler"/> can set this to <c>true</c> to cause the
			/// <see cref="EntityContext.EntityEvent"/> to be cancelled.
			/// </summary>
			public bool Cancel = false;
		}

		/// <summary>
		/// <see cref="EntityEvent"/> handler delegate type
		/// </summary>
		public delegate void EntityEventHandler(Entity entity, EntityEventArgs args);

		/// <summary>
		/// Occurs when an <see cref="Entity"/> is added to this <see cref="EntityContext"/>
		/// </summary>
		public event EntityEventHandler EntityAdded;

		/// <summary>
		/// Occurs when an <see cref="Entity"/> is removed from this <see cref="EntityContext"/>
		/// </summary>
		public event EntityEventHandler EntityRemoved;

		/// <summary>
		/// Occurs when an <see cref="Entity"/> is renamed in this <see cref="EntityContext"/>
		/// </summary>
		public event EntityEventHandler EntityRenamed;
		#endregion

		#region Constructors
		/// <summary>
		/// Constructs a new <see cref="JGL.Heirarchy.EntityContext"/> with zero initial child entities
		/// </summary>
		/// <param name="entities">Optional parameter array of child <see cref="JGL.Heirarchy.Entity"/> instances</param>
		public EntityContext(params Entity[] entities) : base(null)
		{
			Entities = new EntityCollection(this);
			Add (entities);
		}

		/// <summary>
		/// Constructs a new <see cref="JGL.Heirarchy.EntityContext"/> with zero initial child entities
		/// </summary>
		/// <param name="name">Name for the new <see cref="JGL.Heirarchy.EntityContext"/></param>
		/// <param name="entities">Optional parameter array of child <see cref="JGL.Heirarchy.Entity"/> instances</param>
		public EntityContext (string name, params Entity[] entities) : base (name)
		{
			Entities = new EntityCollection(this);
			Add(entities);
		}
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
		/// Add an <see cref="JGL.Heirarchy.Entity"/> to this <see cref="JGL.Heirarchy.EntityContext"/>
		/// </summary>
		/// <param name="e">The <see cref="JGL.Heirarchy.Entity"/> to add</param>
		/// <remarks>ICollection implementation</remarks>
		public void Add(Entity e)
		{
			Entities.Add(e);
		}

		/// <summary>
		/// Add one or more <see cref="JGL.Heirarchy.Entity"/> instances to this <see cref="JGL.Heirarchy.EntityContext"/>
		/// </summary>
		/// <param name="entities"><see cref="JGL.Heirarchy.Entity"/> instance(s) to add</param>
		public void Add(params Entity[] entities)
		{
			if (entities != null)	// && entities.GetType().IsArray)
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
			return Entities.Remove(e);
		}
		
		/// <summary>
		/// Attempt to remove the given <see cref="JGL.Heirarchy.Entity"/> from this <see cref="JGL.Heirarchy.EntityContext"/>
		/// </summary>
		/// <param name="entityName">The name of the <see cref="JGL.Heirarchy.Entity"/> to attempt to remove</param>
		/// <returns><c>true</c> if found and removed, otherwise, <c>false</c></returns>
		public bool Remove(string entityName)
		{
			return Entities.Remove(entityName);
		}

		/// <summary>
		/// Clear all <see cref="JGL.Heirarchy.Entity"/>s from this <see cref="JGL.Heirarchy.EntityContext"/>
		/// </summary>
		/// <remarks>ICollection[Entity] implementation</remarks>
		public void Clear()
		{
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

