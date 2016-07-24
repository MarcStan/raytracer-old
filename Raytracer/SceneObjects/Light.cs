using Microsoft.Xna.Framework;
using System;

namespace Raytracer.SceneObjects
{
	public class Light
	{
		public Vector3 Position { get; }

		public float Intensity { get; }

		public Color Color { get; }

		public Light(Vector3 pos, Color col, float intensity = 1f)
		{
			if (intensity <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(intensity));
			}

			Position = pos;
			Color = col;
			Intensity = intensity;
		}
	}
}