using Microsoft.Xna.Framework;

namespace Raytracer
{
	public class BasicSurface : ISurface
	{
		public Vector3 Reflect(Vector3 position)
		{
			return Vector3.One;
		}
	}
}