using Microsoft.Xna.Framework;

namespace Raytracer
{
	public class Raytracer
	{
		/// <summary>
		/// When called will trace the scene from the given camera location.
		/// Width * height must equal the length of the color array.
		/// Each pixel in the color array will be traced. The convention is X lines first then y lines (accessed via tracingTarget[x + y * width]).
		/// </summary>
		/// <param name="scene"></param>
		/// <param name="camera"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="tracingTarget"></param>
		public void TraceScene(Scene scene, Camera camera, int width, int height, ref Color[] tracingTarget)
		{
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					tracingTarget[x + y * width] = Color.White;
				}
			}
		}
	}
}