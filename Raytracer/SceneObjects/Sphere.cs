using Microsoft.Xna.Framework;
using System;

namespace Raytracer.SceneObjects
{
	public class Sphere : ISceneObject
	{
		private readonly BoundingSphere _sphere;

		public Sphere(Vector3 pos, float r, ISurface surface)
		{
			if (r <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(r));
			}
			if (surface == null)
			{
				throw new ArgumentNullException(nameof(surface));
			}

			_sphere = new BoundingSphere(pos, r);
			Surface = surface;
		}

		public ISurface Surface { get; }

		public float? Intersects(Ray ray)
		{
			return ray.Intersects(_sphere);
		}

		public Vector3 Normal(Vector3 position)
		{
			return Vector3.Normalize(_sphere.Center - position);
		}
	}
}