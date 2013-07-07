using System;
using System.IO;
using System.Xml;
using System.Xml.Linq;

using JGL;
using JGL.Heirarchy;
//using JGL.Data;
//using JGL.Serialization;

namespace SerializationTests
{
	class MainClass
	{
		public static void Test1()
		{
			TestObject1 testObj1 = new TestObject1();
			JGL.Data.Serializer s = new JGL.Data.XmlSerializer();
			using (FileStream fs = File.Open ("out.xml", FileMode.Create))
			{
				s.Serialize (testObj1, fs);
			}
			
			Console.WriteLine (string.Format ("{0}.InstanceManager: {1} instances", s.GetType ().FullName, s.Instances.Count));
			Console.WriteLine ();
			Console.WriteLine ("Flat instance collection:");
			foreach (ComplexInstance instance in s.Instances)
			{
				Console.WriteLine (instance.ToString (0, 0));
			}
			Console.WriteLine ();
			Console.WriteLine ("Heirarchical instance collection:");
			Console.WriteLine (s.Instances[0].ToString(0, -1));
			Console.WriteLine ();
//			Console.WriteLine ("Heirarchical instance collection XML:");
//			Console.WriteLine (s.Instances.Nodes ());
//			Console.WriteLine ();
		}
		
		public static void Test2()
		{
//			Scene scene = new Scene() { Name = "Scene01" };
//			Sector s1 = new Sector() { Name = "sector01"};
//			scene.Entities.Add (s1);
//			
//			XmlSerializer xs = new XmlSerializer();
//			using (FileStream s = File.Open ("out_scene.xml", FileMode.Create))
//			{
//				xs.Serialize (scene, s);
//			}
		}
		
		public static void DeserializeScene()
		{
			Scene scene;
			XmlSerializer xs = new XmlSerializer();
			using (Stream s = File.Open ("/home/jk/Code/JGL/Data/XML/out_scene.xml", FileMode.Open))
			{
				scene = xs.Deserialize (s) as Scene;
			}
			
		}
	
		public static Scene BuildScene()
		{
			return new Scene(
				new Sector(
					new JGL.Heirarchy.MeshObject(
						new Mesh() { Name = "Mesh#1" },
						new Mesh() { Name = "Mesh#2" })),
				new Sector(
					new JGL.Heirarchy.MeshObject(
						new Mesh() {Name="Mesh#2.1"}))
				);
		}
		
		public static void TestXmlFormatter()
		{
//			using (FileStream fs = File.Open ("out.xf.xml", FileMode.Create))
//			{
//				XmlFormatter xf = new XmlFormatter();
//				xf.Serialize(fs, xf);
//			}
		}
		
		public static void Main (string[] args)
		{
			try
			{
	//			Test2();
	//			DeserializeScene ();
				
				Scene scene = BuildScene ();
				;
				
				
			}
			catch (Exception ex)
			{
				Console.WriteLine (ex.ToString ());
				Console.ReadLine();
			}
		}
	}
}
