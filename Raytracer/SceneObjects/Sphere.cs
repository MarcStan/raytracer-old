using Microsoft.Xna.Framework;
using System;

namespace Raytracer.SceneObjects
{
	public class Sphere : ISceneObject
	{
		private readonly BoundingSphere _sphere;

		public Sphere(Vector3 pos, float r)
		{
			if (r <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(r));
			}

			_sphere = new BoundingSphere(pos, r);
		}

		public float? Intersects(Ray ray)
		{
			return ray.Intersects(_sphere);
		}
	}
}