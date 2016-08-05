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
		/// The target array that is to be filled with the raytraced color values.
		/// Its size is guaranteed to be Width*Height.
		/// </summary>
		public Color[] TracingTarget { get; }

		public TracingOptions(int w, int h, Color[] target)
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
		}
	}
}