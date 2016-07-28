using Microsoft.Xna.Framework;
using System;
using System.Threading;

namespace Raytracer
{
	public class TracingOptions
	{
		public int Width { get; }

		public int Height { get; }

		public Color[] TracingTarget { get; }

		public int? RasterSize { get; }

		public CancellationToken? CancellationToken { get; }

		public TracingOptions(int w, int h, Color[] target, int? raster = null, CancellationToken? token = null)
		{
			if (w < 0 || h < 0)
			{
				throw new ArgumentOutOfRangeException();
			}
			if (target == null)
			{
				throw new ArgumentNullException(nameof(target));
			}
			if (w * h != target.Length)
			{
				throw new ArgumentException("target array size must equal w*h");
			}

			Width = w;
			Height = h;
			TracingTarget = target;
			RasterSize = raster;
			CancellationToken = token;
		}
	}
}