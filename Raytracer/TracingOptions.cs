using Microsoft.Xna.Framework;
using System;

namespace Raytracer
{
	/// <summary>
	/// Holds tracing options for the raytracer.
	/// </summary>
	public class TracingOptions
	{
		public int Width { get; }

		public int Height { get; }

		/// <summary>
		/// Gets how many rays are cast per pixel.
		/// </summary>
		public int SampleCount { get; }

		/// <summary>
		/// The target array that is to be filled with the raytraced color values.
		/// Its size is guaranteed to be Width*Height.
		/// </summary>
		public Color[] TracingTarget { get; }

		public TracingOptions(int w, int h, Color[] target, int sampleCount)
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
			if (sampleCount < 1)
			{
				throw new ArgumentOutOfRangeException(nameof(sampleCount));
			}

			SampleCount = sampleCount;
			Width = w;
			Height = h;
			TracingTarget = target;
		}
	}
}