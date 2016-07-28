using Microsoft.Xna.Framework;
using System;

namespace Raytracer.Surfaces
{
	public class CheckerboardSurface : ISurface
	{
		public float Reflect(Vector3 position)
		{
			return 0.5f;
		}

		public Vector3 Diffuse(Vector3 position)
		{
			return (Math.Floor(position.X) + Math.Floor(position.Z)) % 2 == 0
				? Vector3.One
				: Vector3.Zero;
		}

		public Vector3 Specular(Vector3 position)
		{
			return Vector3.One;
		}

		public float Shininess => 150;
	}
}