using Microsoft.Xna.Framework;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Raytracer
{
	/// <summary>
	/// The multithreaded raytracer will raytrace the scene in parallel.
	/// </summary>
	public class MultiThreadedRaytracer : Raytracer
	{
		public override bool TraceScene(Scene scene, Camera camera, TracingOptions options, CancellationToken? token = null)
		{
			// using Parallel.For to automatically get multithreading
			var width = options.Width;
			var height = options.Height;
			var rayCountX = width - 1;
			var rayCountY = height - 1;
			var tracingTarget = options.TracingTarget;
			int range = options.Width * options.Height;

			// divided by 2 because we usually have foreground + background raytracer running at the same time, if we don't then each will spawn as many threads as we have cores which will cause additional lag
			// due to too many thread context switches
			var pi = new ParallelOptions { MaxDegreeOfParallelism = Environment.ProcessorCount / 2 };
			var r = Parallel.For(0, range, pi, (i, loopState) =>
			 {

				 int x = i / options.Width;
				 int y = i % options.Height;

				 if (loopState.ShouldExitCurrentIteration)
					 return;

				 if (token?.IsCancellationRequested ?? false)
				 {
					 loopState.Stop();
					 return;
				 }
				 Vector3 averageColor = Vector3.Zero;
				 for (int sample = 0; sample < options.SampleCount; sample++)
				 {
					 var ray = camera.GetRayForRasterPosition(x, y, rayCountX, rayCountY);
					 var cv = GetColorVectorForRay(scene, ray, 0, sample);
					 cv = Vector3.Clamp(cv, Vector3.Zero, Vector3.One);
					 averageColor += cv;
				 }
				 averageColor /= options.SampleCount;
				 tracingTarget[x + y * width] = new Color(averageColor);
			 });

			return r.IsCompleted;
		}
	}
}