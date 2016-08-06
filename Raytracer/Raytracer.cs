using Microsoft.Xna.Framework;
using Raytracer.SceneObjects;
using System;
using System.Linq;
using System.Threading;

namespace Raytracer
{
	public class Raytracer
	{
		/// <summary>
		/// Number of times the ray may bounce off of surfaces for reflection calculations
		/// </summary>
		private const int MaxDepth = 4;

		private readonly Random _random = new Random();

		/// <summary>
		/// When called will trace the scene from the given camera location.
		/// Width * height must equal the length of the color array.
		/// Each pixel in the color array will be traced. The convention is X lines first then y lines (accessed via tracingTarget[x + y * width]).
		/// </summary>
		/// <param name="scene"></param>
		/// <param name="camera"></param>
		/// <param name="options"></param>
		/// <param name="token">Optional cancellation token. If cancelled the method will return false.</param>
		/// <returns>True if the scene was fully traced, false otherwise.</returns>>
		public virtual bool TraceScene(Scene scene, Camera camera, TracingOptions options, CancellationToken? token = null)
		{
			var width = options.Width;
			var height = options.Height;
			var rayCountX = width - 1;
			var rayCountY = height - 1;
			var tracingTarget = options.TracingTarget;
			for (int y = 0; y < height; y++)
			{
				for (int x = 0; x < width; x++)
				{
					if (token?.IsCancellationRequested ?? false)
					{
						return false;
					}
					Vector3 averageColor = Vector3.Zero;
					for (int i = 0; i < options.SampleCount; i++)
					{
						var ray = camera.GetRayForRasterPosition(x, y, rayCountX, rayCountY);
						var cv = GetColorVectorForRay(scene, ray, 0, i);
						cv = Vector3.Clamp(cv, Vector3.Zero, Vector3.One);
						averageColor += cv;
					}
					averageColor /= options.SampleCount;
					tracingTarget[x + y * width] = new Color(averageColor);
				}
			}
			return true;
		}


		/// <summary>
		/// Traces the ray into the scene and returns the vector representing the color.
		/// </summary>
		/// <param name="scene"></param>
		/// <param name="ray"></param>
		/// <param name="depth"></param>
		/// <param name="sampleId"></param>
		/// <returns></returns>
		protected Vector3 GetColorVectorForRay(Scene scene, Ray ray, int depth, int sampleId)
		{
			var intersection = CheckRayIntersection(ray, scene);
			if (!intersection.HasValue)
			{
				return Vector3.Zero;
			}
			var intersectionPoint = intersection.Value;

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

			var color = CalculateNaturalColor(posOnObject, normal, scene, intersectionPoint.IntersectedObject.Surface, sampleId);
			if (depth >= MaxDepth)
			{
				return color + Vector3.One / 2f;
			}
			return color + GetReflectionColor(intersectionPoint.IntersectedObject, posOnObject, reflectionDirection, scene, depth, sampleId);
		}

		/// <summary>
		/// Returns the closest intersection point (if any), otherwise null.
		/// </summary>
		/// <param name="ray"></param>
		/// <param name="scene"></param>
		/// <returns></returns>
		public Intersection? CheckRayIntersection(Ray ray, Scene scene)
		{
			var intersections = scene.GetIntersections(ray);
			if (intersections.Count == 0)
			{
				return null;
			}
			// sort by distance, we need closest object
			intersections.Sort((a, b) => a.Distance.CompareTo(b.Distance));

			var intersectionPoint = intersections.First();
			return intersectionPoint;
		}

		private Vector3 GetReflectionColor(ISceneObject intersectedObject, Vector3 posOnObject, Vector3 reflectionDirection, Scene scene, int depth, int sampleId)
		{
			// multiply reflection power with a ray from the object surface in the reflection direction
			// move ray slightly away from the object, otherwise we don't get the correct reflection as the ray will simply hit the object with distance = 0
			Ray reflectionRay = new Ray(posOnObject + 0.001f * reflectionDirection, reflectionDirection);
			return intersectedObject.Surface.Reflect(posOnObject) * GetColorVectorForRay(scene, reflectionRay, depth + 1, sampleId);
		}

		private float Random()
		{
			return (float)_random.NextDouble();
		}

		/// <summary>
		/// Calculates the color at the specific location.
		/// </summary>
		/// <param name="posOnObject"></param>
		/// <param name="normal"></param>
		/// <param name="scene"></param>
		/// <param name="surface">The surface that was hit.</param>
		/// <returns></returns>
		private Vector3 CalculateNaturalColor(Vector3 posOnObject, Vector3 normal, Scene scene, ISurface surface, int sampleId)
		{
			var ret = new Vector3(0.1f);
			foreach (var light in scene.Lights)
			{
				var lPos = light.Position;
				Vector3 rndVec = Vector3.Zero;
				if (sampleId > 1)
				{
					// apply random offset for light source (this will create a slightly different light direction and thus a slightly different light bounce direction)
					// and thus in turn create soft shadows
					// note that we only apply this random offset to samples 2 and on
					// if the user selected only a single sample then we don't create offset and thus allow him to raytrace with hard shadows

					// offset light by +- factor in XYZ
					// lights can be anywhere in the box of [pos - vec(factor);pos + vec(factor)]
					const float factor = 0.1f;
					rndVec = new Vector3(-factor + Random() * factor * 2, -factor + Random() * factor * 2, -factor + Random() * factor * 2);
				}
				lPos += rndVec;
				var lightDistance = lPos - posOnObject;
				var lightDir = Vector3.Normalize(lightDistance);

				// check if there is another object between the light source and the position for which to calculate the lighting
				// if so, then this light doesn't actually hit the object, so it can be ignored
				var intersects = CheckRayIntersection(new Ray(posOnObject + lightDir * 0.001f, lightDir), scene);
				if (intersects.HasValue)
				{
					var distance = intersects.Value.Distance;
					bool isInShadow = distance <= lightDistance.Length();
					// ignore light if the object is in the shadow of another object
					if (isInShadow)
						continue;
				}
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