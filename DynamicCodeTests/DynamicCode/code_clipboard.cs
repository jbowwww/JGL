System.IO.Stream s = System.IO.File.OpenWrite("/home/jk/scene.xml");
System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(typeof(DynamicCodeTests.Scene1), new Type[] { typeof(DynamicCodeTests.Origin), typeof(DynamicCodeTests.Tree1), typeof(DynamicCodeTests.WhiteLight), typeof(JGL.Heirarchy.Scene), typeof(JGL.Heirarchy.MeshObject), typeof(JGL.Heirarchy.EntityContext), typeof(JGL.Heirarchy.Entity) });
System.Xml.Serialization.XmlSerializer xs = new System.Xml.Serialization.XmlSerializer(typeof(JGL.Heirarchy.Scene));
xs.Serialize(s, Scenes[0]);
s.Close();

