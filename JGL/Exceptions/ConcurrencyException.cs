using System;
using System.Runtime.Serialization;

namespace JGL
{
	/// <summary>
	/// Concurrency exception.
	/// </summary>
	public class ConcurrencyException : ApplicationException
	{
		public int RetryCount { get; set; }

//		public int? RetryDelayMs { get; set; }

		public int RetryDelayCycles { get { return Engine.Options.ConcurrentCollectionOperationRetryDelayCycles; } }

		public ConcurrencyException() : base("A concurrency exception has occurred.") { }

		public ConcurrencyException(string message) : base(message) { }

		public ConcurrencyException(string message, Exception innerException) : base(message, innerException) { }

		protected ConcurrencyException(SerializationInfo info, StreamingContext context) : base(info, context) { }

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("RetryCount", RetryCount, typeof(int));
			info.AddValue("RetryDelayCycles", RetryDelayCycles, typeof(int));
		}
	}
}

