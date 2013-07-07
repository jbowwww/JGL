using System;

namespace JGL.Geometry
{
	public class Vertex : Vector3d
	{
		public Vertex() {}
		
		public Vertex(double x, double y, double z)
			: base (x, y, z) {}
		
		public Vertex(double[] components)
			: base (components) {}
	}
}

