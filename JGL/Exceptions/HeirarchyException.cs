using System;
using System.Runtime.Serialization;

namespace JGL
{
	public class HeirarchyException : ApplicationException
	{
		/// <summary>
		/// Optional (but really should always be supplied, I think) identifier of the <see cref="JGL.Heirarchy.EntityContext"/>
		/// that the exception occurred in
		/// </summary>
		public string ContextId { get; set; }

		/// <summary>
		/// Optional name of an <see cref="JGL.Heirarchy.Entity"/> that the exception relates to
		/// </summary>
		public string EntityName { get; set; }

		public HeirarchyException() : base("A heirarchy exception has occurred.") { }

		public HeirarchyException(string message) : base(message) { }

		public HeirarchyException(string message, Exception innerException) : base(message, innerException) { }

		protected HeirarchyException(SerializationInfo info, StreamingContext context) : base(info, context) { }

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			if (!string.IsNullOrEmpty(ContextId))
				info.AddValue("ContextId", ContextId, typeof(int));
			if (!string.IsNullOrEmpty(EntityName))
				info.AddValue("EntityName", EntityName, typeof(int));
		}
	}
}

