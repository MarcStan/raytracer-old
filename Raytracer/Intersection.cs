using Raytracer.SceneObjects;
using System;

namespace Raytracer
{
	/// <summary>
	/// Object holding data about an intersection that occured.
	/// </summary>
	public struct Intersection
	{
		/// <summary>
		/// The object that the ray intersected with.
		/// </summary>
		public ISceneObject IntersectedObject { get; }

		/// <summary>
		/// The distance along the ray where the intersection occured.
		/// </summary>
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