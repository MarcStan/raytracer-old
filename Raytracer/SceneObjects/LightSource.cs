using Microsoft.Xna.Framework;

namespace Raytracer.SceneObjects
{
	public class LightSource : ISceneObject
	{
		public Light Light { get; }
		private readonly Sphere _sphere;

		public LightSource(Light light)
		{
			Light = light;
			_sphere = new Sphere(light.Position, 0.1f);
		}

		public Vector3 Normal(Vector3 position)
		{
			throw new System.NotImplementedException();
		}

		public float? Intersects(Ray ray)
		{
			return _sphere.Intersects(ray);
		}
	}
}