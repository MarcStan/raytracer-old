using Microsoft.Xna.Framework;
using System;

namespace Raytracer.SceneObjects
{
	/// <summary>
	/// Light entity that defines light sources within the scene.
	/// </summary>
	public class Light
	{
		/// <summary>
		/// The position of the light origin in the scene.
		/// </summary>
		public Vector3 Position { get; }

		/// <summary>
		/// The intensitiy of the light source. Defaults to 1f.
		/// </summary>
		public float Intensity { get; }

		/// <summary>
		/// The color of the light.
		/// </summary>
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