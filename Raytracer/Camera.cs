using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Raytracer
{
	public class Camera
	{
		private readonly GraphicsDevice _device;
		private Vector3 _position;
		private Vector3 _direction;
		private readonly Vector3 _initialDirection = new Vector3(-1, 0, 0);
		private float _rotateHorizontal, _rotateVertical;

		private readonly Vector3 _unitUp = new Vector3(0, 1, 0);

		/// <summary>
		/// Creates a new camera that looks into -Z direction by default
		/// </summary>
		/// <param name="device"></param>
		/// <param name="position"></param>
		public Camera(GraphicsDevice device, Vector3 position)
		{
			_device = device;
			_position = position;
			_direction = _initialDirection;
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
			var right = Right;
			var up = Up;

			// x is in range 0 - width, we need it to be -1 to 1
			var scalarOffsetX = -1f + x / (float)width * 2f;
			// y is in range 0 - height, we need it to be -1 to 1
			// needs to be the inverse of x since x is left to right both in world and raster space
			// but -1 y is down in world space but up in raster space
			var scalarOffsetY = 1f - y / (float)height * 2f;
			var dir = _direction + scalarOffsetX * right + scalarOffsetY * up;
			dir.Normalize();
			return new Ray(_position, dir);
		}

		private Vector3 Up
		{
			get
			{
				var v2 = Vector3.Cross(_direction, Right);
				v2.Normalize();
				var up = v2;
				return up;
			}
		}

		private Vector3 Right
		{
			get
			{
				var v1 = Vector3.Cross(_unitUp, _direction);
				v1.Normalize();
				var right = v1 * _device.Viewport.AspectRatio;
				return right;
			}
		}

		/// <summary>
		/// Rotates the camera around the given x and y amount.
		/// </summary>
		/// <param name="x">The amount of left/right rotation.</param>
		/// <param name="y">The amount of up/down rotation.</param>
		public void Rotate(float x, float y)
		{
			_rotateHorizontal += x;
			_rotateVertical = MathHelper.Clamp(_rotateVertical + y, -MathHelper.PiOver2 + 0.0001f, MathHelper.PiOver2 - 0.0001f);

			_direction = Vector3.Transform(_initialDirection, Matrix.CreateRotationZ(_rotateVertical) * Matrix.CreateRotationY(_rotateHorizontal));
		}

		/// <summary>
		/// Moves the camera around. The values are in relation to the direction the camera is facing.
		/// </summary>
		/// <param name="x">The amount of forward/backwards movement. Positive means forward, negative backwards.</param>
		/// <param name="y">The amount of left/right movement. Positive means right, negative left.</param>
		public void Move(float x, float y)
		{
			_position += _direction * x;
			_position += Right * y;
		}

		/// <summary>
		/// Creates a new camera with the same parameters as the current one.
		/// </summary>
		/// <returns></returns>
		public Camera Clone()
		{
			var cam = new Camera(_device, _position)
			{
				_direction = _direction,
				_rotateHorizontal = _rotateHorizontal,
				_rotateVertical = _rotateVertical
			};
			return cam;
		}
	}
}