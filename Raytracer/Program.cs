using System;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace Raytracer
{
	class Program
	{
		[STAThread]
		static void Main(string[] args)
		{
			if (!File.Exists("raytracer.ini"))
			{
				try
				{
					// try to write default ini to disk
					using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Raytracer.raytracer.ini"))
					using (var f = File.OpenWrite("raytracer.ini"))
					{
						stream.CopyTo(f);
					}
					MessageBox.Show("Missing raytracer.ini. Created default ini.");
					return;
				}
				catch
				{
					MessageBox.Show("Missing raytracer.ini");
				}
			}
			var options = IniOptions.Parse("raytracer.ini");
			using (var g = new RaytracerGame(options))
			{
				g.Run();
			}
		}
	}
}
