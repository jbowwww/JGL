using System;
//using System.Diagnostics;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;
using JGL.Debugging;
//using JGL.Data;

namespace JGL.Heirarchy
{
	/// <summary>
	/// Top level base class for heirarchy entities
	/// </summary>
	/// <remarks>
	/// TODO: Support concurrency? e.g. any changes or initial lazy load of default Name/ID, Parent setter,
	/// should all lock the values they are reading/writing, AND ALSO, should lock around potential Entity
	/// enumeration accessed in through Parent context's (don't change collections while enumerating)
	/// Think about it, first, then code
	/// </remarks>
	[Serializable]
	public class Entity : IXmlSerializable
	{
		/// <summary>
		/// Tracing
		/// </summary>
		public static readonly AutoTraceSource Trace = AutoTraceSource.GetOrCreate(AsyncXmlFileTraceListener.GetOrCreate("JGL"));

		public const string EntityIdSeparator = ":";

		#region Private fields
		private string _name;
		private string _id = null;
		private EntityContext _parent = null;
		private EntityContext _owner = null;
		#endregion

		#region Properties
		/// <summary>
		/// Gets or sets the <see cref="JGL.Heirarchy.Entity"/> name
		/// </summary>
		[XmlElement("Name")]
		public string Name {
			get
			{
				if (IsAutoNamed || string.IsNullOrEmpty(_name))
				{
					if (_name != null)
						throw new HeirarchyException("Entity invalid: IsAutoNamed==true && Name != null")
						{
							ContextId = Parent == null ? null : Parent.Id,
							EntityName = _name
						};
					IsAutoNamed = false;
					return _name = GenerateAutoName();
				}
//				if (string.IsNullOrEmpty(_name))
//					throw new HeirarchyException("Entity invalid: IsAutoNamed==false && Name == null")
//						{ ContextId = Parent == null ? null : Parent.Id };
				return _name;
			}
			set
			{
				if (value == null)
				{
					if (_parent != null)
						throw new HeirarchyException("Could not set entity name to null (for autonaming): It is already has a parent")
							{ ContextId = Parent.Id, EntityName = _name };
					IsAutoNamed = true;
					_name = null;
				}
				else if (value != _name)
					{
						if (_parent != null)
						{
							if (_parent.Contains(value))			// UpdateName(this, value)
								throw new HeirarchyException("Could not change entity name: Name already exists")
								{ ContextId = _parent.Id, EntityName = _name };
							_parent.Entities.UpdateName(this, value);			// update the EntityContext's collection
						}
						IsAutoNamed = false;
						_name = value;
						_id = null;
					}
			}
		}

		/// <summary>
		/// Id of this entity is the concatenation of the parent's Id, a period, and the entity name, or
		/// if this entity has no parent, Id is equal to the name
		/// </summary>
		public string Id {
			get
			{
				return _id != null ? _id :
					_id = _parent == null || _parent.IsRootContext ?
						Name : string.Concat(_parent.Id, EntityIdSeparator, Name);
			}
		}
		
		/// <summary>
		/// Parent entity contains this entity
		/// </summary>
		public EntityContext Parent {
			get { return _parent; }
			set
			{
				Debug.Assert (_owner == null || _parent == null || _parent._owner == _owner);
				if (value != _parent)
				{
					_parent = value;
					_id = null;
				}
			}
		}
		
		/// <summary>
		/// Owner is the top level entity in a heirarchy, and has no parent
		/// </summary>
		public EntityContext Owner {
			get
			{
				if (_owner == null)
				{
					Entity e = this;
					while (e.Parent != null)
						e = e.Parent;
					Debug.Assert(e is EntityContext);
					_owner = e as EntityContext;
				}
				return _owner;
			}
		}
		
		/// <summary>
		/// Gets a value indicating whether this Entity has been autonamed (either a name was never set at construction,
		/// or the name has deliberately been set to <c>null</c> to cause an auto name to be generated as required, when
		/// adding to an <see cref="JGL.Heirarchy.Context"/>)
		/// </summary>
		public bool IsAutoNamed { get; private set; }

