using Microsoft.Xna.Framework;

namespace Raytracer
{
	/// <summary>
	/// Defines a surface and its reflective values.
	/// </summary>
	public interface ISurface
	{
		/// <summary>
		/// Returns the power of the reflection at the given location.
		/// </summary>
		/// <param name="position"></param>
		/// <returns></returns>
		float Reflect(Vector3 position);

		/// <summary>
		/// Returns the diffuse color of the reflection at the given location.
		/// </summary>
		/// <param name="position"></param>
		/// <returns></returns>
		Vector3 Diffuse(Vector3 position);

		/// <summary>
		/// Returns the specular color of the reflection at the given location.
		/// </summary>
		/// <param name="position"></param>
		/// <returns></returns>
		Vector3 Specular(Vector3 position);


		float Shininess { get; }
	}
}