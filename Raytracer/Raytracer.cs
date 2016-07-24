using Microsoft.Xna.Framework;
using System;
using System.Linq;

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
			var rayCountX = width - 1;
			var rayCountY = height - 1;
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					tracingTarget[x + y * width] = ComputeColorAtPosition(x, y, rayCountX, rayCountY, camera, scene);
				}
			}
		}

		/// <summary>
		/// Returns the color at the specific position on the raster.
		/// All rays will be traced from the camera into the scene.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		/// <param name="camera"></param>
		/// <param name="scene"></param>
		/// <returns></returns>
		private Color ComputeColorAtPosition(int x, int y, int width, int height, Camera camera, Scene scene)
		{
			if (width <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(width));
			}
			if (height <= 0)
			{
				throw new ArgumentOutOfRangeException(nameof(height));
			}
			if (x < 0 || x > width)
			{
				throw new ArgumentOutOfRangeException(nameof(x));
			}
			if (y < 0 || y > height)
			{
				throw new ArgumentOutOfRangeException(nameof(y));
			}

			var ray = camera.GetRayForRasterPosition(x, y, width, height);
			var intersections = scene.GetIntersections(ray);
			if (intersections.Count == 0)
				return Color.Black;
			// sort by distance, we need closest object
			intersections.Sort((a, b) => a.Distance.CompareTo(b.Distance));

			var intersectionPoint = intersections.First();
			return CalculateColorFromLights(intersectionPoint, scene);
		}

		private Color CalculateColorFromLights(Intersection intersectionPoint, Scene scene)
		{
			throw new System.NotImplementedException();
		}
	}
}