using Microsoft.Xna.Framework;

namespace Raytracer
{
	/// <summary>
	/// Defines a surface and its reflective values.
	/// </summary>
	public interface ISurface
	{
		/// <summary>
		/// Returns the color vector of the reflection at the given location.
		/// </summary>
		/// <param name="position"></param>
		/// <returns></returns>
		Vector3 Reflect(Vector3 position);
	}
}