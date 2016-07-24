using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Raytracer
{
	public class Camera
	{
		private readonly GraphicsDevice _device;
		private readonly Vector3 _position;
		private readonly Vector3 _lookAt;
		private readonly Vector3 _direction;

		public Camera(GraphicsDevice device, Vector3 position, Vector3 lookAt)
		{
			_device = device;
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
			var down = new Vector3(0, -1, 0);
			var v1 = Vector3.Cross(_direction, down);
			v1.Normalize();
			var right = v1 * _device.Viewport.AspectRatio;
			var v2 = Vector3.Cross(_direction, right);
			v2.Normalize();
			var up = v2;

			// x is in range 0 - width, we need it to be -1 to 1
			var scalarOffsetX = -1f + x / (float)width * 2f;
			// y is in range 0 - height, we need it to be -1 to 1
			var scalarOffsetY = -1f + y / (float)height * 2f;
			var dir = _direction + scalarOffsetX * right + scalarOffsetY * up;
			dir.Normalize();
			return new Ray(_position, dir);
		}
	}
}