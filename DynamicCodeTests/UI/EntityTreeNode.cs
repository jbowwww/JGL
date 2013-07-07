using System;
using Gtk;
using JGL.Heirarchy;

namespace Dynamic.UI
{
	[Gtk.TreeNode]
	class EntityTreeNode : TreeNode
	{
		public readonly Entity Entity;
		
		public EntityTreeNode(Entity entity)
		{
			Entity = entity;
		}
		
		[Gtk.TreeNodeValue(Column=0)]
		public string Name {
			get { return Entity.Name; }
		}
		
		[Gtk.TreeNodeValue(Column=1)]
		public string Type {
			get { return Entity.GetType().ToString(); }
		}
	}
}

