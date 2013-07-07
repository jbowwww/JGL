using System;
//using JGL.Data;

namespace SerializationTests
{
	[JGL.Data.Contract]
	public class TestObject1
	{
		[JGL.Data.Member(Order = 1)]	//, CustomProperties = new string[] { "XmlSerializer:Attribute" })]
		public string Name = "TestName";

		[JGL.Data.Member(Order = 2)]	//, CustomProperties = new string[] { "XmlSerializer:Attribute" })]
		public DateTime TimeStamp = DateTime.Now;

		[JGL.Data.Member(Order = 3)]	//, CustomProperties = new string[] { "XmlSerializer:Attribute" })]
		public int x = 1;

		[JGL.Data.Member(Order = 4)]
		public int y = 2;

		[JGL.Data.Member(Order = 5)]
		public int z = 3;

		[JGL.Data.Member(HideRoot = true, Order = 10)]
		public int[] items = new int[] { 3, 8, 1, 3 };

		public TestObject1 ()
		{
		}
	}
}

