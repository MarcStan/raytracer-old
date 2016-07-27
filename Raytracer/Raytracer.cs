using Microsoft.Xna.Framework;
using Raytracer.SceneObjects;
using System;
using System.Linq;

namespace Raytracer
{
	public class Raytracer
	{
		/// <summary>
		/// Returns the default raster size this raytracer uses.
		/// </summary>
		public int RasterSize { get; }

		/// <summary>
		/// Creates a new raytracer with the specific stepsize.
		/// </summary>
		/// <param name="rasterSize">Must be either 1 or power of 2. The rasterSize determines how many pixels are rasterized. A rasterSize of 1 means every pixel is rasterized.
		/// rasterSize of 2 means every second pixel is rasterized (thus the smallest unit is a 2 by 2 pixel block), etc.</param>
		public Raytracer(int rasterSize = 1)
		{
			// check if power of 2
			if ((rasterSize & (rasterSize - 1)) != 0)
			{
				throw new ArgumentOutOfRangeException(nameof(rasterSize), "must be power of 2");
			}
			RasterSize = rasterSize;
		}

		/// <summary>
		/// When called will trace the scene from the given camera location.
		/// Width * height must equal the length of the color array.
		/// Each pixel in the color array will be traced. The convention is X lines first then y lines (accessed via tracingTarget[x + y * width]).
		/// </summary>
		/// <param name="scene"></param>
		/// <param name="camera"></param>
		/// <param name="options"></param>
		/// <returns>True if the scene was fully traced, false otherwise.</returns>>
		public bool TraceScene(Scene scene, Camera camera, TracingOptions options)
		{
			var width = options.Width;
			var height = options.Height;
			var raster = options.RasterSize ?? RasterSize;
			var rayCountX = width - 1;
			var rayCountY = height - 1;
			var tracingTarget = options.TracingTarget;
			for (int y = 0; y < height; y += raster)
			{
				for (int x = 0; x < width; x += raster)
				{
					if (options.CancellationToken?.IsCancellationRequested ?? false)
					{
						return false;
					}
					var color = ComputeColorAtPosition(x, y, rayCountX, rayCountY, camera, scene);
					// set color to the entire raster block size
					for (int i = 0; i < raster; i++)
					{
						for (int j = 0; j < raster; j++)
						{
							tracingTarget[x + j + (y + i) * width] = color;
						}
					}
				}
			}
			return true;
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
			return new Color(GetColorVectorForRay(scene, ray));
		}

		/// <summary>
		/// Traces the ray into the scene and returns the vector representing the color.
		/// </summary>
		/// <param name="scene"></param>
		/// <param name="ray"></param>
		/// <returns></returns>
		private Vector3 GetColorVectorForRay(Scene scene, Ray ray)
		{
			var intersections = scene.GetIntersections(ray);
			if (intersections.Count == 0)
			{
				return Vector3.Zero;
			}
			// sort by distance, we need closest object
			intersections.Sort((a, b) => a.Distance.CompareTo(b.Distance));

			var intersectionPoint = intersections.First();

			// special case. any lightsource will be unaffected by other lights and simply return its own color
			// this is mainly for debugging purposes (see where the light in the scene is actually positioned)
			if (intersectionPoint.IntersectedObject is LightSource)
			{
				var light = ((LightSource)intersectionPoint.IntersectedObject).Light;
				return light.Color.ToVector3();
			}

			// get hit location and normal at hit location
			var posOnObject = ray.Position + ray.Direction * intersectionPoint.Distance;
			var normal = intersectionPoint.IntersectedObject.Normal(posOnObject);

			return CalculateNaturalColor(posOnObject, normal, scene);
		}

		/// <summary>
		/// Calculates the color at the specific location.
		/// </summary>
		/// <param name="posOnObject"></param>
		/// <param name="normal"></param>
		/// <param name="scene"></param>
		/// <returns></returns>
		private Vector3 CalculateNaturalColor(Vector3 posOnObject, Vector3 normal, Scene scene)
		{
			var ret = Vector3.Zero;
			foreach (var light in scene.Lights)
			{
				var lightDistance = posOnObject - light.Position;
				var lightDir = Vector3.Normalize(lightDistance);

				// calculate brightness
				var illumination = Vector3.Dot(lightDir, normal);
				var color = illumination > 0 ? illumination * light.Color.ToVector3() * light.Intensity : Vector3.Zero;
				ret += color;
			}
			return ret;
		}
	}
}