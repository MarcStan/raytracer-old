using Microsoft.Xna.Framework;

namespace Raytracer.Surfaces
{
	/// <summary>
	/// Degault surface that is reflective.
	/// </summary>
	public class BasicSurface : ISurface
	{
		public float Reflect(Vector3 position)
		{
			return 0.5f;
		}

		public Vector3 Diffuse(Vector3 position)
		{
			return new Vector3(1f);
		}

		public Vector3 Specular(Vector3 position)
		{
			return new Vector3(0.1f);
		}

		public float Shininess => 200;
	}
}