		/// <summary>
		/// Gets a value indicating whether this <see cref="JGL.Heirarchy.Entity"/> is an <see cref="JGL.Heirarchy.EntityContext"/>
		/// </summary>
		public bool IsContext {
			get { return this is EntityContext; }		//.GetType().IsSubclassOf(typeof(EntityContext)); }
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="Entity"/> is an <see cref="EntityRootContext"/>
		/// </summary>
		public bool IsRootContext {
			get { return this is EntityRootContext; }
		}
		#endregion

		// This is going to mean I need to comment out and then tidy/clear out a fair few other bits of constructor code,
		// but since I changed (improved, I think, hope) the way Entity.Name decides if it is "auto-named" or not, it shouldn't
		// be necessary to define overriden constructors, with and without a string name argument.
		// If you want to explicitly set an entity's name, just set the property direclty using braces {} after the constructor
		/// <summary>
		/// Initializes a new <see cref="JGL.Heirarchy.Entity"/> instance
		/// <see cref="JGL.Heirarchy.Context.Entities"/>
		/// </summary>
		/// <param name="name">Entity Name</param>
//		public Entity (string name = null)
//		{
//			Name = name;
//		}

		#region Methods
		/// <summary>
		/// Gets this <see cref="JGL.Heirarchy.Entity"/>'s ID relative to the given <see cref="JGL.Heirarchy.EntityContext"/>,
		/// which should be an ancestor of this <see cref="JGL.Heirarchy.Entity"/> in the heirarchy.
		/// </summary>
		/// <returns>The ID of this <see cref="JGL.Heirarchy.Entity"/> relative to <paramref name="reference"/></returns>
		/// <param name='reference'><see cref="JGL.Heirarchy.Context"/> that the returned relative ID should be in relation to</param>
		/// <exception cref='ArgumentOutOfRangeException'>Thrown if <paramref name="reference"/> is not an ancestor of this <see cref="JGL.Heirarchy.Entity"/></exception>
		public string GetRelativeId(EntityContext reference)
		{
			Debug.Assert (reference != null && this.Id.StartsWith(reference.Id + EntityIdSeparator));
			if (!this.Id.StartsWith(reference.Id + EntityIdSeparator))
				throw new ArgumentOutOfRangeException("reference", reference, string.Concat ("Not an ancestor of Entity \"", this.Id, "\""));
			return this.Id.Substring ((reference.Id + EntityIdSeparator).Length);
		}

		/// <summary>
		/// Generates a base name for use when auto-naming <see cref="JGL.Heirarchy.Entity"/>s (ie when <see cref="JGL.Heirarchy.Entity.Name"/>
		/// has not been explicitly set, or it has been set to null
		/// </summary>
		/// <returns>
		/// The name.
		/// </returns>
		protected virtual string GenerateAutoName()
		{
			byte[] tickBytes = BitConverter.GetBytes(DateTime.Now.Ticks);
			short timeStamp = (short)(BitConverter.ToInt16(tickBytes, 0) + BitConverter.ToInt16(tickBytes, 2) +
				BitConverter.ToInt16(tickBytes, 4) + BitConverter.ToInt16(tickBytes, 6));
			return string.Format("{0} #{1:x4}", this.GetType().Name, timeStamp);
		}
		#endregion

		#region IXmlSerializable implementation
		public System.Xml.Schema.XmlSchema GetSchema()
		{
			throw new NotImplementedException();
		}

		public void ReadXml(XmlReader reader)
		{
			throw new NotImplementedException();
		}

		public void WriteXml(XmlWriter writer)
		{
			Type T = this.GetType();
//			writer
			writer.WriteStartElement(T.Name, XmlConvert.EncodeName(T.Namespace));
			WriteXmlContents(writer);
			if (IsContext)
			{
				EntityContext ec = this as EntityContext;
				foreach (Entity e in ec)
					e.WriteXml(writer);
			}
			writer.WriteEndElement();
		}

		public virtual void WriteXmlContents(XmlWriter writer)
		{
			if (!IsAutoNamed)
				writer.WriteAttributeString("name", Name);
		}

		#endregion
	}
}

