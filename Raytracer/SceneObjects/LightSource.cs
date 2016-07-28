using Microsoft.Xna.Framework;
using Raytracer.Surfaces;

namespace Raytracer.SceneObjects
{
	/// <summary>
	/// Debug entity to visualize a light source.
	/// This entity will only be rendered and the surface will not be used.
	/// </summary>
	public class LightSource : ISceneObject
	{
		public Light Light { get; }

		private readonly Sphere _sphere;

		public LightSource(Light light)
		{
			Light = light;
			_sphere = new Sphere(light.Position, 0.1f, new BasicSurface());
		}

		public Vector3 Normal(Vector3 position)
		{
			throw new System.NotImplementedException();
		}

		public ISurface Surface => null;

		public float? Intersects(Ray ray)
		{
			return _sphere.Intersects(ray);
		}
	}
}