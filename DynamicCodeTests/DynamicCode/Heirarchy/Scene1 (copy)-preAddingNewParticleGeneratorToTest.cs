using System;
using System.Linq;
using System.Runtime;
using System.Reflection;

using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

using JGL.Heirarchy;
using JGL.Heirarchy.Resources;
using JGL.Heirarchy.Library;
using JGL.Heirarchy.Behaviours;

namespace Dynamic
{
	[Serializable]
	public class Scene1 : Scene
	{
		public Scene1()		// : base()
		{
			base.Name = "Scene1";
			Init();
		}

//		public Scene1()		//(string name) : base(name)
//		{
//			Init();
//		}

		public void Init()
		{
			Add(
				new Origin(),
				new Tree1() { Position = new Vector3d(3, 0, -13), Rotation = new Vector3d(0.15, 68, 0) },
				new Tree1() { Position = new Vector3d(-2, -0.12, -10), Rotation = new Vector3d(-0.2, -30, 0) },
				new Tree1() { Position = new Vector3d(1.25, -0.3, -8), Rotation = new Vector3d(1, 2, 0) },
				new WhiteLight() { Position = new Vector3d(3.5, 3.5, -1) },
				new WhiteLight(0.68f, 0.22f, 0.338f) { Position = new Vector3d(-0.2, 1.8, 2) },
				new Box(3, 2, 1)
				{
					Position = new Vector3d(-2, 1, 0.5),
					Material = new Material("White")
					{
						TextureAmbient = Resource.Get<Texture>("test1.png")				//new Texture("test1.png")
					},
					TwoSided = true
				},
				new Quad(7.8, 7.2)
				{
					Position = new Vector3d(+1.2, -0.042, -0.55),
					Material = new Material("tex2") { TextureAmbient = Resource.Get<Texture>("tex2.png") },				//new Texture("tex2.png") },
					TwoSided = true
				});
			DefaultCamera.Position = new Vector3d(0.5, 0.75, 8);

			Gravity gravity = new Gravity();
			foreach (JGL.Heirarchy.Object o in this.Entities.OfType<JGL.Heirarchy.Object>())
				gravity.Entities.Add(o);
			Behaviours.Add(gravity);
		}
	}
}