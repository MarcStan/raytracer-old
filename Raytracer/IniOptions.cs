using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace Raytracer
{
	public class IniOptions
	{
		/// <summary>
		/// This value has proven to work in realtime on a decent computer (rasterized a 100*100 image in less than 10ms).
		/// </summary>
		public int RealtimeRasterLevel { get; }

		/// <summary>
		/// The number of rays cast per pixel when in realtime mode.
		/// More samples means a better shadow representation (soft shadows), although it is recommended to keep it at 1 for realtime samples.
		/// </summary>
		public int RealtimeSampleCount { get; }

		/// <summary>
		/// While not realtime capable this will result in a decent image in less than 130ms on a decent pc.
		/// </summary>
		public int BackgroundRasterLevel { get; }

		/// <summary>
		/// The number of rays cast per pixel when in background mode.
		/// More samples means a better shadow representation (soft shadows), the result will look decent starting at 32.
		/// </summary>
		public int BackgroundSampleCount { get; }

		/// <summary>
		/// If true, lightsources will be displayed as spheres.
		/// These spheres will not react to the lighting model but instead only return their base color.
		/// </summary>
		public bool ShowLightSources { get; }

		public bool Multithreaded { get; }

		public int Width { get; }

		public int Height { get; }

		public IniInput Input { get; }

		public SamplerState RealtimeSamplerState { get; }

		public SamplerState BackgroundSamplerState { get; }

		private IniOptions(int w, int h, int realtimeRaster, int backgroundRaster, bool showLightSources, IniInput input, SamplerState realtimeSampler, SamplerState backgroundSampler, int realtimeSamples, int backgroundSamples, bool multithread)
		{
			RealtimeSampleCount = realtimeSamples;
			BackgroundSampleCount = backgroundSamples;
			Width = w;
			Height = h;
			RealtimeRasterLevel = realtimeRaster;
			BackgroundRasterLevel = backgroundRaster;
			Input = input;
			ShowLightSources = showLightSources;
			RealtimeSamplerState = realtimeSampler;
			BackgroundSamplerState = backgroundSampler;
			Multithreaded = multithread;
		}

		public static IniOptions Parse(string file)
		{
			var lines = File.ReadAllLines(file);
			string sectionName = null;
			var map = new LoadedIniMap();
			var values = new Dictionary<string, string>();
			for (int i = 0; i < lines.Length; i++)
			{
				var trimmed = lines[i].Trim();
				if (trimmed.StartsWith("#") || string.IsNullOrEmpty(trimmed))
				{
					continue;
				}

				if (trimmed.StartsWith("[") && trimmed.EndsWith("]"))
				{
					if (sectionName != null)
					{
						map.Add(sectionName, values);
						values = new Dictionary<string, string>();
					}
					sectionName = trimmed.Substring(1, trimmed.Length - 2);
				}
				else
				{
					if (sectionName == null)
					{
						throw new NotSupportedException("Found ini values before first section");
					}
					var sp = trimmed.Split('=');
					var k = sp[0].Trim();
					var v = sp[1].Trim();
					values.Add(k, v);
				}
			}
			if (sectionName != null)
			{
				map.Add(sectionName, values);
			}

			bool invertX = bool.Parse(map["mouse"]["InvertXAxis"]);
			bool invertY = bool.Parse(map["mouse"]["InvertYAxis"]);
			float accelX = float.Parse(map["mouse"]["MouseAccelerationX"], NumberFormatInfo.InvariantInfo);
			float accelY = float.Parse(map["mouse"]["MouseAccelerationY"], NumberFormatInfo.InvariantInfo);

			var keyMap = map.First(k => k.Key == "input").Value.Select(d =>
			{
				var k = d.Value;
				var keys = (k.Contains("|") ? k.Split('|') : new[] { k }).Select(key => (Keys)Enum.Parse(typeof(Keys), key)).ToArray();
				return new KeyValuePair<string, Keys[]>(d.Key, keys);
			}).ToDictionary(d => d.Key, d => d.Value);


			var input = new IniInput(invertX, invertY, accelX, accelY, keyMap);

			int w = int.Parse(map["video"]["Width"]);
			int h = int.Parse(map["video"]["Height"]);
			int realtimeRaster = int.Parse(map["video"]["RealtimeRasterSize"]);
			int backgroundRaster = int.Parse(map["video"]["BackgroundRasterSize"]);
			int realtimeSamples = int.Parse(map["video"]["RealtimeSampleCount"]);
			int backgroundSamples = int.Parse(map["video"]["BackgroundSampleCount"]);
			var showLightSources = bool.Parse(map["video"]["ShowLightSources"]);

			var realtimeSampler = ParseSampler(map["video"]["RealtimeSamplerState"]);
			var backgroundSampler = ParseSampler(map["video"]["BackgroundSamplerState"]);
			var multithread = bool.Parse(map["video"]["Multithreaded"]);
			return new IniOptions(w, h, realtimeRaster, backgroundRaster, showLightSources, input, realtimeSampler, backgroundSampler, realtimeSamples, backgroundSamples, multithread);
		}

		private static SamplerState ParseSampler(string s)
		{
			// unfortunatelly SamplerState is not an enum, so we parse manually
			// we also don't care about wrap or clamp as our texture is only displayed once, so default to clamp always
			switch (s)
			{
				case "Point":
					return SamplerState.PointClamp;
				case "Linear":
					return SamplerState.LinearClamp;
				case "Anisotropic":
					return SamplerState.AnisotropicClamp;
				default:
					throw new NotSupportedException("Valid states are: Anisotropic, Linear, Point");
			}
		}

		public class IniInput
		{
			private readonly Dictionary<string, Keys[]> _actionMap;

			public bool InvertXAxis { get; }

			public bool InvertYAxis { get; }

			public float MouseAccelerationX { get; }

			public float MouseAccelerationY { get; }

			public IniInput(bool invertXAxis, bool invertYAxis, float mouseAccelerationX, float mouseAccelerationY, Dictionary<string, Keys[]> keyMap)
			{
				if (keyMap == null)
				{
					throw new ArgumentNullException(nameof(keyMap));
				}

				InvertXAxis = invertXAxis;
				InvertYAxis = invertYAxis;
				MouseAccelerationX = mouseAccelerationX;
				MouseAccelerationY = mouseAccelerationY;
				_actionMap = keyMap;
			}

			public bool InputAction(string key, Func<Keys, bool> checkKeyState)
			{
				return _actionMap[key].Any(checkKeyState);
			}
		}

		private class LoadedIniMap : Dictionary<string, Dictionary<string, string>>
		{

		}
	}
}