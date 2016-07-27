using Raytracer.SceneObjects;
using System;

namespace Raytracer
{
	public struct Intersection
	{
		public ISceneObject IntersectedObject { get; }

		public float Distance { get; }

		public Intersection(ISceneObject o, float d)
		{
			if (o == null)
			{
				throw new ArgumentNullException(nameof(o));
			}
			// 0 is legal if we hit an object with the camera
			if (d < 0)
			{
				throw new ArgumentOutOfRangeException(nameof(d));
			}

			IntersectedObject = o;
			Distance = d;
		}
	}
}