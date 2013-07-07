using System;
using OpenTK.Graphics.OpenGL;
using JGL.Debugging;

namespace JGL.Geometry
{
	public class Vector3d
	{
		#region Private fields
		private double[] _components = new double[] { 0, 0, 0 };
		private bool _isModified = true;
		#endregion
		
		public bool IsModified {
			get
			{
				if (_isModified)
				{
					_isModified = false;
					return true;
				}
				return false;
			}
			private set
			{
				_isModified = value;
			}
		}
					
		public double X {
			get { return _components[0]; }
			set { _components[0] = value; IsModified = true; }
		}
		
		public double Y {
			get { return _components[1]; }
			set { _components[1] = value; IsModified = true; }
		}
		
		public double Z {
			get { return _components[2]; }
			set { _components[2] = value; IsModified = true; }
		}
		
		public Vector3d()
		{
		}
		
		public Vector3d(double x, double y, double z)
		{
//			_x = x;
//			_y = y;
//			_z = z;
			_components = new double[] { x, y, z };
		}
		
		public Vector3d(double[] components)
		{
			Debug.Assert(components.Length == 3);
//			_x = components[0];
//			_y = components[1];
//			_z = components[2];
			components.CopyTo(_components, 0);
		}
		
		public double[] GetComponents()
		{
			return _components;	
		}
		
		static public implicit operator Vector3d(double x, double y, double z)
		{
			return new Vector3d(x, y, z);
		}
		
		static public implicit operator Vector3d(double[] components)
		{
			return new Vector3d(components);
		}
	}
}

