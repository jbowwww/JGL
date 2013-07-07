using System;

namespace JGL.Geometry
{
	public class Normal : Vector3d
	{
		public Normal() {}
		
		public Normal(double x, double y, double z)
			: base (x, y, z) {}
		
		public Normal(double[] components)
			: base (components) {}
	}
}

