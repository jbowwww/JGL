using System;
using System.Collections;
using JGL.Geometry;

namespace SmallConsoleTests
{
	public static class MainClass
	{
		internal static void Write(IEnumerable objs)
		{
			foreach (object o in objs)
			{
				if (o != null)
					Console.WriteLine(o.ToString());
			}
		}

		internal static void TestVector3dClass()
		{
			Vector3d[] v = new Vector3d[8];

			v[0] = new Vector3d(0, 0, 0);
			v[1] = new Vector3d(2, 2, 2);
			v[2] = new Vector3d(new double[] { 3, 3, 3 });
			v[3] = new Vector3d(v[2] * 2);
			v[4] = new[] { 4.4d, 4.4, 4.4 };
			v[5] = new[] { 5.5 , 5.5, 5.5 };
			Write(v);

			v[0].Set(10, 10, 10);
			double[] cmps = v[1].Components;
			cmps = new[] { 20d , 20d, 20d };				// confirms the array that Components property is indeed a value type
			v[2] += new Vector3d(30, 30, 30);
			v[3] = v[0] * 5.5;
			v[5] = (v[4] = v[3] / 5);
			Write(v);
		}

		internal static void TestVector3dStruct()
		{
			V3D[] v = new V3D[8];
			

						Write(v);
			//			v[0] = new;
		}

		public static void Main(string[] argv)
		{
			TestVector3dClass ();

//			TestVector3dStruct ();

			JGL.Debugging.AutoTraceSource.StopTraceThread();
		}
	}
}

