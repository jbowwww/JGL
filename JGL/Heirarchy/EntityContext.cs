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

		#region Private members
		/// <summary>
		/// Constant: Maximum number of indexes that can be used for autonaming an <see cref="Entity"/>
		/// </summary>
		private const int _maxAutoNameIndexes = 0xffffff;

		/// <summary>
		/// Inner <see cref="System.Collections.Concurrent.ConcurrentDictionary`1[System.string, JGL.Heirarchy.Entity]"/> to store the <see cref="JGL.Heirarchy.Entity"/>s
		/// references for this <see cref="JGL.Heirarchy.EntityDictionary"/> 
		/// </summary>
		private ConcurrentDictionary<string, Entity> _entities = new ConcurrentDictionary<string, Entity>();
		#endregion

		#region Properties and indexers
		/// <summary>
		/// Return a <see cref="System.Collections.Generic.ICollection[JGL.Heirarchy.Entity]"/>
		/// representing the current direct child entities of this <see cref="JGL.Heirarchy.EntityContext"/>
		/// </summary>
		public ICollection<Entity> Entities {
			get { return (this as ICollection<Entity>); }// _entities.Values; }
		}

		/// <summary>
		/// Return a <see cref="System.Collections.Generic.ICollection[JGL.Heirarchy.Object]"/>
		/// representing the current direct child entities of this <see cref="JGL.Heirarchy.EntityContext"/>
		/// which are of type <see cref="JGL.Heirarchy.Object"/>
		/// </summary>
		/// <remarks>
		///	-	TODO: Test this. Might need to change to return <c>this.OfType`1[Object]()</c>
		/// </remarks>
		public ICollection<Object> Objects {
			get { return (this as ICollection<Object>); }// _entities.Values; }
		}

		/// <summary>
		/// Get <see cref="Entity"/>s contained in this <see cref="EntityContext"/> which are of type <typeparamref name="TEntity"/>
		/// </summary>
		/// <returns><see cref="Entity"/>s of type <typeparamref name="TEntity"/></returns>
		/// <typeparam name="TEntity">The type of <see cref="Entity"/>s to return</typeparam>
		public ICollection<TEntity> OfType<TEntity>()
		{
			return (this as ICollection<Entity>).OfType<TEntity>().ToList();
		}

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
				foreach (Entity e in this)
				{
					yield return e;
					if (e is EntityContext)
						foreach (Entity de in (e as EntityContext).Descendants)
							yield return de;
				}
			}
		}

		/// <summary>
		/// Get the <see cref="JGL.Heirarchy.Entity"/> with the specified <paramref name="entityName"/>
		/// </summary>
		/// <param name='entityName'>Name of entity to get</param>
		public Entity this[string entityName] {
			get
			{
				return Get(entityName);
//				Debug.Assert (!entityName.Contains ('.'));		// ensure it is only an entity name, not a relative ID
//				return _entities[entityName];
			}
		}
		
		/// <summary>
		/// Gets the <see cref="JGL.Heirarchy.Entity"/> at the specified index <paramref name="entityIndex"/>
		/// </summary>
		/// <param name='entityIndex'>Index of the <see cref="JGL.Heirarchy.Entity"/> to get</param>
		public Entity this [int entityIndex] {
			get { return _entities.Values.ElementAt (entityIndex); }
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
		public EntityContext (params Entity[] entities)
			: base(null)
		{
			Add (entities);
		}

		/// <summary>
		/// Gets a <see cref="System.Collections.Generic.ICollection<Entity>"/> representing the child
		/// <see cref="JGL.Heirarchy.Entity"/>s contained in this <see cref="JGL.Heirarchy.EntityContext"/>
		/// </summary/// <summary>
		/// Constructs a new <see cref="JGL.Heirarchy.EntityContext"/> with zero initial child entities
		/// </summary>
		/// <param name="name">Name for the new <see cref="JGL.Heirarchy.EntityContext"/></param>
		/// <param name="entities">Optional parameter array of child <see cref="JGL.Heirarchy.Entity"/> instances</param>
		public EntityContext (string name, params Entity[] entities)
			: base (name)
		{
			Add(entities);
		}
		#endregion

		#region Methods
		/// <summary>
		/// Called by <see cref="Entity.Name"/>.set when an <see cref="Entity"/> changes its name,
		/// to ensure consistency in the <see cref="EntityContext"/>
		/// </summary>
		/// <returns><c>True</c> if updated successfully, otherwise, <c>false</c></returns>
		/// <param name='entity'>The <see cref="Entity"/> to update name for, with its current name still set</param>
		/// <param name='newName'>The new <see cref="Entity.Name"/></param>
		/// <remarks>
		/// </remarks>
		internal void UpdateName(Entity entity, string newName)
		{
			if (!_entities.TryAdd(newName, entity))
				throw new Exception(string.Format("EntityContext.UpdateName(entity = \"{0}\", newName =\"{1}\"): Failed to add entity using its new name to context \"{3}\" (after determining that the new name did not already exist)", entity.Id, newName, Id));
			Entity o;
			if (!_entities.TryRemove(entity.Name, out o))
				throw new Exception(string.Format("EntityContext.UpdateName(entity = \"{0}\", newName =\"{1}\"): Failed to remove entity's previous name from context \"{3}\" (after determining that the new name did not already exist, and adding the entity using its new name)", entity.Id, newName, Id));
			if (entity != o)
				throw new Exception(string.Format("EntityContext.UpdateName(entity = \"{0}\", newName =\"{1}\"): Removed an entity using the old entity name from context \"{3}\" , and it is not the same instance that was passed to this method", entity.Id, newName, Id));
		}

		/// <summary>
		/// Adds <paramref name="entities"/> as child <see cref="JGL.Heirarchy.Entity"/>s in this <see cref="JGL.Heirarchy.EntityContext"/>
		/// </summary>
		/// <summary>
		/// Get the <see cref="JGL.Heirarchy.Entity"/> using an ID relative to this <see cref="JGL.Heirarchy.EntityContext"/>
		/// </summary>
		/// <param name='relativeId'>An Id string relative to this <see cref="JGL.Heirarchy.EntityContext"/></param>
		/// <returns>The <see cref="JGL.Heirarchy.Entity"/> specified by the relative ID</returns>
		public Entity Get(string relativeId)
		{
			Debug.Assert(!string.IsNullOrEmpty(relativeId));
			Entity e = null;
			foreach (string partId in relativeId.Split ('.'))
				e = e == null ? _entities[partId] : (e as EntityContext)._entities[partId];
			return e;
		}
		#endregion

		#region Collection implementation (ICollection members and overloaded members of same name(s))
		/// <summary>
		/// Test if the given <see cref="JGL.Heirarchy.Entity"/> exists in this <see cref="JGL.Heirarchy.EntityContext"/>
		/// </summary>
		/// <param name='e'>The <see cref="JGL.Heirarchy.Entity"/> to check for existence of</param>
		/// <returns><c>True</c> if found, otherwise <c>false</c></returns>
		public bool Contains (Entity e)
		{
			return _entities.ContainsKey(e.Name);
		}

		/// <summary>
		/// Test if the given <see cref="JGL.Heirarchy.Entity"/> exists in this <see cref="JGL.Heirarchy.EntityContext"/>
		/// </summary>
		/// <param name='eName'>The entity name to check for existence of</param>
		/// <returns><c>True</c> if found, otherwise <c>false</c></returns>
		/// <remarks>
		/// 	- NOT a member of ICollection[Entity]
		/// 	- Checks collection for an <see cref="Entity"/> based on its name
		/// </remarks>
		public bool Contains (string eName)
		{
			return _entities.ContainsKey (eName);
		}

		/// <summary>
		/// Add an <see cref="JGL.Heirarchy.Entity"/> to this <see cref="JGL.Heirarchy.EntityContext"/>
		/// </summary>
		/// <param name='e'>The <see cref="JGL.Heirarchy.Entity"/> to add</param>
		/// <exception cref='InvalidOperationException'>An <see cref="JGL.Heirarchy.Entity"/> already exists with the same name as <paramref name="e"/></exception>
		public void Add(Entity e)
		{
			Debug.Assert(e.Parent == null);		// Should not exist in a parent entity context already

			if (e.IsAutoNamed)								// Entity needs autonaming
			{
				// TODO: Trace log (verbose) entity of type "" being autonamed to ""
				
				string baseName = e.Name;
				for (int i = 1; i < _maxAutoNameIndexes; i++)
				{
					e.Name = string.Format("{0} #{1:x3}", baseName, i);
					if (!_entities.ContainsKey(e.Name))
						break;
				}
			}

			if (!_entities.TryAdd(e.Name, e))		// Try to add the entity to this context (concurrent dictionary)
				throw new InvalidOperationException(string.Format(
					"EntityContext.Add(entity = \"{0}\"): Unable to add the entity to the context \"{1}\" ({2})", e.Name, Id, _entities.ContainsKey(e.Name) ?
					"Context already contains an entity with that name" : "Context does NOT contain an entity with that name"));

			// TODO: Trace log (info) entity added to context

			if (!(this is EntityRootContext))		// If top level entities (e.g. Scene) are being stored in an EntityRootContext,
				e.Parent = this;									// it is NOT set as their parent so ID etc properties do not include the root context

			EntityEventArgs args = new EntityEventArgs();
			if (EntityAdded != null)
				EntityAdded(e, args);
		}

		/// <summary>
		/// Add one or more <see cref="JGL.Heirarchy.Entity"/> instances to this <see cref="JGL.Heirarchy.EntityContext"/>
		/// </summary>
		/// <param name='entities'><see cref="JGL.Heirarchy.Entity"/> instance(s) to add</param>
		/// <remarks>
		/// 	- NOT a member of ICollection[Entity]
		/// 	- Adds multiple <see cref="Entity"/>s to this <see cref="EntityContext"/>
		/// </remarks>
		public void Add(params Entity[] entities)
		{
			if (entities != null && entities.GetType ().IsArray)
				foreach (Entity child in entities)
					Add (child);
		}
		
		/// <summary>
		/// Clear all <see cref="JGL.Heirarchy.Entity"/>s from this <see cref="JGL.Heirarchy.EntityContext"/>
		/// </summary>
		public void Clear()
		{
			foreach (Entity e in _entities.Values)
				if (e != null && e.Parent != null)
					e.Parent = null;
			_entities.Clear();
		}
		
		/// <summary>
		/// Attempt to remove the given <see cref="JGL.Heirarchy.Entity"/> from this <see cref="JGL.Heirarchy.EntityContext"/>
		/// </summary>
		/// <param name='item'>The <see cref="JGL.Heirarchy.Entity"/> to attempt to remove</param>
		/// <returns><c>True</c> if found and removed, otherwise, <c>false</c></returns>
		public bool Remove (Entity e)
		{
			Entity o;
			bool r = _entities.TryRemove (e.Name, out o);
			Debug.Assert (!r || r && Entity.ReferenceEquals (e, o));
			if (r)
				e.Parent = null;
			return r;
		}
		
		/// <summary>
		/// Copies all <see cref="JGL.Heirarchy.Entity"/>s in this <see cref="JGL.Heirarchy.EntityContext"/>
		/// to <paramref name="array"/>, starting at index <paramref name="arrayIndex"/>
		/// </summary>
		/// <param name="array">The array to copy the <see cref="JGL.Heirarchy.Entity"/>s to</param>
		/// <param name="arrayIndex">The base index into the array to start copying to</param>
		public void CopyTo (Entity[] array, int arrayIndex)
		{
			_entities.Values.CopyTo (array, arrayIndex);
		}

		/// <summary>
		/// Gets the number of <see cref="JGL.Heirarchy.Entity"/>s in this <see cref="JGL.Heirarchy.EntityContext"/> 
		/// </summary>
		public int Count {
			get { return _entities.Count; }
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="JGL.Heirarchy.EntityContext"/> instance is read only.
		/// </summary>
		/// <returns><c>False</c></returns>
		public bool IsReadOnly {
			get { return false; }
		}
		
		/// <summary>
		/// Gets a generic <see cref="JGL.Heirarchy.Entity"/> enumerator.
		/// </summary>
		/// <returns>An <see cref="System.Collections.Generic.IEnumerator<JGL.Heirarchy.Entity>"/> enumerator</returns>
		/// <remarks>IEnumerable[Entity] implementation</remarks>
		public IEnumerator<Entity> GetEnumerator ()
		{
			return _entities.Values.GetEnumerator ();
		}
		
		/// <summary>
		/// Gets a non-generic enumerator
		/// </summary>
		/// <returns>An <see cref="System.Collections.IEnumerator"/> enumerator</returns>
		/// <remarks>IEnumerable implementation</remarks>
		IEnumerator IEnumerable.GetEnumerator ()
		{
			return GetEnumerator () as IEnumerator;
		}
		#endregion
	}
}

