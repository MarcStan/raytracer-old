using Microsoft.Xna.Framework;

namespace Raytracer.SceneObjects
{
	public interface ISceneObject
	{
		/// <summary>
		/// The surface of the current object.
		/// </summary>
		ISurface Surface { get; }

		/// <summary>
		/// Returns whether the given object intersects with the ray or not.
		/// </summary>
		/// <param name="ray"></param>
		/// <returns>Null if no intersection, otherwise returns the distance from the origin of the ray.
		/// The intersection point can thus be found as ray.Start + ray.Direction * distance.</returns>
		float? Intersects(Ray ray);

		/// <summary>
		/// Returns the normal for the specific point on the objects surface.
		/// </summary>
		/// <param name="position"></param>
		/// <returns></returns>
		Vector3 Normal(Vector3 position);
	}
}