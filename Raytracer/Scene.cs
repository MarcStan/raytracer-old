using Microsoft.Xna.Framework;
using Raytracer.SceneObjects;
using System;
using System.Collections.Generic;

namespace Raytracer
{
	/// <summary>
	/// Describes a scene that can be raytraced.
	/// </summary>
	public class Scene
	{
		private readonly List<ISceneObject> _sceneObjects;
		private readonly List<Light> _lights;

		public Scene()
		{
			_sceneObjects = new List<ISceneObject>();
			_lights = new List<Light>();
		}

		public IReadOnlyList<ISceneObject> SceneObjects => _sceneObjects;

		public IReadOnlyList<Light> Lights => _lights;

		/// <summary>
		/// Adds a new light to the scene.
		/// </summary>
		/// <param name="l"></param>
		public void Add(Light l)
		{
			if (l == null)
			{
				throw new ArgumentNullException(nameof(l));
			}

			_lights.Add(l);
		}

		/// <summary>
		/// Adds a new object to the scene.
		/// </summary>
		/// <param name="o"></param>
		public void Add(ISceneObject o)
		{
			if (o == null)
			{
				throw new ArgumentNullException(nameof(o));
			}

			_sceneObjects.Add(o);
		}

		/// <summary>
		/// Clears all objects and lights from the scene.
		/// </summary>
		public void Clear()
		{
			_lights.Clear();
			_sceneObjects.Clear();
		}

		/// <summary>
		/// Returns an unsorted list of all objects that have been hit by the ray.
		/// </summary>
		/// <param name="ray"></param>
		/// <returns></returns>
		public List<Intersection> GetIntersections(Ray ray)
		{
			var intersections = new List<Intersection>();
			foreach (var o in SceneObjects)
			{
				var d = o.Intersects(ray);
				if (d.HasValue)
					intersections.Add(new Intersection(o, d.Value));
			}
			return intersections;
		}
	}
}