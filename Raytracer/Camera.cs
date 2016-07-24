using Microsoft.Xna.Framework;

namespace Raytracer
{
	public class Camera
	{
		private readonly Vector3 _position;
		private readonly Vector3 _lookAt;
		private Vector3 _direction;


		public Camera(Vector3 position, Vector3 lookAt)
		{
			_position = position;
			_lookAt = lookAt;
			_direction = lookAt - position;
			_direction.Normalize();
		}

		/// <summary>
		/// Returns a ray that starts at the camera position and looks towards the target offseted by x and y.
		/// </summary>
		/// <param name="x">Value between 0 and width.</param>
		/// <param name="y">Value between 0 and height.</param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <returns></returns>
		public Ray GetRayForRasterPosition(int x, int y, int width, int height)
		{
			return new Ray(_position, _direction);
		}
	}
}