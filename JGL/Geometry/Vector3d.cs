using System;
using OpenTK.Graphics.OpenGL;
using JGL.Debugging;

namespace JGL.Geometry
{
	public class Vector3d
	{
		/// <summary>
		/// Tracing
		/// </summary>
		public static readonly AutoTraceSource Trace = AutoTraceSource.GetOrCreate(AsyncXmlFileTraceListener.GetOrCreate("JGL"));

		private double[] _components;

		public double[] Components {
			get { return _components; }
		}

		public double X {
			get { return _components[0]; }
			set { _components[0] = value; }
		}
		
		public double Y {
			get { return _components[1]; }
			set { _components[1] = value; }
		}

		public double Z {
			get { return _components[2]; }
			set { _components[2] = value; }
		}
		
		public Vector3d(double x, double y, double z)
		{
			Set(x, y, z);
		}
		
		public Vector3d(double[] components)
		{
			Set(components);
		}

		public Vector3d(Vector3d source)
		{
			Set(source);
		}

		public Vector3d Set(double x, double y, double z)
		{
			_components = new double[] { x, y, z };
			return this;
		}

		public Vector3d Set(double[] components)
		{
			_components = components;
			return this;
		}

		public Vector3d Set(Vector3d source)
		{
			_components = source._components;
			return this;
		}

		public static Vector3d operator+ (Vector3d v1, Vector3d v2)
		{
			return new Vector3d(v1.X + v2.X, v1.Y + v2.Y, v1.Z + v2.Z);
		}

		public static Vector3d operator- (Vector3d v1, Vector3d v2)
		{
			return new Vector3d(v1.X - v2.X, v1.Y - v2.Y, v1.Z - v2.Z);
		}

		public static Vector3d operator* (Vector3d v, double s)
		{
			return new Vector3d(v.X * s, v.Y * s, v.Z * s);
		}

		public static Vector3d operator/ (Vector3d v, double s)
		{
			return new Vector3d(v.X / s, v.Y / s, v.Z / s);
		}
		
		static public implicit operator Vector3d(double[] components)
		{
			return new Vector3d(components);
		}

		public override bool Equals(object obj)
		{
			if (obj == null || !(obj is Vector3d))
				return false;
			Vector3d v = obj as Vector3d;
			return X == v.X && Y == v.Y && Z == v.Z;
		}

		/// <summary>
		/// Serves as a hash function for a <see cref="JGL.Geometry.Vector3d"/> object.
		/// </summary>
		/// <returns>
		/// A hash code for this instance that is suitable for use in hashing algorithms and data structures such as a hash table.
		///	-	This is a giant stabby hack in the dark, I have no idea if it is at all suitable as a hash function
		/// 		(When is it required? Sorting and comparing in dictionaries and such??)
		/// </returns>
		public override int GetHashCode()
		{
			return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode();
		}

		public override string ToString()
		{
			return string.Format("Vector3d({0}, {1}, {2})", X, Y, Z);
		}
	}
}

