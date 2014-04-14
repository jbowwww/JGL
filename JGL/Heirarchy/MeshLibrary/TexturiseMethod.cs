using System;

namespace JGL.Heirarchy.MeshLibrary
{
	/// <summary>
	/// An abstract representation of a "Texturise" method. Generates texture coordinates and
	/// applies them to a specific type of mesh (e.g. <see cref="Box"/>, <see cref="Quad"/>)
	/// </summary>
	/// <typeparam name="T">A <see cref="Mesh"/>-derived type that this texturise method operates on</typeparam>
	public abstract class TexturiseMethod<TMesh>
		where TMesh : Mesh
	{
		/// <summary>
		/// Texturise the specified mesh.
		/// </summary>
		/// <param name="mesh">A <see cref="Mesh"/>-derived instance to be texturised</param>
		public abstract void Texturise(TMesh mesh);

		/// <summary>
		/// Initializes a new instance of the <see cref="TexturiseMethod`1"/> class.
		/// </summary>
//		public TexturiseMethod()
//		{
//		}
	}
}
