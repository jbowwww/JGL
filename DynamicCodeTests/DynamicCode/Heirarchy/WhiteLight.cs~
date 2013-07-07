using System;
using OpenTK.Graphics;
using JGL.Heirarchy;

namespace Dynamic
{
	public class WhiteLight : Light
	{
		public WhiteLight()
		{
			Init();
		}

		public WhiteLight(float scaleAmbient = 1, float scaleDiffuse = 1, float scaleSpecular = 1)
		{
			Init(scaleAmbient, scaleDiffuse, scaleSpecular);
		}

		public void Init(float scaleAmbient = 1, float scaleDiffuse = 1, float scaleSpecular = 1)
		{
			Ambient = new Color4(0.08f * scaleAmbient, 0.08f * scaleAmbient, 0.08f * scaleAmbient, 1);
			Diffuse = new Color4(0.67f * scaleDiffuse, 0.67f * scaleDiffuse, 0.67f * scaleDiffuse, 1);
			Specular = new Color4(0.77f * scaleSpecular, 0.77f * scaleSpecular, 0.77f * scaleSpecular, 1);
		}
	}
}

