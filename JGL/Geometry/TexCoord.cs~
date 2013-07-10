using System;
using JGL.Debugging;

namespace JGL.Geometry
{
	public class TexCoord
	{
		/// <summary>
		/// Tracing
		/// </summary>
		public static readonly AutoTraceSource Trace = AutoTraceSource.GetOrCreate(AsyncFileTraceListener.GetOrCreate("JGL"));
		private bool _isModified = true;
		
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
		
//		private double _u = 0, _v = 0;
		private double[] _components = new double[] { 0, 0 };
		
		private double U {
			get { return _components[0]; }
			set { _components[0] = value; IsModified = true; }
		}
		
		private double V {
			get { return _components[1]; }
			set { _components[1] = value; IsModified = true; }
		}
		
		public TexCoord() {}
		
		public TexCoord(double u, double v)
		{
//			_u = u;
//			_v = v;
			_components = new double[] { u, v };
		}
		
		public TexCoord(double[] components)
		{
//			_u = components[0];
//			_v = components[1];
			components.CopyTo(_components, 0);
		}
		
		public double[] GetComponents()
		{
			return _components;
		}
	}
}

