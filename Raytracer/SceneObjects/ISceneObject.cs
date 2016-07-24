using Microsoft.Xna.Framework;

namespace Raytracer.SceneObjects
{
	public interface ISceneObject
	{
		/// <summary>
		/// Returns whether the given object intersects with the ray or not.
		/// </summary>
		/// <param name="ray"></param>
		/// <returns>Null if no intersection, otherwise returns the distance from the origin of the ray.
		/// The intersection point can thus be found as ray.Start + ray.Direction * distance.</returns>
		float? Intersects(Ray ray);
	}
}