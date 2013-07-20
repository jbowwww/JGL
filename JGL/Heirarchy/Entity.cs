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

		#region Private fields and methods
		private EntityContext _owner = null;
		private EntityContext _parent = null;
		private string _name = null;
		private string _id = null;
		private bool _isAutoNamed = true;
		private static int _genNameCount = 0;
		#endregion
		
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
					Debug.Assert (e is EntityContext);
					_owner = e as EntityContext;
				}
				return _owner;
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
		/// Id of this entity is the concatenation of the parent's Id, a period, and the entity name, or
		/// if this entity has no parent, Id is equal to the name
		/// </summary>
		public string Id {
			get
			{
				string pid;
				return _parent == null || (pid = _parent.Id) == string.Empty ? Name : string.Concat(pid, ".", Name);
//				return _id != null ? _id :
//					_id = _parent == null || _parent.Id == string.Empty ?
//						Name : string.Concat (_parent.Id, ".", Name);
			}
		}
		
		/// <summary>
		/// Gets or sets the <see cref="JGL.Heirarchy.Entity"/> name
		/// </summary>
		[XmlElement("Name")]
		public string Name {
			get { return _name != null ? _name : _name = GenerateBaseName(); }
			set
			{
				Debug.Assert(!string.IsNullOrWhiteSpace(value));
				if (value != _name)
				{
					if (_parent != null)
					{
						if (value == null)
							throw new InvalidOperationException(string.Format(
								"Could not set entity \"{0}\" name to null (for entity autonaming) because it is contained by context \"{1}\"", _name != null ? _name : "(null)", Id));
						if (_parent.Contains(value))			// UpdateName(this, value)
							throw new InvalidOperationException("Could not change entity \"{0}\" name to \"{1}\" because the context \"{2}\" already has an entity with that name");
						_parent.UpdateName(this, value);			// update the EntityContext's collection
					}
					_name = value;
					_isAutoNamed = _name == null;
					_id = null;
				}
			}
		}

		/// <summary>
		/// Gets a value indicating whether this Entity has been autonamed (either a name was never set at construction,
		/// or the name has deliberately been set to <c>null</c> to cause an auto name to be generated as required, when
		/// adding to an <see cref="JGL.Heirarchy.Context"/>)
		/// </summary>
		public bool IsAutoNamed {
			get { return _isAutoNamed; }
		}

		/// <summary>
		/// Gets a value indicating whether this <see cref="JGL.Heirarchy.Entity"/> is an <see cref="JGL.Heirarchy.EntityContext"/>
		/// </summary>
		public bool IsContext {
			get { return this.GetType().IsSubclassOf(typeof(EntityContext)); }
		}

		#region Constructors
		/// <summary>
		/// Initializes a new <see cref="JGL.Heirarchy.Entity"/> instance without setting a name
		/// </summary>
		public Entity ()
		{

		}

		/// <summary>
		/// Initializes a new <see cref="JGL.Heirarchy.Entity"/> instance
		/// <see cref="JGL.Heirarchy.Context.Entities"/>
		/// </summary>
		/// <param name='name'>Entity Name</param>
		public Entity (string name)
		{
			Name = name;
		}
		#endregion

		/// <summary>
		/// Generates a base name for use when auto-naming <see cref="JGL.Heirarchy.Entity"/>s (ie when <see cref="JGL.Heirarchy.Entity.Name"/>
		/// has not been explicitly set, or it has been set to null
		/// </summary>
		/// <returns>
		/// The name.
		/// </returns>
		public virtual string GenerateBaseName()
		{
//			return string.Format ("AutoEntity:{0}#{1:d4}", this.GetType ().Name, _genNameCount++);
			_genNameCount++;
			return this.GetType().Name;
		}

		/// <summary>
		/// Gets this <see cref="JGL.Heirarchy.Entity"/>'s ID relative to the given <see cref="JGL.Heirarchy.EntityContext"/>,
		/// which should be an ancestor of this <see cref="JGL.Heirarchy.Entity"/> in the heirarchy.
		/// </summary>
		/// <returns>The ID of this <see cref="JGL.Heirarchy.Entity"/> relative to <paramref name="reference"/></returns>
		/// <param name='reference'><see cref="JGL.Heirarchy.Context"/> that the returned relative ID should be in relation to</param>
		/// <exception cref='ArgumentOutOfRangeException'>Thrown if <paramref name="reference"/> is not an ancestor of this <see cref="JGL.Heirarchy.Entity"/></exception>
		public string GetRelativeId(EntityContext reference)
		{
			Debug.Assert (reference != null && this.Id.StartsWith(reference.Id + "."));
			if (!this.Id.StartsWith(reference.Id + "."))
				throw new ArgumentOutOfRangeException("reference", reference, string.Concat ("Not an ancestor of Entity \"", this.Id, "\""));
			return this.Id.Substring ((reference.Id + ".").Length);
		}

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

