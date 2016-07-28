﻿using Microsoft.Xna.Framework;
using Raytracer.SceneObjects;
using System;
using System.Linq;

namespace Raytracer
{
	public class Raytracer
	{
		/// <summary>
		/// Number of times the ray may bounce off of surfaces for reflection calculations
		/// </summary>
		private const int MaxDepth = 4;

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
					// invert Y position because in world coordinates we declared Y as up, but in pixels Y is down
					var color = ComputeColorAtPosition(x, rayCountY - y, rayCountX, rayCountY, camera, scene);
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
			var cv = GetColorVectorForRay(scene, ray, 0);
			cv = Vector3.Clamp(cv, Vector3.Zero, Vector3.One);
			return new Color(cv);
		}

		/// <summary>
		/// Traces the ray into the scene and returns the vector representing the color.
		/// </summary>
		/// <param name="scene"></param>
		/// <param name="ray"></param>
		/// <param name="depth"></param>
		/// <returns></returns>
		private Vector3 GetColorVectorForRay(Scene scene, Ray ray, int depth)
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

			var reflectionDirection = ray.Direction - 2 * Vector3.Dot(normal, ray.Direction) * normal;

			var color = CalculateNaturalColor(posOnObject, normal, scene, intersectionPoint.IntersectedObject.Surface);
			if (depth >= MaxDepth)
			{
				return color + Vector3.One / 2f;
			}
			return color + GetReflectionColor(intersectionPoint.IntersectedObject, posOnObject, reflectionDirection, scene, depth);
		}

		private Vector3 GetReflectionColor(ISceneObject intersectedObject, Vector3 posOnObject, Vector3 reflectionDirection, Scene scene, int depth)
		{
			// multiply reflection power with a ray from the object surface in the reflection direction
			// move ray slightly away from the object, otherwise we don't get the correct reflection as the ray will simply hit the object with distance = 0
			Ray reflectionRay = new Ray(posOnObject + 0.001f * reflectionDirection, reflectionDirection);
			return intersectedObject.Surface.Reflect(posOnObject) * GetColorVectorForRay(scene, reflectionRay, depth + 1);
		}

		/// <summary>
		/// Calculates the color at the specific location.
		/// </summary>
		/// <param name="posOnObject"></param>
		/// <param name="normal"></param>
		/// <param name="scene"></param>
		/// <param name="surface">The surface that was hit.</param>
		/// <returns></returns>
		private Vector3 CalculateNaturalColor(Vector3 posOnObject, Vector3 normal, Scene scene, ISurface surface)
		{
			var ret = Vector3.Zero;
			foreach (var light in scene.Lights)
			{
				var lightDistance = light.Position - posOnObject;
				var lightDir = Vector3.Normalize(lightDistance);

				// calculate brightness
				var illumination = Vector3.Dot(lightDir, normal);
				var c = light.Color.ToVector3() * light.Intensity;
				var color = illumination > 0 ? c * illumination : Vector3.Zero;
				ret += color * surface.Diffuse(posOnObject);

				var specular = Vector3.Dot(lightDir, normal);
				var specularColor = specular > 0 ? c * (float)Math.Pow(specular, surface.Shininess) : Vector3.Zero;
				ret += specularColor * surface.Specular(posOnObject);
			}
			return ret;
		}
	}
}