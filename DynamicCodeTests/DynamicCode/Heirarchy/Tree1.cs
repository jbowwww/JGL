using System;
using System.Xml;
using System.Xml.Serialization;
using JGL.Heirarchy;

namespace Dynamic
{
	public class Tree1 : Mesh
	{
		public Tree1() : base("/home/jk/Code/JGL/Data/Models/tree.obj", false)
		{
		}
	}
}

