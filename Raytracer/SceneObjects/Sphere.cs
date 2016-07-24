using Microsoft.Xna.Framework;
using System;

namespace Raytracer.SceneObjects
{
	public class Sphere : ISceneObject
	{
		private Vector3 _position;
		private float _radius;

		public Sphere(Vector3 pos, float r)
		{
			if (r <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(r));
			}

			_position = pos;
			_radius = r;
		}

		public float? Intersects(Ray ray)
		{
			return null;
		}
	}
}