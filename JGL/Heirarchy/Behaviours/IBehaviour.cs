namespace JGL.Heirarchy.Behaviours
{
	/// <summary>
	/// Defines a behaviour that belongs to an owner <see cref="JGL.Heirarchy.Scene"/> and
	/// can operate on one or many <see cref="JGL.Heirarchy.Entity"/>s
	/// </summary>
	public interface IBehaviour
	{
		/// <summary>
		/// Perform the implemented behaviour by process the supplied <paramref name="entity"/>
		/// </summary>
		/// <remarks>See <see cref="JGL.Heirarchy.Behaviours.Behaviour"/></remarks>
		void Process();

		// Do i need this? Might be a pain in the ass for behaviours that don't need to use it - maybe make it an
		// empty virtual method in Behaviour abstract class
//		void UnapplyFrom(JGL.Heirarchy.Entity entity);
	}
}

