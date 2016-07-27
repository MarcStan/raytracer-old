using Microsoft.Xna.Framework;
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
			Width = w;
			Height = h;
			TracingTarget = target;
			RasterSize = raster;
			CancellationToken = token;
		}
	}
}