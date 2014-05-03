using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;

using JGL.Debugging;

namespace JGL.Heirarchy
{
	/// <summary>
	/// A collection of <see cref="Entity"/>s contained by an <see cref="EntityContext"/>
	/// </summary>
	public class EntityCollection : EntityCollection<Entity> { }

	public class EntityCollection<TEntity> : ICollection<TEntity>
		where TEntity : Entity
	{
		/// <summary>
		/// The _inner dictionary.
		/// </summary>
		private ConcurrentDictionary<string, TEntity> _innerDictionary = new ConcurrentDictionary<string, TEntity>();

		#region Properties & indexers
		/// <summary>
		/// Get the names of all <see cref="Entity"/> instances contained in this <see cref="EntityCollection"/>
		/// </summary>
		public IEnumerable<string> Names {
			get { return _innerDictionary.Keys; }
		}

		/// <summary>
		/// Gets the <see cref="JGL.Heirarchy.Entity"/> with the specified name.
		/// </summary>
		/// <param name="relativeId">Name of the required <see cref="JGL.Heirarchy.Entity"/></param>
		public TEntity this[string relativeId] {
			get { return _innerDictionary[relativeId]; }
		}
		
		/// <summary>
		/// Gets the <see cref="JGL.Heirarchy.Entity"/> at the specified index <paramref name="entityIndex"/>
		/// </summary>
		/// <param name="entityIndex">Index of the <see cref="JGL.Heirarchy.Entity"/> to get</param>
		public TEntity this[int entityIndex] {
			get { return _innerDictionary.Values.ElementAt(entityIndex); }
		}

		/// <summary>
		/// Get the number of <see cref="Entity"/>s in this <see cref="EntityCollection"/>
		/// </summary>
		/// <remarks>ICollection[Entity] implementation</remarks>
		public int Count {
			get { return _innerDictionary.Count; }
		}

		/// <summary>
		/// Gets a value indicating whether this instance is read only.
		/// </summary>
		/// <remarks>ICollection[Entity] implementation</remarks>
		public bool IsReadOnly {
			get { return false; }
		}
		#endregion

		#region Events
		/// <summary>
		/// <see cref="EntityEvent"/> handler delegate type
		/// </summary>
		public delegate void EntityEventHandler(EntityEventArgs args);

		/// <summary>
		/// Entity event arguments.
		/// </summary>
		public class EntityEventArgs : EventArgs
		{
			public Entity Entity;
		}

		/// <summary>
		/// Renamed Entity event arguments.
		/// </summary>
		public class EntityRenamingEventArgs : EntityEventArgs
		{
			public string NewName;
		}

		/// <summary>
		/// Occurs when an <see cref="Entity"/> is added to this <see cref="EntityContext"/>
		/// </summary>
		public event EntityEventHandler Added;

		/// <summary>
		/// Occurs when an <see cref="Entity"/> is removed from this <see cref="EntityContext"/>
		/// </summary>
		public event EntityEventHandler Removed;

		/// <summary>
		/// Occurs when an <see cref="Entity"/> is renamed in this <see cref="EntityContext"/>
		/// </summary>
		public event EntityEventHandler Renaming;
		#endregion

		#region Methods
		/// <summary>
		/// Called by <see cref="Entity.Name"/> set accessor when an <see cref="Entity"/> changes its name, to ensure
		/// consistency in the <see cref="EntityContext"/>
		/// </summary>
		/// <returns><c>true</c> if updated successfully, otherwise, <c>false</c></returns>
		/// <param name="entity">The <see cref="Entity"/> to update name for, with its current name still set</param>
		/// <param name="newName">The new <see cref="Entity.Name"/></param>
		/// <remarks>
		/// Note this is not meant to check that the entity's new name is valid or not already taken - that's in Entity.Name set accessor
		/// This just performs the update in this collection, by adding the same Entity reference with the new name as key, then
		/// removing the old one
		/// </remarks>
		internal void UpdateName(TEntity entity, string newName)
		{
			TEntity o = null;
			int retryCount;
			for (retryCount = 0; retryCount < Engine.Options.ConcurrentCollectionOperationRetryLimit; retryCount++)
				if (_innerDictionary.TryAdd(newName, entity))
					break;
			if (retryCount == Engine.Options.ConcurrentCollectionOperationRetryLimit)
				throw new ConcurrencyException("UpdateName: _innerDictionary.TryAdd") { RetryCount = retryCount };
			for (retryCount = 0; retryCount < Engine.Options.ConcurrentCollectionOperationRetryLimit; retryCount++)
				if (_innerDictionary.TryRemove(entity.Name, out o))
					break;
			if (retryCount == Engine.Options.ConcurrentCollectionOperationRetryLimit)
				throw new ConcurrencyException("UpdateName: _innerDictionary.TryRemove") { RetryCount = retryCount };
			else if (entity != o) throw new ApplicationException(string.Format(
				"UpdateName: Removed entity reference \"{0}\" is not equal to the updated entity \"{1}\"", entity.Id, newName));
			if (Renaming != null)
				Renaming(new EntityRenamingEventArgs() { NewName = newName });
		}

		/// <summary>
		/// Determines if this <see cref="EntityCollection"/> contains the specified <see cref="Entity"/>
		/// </summary>
		/// <param name="e">The <see cref="Entity"/> to check for</param>
		/// <remarks>ICollection[Entity] implementation</remarks>
		public bool Contains(TEntity e)
		{
			return _innerDictionary.ContainsKey(e.Name);
		}

		/// <summary>
		/// Determines if this <see cref="EntityCollection"/> contains an <see cref="Entity"/> with the specified name.
		/// </summary>
		/// <param name="entityName">The <see cref="Entity"/> name to check for</param>
		public bool Contains(string entityName)
		{
			return _innerDictionary.ContainsKey(entityName);
		}

		/// <summary>
		/// Adds an <see cref="JGL.Heirarchy.Entity"/>s to this <see cref="JGL.Heirarchy.EntityCollection"/>
		/// </summary>
		/// <param name="e">The <see cref="JGL.Heirarchy.Entity"/> to add</param>
		/// <exception cref="ArgumentException">
		/// Thrown when an <see cref="JGL.Heirarchy.Entity"/> with the same name already exists
		/// </exception>
		/// <exception cref="ConcurrencyException">
		/// Thrown when <paramref name="entity"/> is found but failed to be added to the inner dictionary
		/// </exception>
		/// <remarks>ICollection[Entity] implementation</remarks>
		public void Add(TEntity entity)
		{
			int retryCount;
			for (retryCount = 0; retryCount < Engine.Options.ConcurrentCollectionOperationRetryLimit; retryCount++)
				if (_innerDictionary.TryAdd(entity.Name, entity))
					break;
				else if (Engine.Options.ConcurrentCollectionOperationRetryDelayCycles > 0)
					Thread.SpinWait(Engine.Options.ConcurrentCollectionOperationRetryDelayCycles);
			if (retryCount == Engine.Options.ConcurrentCollectionOperationRetryLimit)
				throw new ConcurrencyException() { RetryCount = retryCount };
			if (Added != null)
				Added(new EntityEventArgs() { Entity = entity });
		}

		/// <summary>
		/// Add the specified <see cref="JGL.Heirarchy.Entity"/> instances
		/// </summary>
		/// <param name="entities">One or more <see cref="JGL.Heirarchy.Entity"/> instances</param>
		public void AddRange(params TEntity[] entities)
		{
//			if (entities != null)
				foreach (TEntity e in entities)
					Add(e);
		}

		/// <summary>
		/// Remove the specified <see cref="Entity"/> from this <see cref="EntityCollection"/>
		/// </summary>
		/// <param name="e">The <see cref="Entity"/> to remove</param>
		/// <exception cref="ConcurrencyException">
		/// Thrown when <paramref name="e"/> is found but failed to be removed
		/// </exception>
		/// <remarks>ICollection[Entity] implementation</remarks>
		public bool Remove(TEntity e)
		{
			return Remove(e.Name);
		}

		/// <summary>
		/// Remove the <see cref="Entity"/> with the specified name.
		/// </summary>
		/// <param name="entityName">The <see cref="Entity"/> name to remove</param>
		/// <exception cref="ConcurrencyException">
		/// Thrown when the <paramref name="entityName"/> is found but failed to be removed
		/// </exception>
		public bool Remove(string entityName)
		{
			TEntity outValue = null;
			int retryCount;
			for (retryCount = 0; retryCount < Engine.Options.ConcurrentCollectionOperationRetryLimit; retryCount++)
				if (_innerDictionary.TryRemove(entityName, out outValue))
					break;
				else if (Engine.Options.ConcurrentCollectionOperationRetryDelayCycles > 0)
						Thread.SpinWait(Engine.Options.ConcurrentCollectionOperationRetryDelayCycles);
			if (retryCount == Engine.Options.ConcurrentCollectionOperationRetryLimit)
				throw new ConcurrencyException() { RetryCount = retryCount };
			if (outValue != null)
			{
				if (Removed != null)
					Removed(new EntityEventArgs() { Entity = outValue });
				return true;
			}
			return false;
		}

		/// <summary>
		/// Clears this <see cref="EntityCollection"/>
		/// </summary>
		/// <remarks>ICollection[Entity] implementation</remarks>
		public void Clear()
		{
			if (_innerDictionary.Count > 0)
			{
				TEntity[] entities = new TEntity[_innerDictionary.Count];
				_innerDictionary.Values.CopyTo(entities, 0);
				_innerDictionary.Clear();
				if (Removed != null)
					foreach (Entity e in entities)
						Removed(new EntityEventArgs() { Entity = e });
			}
		}

		/// <summary>
		/// Copies the <see cref="Entity"/>s in this <see cref="EntityCollection"/> to the specified array
		/// </summary>
		/// <param name="array">The array to copy the <see cref="EntityCollection"/> to</param>
		/// <param name="arrayIndex">The index into the destination array to begin copying at</param>
		/// <remarks>ICollection[Entity] implementation</remarks>
		public void CopyTo(TEntity[] array, int arrayIndex)
		{
			_innerDictionary.Values.CopyTo(array, arrayIndex);
		}
		#endregion

		#region IEnumerable[TEntity] & IEnumerable implementation
		/// <summary>
		/// Gets the enumerator.
		/// </summary>
		public IEnumerator<TEntity> GetEnumerator()
		{
			return _innerDictionary.Values.GetEnumerator();
		}

		/// <summary>
		/// Gets the enumerator.
		/// </summary>
		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}
		#endregion
	}
}