namespace Raytracer
{
	class Program
	{
		static void Main(string[] args)
		{
			using (var g = new RaytracerGame())
			{
				g.Run();
			}
		}
	}
}
