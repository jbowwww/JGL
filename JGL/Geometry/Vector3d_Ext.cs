using System;
using OpenTK;
using JGL.Debugging;

namespace JGL.Geometry
{
	public static class Vector3d_Ext
	{
		public static Vector3d Set(this Vector3d v, double x, double y, double z)
		{
			v.X = x;
			v.Y = y;
			v.Z = z;
			return v;
		}

//		static public implicit operator Vector3d(double x, double y, double z)
//		{
//			return new Vector3d(x, y, z);
//		}
//		
//		static public implicit operator Vector3d(double[] components)
//		{
//			return new Vector3d(components);
//		}
	}
}

