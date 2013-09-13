using System;

namespace SmallConsoleTests
{
	public struct V3D
	{
//		double[] _components = { 0, 0, 0 };
//
//		double X {
//			get { return _components[0]; }
//			set { _components[0] = value; }
//		}
//
//		double Y {
//			get { return _components[1]; }
//			set { _components[1] = value; }
//		}
//
//		double Z {
//			get { return _components[2]; }
//			set { _components[2] = value; }
//		}
//
//		public V3D(double x, double y, double z)
//		{
//			_components = new[] { x, y, z };
//		}

		public double X, Y, Z;

		public static implicit operator V3D(double x, double y, double z)
		{
			V3D v3d;
			v3d.X = x;
			v3d.Y = y;
			v3d.Z = z;
			return v3d;
		}

		public override string ToString()
		{
			return string.Format("V3D({0}, {1}, {2})", X, Y, Z);
		}
	}
}

