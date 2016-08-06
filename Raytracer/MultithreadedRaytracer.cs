using Microsoft.Xna.Framework;
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
			var width = options.Width;
			var height = options.Height;
			var rayCountX = width - 1;
			var rayCountY = height - 1;
			var tracingTarget = options.TracingTarget;
			int range = options.Width * options.Height;
			var r = Parallel.For(0, range, (i, loopState) =>
			{
				int x = i / options.Width;
				int y = i % options.Height;

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