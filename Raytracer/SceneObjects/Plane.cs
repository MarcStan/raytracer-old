using Microsoft.Xna.Framework;
using System;
using XPlane = Microsoft.Xna.Framework.Plane;
namespace Raytracer.SceneObjects
{
	public class Plane : ISceneObject
	{
		private readonly XPlane _plane;

		public Plane(Vector3 normal, float d, ISurface surface)
		{
			if (surface == null)
			{
				throw new ArgumentNullException(nameof(surface));
			}

			Surface = surface;
			_plane = new XPlane(normal, d);
		}
		public ISurface Surface { get; }

		public float? Intersects(Ray ray)
		{
			return ray.Intersects(_plane);
		}

		public Vector3 Normal(Vector3 position)
		{
			return _plane.Normal;
		}
	}
}